using System.Web.Http;
using Infrastructure.Application;

namespace CaldaiaBackend.SelfHosted.Controllers
{
    public class ClientInfoController : System.Web.Http.ApiController
    {
        [HttpGet]
        [Route("api/client-info")]
        public IHttpActionResult LatestData()
        {
            var clientInfo = this.GetInfrastructureExecutionContext().GetClientId();
            return Ok(clientInfo);
        }

    }
}
