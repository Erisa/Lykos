using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using Lykos.Modules;
using Minio;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Lykos
{
    class Program
    {
        static DiscordClient discord;
        static CommandsNextExtension commands;
        public static Random rnd = new Random();
        public static ConfigJson cfgjson;
        public static HasteBinClient hasteUploader = new HasteBinClient("https://paste.erisa.moe");
        public static InteractivityExtension interactivity;
        public static MinioClient minio;

        static void Main()
        {
            MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var json = "";
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);

            minio = new MinioClient(cfgjson.S3.Endpoint, cfgjson.S3.AccessKey, cfgjson.S3.SecretKey, cfgjson.S3.Region).WithSSL();

            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug,

            });

            interactivity = discord.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = new System.TimeSpan(60)
            });

            discord.Ready += e =>
            {
                Console.WriteLine($"Logged in as {e.Client.CurrentUser.Username}#{e.Client.CurrentUser.Discriminator}");
                return Task.CompletedTask;
            };

            discord.MessageCreated += async e =>
            {
                if (e.Channel.Id == 671182122429710346 && e.Message.Content != "")
                {
                    await e.Message.DeleteAsync();
                    var log = await e.Client.GetChannelAsync(671183700448509962);
                    await log.SendMessageAsync($"{e.Author.Mention}:\n>>> {e.Message.Content}");
                }

                if (e.Message.Content.ToLower() == $"what prefix <@{e.Client.CurrentUser.Id}>" || e.Message.Content.ToLower() == $"what prefix <@!{e.Client.CurrentUser.Id}>")
                {
                    await e.Channel.SendMessageAsync($"My prefixes are: ```json\n{JsonConvert.SerializeObject(cfgjson.Prefixes)}```");
                }


            };

            discord.GuildMemberRemoved += async e =>
            {
                if (e.Guild.Id == 228625269101953035)
                {
                    var channel = await e.Client.GetChannelAsync(228625269101953035);
                    await channel.SendMessageAsync($"**{e.Member.DisplayName}** has left us 😔");
                }
            };

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = cfgjson.Prefixes,

            });

            commands.CommandErrored += async e =>
            {
                var ctx = e.Context;
                if (e.Command != null && e.Command.Name == "avatar" && e.Exception is System.ArgumentException)
                {
                    await ctx.RespondAsync($"{Program.cfgjson.Emoji.Xmark} User not found! Only mentions, IDs and Usernames are accepted.\nNote: It is no longer needed to specify `byid`, simply use the ID directly.");
                }

            };

            commands.RegisterCommands(typeof(Utility));
            commands.RegisterCommands(typeof(Mod));
            commands.RegisterCommands(typeof(Owner));
            commands.RegisterCommands(typeof(Fun));

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }

    public class Require​Owner​Attribute : CheckBaseAttribute
    {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return Task.FromResult(Program.cfgjson.Owners.Contains(ctx.Member.Id));
        }
    }


    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefixes")]
        public string[] Prefixes { get; private set; }

        [JsonProperty("gravatar")]
        public GravatarConfig Gravatar { get; private set; }

        [JsonProperty("s3")]
        public JsonCfgS3 S3 { get; private set; }

        [JsonProperty("owners")]
        public List<ulong> Owners { get; private set; }

        [JsonProperty("cloudflare")]
        public CloudflareConfig Cloudflare { get; private set; }

        [JsonProperty("emoji")]
        public EmojiConfig Emoji { get; private set; }
    }

    public class CloudflareConfig
    {
        [JsonProperty("apiToken")]
        public string Token { get; private set; }

        [JsonProperty("zoneID")]
        public string ZoneID { get; private set; }

        [JsonProperty("urlPrefix")]
        public string UrlPrefix { get; private set; }
    }

    public class EmojiConfig
    {
        [JsonProperty("blobpats")]
        public string BlobPats { get; private set; }

        [JsonProperty("blobhug")]
        public string BlobHug { get; private set; }

        [JsonProperty("xmark")]
        public string Xmark { get; private set; }

        [JsonProperty("check")]
        public string Check { get; private set; }

        [JsonProperty("loading")]
        public string Loading { get; private set; }
    }

    public class JsonCfgS3
    {
        [JsonProperty("endpoint")]
        public string Endpoint { get; private set; }

        [JsonProperty("region")]
        public string Region { get; private set; }

        [JsonProperty("bucket")]
        public string Bucket { get; private set; }

        [JsonProperty("accessKey")]
        public string AccessKey { get; private set; }

        [JsonProperty("secretKey")]
        public string SecretKey { get; private set; }

        [JsonProperty("providerDisplayName")]
        public string displayName { get; private set; }
    }

    public class GravatarConfig
    {
        [JsonProperty("email")]
        public string Email { get; private set; }

        [JsonProperty("password")]
        public string Password { get; private set; }
    }
}
