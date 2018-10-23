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
        static Random rnd = new Random();
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
                if (e.Channel.Id == 504055549965631488)
                {
                    var currentRaw = e.Message.Content;
                    var previousList = await e.Channel.GetMessagesAsync(1, e.Message.Id);
                    var previousRaw = previousList[0].Content;

                    bool currentValid = int.TryParse(currentRaw, out int currentInt);
                    bool previousValid = int.TryParse(previousRaw, out int previousInt);

                    if (e.Message.Author.Id == previousList[0].Author.Id)
                    {
                        await e.Message.DeleteAsync();
                        var msg = await e.Channel.SendMessageAsync($"❌ | {e.Author.Mention}, you wrote the last message, let someone else have a go.");
                        await Task.Delay(2000);
                        await msg.DeleteAsync();
                        Console.WriteLine($"Deleted '{currentRaw}' because of a double post.");
                    }

                    if (e.Message.Content.StartsWith("0")) {
                        await e.Message.DeleteAsync();
                        var msg = await e.Channel.SendMessageAsync($"❌ | {e.Author.Mention}, leading zeroes are gay.");
                        await Task.Delay(2000);
                        await msg.DeleteAsync();
                        Console.WriteLine($"Deleted '{currentRaw}' because of leading zeroes.");
                    }

                    if (!currentValid || !previousValid)
                    {
                        await e.Message.DeleteAsync();
                        var msg = await e.Channel.SendMessageAsync($"❌ | {e.Author.Mention}, that wasn't a number!");
                        await Task.Delay(2000);
                        await msg.DeleteAsync();
                        Console.WriteLine($"Deleted '{currentRaw}' because it was not valid or the previous was not valid.");
                    } else
                    {
                        if (currentInt != (previousInt + 1)) {
                            await e.Message.DeleteAsync();
                            var msg = await e.Channel.SendMessageAsync($"❌ | {e.Author.Mention}, `{currentRaw}` is not one step higher than `{previousRaw}`!");
                            await Task.Delay(2000);
                            await msg.DeleteAsync();
                            Console.WriteLine($"Deleted '{currentRaw}' because it was not 1 higher than `{previousRaw}`");
                        }
                    }
                }
                if (e.Message.Content.ToLower() == "what prefix <@279811031805591555>" || e.Message.Content.ToLower() == "what prefix <@!279811031805591555>")
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
                if (e.Exception is System.ArgumentException)
                    await ctx.CommandsNext.SudoAsync(ctx.User, ctx.Channel, $"help {ctx.Command.Name}");
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
