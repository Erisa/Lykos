using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
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
        public static MinioClient minio;

        static void Main()
        {
            MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            string configFile = "config.json";
            string json = "";

            if (!File.Exists(configFile))
                configFile = "config/config.json";

            if (File.Exists(configFile))
            {
                using (FileStream fs = File.OpenRead(configFile))
                using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                    json = await sr.ReadToEndAsync();
            } else
            {
                json = System.Environment.GetEnvironmentVariable("LYKOS_CONFIG");
            }

            cfgjson = JsonConvert.DeserializeObject<ConfigJson>(json);
            hasteUploader = new HasteBinClient(cfgjson.HastebinEndpoint);

            minio = new MinioClient
            (
                cfgjson.S3.Endpoint,
                cfgjson.S3.AccessKey,
                cfgjson.S3.SecretKey,
                cfgjson.S3.Region
            ).WithSSL();

            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
            });

            Task OnReady(DiscordClient client, ReadyEventArgs e)
            {
                Console.WriteLine($"Logged in as {client.CurrentUser.Username}#{client.CurrentUser.Discriminator}");
                return Task.CompletedTask;
            };


            commands = discord.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = cfgjson.Prefixes,

            });

            async Task CommandErrored(CommandsNextExtension cnext, CommandErrorEventArgs e)
            {
                CommandContext ctx = e.Context;
                // This is a fairly ugly workaround but, it does appear to be stable for this command at least.
                if (e.Command != null && e.Command.Name == "avatar" && e.Exception is System.ArgumentException)
                {
                    await ctx.Channel.SendMessageAsync($"{Program.cfgjson.Emoji.Xmark} User not found! " +
                        $"Only mentions, IDs and Usernames are accepted.\n" +
                        $"Note: It is not needed to specify `byid`, simply use the ID directly.");
                }

            };

            Type[] commandClasses =
            {
                typeof(Utility),
                typeof(Mod),
                typeof(Owner),
                typeof(Fun)
            };

            foreach (Type cmdClass in commandClasses)
            {
                commands.RegisterCommands(cmdClass);
            }

            async Task MessageCreated(DiscordClient client, MessageCreateEventArgs e)
            {
                // gallery
                if (e.Channel.Id == 671182122429710346)
                {
                    // Delete the message if there are no attachments, unless the message contains a URL.
                    if (e.Message.Attachments.Count == 0 && !(e.Message.Content.Contains("http")))
                    {
                        await e.Message.DeleteAsync();
                        DiscordChannel log = await client.GetChannelAsync(671183700448509962);
                        var embed = new DiscordEmbedBuilder()
                        .WithDescription(e.Message.Content)
                        .WithTimestamp(DateTime.Now)
                        .WithFooter(
                            "Relayed from #gallery",
                            null
                        )
                        .WithAuthor(
                            e.Author.Username,
                            null,
                            $"https://cdn.discordapp.com/avatars/{e.Author.Id}/{e.Author.AvatarHash}.png?size=128"
                        )
                        ;

                        await log.SendMessageAsync(null, embed);
                    }
                }

                // story 2
                if (e.Channel.Id == 695636314959118376)
                {
                    System.Collections.Generic.IReadOnlyList<DSharpPlus.Entities.DiscordMessage> prevMsgs = await e.Channel.GetMessagesBeforeAsync(e.Message.Id, 1);
                    DSharpPlus.Entities.DiscordMessage prevMsg = prevMsgs[0];
                    DSharpPlus.Entities.DiscordChannel log = await client.GetChannelAsync(695636452804919297);
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

                // Prefix query handling
                if
                (
                  e.Message.Content.ToLower() == $"what prefix <@{client.CurrentUser.Id}>" ||
                  e.Message.Content.ToLower() == $"what prefix <@!{client.CurrentUser.Id}>"
                )
                {
                    await e.Channel.SendMessageAsync($"My prefixes are: ```json\n" +
                        $"{JsonConvert.SerializeObject(cfgjson.Prefixes)}```");
                }

                // Yell at people who get the prefix wrong, but only if the argument is an actual command.
                if (e.Message.Content.ToLower().StartsWith("ik "))
                {
                    string potentialCmd = e.Message.Content.Split(' ')[1];
                    foreach (System.Collections.Generic.KeyValuePair<string, Command> cmd in commands.RegisteredCommands)
                    {
                        // Checks command name, display name and all aliases.
                        if (cmd.Key == potentialCmd || potentialCmd == cmd.Value.QualifiedName || cmd.Value.Aliases.Contains(potentialCmd))
                        {
                            await e.Channel.SendMessageAsync("It looks like you misundestood my prefix.\n" +
                                "The main prefix for me is `lk`. The first letter is a lowercase `l`/`L`, not an uppercase `i`/`I\n`" +
                                "The prefix is inspired by my name, **L**y**k**os.");
                            break;
                        }
                    }
                }

            };

            // Gallery edit handling
            async Task MessageUpdated(DiscordClient client, MessageUpdateEventArgs e)
            {
                // #gallery
                if (e.Channel.Id == 671182122429710346)
                {
                    // Delete the message if there are no attachments, unless the message contains a URL.
                    if (e.Message.Attachments.Count == 0 && !(e.Message.Content.Contains("http")))
                    {
                        await e.Message.DeleteAsync();
                        DSharpPlus.Entities.DiscordChannel log = await client.GetChannelAsync(671183700448509962);
                        await log.SendMessageAsync($"[EDIT] {e.Author.Mention}:\n>>> {e.Message.Content}");
                    }
                }
            };

            // Leave event handling, for my servers
            async Task GuildMemberRemoved(DiscordClient client, GuildMemberRemoveEventArgs e)
            {
                DSharpPlus.Entities.DiscordChannel channel = null;
                // Erisa's Corner
                if (e.Guild.Id == 228625269101953035)
                {
                    // #general-chat
                    channel = await client.GetChannelAsync(751534914469363832);
                }
                // Project Evenfall
                else if (e.Guild.Id == 535688189659316245)
                {
                    // #greetings
                    channel = await client.GetChannelAsync(542497115583283220);
                }

                if (channel != null)
                {
                    await channel.SendMessageAsync($"**{e.Member.Username}** has left us 😔");
                }
            };

            discord.Ready += OnReady;
            discord.MessageCreated += MessageCreated;
            discord.MessageUpdated += MessageUpdated;
            discord.GuildMemberRemoved += GuildMemberRemoved;
            commands.CommandErrored += CommandErrored;

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
