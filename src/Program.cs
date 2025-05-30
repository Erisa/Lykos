﻿using OpenAI_API;
using OpenAI_API.Chat;
using Microsoft.Extensions.Logging;
using Serilog.Configuration;
using Serilog.Sinks.SystemConsole.Themes;
using Serilog;
using System.Net.Http.Headers;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Lykos
{
    class Program
    {
        public static DiscordClient discord;
        public static Random rnd = new();
        public static ConnectionMultiplexer redis;
        public static IDatabase db;
        public static HasteBinClient hasteUploader;
        public static MinioClient minio;
        internal static EventId EventID { get; } = new EventId(1000, "Bot");
        public static ConfigJson cfgjson;

        public static OpenAIAPI openai;

        public static Dictionary<ulong, Conversation> conversations = new();

        public static Dictionary<string, Command> registeredCommands = new();

        static void Main()
        {
            MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task GalleryHandler(DiscordMessage message, DiscordClient client)
        {
            if (message.Channel.Id == 671182122429710346)
            {
                // Delete the message if there are no attachments, unless the message contains a URL.
                if (message.Attachments.Count == 0 && message.Content != "" && !message.Content.Contains("http"))
                {
                    await message.DeleteAsync();
                    DiscordChannel log = await client.GetChannelAsync(671183700448509962);
                    var embed = new DiscordEmbedBuilder()
                    .WithDescription(message.Content)
                    .WithTimestamp(DateTime.Now)
                    .WithFooter(
                        "Relayed from #gallery",
                        null
                    )
                    .WithAuthor(
                        await DisplayName(message.Author),
                        null,
                        await UserOrMemberAvatarURL(message.Author, message.Channel.Guild, "png", 128)
                    );

                    DiscordMessage prevMsg;

                    if (message.ReferencedMessage != null)
                    {
                        prevMsg = message.ReferencedMessage;
                        embed.WithTitle($"Replying to {await DisplayName(message.ReferencedMessage.Author)}")
                            .WithUrl($"https://discord.com/channels/{message.Channel.Guild.Id}/{message.Channel.Id}/{message.ReferencedMessage.Id}");
                    }
                    else
                    {
                        prevMsg = message.Channel.GetMessagesBeforeAsync(message.Id, 1).ToBlockingEnumerable().First();
                        embed.WithTitle($"Likely replying to {await DisplayName(prevMsg.Author)}")
                            .WithUrl($"https://discord.com/channels/{message.Channel.Guild.Id}/{message.Channel.Id}/{prevMsg.Id}");
                    }

                    if (prevMsg.Attachments.Count > 0)
                    {
                        embed.WithThumbnail(prevMsg.Attachments[0].Url);
                    }
                    else if (prevMsg.Embeds.Count > 0 && prevMsg.Embeds[0].Image != null)
                    {
                        embed.WithThumbnail(prevMsg.Embeds[0].Image.Url.ToString());
                    } else if (prevMsg.Embeds.Count > 0 && prevMsg.Embeds[0].Thumbnail != null)
                    {
                        embed.WithThumbnail(prevMsg.Embeds[0].Thumbnail.Url.ToString());
                    }

                    await log.SendMessageAsync(null, embed);
                }
            }
        }
        static async Task MainAsync()
        {
            Console.OutputEncoding = Encoding.UTF8;

            var logFormat = "[{Timestamp:yyyy-MM-dd HH:mm:ss zzz}] [{Level}] {Message}{NewLine}{Exception}";

            var loggerConfig = new LoggerConfiguration()
                .WriteTo.Console(outputTemplate: logFormat, theme: AnsiConsoleTheme.Literate)
                .Filter.ByExcluding(log => { return log.ToString().Contains("DSharpPlus.Exceptions.NotFoundException: Not found: NotFound"); })
                .MinimumLevel.Debug();

            Log.Logger = loggerConfig.CreateLogger();

            string configFile = "config.json";

            if (!File.Exists(configFile))
                configFile = "config/config.json";

            DotEnv.Load(Path.Combine(Directory.GetCurrentDirectory(), ".env"));

            IConfiguration config = new ConfigurationBuilder()
                .AddJsonFile(configFile)
                .AddEnvironmentVariables("LYKOS_")
                .Build();

            cfgjson = config.Get<ConfigJson>();

            var redisConfigurationOptions = new ConfigurationOptions
            {
                AllowAdmin = false,
                Ssl = cfgjson.Redis.TLS,
                Password = cfgjson.Redis.Password,
                EndPoints = {
                    {  cfgjson.Redis.Host, cfgjson.Redis.Port }
                }
            };

            openai = new OpenAIAPI(cfgjson.OpenAI.Token);

            if (cfgjson.OpenAI.Endpoint != "")
            {
                openai.ApiUrlFormat = $"{cfgjson.OpenAI.Endpoint}/v1/{{1}}";
            }

            redis = ConnectionMultiplexer.Connect(redisConfigurationOptions);

            db = redis.GetDatabase();

            hasteUploader = new HasteBinClient(cfgjson.HastebinEndpoint);

            minio = new MinioClient()
                .WithEndpoint(cfgjson.S3.Endpoint)
                .WithCredentials(cfgjson.S3.AccessKey, cfgjson.S3.SecretKey)
                .WithRegion(cfgjson.S3.Region).WithSSL()
                .WithHttpClient(new HttpClient());

            DiscordClientBuilder discordBuilder = DiscordClientBuilder.CreateDefault(cfgjson.Token, DiscordIntents.All).SetLogLevel(LogLevel.Debug);

            discordBuilder.ConfigureExtraFeatures(clientConfig =>
            {
                clientConfig.LogUnknownEvents = false;
                clientConfig.LogUnknownAuditlogs = false;
            });

            discordBuilder.ConfigureLogging(logging =>
            {
                logging.AddSerilog();
            });

            discordBuilder.ConfigureServices(services =>
            {
#pragma warning disable CS0618 // Type or member is obsolete
                services.AddSlashCommandsExtension(slash =>
                {
                    slash.RegisterCommands<SlashCommands>();
                });
#pragma warning restore CS0618 // Type or member is obsolete
            });

            IServiceProvider serviceProvider = new ServiceCollection().AddLogging(x => x.AddSerilog()).BuildServiceProvider();

            Task OnReady(DiscordClient client, SessionCreatedEventArgs e)
            {
                client.Logger.LogInformation("Logged in as {username}", $"{client.CurrentUser.Username}#{client.CurrentUser.Discriminator}");
                return Task.CompletedTask;
            };

            Type[] commandClasses =
            {
                typeof(Utility),
                typeof(Mod),
                typeof(Owner),
                typeof(Fun)
            };

            discordBuilder.UseCommandsNext(commands =>
            {
                foreach (Type cmdClass in commandClasses)
                {
                    commands.RegisterCommands(cmdClass);
                }
                commands.CommandErrored += CommandsNextService_CommandErrored;

                registeredCommands = (Dictionary<string, Command>)commands.RegisteredCommands;
            }, new CommandsNextConfiguration
            {
                StringPrefixes = cfgjson.Prefixes,
            });


            discordBuilder.ConfigureEventHandlers(
                builder => builder.HandleSessionCreated(OnReady)
                .HandleMessageCreated(MessageCreated)
                .HandleMessageUpdated(MessageUpdated)
                .HandleThreadCreated(Discord_ThreadCreated)
                .HandleThreadDeleted(Discord_ThreadDeleted)
                .HandleThreadUpdated(Discord_ThreadUpdated)
                .HandleThreadListSynced(Discord_ThreadListSynced)
                .HandleThreadMemberUpdated(Discord_ThreadMemberUpdated)
                .HandleThreadMembersUpdated(Discord_ThreadMembersUpdated)
                .HandleGuildMemberAdded(MemberJoined)
            );

            discord = discordBuilder.Build();

            await discord.ConnectAsync();

            while (true)
            {
                await Task.Delay(10000);
                Utility.CheckRemindersAsync();
            }
        }

        public static async Task MessageCreated(DiscordClient client, MessageCreatedEventArgs e)
        {
            // gallery
            _ = GalleryHandler(e.Message, client);

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
                foreach (KeyValuePair<string, Command> cmd in registeredCommands)
                {
                    // Checks command name, display name and all aliases.
                    if (cmd.Key == potentialCmd || potentialCmd == cmd.Value.QualifiedName || cmd.Value.Aliases.Contains(potentialCmd))
                    {
                        await e.Channel.SendMessageAsync("It looks like you misunderstood my prefix.\n" +
                            "The main prefix for me is `lk`. The first letter is a lowercase `l`/`L`, not an uppercase `i`/`I\n`" +
                            "The prefix is inspired by my name, **L**y**k**os.");
                        break;
                    }
                }
            }

            // ai handling

            if (!e.Author.IsBot && !e.Message.Content.Contains("gptreset") && (e.Channel.IsPrivate || (e.Channel.Id == 1236484733362503751 && !e.Message.Content.StartsWith('.'))))
            {
                await e.Channel.TriggerTypingAsync();

                var msg = await ProcessAIMessage(e.Author, e.Channel, e.Message.Content, false);

                if (e.Channel.IsPrivate)
                {
                    await e.Channel.SendMessageAsync(msg);
                }
                else
                {
                    await e.Channel.SendMessageAsync(msg.WithReply(e.Message.Id, true, false));
                }

            }

        }

        public static async Task MemberJoined(DiscordClient client, GuildMemberAddedEventArgs e)
        {
            if (!e.Member.IsBot && e.Guild.Id == 228625269101953035)
            {
                await e.Member.GrantRoleAsync(await e.Guild.GetRoleAsync(956193735044124712));
            }
        }

        // Gallery edit handling
        public static async Task MessageUpdated(DiscordClient client, MessageUpdatedEventArgs e)
        {
            // #gallery
            GalleryHandler(e.Message, client);
        }

        public static async Task CommandsNextService_CommandErrored(CommandsNextExtension cnext, CommandErrorEventArgs e)
        {
            if (e.Exception is CommandNotFoundException && (e.Command == null || e.Command.QualifiedName != "help"))
                return;

            CommandContext ctx = e.Context;
            e.Context.Client.Logger.LogError(EventID, e.Exception, "Exception occurred during {0}'s invocation of '{1}'", e.Context.User.Username, e.Context.Command.QualifiedName);

            var exs = new List<Exception>();
            if (e.Exception is AggregateException ae)
                exs.AddRange(ae.InnerExceptions);
            else
                exs.Add(e.Exception);

            foreach (var ex in exs)
            {
                if (ex is CommandNotFoundException && (e.Command == null || e.Command.QualifiedName != "help"))
                    return;

                if (ex is ChecksFailedException && (e.Command.Name != "help"))
                    return;

                var embed = new DiscordEmbedBuilder
                {
                    Color = new DiscordColor("#FF0000"),
                    Title = "An exception occurred when executing a command",
                    Description = $"`{e.Exception.GetType()}` occurred when executing `{e.Command.QualifiedName}`.",
                    Timestamp = DateTime.UtcNow
                };
                embed.WithFooter(discord.CurrentUser.Username, discord.CurrentUser.AvatarUrl)
                    .AddField("Message", ex.Message);
                if (e.Command != null && e.Command.Name == "avatar" && e.Exception is System.ArgumentException
                    && ex.Message != "The format of `gif` only applies to animated avatars.\nThe user you are trying to lookup does not have an animated avatar.")
                {
                    embed.AddField("Hint", $"This might mean the user is not found.\n" +
                        $"Only mentions, IDs and Usernames are accepted.\n" +
                        $"Note: It is not needed to specify `byid`, simply use the ID directly.");
                }
                await e.Context.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
            }
        }

        public static async Task Discord_ThreadCreated(DiscordClient client, ThreadCreatedEventArgs e)
        {
            client.Logger.LogDebug(eventId: EventID, $"Thread created in {e.Guild.Name}. Thread Name: {e.Thread.Name}");
        }

        public static async Task Discord_ThreadUpdated(DiscordClient client, ThreadUpdatedEventArgs e)
        {
            client.Logger.LogDebug(eventId: EventID, $"Thread updated in {e.Guild.Name}. New Thread Name: {e.ThreadAfter.Name}");
        }

        public static async Task Discord_ThreadDeleted(DiscordClient client, ThreadDeletedEventArgs e)
        {
            client.Logger.LogDebug(eventId: EventID, $"Thread deleted in {e.Guild.Name}. Thread Name: {e.Thread.Name ?? "Unknown"}");
        }

        public static async Task Discord_ThreadListSynced(DiscordClient client, ThreadListSyncedEventArgs e)
        {
            client.Logger.LogDebug(eventId: EventID, $"Threads synced in {e.Guild.Name}.");
        }

        public static async Task Discord_ThreadMemberUpdated(DiscordClient client, ThreadMemberUpdatedEventArgs e)
        {
            client.Logger.LogDebug(eventId: EventID, $"Thread member updated.");
            Console.WriteLine($"Discord_ThreadMemberUpdated fired for thread {e.ThreadMember.ThreadId}. User ID {e.ThreadMember.Id}.");
        }

        public static async Task Discord_ThreadMembersUpdated(DiscordClient client, ThreadMembersUpdatedEventArgs e)
        {
            client.Logger.LogDebug(eventId: EventID, $"Thread members updated in {e.Guild.Name}.");
        }

        public static async Task<DiscordMessageBuilder> ProcessAIMessage(DiscordUser invoker, DiscordChannel channel, string input, bool includeQuote = false)
        {
            string name = (await DisplayName(invoker)).ToLower();

            if (!conversations.ContainsKey(channel.Id))
            {
                conversations[channel.Id] = openai.Chat.CreateConversation(new ChatRequest()
                {
                    Model = cfgjson.OpenAI.Model,
                    MaxTokens = 4096
                });

                conversations[channel.Id].AppendSystemMessage(cfgjson.OpenAI.Prompt);
            }

            conversations[channel.Id].AppendUserInputWithName(name, $"{name}: " + input);

            string response = await conversations[channel.Id].GetResponseFromChatbotAsync();

            if (response is null || response.Length == 0)
            {
                conversations[channel.Id] = openai.Chat.CreateConversation(new ChatRequest()
                {
                    Model = cfgjson.OpenAI.Model,
                    MaxTokens = 4096
                });
                response = "`[a potential history issue was detected so the history was reset! in future this will be handled in a cleaner way]`\n";

                conversations[channel.Id].AppendSystemMessage(cfgjson.OpenAI.Prompt);
                conversations[channel.Id].AppendUserInputWithName((await DisplayName(invoker)).ToLower(), $"{(await DisplayName(invoker)).ToLower()}: " + input);
                response += await conversations[channel.Id].GetResponseFromChatbotAsync();
            }

            DiscordMessageBuilder msg;

            string msgContent = "";

            if (includeQuote)
                msgContent = $"> {input}\n";
            else
                msgContent = response;

            if (response.Length > 4096)
            {
                var stream = new MemoryStream(Encoding.UTF8.GetBytes(response));
                msg = new DiscordMessageBuilder().AddFile("message.txt", stream);
                if (includeQuote)
                    msg.WithContent(msgContent);
            }
            else if (msgContent.Length > 2000)
            {
                msg = new DiscordMessageBuilder().AddEmbed(new DiscordEmbedBuilder().WithDescription(response).Build());
                if (includeQuote)
                    msg.WithContent(msgContent);
            }
            else
            {
                if (includeQuote)
                    msgContent = msgContent + response;
                msg = new DiscordMessageBuilder().WithContent(msgContent);
            }

            return msg;
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
