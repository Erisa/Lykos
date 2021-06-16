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
                await ctx.Channel.SendMessageAsync($"{ctx.User.Mention}, Here's the emoji link you requested:\nhttps://cdn.discordapp.com/emojis/{groups[2].Value}.gif");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"{ctx.User.Mention}, Here's the emoji link you requested:\nhttps://cdn.discordapp.com/emojis/{groups[2].Value}.png");
            }
        }

        [Command("avatar"), Aliases("avy")]
        [Description("Shows the avatar of a user.")]
        public async Task Avatar(CommandContext ctx, [Description("The user whose avatar will be shown.")] DiscordMember target = null, [Description("The format of the resulting image (jpg, png, gif, webp).")] string format = "png or gif")
        {
            if (target == null)
                target = ctx.Member;

            string hash = target.AvatarHash;


            if (format == null || format == "png or gif")
            {
                format = hash.StartsWith("a_") ? "gif" : "png";
            }
            else if (!validExts.Any(format.Contains))
            {
                await ctx.Channel.SendMessageAsync($"{Program.cfgjson.Emoji.Xmark} You supplied an invalid format, " +
                    $"either give none or one of the following: `gif`, `png`, `jpg`, `webp`");
                return;
            }
            else if (format == "gif" && !hash.StartsWith("a_"))
            {
                await ctx.Channel.SendMessageAsync($"{Program.cfgjson.Emoji.Xmark} The format of `gif` only applies to animated avatars.\n" +
                    $"The user you are trying to lookup does not have an animated avatar.");
                return;
            }

            string avatarUrl = $"https://cdn.discordapp.com/avatars/{target.Id}/{hash}.{format}?size=4096";
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

            await ctx.Channel.SendMessageAsync(null, embed);
        }

        [Command("prefix")]
        [Description("Find out what prefixes you can use to trigger me!")]
        [Aliases("prefixes", "px", "h")]
        public async Task Prefix(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync($"My prefixes are: ```json\n{JsonConvert.SerializeObject(Program.cfgjson.Prefixes)}```");
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
            string resp = "My owners are:\n";
            foreach (ulong id in Program.cfgjson.Owners)
            {
                DiscordUser user = await ctx.Client.GetUserAsync(id);
                resp += $"- **{user.Username}#{user.Discriminator}** (`{user.Id}`)\n";
            }
            await ctx.Channel.SendMessageAsync(resp);
        }

    }
}
