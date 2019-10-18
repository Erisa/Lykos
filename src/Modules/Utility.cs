using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lykos.Modules
{
    class Utility
    {
        readonly string[] validExts = { "gif", "png", "jpg", "webp" };

        [Command("avatar"), Aliases("avy")]
        [Description("Shows the avatar of a user.")]
        public async Task Avatar(CommandContext ctx, [Description("The user whose avatar will be shown.")] DiscordMember target = null, [Description("The format of the resulting image (jpg, png, gif, webp).")] string format = "png or gif")
        {
            if (target == null)
                target = ctx.Member;

            var hash = target.AvatarHash;


            if (format == null ||  format == "png or gif")
            {
                format = hash.StartsWith("a_") ? "gif" : "png";
            }
            else if (!validExts.Any(format.Contains))
            {
                await ctx.RespondAsync("<:xmark:314349398824058880> You supplied an invalid format, either give none or one of the following: `gif`, `png`, `jpg`, `webp`");
                return;
            }
            else if (format == "gif" && !hash.StartsWith("a_"))
            {
                await ctx.RespondAsync("<:xmark:314349398824058880> The format of `gif` only applies to animated avatars.\nThe user you are trying to lookup does not have an animated avatar.");
                return;
            }

            string avatarUrl = $"https://cdn.discordapp.com/avatars/{target.Id}/{hash}.{format}?size=1024";
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
        [Aliases("prefixes", "px", "h")]
        public async Task Prefix(CommandContext ctx)
        {
            await ctx.RespondAsync($"My prefixes are: ```json\n{JsonConvert.SerializeObject(Program.cfgjson.Prefixes)}```");
        }


        [Command("ping")]
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

        [Command("yt")]
        public async Task YouTube(CommandContext ctx)
        {
            await ctx.RespondAsync("This command has been removed because the YouTube API is awful, thank you for understanding!");
        }

    }
}
