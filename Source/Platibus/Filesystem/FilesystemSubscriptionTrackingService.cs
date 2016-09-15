// The MIT License (MIT)
// 
// Copyright (c) 2016 Jesse Sweetland
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

namespace Platibus.Filesystem
{
    /// <summary>
    /// A <see cref="ISubscriptionTrackingService"/> that stores subscriptions as files on disk
    /// </summary>
    public class FilesystemSubscriptionTrackingService : ISubscriptionTrackingService, IDisposable
    {
        private bool _disposed;
        private readonly DirectoryInfo _baseDirectory;

        private readonly ConcurrentDictionary<TopicName, IEnumerable<ExpiringSubscription>> _subscriptions =
            new ConcurrentDictionary<TopicName, IEnumerable<ExpiringSubscription>>();

        private readonly SemaphoreSlim _fileAccess = new SemaphoreSlim(1);

        /// <summary>
        /// Initializes a new <see cref="FilesystemSubscriptionTrackingService"/> that will create
        /// directories and files relative to the specified <paramref name="baseDirectory"/>
        /// </summary>
        /// <param name="baseDirectory">(Optional) The directory in which subscription files
        /// will be stored</param>
        /// <remarks>
        /// If a base directory is not specified then the base directory will default to a
        /// directory named <c>platibus\subscriptions</c> beneath the current app domain base 
        /// directory.  If the base directory does not exist it will be created in the
        /// <see cref="Init"/> method.
        /// </remarks>
        public FilesystemSubscriptionTrackingService(DirectoryInfo baseDirectory = null)
        {
            if (baseDirectory == null)
            {
                var appdomainDirectory = AppDomain.CurrentDomain.BaseDirectory;
                baseDirectory = new DirectoryInfo(Path.Combine(appdomainDirectory, "platibus", "subscriptions"));
            }
            _baseDirectory = baseDirectory;
        }

        /// <summary>
        /// Adds or updates a subscription
        /// </summary>
        /// <param name="topic">The topic to which the <paramref name="subscriber"/> is
        /// subscribing</param>
        /// <param name="subscriber">The base URI of the subscribing Platibus instance</param>
        /// <param name="ttl">(Optional) The maximum Time To Live (TTL) for the subscription</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by
        /// the caller to cancel the addition of the subscription</param>
        /// <returns>Returns a task that will complete when the subscription has been added or
        /// updated</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="topic"/> or
        /// <paramref name="subscriber"/> is <c>null</c></exception>
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

        /// <summary>
        /// Removes a subscription
        /// </summary>
        /// <param name="topic">The topic to which the <paramref name="subscriber"/> is
        /// subscribing</param>
        /// <param name="subscriber">The base URI of the subscribing Platibus instance</param>
        /// <param name="cancellationToken">(Optional) A cancellation token that can be used by
        /// the caller to cancel the subscription removal</param>
        /// <returns>Returns a task that will complete when the subscription has been removed</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="topic"/> or
        /// <paramref name="subscriber"/> is <c>null</c></exception>
        public Task RemoveSubscription(TopicName topic, Uri subscriber,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            _subscriptions.AddOrUpdate(topic, new ExpiringSubscription[0],
                (t, existing) => existing.Where(se => se.Subscriber != subscriber).ToList());

            return FlushSubscriptionsToDisk(topic);
        }

        /// <summary>
        /// Returns a list of the current, non-expired subscriber URIs for a topic
        /// </summary>
        /// <param name="topic">The topic</param>
        /// <param name="cancellationToken">A cancellation token that can be used by the caller
        /// to cancel the query</param>
        /// <returns>Returns a task whose result is the distinct set of base URIs of all Platibus
        /// instances subscribed to the specified local topic</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="topic"/> is <c>null</c>
        /// </exception>
        public Task<IEnumerable<Uri>> GetSubscribers(TopicName topic,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            CheckDisposed();
            IEnumerable<ExpiringSubscription> subscriptions;
            _subscriptions.TryGetValue(topic, out subscriptions);
            var activeSubscribers = (subscriptions ?? Enumerable.Empty<ExpiringSubscription>())
                .Where(s => s.ExpirationDate > DateTime.UtcNow)
                .Select(s => s.Subscriber)
                .ToList();

            return Task.FromResult(activeSubscribers.AsEnumerable());
        }

        /// <summary>
        /// Initializes the filesystem subscription tracking service
        /// </summary>
        /// <returns>Returns a task that will complete when the filesystem subscription 
        /// tracking service has finished initializing</returns>
        /// <remarks>
        /// Creates directories if they do not exist and loads current subscriptions from disk
        /// </remarks>
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

            string fileContents;
            await _fileAccess.WaitAsync();
            try
            {
                using (var fileStream = subscriptionFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var fileReader = new StreamReader(fileStream))
                {
                    fileContents = await fileReader.ReadToEndAsync();        
                }
            }
            finally
            {
                _fileAccess.Release();
            }

            using(var stringReader = new StringReader(fileContents))
            {
                string line;
                while ((line = stringReader.ReadLine()) != null)
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

            string fileContents;
            using (var stringWriter = new StringWriter())
            {
                foreach (var subscription in expiringSubscriptions.Where(s => s.ExpirationDate > DateTime.UtcNow))
                {
                    var address = subscription.Subscriber.ToString();
                    var expirationDate = subscription.ExpirationDate;
                    var line = string.Format(CultureInfo.InvariantCulture, "{0} {1:o}", address, expirationDate);
                    stringWriter.WriteLine(line);
                }
                fileContents = stringWriter.ToString();
            }

            var subscriptionFile = GetSubscriptionFile(topicName);
            await _fileAccess.WaitAsync();
            try
            {
                using (var fileStream = subscriptionFile.Open(FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var fileWriter = new StreamWriter(fileStream))
                {
                    await fileWriter.WriteAsync(fileContents);
                }
            }
            finally
            {
                _fileAccess.Release();
            }
        }

        /// <summary>
        /// Finalizer that ensures that resources are released
        /// </summary>
        ~FilesystemSubscriptionTrackingService()
        {
            Dispose(false);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (_disposed) return;
            Dispose(true);
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Called by <see cref="Dispose()"/> or the finalizer to ensure that resources are released
        /// </summary>
        /// <param name="disposing">Indicates whether this method is called from the 
        /// <see cref="Dispose()"/> method (<c>true</c>) or the finalizer (<c>false</c>)</param>
        /// <remarks>
        /// This method will not be called more than once
        /// </remarks>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_fileAccess")]
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _fileAccess.TryDispose();
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