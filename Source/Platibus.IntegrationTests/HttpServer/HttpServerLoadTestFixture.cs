namespace Platibus.IntegrationTests.HttpServer
{
    public class HttpServerLoadTestFixture : HttpServerFixture
    {
        public HttpServerLoadTestFixture() 
            : base("platibus.http-load0", "platibus.http-load1")
        {
        }
    }
}