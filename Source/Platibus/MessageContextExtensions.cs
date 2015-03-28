using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Platibus
{
    public static class MessageContextExtensions
    {
        public static TransactionScope StartTransaction(this IMessageContext messageContext, TransactionScopeOption option = TransactionScopeOption.Required)
        {
            var scope = new TransactionScope(option);
            messageContext.EnlistInCurrentTransaction();
            return scope;
        }

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
