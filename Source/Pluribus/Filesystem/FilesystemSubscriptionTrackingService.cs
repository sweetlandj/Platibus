// The MIT License (MIT)
// 
// Copyright (c) 2014 Jesse Sweetland
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Pluribus.Filesystem
{
    public class FilesystemSubscriptionTrackingService : ISubscriptionTrackingService, IDisposable
    {
        private bool _disposed;
        private readonly DirectoryInfo _baseDirectory;

        private readonly ConcurrentDictionary<TopicName, IEnumerable<ExpiringSubscription>> _subscriptions =
            new ConcurrentDictionary<TopicName, IEnumerable<ExpiringSubscription>>();

        private readonly SemaphoreSlim _writeAccess = new SemaphoreSlim(1);

        public FilesystemSubscriptionTrackingService(DirectoryInfo baseDirectory = null)
        {
            if (baseDirectory == null)
            {
                var appdomainDirectory = AppDomain.CurrentDomain.BaseDirectory;
                baseDirectory = new DirectoryInfo(Path.Combine(appdomainDirectory, "pluribus", "subscriptions"));
            }
            _baseDirectory = baseDirectory;
        }

        public Task AddSubscription(TopicName topic, Uri subscriber, TimeSpan ttl = default(TimeSpan),
            CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            var expirationDate = ttl <= TimeSpan.Zero ? DateTime.MaxValue : DateTime.UtcNow + ttl;
            var expiringSubscription = new ExpiringSubscription(subscriber, expirationDate);

            _subscriptions.AddOrUpdate(topic, new[] {expiringSubscription},
                (t, existing) => new[] {expiringSubscription}.Union(existing).ToList());

            return FlushSubscriptionsToDisk(topic);
        }

        public Task RemoveSubscription(TopicName topic, Uri subscriber, TimeSpan ttl = default(TimeSpan),
            CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            _subscriptions.AddOrUpdate(topic, new ExpiringSubscription[0],
                (t, existing) => existing.Where(se => se.Subscriber != subscriber).ToList());

            return FlushSubscriptionsToDisk(topic);
        }

        public IEnumerable<Uri> GetSubscribers(TopicName topicName)
        {
            CheckDisposed();
            IEnumerable<ExpiringSubscription> subscriptions;
            _subscriptions.TryGetValue(topicName, out subscriptions);
            return (subscriptions ?? Enumerable.Empty<ExpiringSubscription>())
                .Where(s => s.ExpirationDate > DateTime.UtcNow)
                .Select(s => s.Subscriber);
        }

        public Task Init()
        {
            _baseDirectory.Refresh();
            if (!_baseDirectory.Exists)
            {
                _baseDirectory.Create();
                _baseDirectory.Refresh();
            }

            var subscriptionFiles = _baseDirectory.GetFiles("*.psub");
            var topicNames = subscriptionFiles
                .Select(file => new TopicName(file.Name.Replace(".psub", "")));

            return Task.WhenAll(topicNames.Select(LoadSubscriptionsFromDisk));
        }

        private async Task LoadSubscriptionsFromDisk(TopicName topicName)
        {
            var subscriptions = new List<ExpiringSubscription>();
            var subscriptionFile = GetSubscriptionFile(topicName);
            if (!subscriptionFile.Exists) return;

            using (var fileStream = subscriptionFile.Open(FileMode.Open, FileAccess.Read, FileShare.Read))
            using (var fileReader = new StreamReader(fileStream))
            {
                string line;
                while ((line = await fileReader.ReadLineAsync().ConfigureAwait(false)) != null)
                {
                    var parts = line.Split(' ');
                    var addressPart = parts[0];
                    var expirationPart = parts.Length > 1 ? parts[1] : null;

                    var subscriber = new Uri(addressPart);
                    var expirationDate = DateTime.ParseExact(expirationPart, "o", CultureInfo.InvariantCulture);
                    if (expirationDate > DateTime.UtcNow)
                    {
                        subscriptions.Add(new ExpiringSubscription(subscriber, expirationDate));
                    }
                }
            }
            _subscriptions.AddOrUpdate(topicName, subscriptions, (t, s) => s.Union(subscriptions).ToList());
        }

        private async Task FlushSubscriptionsToDisk(TopicName topicName)
        {
            IEnumerable<ExpiringSubscription> expiringSubscriptions;
            if (!_subscriptions.TryGetValue(topicName, out expiringSubscriptions))
            {
                return;
            }

            var subscriptionFile = GetSubscriptionFile(topicName);
            await _writeAccess.WaitAsync();
            try
            {
                using (var fileStream = subscriptionFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var fileWriter = new StreamWriter(fileStream))
                {
                    foreach (var subscription in expiringSubscriptions.Where(s => s.ExpirationDate > DateTime.UtcNow))
                    {
                        var address = subscription.Subscriber.ToString();
                        var expirationDate = subscription.ExpirationDate;
                        var line = string.Format(CultureInfo.InvariantCulture, "{0} {1:o}", address, expirationDate);
                        await fileWriter.WriteLineAsync(line).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                _writeAccess.Release();
            }
        }

        ~FilesystemSubscriptionTrackingService()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _writeAccess.Dispose();
            }
        }

        private void CheckDisposed()
        {
            if (_disposed) throw new ObjectDisposedException(GetType().FullName);
        }

        private FileInfo GetSubscriptionFile(TopicName topicName)
        {
            var filename = string.Format("{0}.psub", topicName);
            var filePath = Path.Combine(_baseDirectory.FullName, filename);
            return new FileInfo(filePath);
        }

        private class ExpiringSubscription : IEquatable<ExpiringSubscription>
        {
            private readonly DateTime _expirationDate;
            private readonly Uri _subscriber;

            public ExpiringSubscription(Uri subscriber, DateTime expirationDate)
            {
                _subscriber = subscriber;
                _expirationDate = expirationDate;
            }

            public Uri Subscriber
            {
                get { return _subscriber; }
            }

            public DateTime ExpirationDate
            {
                get { return _expirationDate; }
            }

            public bool Equals(ExpiringSubscription subscription)
            {
                if (ReferenceEquals(this, subscription)) return true;
                if (ReferenceEquals(null, subscription)) return false;
                return Equals(_subscriber, subscription._subscriber);
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as ExpiringSubscription);
            }

            public override int GetHashCode()
            {
                return _subscriber.GetHashCode();
            }
        }
    }
}