using Platibus.SQL.Commands;
#if NET452
using System.Configuration;
#endif
#if NETCOREAPP2_0
using Platibus.Config;
#endif
using Platibus.Config.Extensibility;
using Platibus.SQLite.Commands;
using Xunit;

namespace Platibus.UnitTests.Config.Extensibility
{
    public class CommandBuildersFactoryTests
    {
        protected ConnectionStringSettings ConnectionStringSettings = new ConnectionStringSettings();
        protected IMessageJournalingCommandBuilders MessageJournalingCommandBuilders;
        protected IMessageQueueingCommandBuilders MessageQueueingCommandBuilders;
        protected ISubscriptionTrackingCommandBuilders SubscriptionTrackingCommandBuilders;

        [Fact]
        public void MSSQLCommandBuildersInitializedForSystemDataSQLClientProvider()
        {
            GivenSystemDataSQLClientProvider();
            WhenInitializingBuilders();
            AssertMSSQLCommandBuilders();
        }

        [Fact]
        public void SQLiteCommandBuildersInitializedForSystemDataSQLiteProvider()
        {
            GivenSystemDataSQLiteProvider();
            WhenInitializingBuilders();
            AssertSQLiteCommandBuilders();
        }

        protected void GivenUnknownProvider()
        {
            ConnectionStringSettings.ProviderName = "System.Data.Unknown";
        }

        protected void GivenSystemDataSQLClientProvider()
        {
            ConnectionStringSettings.ProviderName = "System.Data.SQLClient";
        }

        protected void GivenSystemDataSQLiteProvider()
        {
            ConnectionStringSettings.ProviderName = "System.Data.SQLite";
        }

        protected void WhenInitializingBuilders()
        {
            var factory = new CommandBuildersFactory(ConnectionStringSettings);
            MessageJournalingCommandBuilders = factory.InitMessageJournalingCommandBuilders();
            MessageQueueingCommandBuilders = factory.InitMessageQueueingCommandBuilders();
            SubscriptionTrackingCommandBuilders = factory.InitSubscriptionTrackingCommandBuilders();
        }

        protected void AssertMSSQLCommandBuilders()
        {
            Assert.IsType<MSSQLMessageJournalingCommandBuilders>(MessageJournalingCommandBuilders);
            Assert.IsType<MSSQLMessageQueueingCommandBuilders>(MessageQueueingCommandBuilders);
            Assert.IsType<MSSQLSubscriptionTrackingCommandBuilders>(SubscriptionTrackingCommandBuilders);
        }

        protected void AssertSQLiteCommandBuilders()
        {
            Assert.IsType<SQLiteMessageJournalCommandBuilders>(MessageJournalingCommandBuilders);
            Assert.IsType<SQLiteMessageQueueingCommandBuilders>(MessageQueueingCommandBuilders);
            Assert.IsType<SQLiteSubscriptionTrackingCommandBuilders>(SubscriptionTrackingCommandBuilders);
        }
    }
}
