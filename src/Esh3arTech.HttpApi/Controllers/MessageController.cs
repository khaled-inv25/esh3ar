using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Esh3arTech.Controllers
{
    // V1
    [ControllerName("Message")]
    [Route("api/esh3artech/message")]
    [Authorize]
    public class MessageController : Esh3arTechController
    {
        [HttpPost]
        [Route("send")]
        public string SendMessage()
        {
            return "Hello from MessageController!";
        }
    }
}
