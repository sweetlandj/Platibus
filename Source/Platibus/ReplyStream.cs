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
using System.Collections.Generic;

namespace Platibus
{
    internal class ReplyStream : IObservable<object>
    {
        private bool _complete;
        private Exception _error;
        private readonly IList<object> _replies = new List<object>();
        private readonly IList<Subscription> _subscriptions = new List<Subscription>();
        private readonly object _syncRoot = new object();

        public IDisposable Subscribe(IObserver<object> observer)
        {
            var subscription = new Subscription(observer, Unsubscribe);
            lock (_syncRoot)
            {
                // Add the subscription and get caught up with any replies
                // that have been received between the time the message
                // was sent and the reply stream was created.
                _subscriptions.Add(subscription);
                foreach (var reply in _replies)
                {
                    subscription.Observer.OnNext(reply);
                }

                if (_error != null)
                {
                    subscription.Observer.OnError(_error);
                }
                else if (_complete)
                {
                    subscription.Observer.OnCompleted();
                }
            }
            return subscription;
        }

        private void Unsubscribe(Subscription subscription)
        {
            lock (_syncRoot)
            {
                _subscriptions.Remove(subscription);
            }
        }

        public void NotifyReplyReceived(object reply)
        {
            IList<Subscription> subscriptions;
            lock (_syncRoot)
            {
                _replies.Add(reply);
                subscriptions = new List<Subscription>(_subscriptions);
            }

            foreach (var subscription in subscriptions)
            {
                subscription.Observer.OnNext(reply);
            }
        }

        public void NotifyErrorReply(Exception error)
        {
            IList<Subscription> subscriptions;
            lock (_syncRoot)
            {
                _error = error;
                subscriptions = new List<Subscription>(_subscriptions);
            }

            foreach (var subscription in subscriptions)
            {
                subscription.Observer.OnError(error);
            }
        }

        public void NotifyCompleted()
        {
            IList<Subscription> subscriptions;
            lock (_syncRoot)
            {
                _complete = true;
                subscriptions = new List<Subscription>(_subscriptions);
            }

            foreach (var subscription in subscriptions)
            {
                subscription.Observer.OnCompleted();
            }
        }

        private class Subscription : IDisposable
        {
            private readonly IObserver<object> _observer;
            private readonly Action<Subscription> _unsubscribe;

            public Subscription(IObserver<object> observer, Action<Subscription> unsubscribe)
            {
                _observer = observer;
                _unsubscribe = unsubscribe;
            }

            public IObserver<object> Observer
            {
                get { return _observer; }
            }

            public void Dispose()
            {
                if (_unsubscribe != null) _unsubscribe(this);
            }
        }
    }
}