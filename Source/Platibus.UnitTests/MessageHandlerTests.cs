using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Moq;
using NUnit.Framework;

namespace Platibus.UnitTests
{
    internal class MessageHandlerTests
    {
        [Test]
        public async Task When_Enlisted_Completed_Transaction_Then_Acknowledged()
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
        public async Task When_Enlisted_Incomplete_Transaction_Then_Not_Acknowledged()
        {
            var message = "Hello, world!";
            var mockMessageContext = new Mock<IMessageContext>();
            var cancellationTokenSource = new CancellationTokenSource();
            var messageHandler = new DelegateMessageHandler((msg, msgCtx) =>
            {
                using (var scope = new TransactionScope())
                {
                    msgCtx.EnlistInCurrentTransaction();
                }
            });

            await messageHandler.HandleMessage(message, mockMessageContext.Object, cancellationTokenSource.Token);
            mockMessageContext.Verify(msgCtx => msgCtx.Acknowledge(), Times.Never());
        }
    }
}