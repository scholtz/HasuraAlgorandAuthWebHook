using Algorand;
using Algorand.V2;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Signers;
using System.Security.Claims;
using System.Text;

namespace HasuraAlgorandAuthWebHook.Controllers
{
    [ApiController]

    [Route("/")]
    public class AuthController : ControllerBase
    {
        private static DateTimeOffset? t;
        private static ulong block;

        private readonly ILogger<AuthController> logger;
        private readonly IConfiguration configuration;
        public AuthController(ILogger<AuthController> logger, IConfiguration configuration)
        {
            this.logger = logger;
            this.configuration = configuration;
        }

        public const string AuthPrefix = "SigTx ";


        [HttpGet("webhook")]
        public async Task<ActionResult<Model.WebhookResponse>> Get()
        {
            try
            {

                if (!Request.Headers.ContainsKey("Authorization")) throw new Model.UnauthorizedException();

                var auth = Request.Headers["Authorization"].ToString();
                if (!auth.StartsWith(auth))
                {
                    throw new Model.UnauthorizedException();
                }
                var tx = Convert.FromBase64String(auth.Replace(AuthPrefix, ""));
                var tr = Algorand.Encoder.DecodeFromMsgPack<SignedTransaction>(tx);
                if (tr.tx == null) throw new Model.UnauthorizedException();
                if (!Verify(tr.tx.sender.Bytes, tr.tx.BytesToSign(), tr.sig.Bytes))
                {
                    throw new Model.UnauthorizedException();
                }
                if (Convert.ToBase64String(tr.tx.genesisHash.Bytes) != configuration["algod.networkGenesisHash"])
                {
                    throw new Model.UnauthorizedException();
                }
                if (!string.IsNullOrEmpty(configuration[".realm"]))
                {
                    var realm = Encoding.ASCII.GetString(tr.tx.note);
                    if (configuration[".realm"] != realm)
                    {
                        // todo: add meaningful message
                        logger.LogTrace($"Wrong realm. Expected {configuration["algod.realm"]} received {realm}");
                        throw new Model.UnauthorizedException();
                    }
                }
                DateTimeOffset? expiration = null;
                if (Boolean.TryParse(configuration["algod.CheckExpiration"], out var checkExp))
                {
                    if (checkExp)
                    {
                        ulong estimatedCurrentBlock;
                        if (t.HasValue && t.Value.AddHours(1) > DateTimeOffset.UtcNow)
                        {
                            estimatedCurrentBlock = Convert.ToUInt64((DateTimeOffset.UtcNow - t.Value).TotalSeconds) / 5 + block;
                        }
                        else
                        {
                            var algodHttpClient = HttpClientConfigurator.ConfigureHttpClient(
                                configuration["algod.server"],
                                configuration["algod.token"],
                                configuration["algod.header"]
                            );

                            var algodClient = new Algorand.V2.Algod.DefaultApi(algodHttpClient);
                            if (algodClient == null) throw new Exception("Algod server is not setup correctly");
                            var c = await algodClient.StatusAsync();
                            if (c != null)
                            {
                                t = DateTimeOffset.UtcNow;
                                block = (ulong)c.LastRound;
                            }

                            estimatedCurrentBlock = block;
                        }

                        if (tr.tx.lastValid.Value < estimatedCurrentBlock)
                        {
                            throw new Model.UnauthorizedException();
                        }
                        ulong msPerBlock = 4500;
                        if (ulong.TryParse(configuration["algod.header"], out var ms))
                        {
                            msPerBlock = ms;
                        }
                        expiration = DateTimeOffset.UtcNow + TimeSpan.FromMilliseconds((tr.tx.lastValid.Value - estimatedCurrentBlock) * msPerBlock);
                    }
                }

                var user = tr.tx.sender.ToString();

                var ret = new Model.WebhookResponse() { UserId = user };

                var exp = User.Claims.FirstOrDefault(c => c.Type == "exp");
                if (expiration.HasValue) ret.Expires = expiration;

                return Ok(ret);
            }
            catch (Model.UnauthorizedException unauth)
            {

                var ret = new Model.WebhookResponse() { UserId = "?", Role = "unauthorized" };
                return Ok(ret);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        private bool Verify(byte[] address, byte[] message, byte[] sig)
        {

            var signer = new Ed25519Signer();
            var pk = new Ed25519PublicKeyParameters(address, 0);
            signer.Init(false, pk);
            signer.BlockUpdate(message.ToArray(), 0, message.ToArray().Length);
            return signer.VerifySignature(sig);
        }
    }
}