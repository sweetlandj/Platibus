using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Moq;
using NUnit.Framework;

namespace Platibus.UnitTests
{
    internal class MessageContextExtensionsTests
    {
        [Test]
        public async Task MessageAcknowledgedWhenContextEnlistedInCompletedTransaction()
        {
            var message = "Hello, world!";
            var mockMessageContext = new Mock<IMessageContext>();
            var cancellationTokenSource = new CancellationTokenSource();
            var messageHandler = new DelegateMessageHandler((msg, msgCtx) =>
            {
                using (var scope = new TransactionScope())
                {
                    msgCtx.EnlistInCurrentTransaction();
                    scope.Complete();
                }
            });

            await messageHandler.HandleMessage(message, mockMessageContext.Object, cancellationTokenSource.Token);
            mockMessageContext.Verify(msgCtx => msgCtx.Acknowledge(), Times.Once());
        }

        [Test]
        public async Task MessageNotAcknowledgedWhenContextEnlistedInIncompleteTransaction()
        {
            var message = "Hello, world!";
            var mockMessageContext = new Mock<IMessageContext>();
            var cancellationTokenSource = new CancellationTokenSource();
            var messageHandler = new DelegateMessageHandler((msg, msgCtx) =>
            {
                using (new TransactionScope())
                {
                    msgCtx.EnlistInCurrentTransaction();
                }
            });

            await messageHandler.HandleMessage(message, mockMessageContext.Object, cancellationTokenSource.Token);
            mockMessageContext.Verify(msgCtx => msgCtx.Acknowledge(), Times.Never());
        }
    }
}