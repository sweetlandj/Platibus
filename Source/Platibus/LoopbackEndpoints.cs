using System;

namespace Platibus
{
    internal class LoopbackEndpoints : EndpointCollection
    {
        private readonly Uri _baseUri;

        public LoopbackEndpoints(EndpointName name, Uri baseUri)
        {
            _baseUri = baseUri ?? throw new ArgumentNullException(nameof(baseUri));
            base.Add(name, new Endpoint(baseUri));
        }

        public override void Add(EndpointName endpointName, IEndpoint endpoint)
        {
        }

        public override IEndpoint this[EndpointName endpointName] => new Endpoint(_baseUri);

        public override bool TryGetEndpointByAddress(Uri address, out IEndpoint endpoint)
        {
            endpoint = new Endpoint(_baseUri);
            return true;
        }

        public override  bool Contains(EndpointName endpointName)
        {
            return true;
        }

    }
}
