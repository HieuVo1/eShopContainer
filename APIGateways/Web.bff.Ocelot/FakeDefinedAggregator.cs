using Ocelot.Middleware;
using Ocelot.Multiplexer;
using System.Net;

namespace Web.bff.Ocelot
{
    public class FakeDefinedAggregator : IDefinedAggregator
    {
        public Task<DownstreamResponse> Aggregate(List<HttpContext> responses)
        {
            foreach (var response in responses)
            {
                string responseBody = new StreamReader(response.Response.Body).ReadToEnd(); 
            }
            var httpContext = new DefaultHttpContext();
            var result = new DownstreamResponse(new StringContent("test"), HttpStatusCode.OK,
                new List<KeyValuePair<string, IEnumerable<string>>>(), "some reason");
            return Task.Run(() => result);
        }
    }
}
