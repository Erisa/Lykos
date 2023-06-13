namespace Lykos.Modules
{
    class Utility : BaseCommandModule
    {
        [Command("gptreset")]

        public async Task GptReset(CommandContext ctx)
        {
            Program.conversations.Remove(ctx.Channel.Id);
            await ctx.RespondAsync("done bestie!");
        }

        readonly string[] validExts = { "gif", "png", "jpg", "webp" };
        readonly Regex emoji_rx = new("<(a?):\\w*:(\\d*)>");

        [Command("bigmoji")]
        [Aliases("steal", "bm")]
        [Description("Steals the first custom emoji in a given message. Mostly for mobile users!")]
        public async Task Bigmoji(CommandContext ctx, [Description("A message ID containing the custom emoji you want to steal, or the emoji itself.")] string messageId)
        {
            DiscordMessage msg = null;
            try
            {
                msg = await ctx.Channel.GetMessageAsync(Convert.ToUInt64(messageId));
            }
            catch
            {
                // do nothing
            }

            if (msg == null)
            {
                try
                {
                    msg = ctx.Message;
                }
                catch
                {
                    await ctx.Channel.SendMessageAsync("This is where eri is supposed to put the url regex but she's too useless to bother yet.");
                    return;
                }
            }

            MatchCollection matches = emoji_rx.Matches(msg.Content);

            if (matches.Count == 0)
            {
                await ctx.Channel.SendMessageAsync("I couldn't find any custom emoji in that message!");
                return;
            }

            GroupCollection groups = matches[0].Groups;


            if (groups[1].Value == "a")
            {
                await ctx.RespondAsync($"Here's the emoji link you requested:\nhttps://cdn.discordapp.com/emojis/{groups[2].Value}.gif");
            }
            else
            {
                await ctx.RespondAsync($"Here's the emoji link you requested:\nhttps://cdn.discordapp.com/emojis/{groups[2].Value}.png");
            }
        }

        [Command("avatar"), Aliases("avy")]
        [Description("Shows the avatar of a user.")]
        public async Task Avatar(CommandContext ctx, [Description("The user whose avatar will be shown.")] DiscordMember target = null, [Description("The format of the resulting image (jpg, png, gif, webp).")] string format = "png or gif", [Description("Whether to show the guild avatar.")] bool showGuildAvatar = true)
        {
            if (target == null)
                target = ctx.Member;

            string avatarUrl;
            if (showGuildAvatar)
                avatarUrl = await Helpers.UserOrMemberAvatarURL(target, ctx.Guild, format);
            else
                avatarUrl = Helpers.UserAvatarURL(target, format, 4096);

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithColor(new DiscordColor(0xC63B68))
            .WithTimestamp(DateTime.UtcNow)
            .WithFooter(
                $"Called by {ctx.User.Username}#{ctx.User.Discriminator} ({ctx.User.Id})",
                ctx.User.AvatarUrl
            )
            .WithImageUrl(avatarUrl)
            .WithAuthor(
                $"Avatar for {target.Username} (Click to open in browser)",
                avatarUrl
            );

            await ctx.RespondAsync(null, embed);
        }

        [Command("prefix")]
        [Description("Find out what prefixes you can use to trigger me!")]
        [Aliases("prefixes", "px", "h")]
        public async Task Prefix(CommandContext ctx)
        {
            await ctx.RespondAsync($"My prefixes are: ```json\n{JsonConvert.SerializeObject(Program.cfgjson.Prefixes)}```");
        }


        [Command("ping")]
        [Description("Pong? This command lets you know whether I'm working well.")]
        public async Task Ping(CommandContext ctx)
        {
            DSharpPlus.Entities.DiscordMessage return_message = await ctx.RespondAsync("Pinging...");
            ulong ping = (return_message.Id - ctx.Message.Id) >> 22;
            Char[] choices = new Char[] { 'a', 'e', 'o', 'u', 'i', 'y' };
            Char letter = choices[Program.rnd.Next(0, choices.Length)];
            await return_message.ModifyAsync($"P{letter}ng! 🏓\n" +
                $"• It took me `{ping}ms` to reply to your message!\n" +
                $"• Last Websocket Heartbeat took `{ctx.Client.Ping}ms`!");
        }

        [Command("owners")]
        [Description("Who owns me??")]
        public async Task Owners(CommandContext ctx)
        {
            string resp = "My owners are:\n";
            foreach (ulong id in Program.cfgjson.Owners)
            {
                DiscordUser user = await ctx.Client.GetUserAsync(id);
                resp += $"- **{user.Username}#{user.Discriminator}** (`{user.Id}`)\n";
            }
            await ctx.RespondAsync(resp);
        }

        public class Reminder
        {
            [JsonProperty("userID")]
            public ulong UserID { get; set; }

            [JsonProperty("channelID")]
            public ulong ChannelID { get; set; }

            [JsonProperty("messageID")]
            public ulong MessageID { get; set; }

            [JsonProperty("messageLink")]
            public string MessageLink { get; set; }

            [JsonProperty("reminderText")]
            public string ReminderText { get; set; }

            [JsonProperty("reminderTime")]
            public DateTime ReminderTime { get; set; }

            [JsonProperty("originalTime")]
            public DateTime OriginalTime { get; set; }
        }

        [Command("remindme")]
        [Aliases("reminder", "rember", "wemember", "remember", "remind")]
        public async Task RemindMe(CommandContext ctx, string timetoParse, [RemainingText] string reminder)
        {
            DateTime t = HumanDateParser.HumanDateParser.Parse(timetoParse);
            if (t <= DateTime.Now)
            {
                await ctx.RespondAsync($"{Program.cfgjson.Emoji.Xmark} Time can't be in the past!");
                return;
            }
#if !DEBUG
            else if (t < (DateTime.Now + TimeSpan.FromSeconds(59)))
            {
                await ctx.RespondAsync($"{Program.cfgjson.Emoji.Xmark} Time must be at least a minute in the future!");
                return;
            }
#endif
            string guildId;

            if (ctx.Channel.IsPrivate)
                guildId = "@me";
            else
                guildId = ctx.Guild.Id.ToString();

            var reminderObject = new Reminder()
            {
                UserID = ctx.User.Id,
                ChannelID = ctx.Channel.Id,
                MessageID = ctx.Message.Id,
                MessageLink = $"https://discord.com/channels/{guildId}/{ctx.Channel.Id}/{ctx.Message.Id}",
                ReminderText = reminder,
                ReminderTime = t,
                OriginalTime = DateTime.Now
            };

            await Program.db.ListRightPushAsync("reminders", JsonConvert.SerializeObject(reminderObject));
            await ctx.RespondAsync($"{Program.cfgjson.Emoji.Check} I'll try my best to remind you about that on <t:{ToUnixTimestamp(t)}:f> (<t:{ToUnixTimestamp(t)}:R>)"); // (In roughly **{Warnings.TimeToPrettyFormat(t.Subtract(ctx.Message.Timestamp.DateTime), false)}**)");
        }

        [Group("colourme")]
        [Aliases("colorme", "color", "colour")]
        class ColourmeCommands : BaseCommandModule
        {
            [GroupCommand]
            [Aliases("set")]
            [Description("Sets a role colour you specify, or a random one.")]
            public async Task ColourmeSet(CommandContext ctx, string colourString)
            {
                DiscordColor newColour;
                var pickedRole = ctx.Member.Roles.OrderByDescending(xr => xr.Position).FirstOrDefault(xr => xr.Color.Value != 0);
                var configuredRoleIDs = Program.db.ListRange($"colourRoles-{ctx.Guild.Id}", 0, -1);

                if (pickedRole == null)
                {
                    await ctx.RespondAsync(":x: You have no roles with colours! So this command won't work.");
                }else if (configuredRoleIDs.Contains(pickedRole.Id))
                {
                    if (colourString == "random")
                    {
                        newColour = new DiscordColor(Program.rnd.Next(1, 16777215));
                        await pickedRole.ModifyAsync(color: newColour);
                        await ctx.RespondAsync(new DiscordMessageBuilder().WithContent($"Okay I changed the colour of the role {pickedRole.Mention} to a completely random colour.\n" +
                            $"Feel free to try again if it's rubbish.").WithAllowedMentions(Mentions.None));
                    } else
                    {
                        newColour = new DiscordColor(colourString);
                        if (newColour.Value == 0)
                        {
                            await ctx.RespondAsync(":x: You can't have no colour, sorry. It breaks things.");
                        } else
                        {
                            await pickedRole.ModifyAsync(color: newColour);
                            await ctx.RespondAsync(new DiscordMessageBuilder().WithContent($"Okay I changed the colour of the role {pickedRole.Mention} to `{colourString}`").WithAllowedMentions(Mentions.None));
                        }
                    }
                } else
                {
                    await ctx.RespondAsync(new DiscordMessageBuilder().WithContent($":x: Your highest colour role, {pickedRole.Mention}, is not configured for colourme.").WithAllowedMentions(Mentions.None));
                }
            }

            [Command("add")]
            [RequireUserPermissions(DSharpPlus.Permissions.ManageRoles)]
            public async Task ColourmeAdd(CommandContext ctx, DiscordRole role)
            {
                var configuredRoleIDs = Program.db.ListRange($"colourRoles-{ctx.Guild.Id}", 0, -1);
                var foundID = configuredRoleIDs.FirstOrDefault(x => x == role.Id);

                if (foundID != default)
                {
                    await ctx.RespondAsync(":x: Already added that role.");
                } else
                {
                    await Program.db.ListRightPushAsync($"colourRoles-{ctx.Guild.Id}", role.Id);
                    await ctx.RespondAsync(new DiscordMessageBuilder().WithContent($"Okay, I added {role.Mention} to the colourme list.\n" +
                        $"Anyone with that role as their top coloured role can use a command like `colourme #cf9ace` or `colourme random` to change their colour, hopefully.").WithAllowedMentions(Mentions.None));
                }
            }

            [Command("remove")]
            [RequireUserPermissions(DSharpPlus.Permissions.ManageRoles)]
            public async Task ColourmeRemove(CommandContext ctx, DiscordRole role)
            {
                var configuredRoleIDs = Program.db.ListRange($"colourRoles-{ctx.Guild.Id}", 0, -1);
                var foundID = configuredRoleIDs.FirstOrDefault(x => x == role.Id);

                if (foundID == default)
                {
                    await ctx.RespondAsync(":x: That role wasn't added, so I can't remove it.");
                }
                else
                {
                    await Program.db.ListRemoveAsync($"colourRoles-{ctx.Guild.Id}", foundID);
                    await ctx.RespondAsync(new DiscordMessageBuilder().WithContent($"Okay, I removed {role.Mention} from the colourme list.").WithAllowedMentions(Mentions.None));
                }
            }

            [Command("list")]
            [RequireUserPermissions(DSharpPlus.Permissions.ManageRoles)]
            public async Task ColourmeList(CommandContext ctx)
            {
                var configuredRoleIDs = Program.db.ListRange($"colourRoles-{ctx.Guild.Id}", 0, -1);

                string dsc = "";

                foreach(var id in configuredRoleIDs)
                {
                    DiscordRole role;
                    role = ctx.Guild.GetRole((ulong)id);
                    if (role is null)
                        continue;

                    dsc += $" {role.Mention}";
                }

                var embed = new DiscordEmbedBuilder()
                    .WithTitle("List of enabled roles")
                    .WithDescription(dsc);

                await ctx.RespondAsync(embed);
            }
        }

        [Command("choose")]
        public async Task ChooseCommand(CommandContext ctx, params string[] choices)
        {
            var choice = choices[Program.rnd.Next(choices.Count())];

            await ctx.RespondAsync($"I choose **{choice}**!");
        }

        public static long ToUnixTimestamp(DateTime? dateTime)
        {
            return ((DateTimeOffset)dateTime).ToUnixTimeSeconds();
        }

        public static async Task<bool> CheckRemindersAsync()
        {
            bool success = false;
            foreach (var reminder in Program.db.ListRange("reminders", 0, -1))
            {
                bool DmFallback = false;
                var reminderObject = JsonConvert.DeserializeObject<Reminder>(reminder);
                if (reminderObject.ReminderTime <= DateTime.Now)
                {
                    var user = await Program.discord.GetUserAsync(reminderObject.UserID);
                    DiscordChannel channel = null;
                    try
                    {
                        channel = await Program.discord.GetChannelAsync(reminderObject.ChannelID);
                    }
                    catch
                    {
                        // channel likely doesnt exist
                    }
                    if (channel == null)
                    {
                        var member = await channel.Guild.GetMemberAsync(user.Id);
                        channel = await member.CreateDmChannelAsync();
                        DmFallback = true;
                    }

                    await Program.db.ListRemoveAsync("reminders", reminder);
                    success = true;

                    var embed = new DiscordEmbedBuilder()
                    .WithDescription(reminderObject.ReminderText)
                    .WithColor(new DiscordColor(0xD084))
                    .WithFooter(
                        "Reminder was set",
                        null
                    )
                    .WithTimestamp(reminderObject.OriginalTime)
                    .WithAuthor(
                        $"Reminder from {TimeToPrettyFormat(DateTime.Now.Subtract(reminderObject.OriginalTime), true)}",
                        null,
                        user.AvatarUrl
                    )
                    .AddField("Context", $"[`Jump to context`]({reminderObject.MessageLink})", true);

                    var msg = new DiscordMessageBuilder()
                        .WithEmbed(embed)
                        .WithContent($"<@!{reminderObject.UserID}>, you asked to be reminded of something:");

                    if (DmFallback)
                    {
                        msg.WithContent("You asked to be reminded of something:");
                        await channel.SendMessageAsync(msg);
                    }
                    else if (reminderObject.MessageID != default)
                    {
                        try
                        {
                            msg.WithReply(reminderObject.MessageID, mention: true, failOnInvalidReply: true)
                                .WithContent("You asked to be reminded of something:");
                            await channel.SendMessageAsync(msg);
                        }
                        catch (DSharpPlus.Exceptions.BadRequestException)
                        {
                            msg.WithContent($"<@!{reminderObject.UserID}>, you asked to be reminded of something:");
                            msg.WithReply(null, false, false);
                            await channel.SendMessageAsync(msg);
                        }
                    }
                    else
                    {
                        await channel.SendMessageAsync(msg);
                    }
                }

            }
            return success;
        }

        public static string TimeToPrettyFormat(TimeSpan span, bool ago = true)
        {

            if (span == TimeSpan.Zero) return "0 seconds";

            if (span.Days > 3649)
                return "a long time";

            var sb = new StringBuilder();
            if (span.Days > 365)
            {
                int years = (int)(span.Days / 365);
                sb.AppendFormat("{0} year{1}", years, years > 1 ? "s" : String.Empty);
                int remDays = (int)(span.Days - (365 * years));
                int months = remDays / 30;
                if (months > 0)
                    sb.AppendFormat(", {0} month{1}", months, months > 1 ? "s" : String.Empty);
                // sb.AppendFormat(" ago");
            }
            else if (span.Days > 0)
                sb.AppendFormat("{0} day{1}", span.Days, span.Days > 1 ? "s" : String.Empty);
            else if (span.Hours > 0)
                sb.AppendFormat("{0} hour{1}", span.Hours, span.Hours > 1 ? "s" : String.Empty);
            else if (span.Minutes > 0)
                sb.AppendFormat("{0} minute{1}", span.Minutes, span.Minutes > 1 ? "s" : String.Empty);
            else
                sb.AppendFormat("{0} second{1}", span.Seconds, (span.Seconds > 1 || span.Seconds == 0) ? "s" : String.Empty);

            string output = sb.ToString();
            if (ago)
                output += " ago";
            return output;
        }

    }
}
