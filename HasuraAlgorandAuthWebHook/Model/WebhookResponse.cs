namespace HasuraAlgorandAuthWebHook.Model
{
    public class WebhookResponse
    {
        /// <summary>
        /// User id - algo address
        /// </summary>
        [Newtonsoft.Json.JsonProperty("X-Hasura-User-Id")]
        public string? UserId { get; set; }
        /// <summary>
        /// Role - user
        /// </summary>
        [Newtonsoft.Json.JsonProperty("X-Hasura-User-Id")]
        public string Role { get; set; } = "user";
        /// <summary>
        /// Is owner
        /// </summary>
        [Newtonsoft.Json.JsonProperty("X-Hasura-User-Id")]
        public bool IsOwner { get; set; } = false;
        /// <summary>
        /// Expires
        /// </summary>
        [Newtonsoft.Json.JsonProperty("X-Hasura-User-Id")]
        public DateTimeOffset? Expires { get; set; }
        /// <summary>
        /// Expires
        /// </summary>
        [Newtonsoft.Json.JsonProperty("Cache-Control")]
        public string CacheControl { get; set; } = "max-age=600";


    }
}
