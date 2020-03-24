using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Lykos.Modules
{
    class Utility : BaseCommandModule
    {
        readonly string[] validExts = { "gif", "png", "jpg", "webp" };
        readonly Regex emoji_rx = new Regex("<(a?):\\w*:(\\d*)>");

        [Command("bigmoji")]
        [Aliases("steal", "bm")]
        public async Task Bigmoji(CommandContext ctx, ulong messageId)
        {

            DiscordMessage msg;
            try
            {
                msg = await ctx.Channel.GetMessageAsync(messageId);
            }
            catch
            {
                await ctx.RespondAsync("Invalid input!");
                return;
            }

            if (msg == null)
            {
                await ctx.RespondAsync("Invalid input!");
                return;
            }

            MatchCollection matches = emoji_rx.Matches(msg.Content);

            if (matches.Count == 0)
            {
                await ctx.RespondAsync("I couldn't find any custom emoji in that message!");
                return;
            }

            GroupCollection groups = matches[0].Groups;


            if (groups[1].Value == "a")
            {
                await ctx.RespondAsync($"I think this should work:\nhttps://cdn.discordapp.com/emojis/{groups[2].Value}.gif");
            }
            else
            {
                await ctx.RespondAsync($"I think this should work:\nhttps://cdn.discordapp.com/emojis/{groups[2].Value}.png");
            }
        }

        [Command("avatar"), Aliases("avy")]
        [Description("Shows the avatar of a user.")]
        public async Task Avatar(CommandContext ctx, [Description("The user whose avatar will be shown.")] DiscordMember target = null, [Description("The format of the resulting image (jpg, png, gif, webp).")] string format = "png or gif")
        {
            if (target == null)
                target = ctx.Member;

            var hash = target.AvatarHash;


            if (format == null || format == "png or gif")
            {
                format = hash.StartsWith("a_") ? "gif" : "png";
            }
            else if (!validExts.Any(format.Contains))
            {
                await ctx.RespondAsync($"{Program.cfgjson.Emoji.Xmark} You supplied an invalid format, either give none or one of the following: `gif`, `png`, `jpg`, `webp`");
                return;
            }
            else if (format == "gif" && !hash.StartsWith("a_"))
            {
                await ctx.RespondAsync($"{Program.cfgjson.Emoji.Xmark} The format of `gif` only applies to animated avatars.\nThe user you are trying to lookup does not have an animated avatar.");
                return;
            }

            string avatarUrl = $"https://cdn.discordapp.com/avatars/{target.Id}/{hash}.{format}?size=4096";
            var embed = new DiscordEmbedBuilder()
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

            await ctx.RespondAsync(null, false, embed);
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
            DSharpPlus.Entities.DiscordMessage return_message = await ctx.Message.RespondAsync("Pinging...");
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
            var resp = "My owners are:\n";
            foreach (var id in Program.cfgjson.Owners)
            {
                var user = await ctx.Client.GetUserAsync(id);
                resp += $"- **{user.Username}#{user.Discriminator}** (`{user.Id}`)\n";
            }
            await ctx.RespondAsync(resp);
        }

    }
}
