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

using System.Transactions;

namespace Platibus
{
    /// <summary>
    /// Extensions for working with <see cref="IMessageContext"/>s
    /// </summary>
    public static class MessageContextExtensions
    {
        /// <summary>
        /// Creates a new transaction and ensures that the <paramref name="messageContext"/> 
        /// will be acknowledged if and when the ambient transaction completes successfully
        /// </summary>
        /// <param name="messageContext">The message context to tie to the transaction</param>
        /// <param name="option">The transaction scope option</param>
        /// <returns>Returns the new transaction scope</returns>
        public static TransactionScope StartTransaction(this IMessageContext messageContext,
            TransactionScopeOption option = TransactionScopeOption.Required)
        {
            var scope = new TransactionScope(option);
            messageContext.EnlistInCurrentTransaction();
            return scope;
        }

        /// <summary>
        /// Enlists the <paramref name="messageContext"/> in the ambient transaction (if there
        /// is one) such that the <paramref name="messageContext"/> will be acknowldged if and
        /// when the ambient transaction completes successfully.
        /// </summary>
        /// <param name="messageContext">The message context to tie to the ambient transaction</param>
        public static void EnlistInCurrentTransaction(this IMessageContext messageContext)
        {
            var currentTransaction = Transaction.Current;
            if (currentTransaction != null)
            {
                currentTransaction.TransactionCompleted += (sender, e) =>
                {
                    var transactionStatus = e.Transaction.TransactionInformation.Status;
                    if (transactionStatus == TransactionStatus.Committed)
                    {
                        messageContext.Acknowledge();
                    }
                };
            }
        }
    }
}