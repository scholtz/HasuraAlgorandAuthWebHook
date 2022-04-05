using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HasuraAlgorandAuthWebHook.Controllers
{
    [ApiController]

    [Route("/")]
    public class AuthController : ControllerBase
    {
        private ILogger<AuthController> logger;
        public AuthController(ILogger<AuthController> logger)
        {
            this.logger = logger;
        }

        [Authorize]
        [HttpGet("webhook")]
        public ActionResult<Model.WebhookResponse> Get()
        {
            if (string.IsNullOrEmpty(User?.Identity?.Name))
            {
                var ret = new Model.WebhookResponse() { UserId = "", Role = "public" };
                return Ok(ret);
            }
            else
            {
                var ret = new Model.WebhookResponse() { UserId = User.Identity.Name };
                var exp = User.Claims.FirstOrDefault(c => c.Type == "exp");
                if (!string.IsNullOrEmpty(exp?.Value)) if (long.TryParse(exp.Value, out var time)) ret.Expires = DateTimeOffset.FromUnixTimeSeconds(time);
                return Ok(ret);
            }
        }
    }
}