namespace Lykos
{
    public sealed class Config
    {
        public sealed class ConfigJson
        {
            [ConfigurationKeyName("token")]
            public string Token { get; set; }

            [ConfigurationKeyName("prefixes")]
            public string[] Prefixes { get; set; }

            [ConfigurationKeyName("gravatar")]
            public GravatarConfig Gravatar { get; set; }

            [ConfigurationKeyName("s3")]
            public JsonCfgS3 S3 { get; set; }

            [ConfigurationKeyName("owners")]
            public List<ulong> Owners { get; set; }

            [ConfigurationKeyName("cloudflare")]
            public CloudflareConfig Cloudflare { get; set; }

            [ConfigurationKeyName("emoji")]
            public EmojiConfig Emoji { get; set; }

            [ConfigurationKeyName("hastebinEndpoint")]
            public string HastebinEndpoint { get; set; }

            [ConfigurationKeyName("workerLinks")]
            public WorkerLinksConfig WorkerLinks { get; set; }

            [ConfigurationKeyName("redis")]
            public RedisConfig Redis { get; set; }

            [ConfigurationKeyName("guilds")]
            public List<ulong> Guilds { get; set; }

            [ConfigurationKeyName("openai")]
            public OpenAIConfig OpenAI { get; set; }
        }

        public sealed class OpenAIConfig
        {
            [ConfigurationKeyName("token")]
            public string token { get; set; }

            [ConfigurationKeyName("prompt")]
            public string prompt { get; set; }
        }

        public sealed class RedisConfig
        {
            [ConfigurationKeyName("host")]
            public string Host { get; set; }

            [ConfigurationKeyName("port")]
            public int Port { get; set; }

            [ConfigurationKeyName("password")]
            public string Password { get; set; }

            [ConfigurationKeyName("tls")]
            public bool TLS { get; set; }
        }

        public sealed class CloudflareConfig
        {
            [ConfigurationKeyName("apiToken")]
            public string ApiToken { get; set; }

            [ConfigurationKeyName("zoneID")]
            public string ZoneID { get; set; }

            [ConfigurationKeyName("urlPrefix")]
            public string UrlPrefix { get; set; }
        }

        public sealed class EmojiConfig
        {
            [ConfigurationKeyName("blobpats")]
            public string BlobPats { get; set; }

            [ConfigurationKeyName("blobhug")]
            public string BlobHug { get; set; }

            [ConfigurationKeyName("xmark")]
            public string Xmark { get; set; }

            [ConfigurationKeyName("check")]
            public string Check { get; set; }

            [ConfigurationKeyName("loading")]
            public string Loading { get; set; }

            [ConfigurationKeyName("kiss")]
            public string Kiss { get; set; }
        }

        public sealed class JsonCfgS3
        {
            [ConfigurationKeyName("endpoint")]
            public string Endpoint { get; set; }

            [ConfigurationKeyName("region")]
            public string Region { get; set; }

            [ConfigurationKeyName("bucket")]
            public string Bucket { get; set; }

            [ConfigurationKeyName("accessKey")]
            public string AccessKey { get; set; }

            [ConfigurationKeyName("secretKey")]
            public string SecretKey { get; set; }

            [ConfigurationKeyName("providerDisplayName")]
            public string ProviderDisplayName { get; set; }

            [ConfigurationKeyName("public-read-acl")]
            public bool PublicReadAcl { get; set; }
        }

        public sealed class GravatarConfig
        {
            [ConfigurationKeyName("email")]
            public string Email { get; set; }

            [ConfigurationKeyName("password")]
            public string Password { get; set; }
        }

        public sealed class WorkerLinksConfig
        {
            [ConfigurationKeyName("baseUrl")]
            public string BaseUrl { get; set; }

            [ConfigurationKeyName("secret")]
            public string Secret { get; set; }
        }

    }
}
