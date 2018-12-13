using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Lykos.Modules;
using Newtonsoft.Json;
using System.Linq;

namespace Lykos {
    class Program {
        static DiscordClient discord;
        static CommandsNextModule commands;
        public static Random rnd = new Random();
        public static ConfigJson cfgjson;

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
            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,
                UseInternalLogHandler = true,
                LogLevel = LogLevel.Debug
            });

            discord.Ready += async e =>
            {
                Console.WriteLine($"Logged in as {e.Client.CurrentUser.Username}#{e.Client.CurrentUser.Discriminator}");
            };

            discord.MessageCreated += async e =>
            {
                if (e.Message.Content.ToLower() == $"what prefix <@{e.Client.CurrentUser.Id}>" || e.Message.Content.ToLower() == $"what prefix <@!{e.Client.CurrentUser.Id}>")
                {
                    await e.Channel.SendMessageAsync($"My prefixes are: ```json\n{JsonConvert.SerializeObject(cfgjson.Prefixes)}```");
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
                if (e.Command.Name == "avatar" && e.Exception is System.ArgumentException)
                {
                    await ctx.RespondAsync("<:xmark:314349398824058880> User not found! Only mentions, IDs and Usernames are accepted.\nNote: It is no longer needed to specify `byid`, simply use the ID directly.");
                }

                // Console.WriteLine(e.Exception is System.ArgumentException);
                //if (e.Exception is System.ArgumentException)
               //     await ctx.CommandsNext.SudoAsync(ctx.User, ctx.Channel, $"help {ctx.Command.Name}");
            };

            commands.RegisterCommands<Dbots>();
            commands.RegisterCommands<Utility>();

            await discord.ConnectAsync();
            await Task.Delay(-1);
        }
    }

    public struct ConfigJson
    {
        [JsonProperty("token")]
        public string Token { get; private set; }

        [JsonProperty("prefixes")]
        public string[] Prefixes { get; private set; }
    }
}
