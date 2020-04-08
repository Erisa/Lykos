using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using Lykos.Modules;
using Minio;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Lykos.Config;

namespace Lykos
{
    class Program
    {
        static DiscordClient discord;
        static CommandsNextExtension commands;
        public static Random rnd = new Random();
        public static ConfigJson cfgjson;
        public static HasteBinClient hasteUploader;
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
            hasteUploader = new HasteBinClient(cfgjson.HastebinEndpoint);

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


            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = cfgjson.Prefixes,

            });

            commands.CommandErrored += async e =>
            {
                var ctx = e.Context;
                if (e.Command != null && e.Command.Name == "avatar" && e.Exception is System.ArgumentException)
                {
                    await ctx.RespondAsync($"{Program.cfgjson.Emoji.Xmark} User not found! Only mentions, IDs and Usernames are accepted.\n" +
                        $"Note: It is no longer needed to specify `byid`, simply use the ID directly.");
                }

            };

            commands.RegisterCommands(typeof(Utility));
            commands.RegisterCommands(typeof(Mod));
            commands.RegisterCommands(typeof(Owner));
            commands.RegisterCommands(typeof(Fun));

            discord.MessageCreated += async e =>
            {
                if (e.Channel.Id == 671182122429710346 && e.Message.Attachments.Count == 0)
                {
                    await e.Message.DeleteAsync();
                    var log = await e.Client.GetChannelAsync(671183700448509962);
                    await log.SendMessageAsync($"{e.Author.Mention}:\n>>> {e.Message.Content}");
                }


                if (e.Channel.Id == 695636314959118376)
                {
                    var prevMsgs = await e.Channel.GetMessagesBeforeAsync(e.Message.Id, 1);
                    var prevMsg = prevMsgs[0];
                    var log = await e.Client.GetChannelAsync(695636452804919297);
                    if (e.Message.Content.Contains(" "))
                    {
                        await e.Message.DeleteAsync();
                        await log.SendMessageAsync($"{e.Author.Mention}:\n>>> {e.Message.Content}");
                    }
                    else if (e.Message.Author.Id == prevMsg.Author.Id)
                    {
                        await e.Message.DeleteAsync();
                        await log.SendMessageAsync($"(SAMEAUTHOR) {e.Author.Mention}:\n>>> {e.Message.Content}");
                    }

                }

                if (e.Message.Content.ToLower() == $"what prefix <@{e.Client.CurrentUser.Id}>" || e.Message.Content.ToLower() == $"what prefix <@!{e.Client.CurrentUser.Id}>")
                {
                    await e.Channel.SendMessageAsync($"My prefixes are: ```json\n" +
                        $"{JsonConvert.SerializeObject(cfgjson.Prefixes)}```");
                }

                if (e.Message.Content.ToLower().StartsWith("ik "))
                {
                    var potentialCmd = e.Message.Content.Split(' ')[1];
                    foreach (var cmd in commands.RegisteredCommands)
                    {
                        if (cmd.Key == potentialCmd || potentialCmd == cmd.Value.QualifiedName || cmd.Value.Aliases.Contains(potentialCmd))
                        {
                            await e.Channel.SendMessageAsync("It looks like you misundestood my prefix.\n" +
                                "The main prefix for me is `lk`. The first letter is a lowercase `l`/`L`, not an uppercase `i`/`I\n`" +
                                "The prefix is inspired by my name, **L**ykos.");
                            break;
                        }
                    }
                }

            };

            discord.MessageUpdated += async e =>
            {
                if (e.Channel.Id == 695636314959118376 && e.Message.Content.Contains(" "))
                {
                    await e.Message.DeleteAsync();
                    var log = await e.Client.GetChannelAsync(695636452804919297);
                    await log.SendMessageAsync($"(EDIT) {e.Author.Mention}:\n>>> {e.Message.Content}");
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

}
