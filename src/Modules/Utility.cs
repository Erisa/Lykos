using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;

namespace Lykos.Modules
{
    class Utility
    {

        string[] validExts = { "gif", "png", "jpg", "webp" };
        [Command("avatar")]
        [Description("Shows the avatar of a user.")]
        public async Task Avatar(CommandContext ctx, [Description("The user whose avatar will be shown.")] DiscordMember target = null, [Description("The format of the resulting image (jpg, png, gif, webp). Defaults to png or gif.")] string format = null)
        {
            if (target == null)
                target = ctx.Member;

            var hash = target.AvatarHash;


            if (format == null)
            {
                format = hash.StartsWith("a_") ? "gif" : "png";
            } else if (!validExts.Any(format.Contains))
            {
                await ctx.RespondAsync("<:xmark:314349398824058880> You supplied an invalid format, either give none or one of the following: `gif`, `png`, `jpg`, `webp`");
                return;
            } else if (format == "gif" && !hash.StartsWith("a_"))
            {
                await ctx.RespondAsync("<:xmark:314349398824058880> The format of `gif` only applies to animated avatars.\nThe user you are trying to lookup does not have an animated avatar.");
                return;
            }

            string avatarUrl = $"https://cdn.discordapp.com/avatars/{target.Id}/{hash}.{format}?size=1024"; 
            var embed = new DiscordEmbedBuilder()
            .WithColor(new DiscordColor(0xC63B68))
            .WithTimestamp(DateTime.UtcNow)
            .WithFooter(
                $"Called by {ctx.User.Username}#{ctx.User.Discriminator}",
                ctx.User.AvatarUrl
            )
            .WithImageUrl(avatarUrl)
            .WithAuthor(
                $"Avatar for {target.Username}"
            );

            await ctx.RespondAsync(null, false, embed);
        }

        [Command("test")]
        [RequireOwner]
        public async Task Test(CommandContext ctx)
        {
            // await ctx.RespondAsync("ur mom");
            var list = await ctx.Channel.GetMessagesAsync(1, ctx.Message.Id);
            await ctx.RespondAsync(list[0].Content);
            // await ctx.RespondAsync("gay");
        }

        [Command("prefix")]
        [Aliases("prefixes", "px")]
        public async Task Prefix(CommandContext ctx)
        {
            await ctx.RespondAsync($"My prefixes are: ```json\n{JsonConvert.SerializeObject(Program.cfgjson.Prefixes)}```");
        }
    }
}
