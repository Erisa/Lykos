namespace Lykos.Modules
{
    public static class BaseContextExtensions
    {
        public static async Task PrepareResponseAsync(this BaseContext ctx)
        {
            await ctx.CreateResponseAsync(DiscordInteractionResponseType.DeferredChannelMessageWithSource);
        }

        public static async Task RespondAsync(this BaseContext ctx, string text = null, DiscordEmbed embed = null, bool ephemeral = false, params DiscordComponent[] components)
        {
            DiscordInteractionResponseBuilder response = new();

            if (text != null) response.WithContent(text);
            if (embed != null) response.AddEmbed(embed);
            if (components.Length != 0) response.AddComponents(components);

            response.AddMentions(Mentions.All);

            response.AsEphemeral(ephemeral);

            await ctx.CreateResponseAsync(DiscordInteractionResponseType.ChannelMessageWithSource, response);
        }

        public static async Task EditAsync(this BaseContext ctx, string text = null, DiscordEmbed embed = null, params DiscordComponent[] components)
        {
            DiscordWebhookBuilder response = new();

            if (text != null) response.WithContent(text);
            if (embed != null) response.AddEmbed(embed);
            if (components.Length != 0) response.AddComponents(components);

            await ctx.EditResponseAsync(response);
        }

        public static async Task FollowAsync(this BaseContext ctx, string text = null, DiscordEmbed embed = null, bool ephemeral = false, params DiscordComponent[] components)
        {
            DiscordFollowupMessageBuilder response = new();

            response.AddMentions(Mentions.All);

            if (text != null) response.WithContent(text);
            if (embed != null) response.AddEmbed(embed);
            if (components.Length != 0) response.AddComponents(components);

            response.AsEphemeral(ephemeral);

            await ctx.FollowUpAsync(response);
        }
    }

    class SlashCommands : ApplicationCommandModule
    {
        readonly string[] validExts = { "gif", "png", "jpg", "webp" };
        [SlashCommand("hug", "Hug someone!")]
        public async Task HugSlashCommand(InteractionContext ctx,
         [Option("user", "The user to hug")] DiscordUser target = default
        )
        {
            if (target == default)
            {
                await ctx.RespondAsync($"{Program.cfgjson.Emoji.BlobHug} \\*gives a tight hug to {ctx.User.Mention}\\*");
            }
            else
            {
                await ctx.RespondAsync($"{Program.cfgjson.Emoji.BlobHug} {target.Mention} was given a tight hug by {ctx.User.Mention}!");
            }
        }

        [SlashCommand("pat", "Pat someone!")]
        public async Task PatSlashCommand(InteractionContext ctx,
         [Option("user", "The user to pat")] DiscordUser target = default
        )
        {
            if (target == default)
            {
                await ctx.RespondAsync($"{Program.cfgjson.Emoji.BlobPats} \\*gives a big headpat to {ctx.User.Mention}\\*");
            }
            else
            {
                await ctx.RespondAsync($"{Program.cfgjson.Emoji.BlobPats} {target.Mention} was given a big headpat by {ctx.User.Mention}!");
            }
        }

        [SlashCommand("chat", "Chat with Lykos"), InteractionCommandInstallType(DiscordApplicationIntegrationType.UserInstall, DiscordApplicationIntegrationType.GuildInstall), InteractionCommandAllowedContexts(DiscordInteractionContextType.PrivateChannel, DiscordInteractionContextType.Guild)]
        public async Task AISlashCommand(InteractionContext ctx,
         [Option("input", "Your message to Lykos")] string input
        )
        {
            await ctx.PrepareResponseAsync();

            var response = await Program.ProcessAIMessage(ctx.User, ctx.Channel, input, true);
            await ctx.EditAsync(response.Content);
        }

        [SlashCommand("avatar", "Show the avatar of a user")]
        public async Task AvatarSlashCommand(InteractionContext ctx,
            [Option("user", "The person you want to see the avatar of")] DiscordUser target,
            [Choice("default", "default")]
            [Choice("jpg", "jpg")]
            [Choice("png", "png")]
            [Choice("gif", "gif")]
            [Choice("webp", "webp")]
            [Option("format", "The format of image you want to see.")] string format = "default",
            [Option("showGuildAvatar", "Whether to show the Guild avatar. Default is true.")] bool showGuildAvatar = true
        )
        {
            string avatarUrl = "";

            try
            {
                if (showGuildAvatar)
                    avatarUrl = await UserOrMemberAvatarURL(target, ctx.Guild, format);
                else
                    avatarUrl = UserAvatarURL(target, format);
            } catch (ArgumentException e)
            {
                await ctx.RespondAsync($"{Program.cfgjson.Emoji.Xmark} {e.Message}", ephemeral: true);
                return;
            }

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

        [ContextMenu(DiscordApplicationCommandType.UserContextMenu, "Show Avatar"), InteractionCommandInstallType(DiscordApplicationIntegrationType.UserInstall, DiscordApplicationIntegrationType.GuildInstall), InteractionCommandAllowedContexts(DiscordInteractionContextType.PrivateChannel, DiscordInteractionContextType.Guild)]
        public async Task ContextAvatar(ContextMenuContext ctx)
        {
            string avatarUrl = "";

            if (ctx.Guild is null)
            {
                avatarUrl = Helpers.UserAvatarURL(ctx.TargetUser);
            } else
            {
                try
                {
                    avatarUrl = await Helpers.UserOrMemberAvatarURL(ctx.TargetUser, ctx.Guild);
                }
                catch (ArgumentException e)
                {
                    await ctx.RespondAsync($"{Program.cfgjson.Emoji.Xmark} {e.Message}", ephemeral: true);
                    return;
                }
            }

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder()
            .WithColor(new DiscordColor(0xC63B68))
            .WithTimestamp(DateTime.UtcNow)
            .WithFooter(
                $"Called by {ctx.User.Username}#{ctx.User.Discriminator} ({ctx.User.Id})",
                ctx.User.AvatarUrl
            )
            .WithImageUrl(avatarUrl)
            .WithAuthor(
                $"Avatar for {ctx.TargetUser.Username} (Click to open in browser)",
                avatarUrl
            );

            await ctx.RespondAsync(null, embed, ephemeral: true);
        }

        [ContextMenu(DiscordApplicationCommandType.UserContextMenu, "lk hug"), InteractionCommandInstallType(DiscordApplicationIntegrationType.UserInstall, DiscordApplicationIntegrationType.GuildInstall), InteractionCommandAllowedContexts(DiscordInteractionContextType.PrivateChannel, DiscordInteractionContextType.Guild)]
        public async Task ContextHug(ContextMenuContext ctx)
        {
            await ctx.RespondAsync($"{Program.cfgjson.Emoji.BlobHug} {ctx.TargetUser.Mention} was given a tight hug by {ctx.User.Mention}!");
        }

        [ContextMenu(DiscordApplicationCommandType.UserContextMenu, "lk pat"), InteractionCommandInstallType(DiscordApplicationIntegrationType.UserInstall, DiscordApplicationIntegrationType.GuildInstall), InteractionCommandAllowedContexts(DiscordInteractionContextType.PrivateChannel, DiscordInteractionContextType.Guild)]
        public async Task ContextPat(ContextMenuContext ctx)
        {
            await ctx.RespondAsync($"{Program.cfgjson.Emoji.BlobPats} {ctx.TargetUser.Mention} was given a big headpat by {ctx.User.Mention}!");
        }

        [ContextMenu(DiscordApplicationCommandType.MessageContextMenu, "Dump message data"), InteractionCommandInstallType(DiscordApplicationIntegrationType.UserInstall, DiscordApplicationIntegrationType.GuildInstall), InteractionCommandAllowedContexts(DiscordInteractionContextType.PrivateChannel, DiscordInteractionContextType.Guild)]
        public async Task DumpMessage(ContextMenuContext ctx)
        {
            var rawMsgData = JsonConvert.SerializeObject(ctx.TargetMessage, Formatting.Indented);
            await ctx.RespondAsync(await CodeOrHasteBinAsync(rawMsgData, "json"), ephemeral: true);
        }


    }
}
