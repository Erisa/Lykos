using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Lykos.Modules;
using Newtonsoft.Json;
using System.Linq;
using Google.Cloud.Storage.V1;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using DSharpPlus.Interactivity;
using System.Threading;

namespace Lykos {
    class Program {
        static DiscordClient discord;
        static CommandsNextModule commands;
        public static Random rnd = new Random();
        public static ConfigJson cfgjson;
        public static string googleProjectId = "erisas-stuff";
        public static StorageClient storageClient = StorageClient.Create();
        public static string bucketName = "cdn.erisa.moe";
        public static HasteBinClient hasteUploader = new HasteBinClient("https://paste.erisa.moe");
        public static InteractivityModule interactivity;

        static void Main(string[] args)
        {
            MainAsync(args).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync(string[] args)
        {
            var json = ""; 
            using (var fs = File.OpenRead("config.json"))
            using (var sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);

            if (cfgjson.YoutubeData != null || cfgjson.YoutubeData != "youtubekeyhere")
            {
                Console.WriteLine("[WARN] YouTube API functions have been deprecated, an API key is not needed and may cause problems in future versions.");
            }

            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });

            interactivity = discord.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = new System.TimeSpan(60)
            });

            discord.Ready += e =>
            {
                Console.WriteLine($"Logged in as {e.Client.CurrentUser.Username}#{e.Client.CurrentUser.Discriminator}");
                return null; 
            };

            discord.MessageCreated += async e =>
            {
                if (e.Message.Content.ToLower() == $"what prefix <@{e.Client.CurrentUser.Id}>" || e.Message.Content.ToLower() == $"what prefix <@!{e.Client.CurrentUser.Id}>")
                {
                    await e.Channel.SendMessageAsync($"My prefixes are: ```json\n{JsonConvert.SerializeObject(cfgjson.Prefixes)}```");
                }

                if (e.Channel.Id == 577871838454218766)
                {
                    var mem = await e.Guild.GetMemberAsync(e.Author.Id);
                    var role = e.Guild.GetRole(577872199344717824);
                    await mem.GrantRoleAsync(role);
                }
            };

            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                CustomPrefixPredicate = msg =>
                {
                    foreach (string prefix in cfgjson.Prefixes)
                    {
                        if (msg.Content.StartsWith(prefix)) {
                            return Task.FromResult(prefix.Length);
                        }
                    }
                    return Task.FromResult(-1);
                }
            });

            commands.CommandErrored += async e =>
            {
                var ctx = e.Context;
                if (e.Command != null  && e.Command.Name == "avatar" && e.Exception is System.ArgumentException)
                {
                    await ctx.RespondAsync("<:xmark:314349398824058880> User not found! Only mentions, IDs and Usernames are accepted.\nNote: It is no longer needed to specify `byid`, simply use the ID directly.");
                }

                // Console.WriteLine(e.Exception is System.ArgumentException);
                //if (e.Exception is System.ArgumentException)
               //await ctx.CommandsNext.SudoAsync(ctx.User, ctx.Channel, $"help {ctx.Command.Name}");
            };

            commands.RegisterCommands<Dbots>();
            commands.RegisterCommands<Utility>();
            commands.RegisterCommands<Mod>();
            commands.RegisterCommands<Owner>();
            commands.RegisterCommands<Fun>();

            await discord.ConnectAsync();
            // var msg = discord.GetChannelAsync(132632676225122304).GetMessageAsync(1);
            await Task.Delay(-1);
        }
    }

    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefixes")]
        public string[] Prefixes { get; private set; }

        [JsonProperty("youtube_data_api")]
        public string YoutubeData { get; private set; }
    }
}
