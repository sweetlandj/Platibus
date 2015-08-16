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