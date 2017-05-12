using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Moq;
using Xunit;

namespace Platibus.UnitTests
{
    [Trait("Category", "UnitTests")]
    public class MessageContextExtensionsTests
    {
        [Fact]
        public async Task MessageAcknowledgedWhenContextEnlistedInCompletedTransaction()
        {
            const string message = "Hello, world!";
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

        [Fact]
        public async Task MessageNotAcknowledgedWhenContextEnlistedInIncompleteTransaction()
        {
            const string message = "Hello, world!";
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