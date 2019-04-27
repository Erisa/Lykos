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
    class Mod
    {

        [Command("ban")]
        [Description("Ban a user. If you can. Do it, I dare you.")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task Ban(CommandContext ctx, [Description("The user to ban. Must be below both you and the bot in role hierachy.")] DiscordUser target, [Description("The reason for banning the user.\n")] string reason = "No reason provided.")
        { 

            if (ctx.Guild.GetMemberAsync(target.Id) == null)
            {
                await ctx.Guild.BanMemberAsync(target.Id, 0, $"[Ban by {ctx.User.Username}#{ctx.User.Discriminator}] ${reason}");
                await ctx.RespondAsync($"🔨 Succesfully bent **{target.Username}#{target.Discriminator} (`{target.Id}`)**");
                return;
            }
            else
            {
                DiscordMember member = await ctx.Guild.GetMemberAsync(target.Id);
                if (AllowedToMod(ctx.Member, member))
                {
                    if (AllowedToMod(await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id), member))
                    {
                        await ctx.RespondAsync($":x: I don't have permission to ban **{target.Username}#{target.Discriminator}**!");
                        return;
                    }
                    else
                    {
                        await member.BanAsync(0, $"[Ban by {ctx.User.Username}#{ctx.User.Discriminator}] ${reason}");
                        await ctx.RespondAsync($"🔨 Succesfully bent **{target.Username}#{target.Discriminator} (`{target.Id}`)**");
                        return;
                    }
                }
                else
                {
                    await ctx.RespondAsync($":x: You aren't allowed to ban **{target.Username}#{target.Discriminator}**!");
                    return;
                }
            }
        }

        [Command("unban")]
        [Description("Unban a user. If you can. Do it, I dare you.")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task Unban(CommandContext ctx, DiscordUser target, string reason = "No reason provided.")
        {
            await target.UnbanAsync(ctx.Guild, $"[Unban by {ctx.User.Username}#{ctx.User.Discriminator}] ${reason}");
            await ctx.RespondAsync($"Succesfully unbanned **{target.Username}#{target.Discriminator}** (`{target.Id}`)");
        }

        [Command("kick")]
        [Description("Kick a user. If you can. Do it, I dare you.")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task Kick(CommandContext ctx, DiscordMember target, string reason = "No reason provided.")
        {
            if (AllowedToMod(ctx.Member, target))
            {
                await target.RemoveAsync($"[Kick by {ctx.User.Username}#{ctx.User.Discriminator}] {reason}");
                await ctx.RespondAsync($"🔨 Succesfully ejected **{target.Username}#{target.Discriminator} (`{target.Id}`)**");
            }
            else
            {
                await ctx.RespondAsync($":x: You aren't allowed to kick **{target.Username}#{target.Discriminator}**!");
            }
        }

        public bool AllowedToMod(DiscordMember invoker, DiscordMember target)
        {
            var invoker_hier = invoker.IsOwner ? int.MaxValue : (invoker.Roles.Count() == 0 ? 0 : invoker.Roles.Max(x => x.Position));
            var target_hier = target.IsOwner ? int.MaxValue : (target.Roles.Count() == 0 ? 0 : target.Roles.Max(x => x.Position));

            return invoker_hier > target_hier;
        }

        [Command("mute")]
        [RequireOwner]
        public async Task Mute(CommandContext ctx, DiscordMember target, string reason = "No reason provided.")
        {
            await target.GrantRoleAsync(ctx.Guild.GetRole(132106771975110656), $"[Mute by {ctx.User.Username}#{ctx.User.Discriminator} {reason}");
            await ctx.RespondAsync("successfully muted, i think");
        }
    }
}
