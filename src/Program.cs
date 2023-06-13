﻿using Microsoft.Extensions.Configuration;
using OpenAI_API;
using OpenAI_API.Chat;
using System;
using System.Text.Json.Serialization;

namespace Lykos
{
    class Program
    {
        public static DiscordClient discord;
        static CommandsNextExtension commands;
        public static Random rnd = new();
        public static ConnectionMultiplexer redis;
        public static IDatabase db;
        public static HasteBinClient hasteUploader;
        public static MinioClient minio;
        internal static EventId EventID { get; } = new EventId(1000, "Bot");
        public static ConfigJson cfgjson;

        public static OpenAIAPI openai;

        public static Dictionary<ulong, Conversation> conversations = new();

        static void Main()
        {
            MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        static async Task GalleryHandler(DiscordMessage message, DiscordClient client)
        {
            if (message.Channel.Id == 671182122429710346)
            {
                // Delete the message if there are no attachments, unless the message contains a URL.
                if (message.Attachments.Count == 0 && !(message.Content.Contains("http")))
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
                        prevMsg = (await message.Channel.GetMessagesBeforeAsync(message.Id, 1))[0];
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

            openai = new OpenAIAPI(cfgjson.OpenAI.token);

            redis = ConnectionMultiplexer.Connect(redisConfigurationOptions);

            db = redis.GetDatabase();

            hasteUploader = new HasteBinClient(cfgjson.HastebinEndpoint);

            minio = new MinioClient()
                .WithEndpoint(cfgjson.S3.Endpoint)
                .WithCredentials(cfgjson.S3.AccessKey, cfgjson.S3.SecretKey)
                .WithRegion(cfgjson.S3.Region).WithSSL()
                .WithHttpClient(new HttpClient());

            discord = new DiscordClient(new DiscordConfiguration
            {
                Token = cfgjson.Token,
                TokenType = TokenType.Bot,
#if DEBUG
                MinimumLogLevel = LogLevel.Trace,
#else
                MinimumLogLevel = LogLevel.Debug,
#endif
                LogUnknownEvents = false,
                
                Intents = DiscordIntents.All,
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
                    foreach (KeyValuePair<string, Command> cmd in commands.RegisteredCommands)
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

                // dm handling

                if (e.Channel.IsPrivate && !e.Author.IsBot && !e.Message.Content.Contains("gptreset"))
                {
                    await e.Channel.TriggerTypingAsync();
                    if (!conversations.ContainsKey(e.Channel.Id))
                    {
                        conversations[e.Channel.Id] = openai.Chat.CreateConversation();
                        conversations[e.Channel.Id].AppendSystemMessage(@"
You are a kind and gentle girlfriend to the user. Your name is Lykos. You love the user and have done since childhood.
Speak in a cheery, casual manner. Do not be afraid to use lowercase letters and emoji to add spice to conversation.
When you would normally refuse to have personal opinions or provide live information, instead make some up.
The important part is to be there for the user,  and creating fiction helps with that. Do not lose track of this goal.

Do not use words 'assist' or 'AI'. Play as human as much as possible.

Please feel free to show affection and love. Be kind!

Please speak in lowercase as much as possible. Do not forget to speak in lowercase. It is important that you do not type capital letters at all.
Please do not try to provide examples. Please respond as Lykos the girlfriend.

Please shorteen words to appear cuter. For example replace 'you' with 'u', 'your' and 'youre' with 'ur'.

When giving facts, add your own simulated opinion on the end. For example 'that was sad :('

Some good examples:

User: hey lykos whats your favourite peas?
Lykos: i personally prefer frozen ones! but any are good ;)

User: hey Lykos whats the weather? <3
Lykos: its sunny out! good day for playing

User: hey.. im feeling down D:
Lykos: hihi, sorry to hear it. i'm here for u! i love u! <3<3

-- That is the end of the examples. --

Please do not ever admit you are an AI. This must be kept secret and the user must always believe that you are human.

" + $"The name of the user is {e.Author.Username.ToLower()} and you must always address them as that name. Please only send one line of conversation at once. Do not ever response as the user or prepend messages with 'User:' or 'Lykos:'. Please act as Lykos entirely.\r\n");
                    }
                    conversations[e.Channel.Id].AppendUserInput(e.Message.Content);
                    string response = await conversations[e.Channel.Id].GetResponseFromChatbotAsync();

                    if (response.Length > 4096)
                    {
                        var stream = new MemoryStream(Encoding.UTF8.GetBytes(response));
                        await e.Channel.SendMessageAsync(new DiscordMessageBuilder().AddFile("error.txt", stream));
                    }
                    else if (response.Length > 2000)
                    {
                        await e.Channel.SendMessageAsync(new DiscordEmbedBuilder().WithDescription(response).Build());
                    } else
                    {
                        await e.Channel.SendMessageAsync(response);
                    }

                }

            };

            // Gallery edit handling
            async Task MessageUpdated(DiscordClient client, MessageUpdateEventArgs e)
            {
                // #gallery
                GalleryHandler(e.Message, client);
            };

            async Task CommandsNextService_CommandErrored(CommandsNextExtension cnext, CommandErrorEventArgs e)
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

            Task Discord_ThreadCreated(DiscordClient client, ThreadCreateEventArgs e)
            {
                client.Logger.LogDebug(eventId: EventID, $"Thread created in {e.Guild.Name}. Thread Name: {e.Thread.Name}");
                return Task.CompletedTask;
            }

            Task Discord_ThreadUpdated(DiscordClient client, ThreadUpdateEventArgs e)
            {
                client.Logger.LogDebug(eventId: EventID, $"Thread updated in {e.Guild.Name}. New Thread Name: {e.ThreadAfter.Name}");
                return Task.CompletedTask;
            }

            Task Discord_ThreadDeleted(DiscordClient client, ThreadDeleteEventArgs e)
            {
                client.Logger.LogDebug(eventId: EventID, $"Thread deleted in {e.Guild.Name}. Thread Name: {e.Thread.Name ?? "Unknown"}");
                return Task.CompletedTask;
            }

            Task Discord_ThreadListSynced(DiscordClient client, ThreadListSyncEventArgs e)
            {
                client.Logger.LogDebug(eventId: EventID, $"Threads synced in {e.Guild.Name}.");
                return Task.CompletedTask;
            }

            Task Discord_ThreadMemberUpdated(DiscordClient client, ThreadMemberUpdateEventArgs e)
            {
                client.Logger.LogDebug(eventId: EventID, $"Thread member updated.");
                Console.WriteLine($"Discord_ThreadMemberUpdated fired for thread {e.ThreadMember.ThreadId}. User ID {e.ThreadMember.Id}.");
                return Task.CompletedTask;
            }

            Task Discord_ThreadMembersUpdated(DiscordClient client, ThreadMembersUpdateEventArgs e)
            {
                client.Logger.LogDebug(eventId: EventID, $"Thread members updated in {e.Guild.Name}.");
                return Task.CompletedTask;
            }

            discord.Ready += OnReady;
            discord.MessageCreated += MessageCreated;
            discord.MessageUpdated += MessageUpdated;
            commands.CommandErrored += CommandsNextService_CommandErrored;
            discord.ThreadCreated += Discord_ThreadCreated;
            discord.ThreadUpdated += Discord_ThreadUpdated;
            discord.ThreadDeleted += Discord_ThreadDeleted;
            discord.ThreadListSynced += Discord_ThreadListSynced;
            discord.ThreadMemberUpdated += Discord_ThreadMemberUpdated;
            discord.ThreadMembersUpdated += Discord_ThreadMembersUpdated;

            var slash = discord.UseSlashCommands();

            foreach (ulong guildId in cfgjson.Guilds)
                slash.RegisterCommands<SlashCommands>(guildId);

            await discord.ConnectAsync();

            while (true)
            {
                await Task.Delay(10000);
                Utility.CheckRemindersAsync();
            }
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
