using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lykos.Modules
{
    public static class BaseContextExtensions
    {
        public static async Task PrepareResponseAsync(this BaseContext ctx)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
        }

        public static async Task RespondAsync(this BaseContext ctx, string text = null, DiscordEmbed embed = null, bool ephemeral = false, params DiscordComponent[] components)
        {
            DiscordInteractionResponseBuilder response = new();

            if (text != null) response.WithContent(text);
            if (embed != null) response.AddEmbed(embed);
            if (components.Length != 0) response.AddComponents(components);

            response.AsEphemeral(ephemeral);

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, response);
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

        [ContextMenu(ApplicationCommandType.UserContextMenu, "lk hug")]
        public async Task ContextHug(ContextMenuContext ctx)
        {
            await ctx.RespondAsync($"{Program.cfgjson.Emoji.BlobHug} {ctx.TargetUser.Mention} was given a tight hug by {ctx.User.Mention}!");
        }

        [ContextMenu(ApplicationCommandType.UserContextMenu, "lk pat")]
        public async Task ContextPat(ContextMenuContext ctx)
        {
            await ctx.RespondAsync($"{Program.cfgjson.Emoji.BlobPats} {ctx.TargetUser.Mention} was given a big headpat by {ctx.User.Mention}!");
        }
    }
}
