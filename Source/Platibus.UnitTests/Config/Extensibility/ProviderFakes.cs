using Platibus.Config.Extensibility;

namespace Platibus.UnitTests.Config.Extensibility
{
    internal class ProviderFakes
    {
        [Provider("Multiple")]
        public class MultipleProvider1 : IFakeProvider { }

        [Provider("Multiple")]
        public class MultipleProvider2 : IFakeProvider { }

        [Provider("Single")]
        public class SingleProvider : IFakeProvider { }
    }
}
