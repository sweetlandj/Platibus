using Moq;
using NUnit.Framework;

namespace Platibus.UnitTests.Mocks
{
    [SetUpFixture]
    public class MockCollectionFixture
    {
        public static MockCollectionFixture Instance;

        [SetUp]
        public void SetUp()
        {
            Instance = new MockCollectionFixture();
        }

        public readonly Mock<IMessageJournalingService> MockMessageJournalingService;

        public MockCollectionFixture()
        {
            MockMessageJournalingService = new Mock<IMessageJournalingService>();
        }
    }
}
