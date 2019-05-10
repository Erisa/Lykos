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
            DiscordMember member = await ctx.Guild.GetMemberAsync(target.Id);

            if (AllowedToMod(ctx.Member, member))
            {
                if (AllowedToMod(await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id), member))
                {
                    await ctx.RespondAsync($":x: I don't have permission to kick **{target.Username}#{target.Discriminator}**!");
                    return;
                }
                else
                {
                    await member.RemoveAsync($"[Kick by {ctx.User.Username}#{ctx.User.Discriminator}] ${reason}");
                    await ctx.RespondAsync($"🔨 Succesfully ejected **{target.Username}#{target.Discriminator} (`{target.Id}`)**");
                    return;
                }
            }
            else
            {
                await ctx.RespondAsync($":x: You aren't allowed to kick **{target.Username}#{target.Discriminator}**!");
                return;
            }
        }

        public bool AllowedToMod(DiscordMember invoker, DiscordMember target)
        {
            var invoker_hier = invoker.IsOwner ? int.MaxValue : (invoker.Roles.Count() == 0 ? 0 : invoker.Roles.Max(x => x.Position));
            var target_hier = target.IsOwner ? int.MaxValue : (target.Roles.Count() == 0 ? 0 : target.Roles.Max(x => x.Position));

            return invoker_hier > target_hier;
        }


        [Command("mute")]
        [Dbots, RequireDbotsPerm(Helpers.dbotsPermLevel.Helper)]
        [Dbots]
        public async Task Mute(CommandContext ctx, DiscordMember target, [RemainingText] string reason = "No reason provided.")
        {
            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            if (!botMember.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageRoles))
            {
                return;
            }

            if (Helpers.getDbotsPerm(ctx.Member) == Helpers.dbotsPermLevel.Helper && !target.IsBot)
            {
                await ctx.RespondAsync($":x: **{target.Username}#{target.Discriminator}** is not a bot! (Helpers can only mute bots)");
                return;
            }

            var NonTestingMute = ctx.Guild.GetRole(132106771975110656);
            var Muted = ctx.Guild.GetRole(132106637614776320);
            String fullReason = $"[Mute by {ctx.User.Username}#{ctx.User.Discriminator}] {reason}";

            if (AllowedToMod(ctx.Member, target))
            {
                if (AllowedToMod(await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id), target))
                {
                    if (target.Roles.Contains(NonTestingMute))
                    {
                        await ctx.RespondAsync($":x: **{target.Username}#{target.Discriminator}** is already muted!");
                        return;
                    }

                    await target.GrantRoleAsync(ctx.Guild.GetRole(132106771975110656), fullReason);

                    if (target.Roles.Contains(Muted))
                    {
                        await target.RevokeRoleAsync(Muted, fullReason);
                    }

                    await ctx.RespondAsync($"<:check:314349398811475968> Successfully muted **{target.Username}#{target.Discriminator}**!");
                }
                else
                {
                    await ctx.RespondAsync($":x: You aren't allowed to mute **{target.Username}#{target.Discriminator}**!");
                    return;

                }
            }
            else
            {
                await ctx.RespondAsync($":x: I don't have permission to mute **{target.Username}#{target.Discriminator}**!");
                return;
            }


        }

        [Command("supermute")]
        [Aliases("megamute")]
        [Dbots, RequireDbotsPerm(Helpers.dbotsPermLevel.Helper)]
        public async Task SuperMute(CommandContext ctx, DiscordMember target, [RemainingText] string reason = "No reason provided.")
        {
            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            if (!botMember.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageRoles))
            {
                return;
            }

            if (Helpers.getDbotsPerm(ctx.Member) == Helpers.dbotsPermLevel.Helper && !target.IsBot)
            {
                await ctx.RespondAsync($":x: **{target.Username}#{target.Discriminator}** is not a bot! (Helpers can only mute bots)");
                return;
            }

            var NonTestingMute = ctx.Guild.GetRole(132106771975110656);
            var Muted = ctx.Guild.GetRole(132106637614776320);
            String fullReason = $"[Supermute by {ctx.User.Username}#{ctx.User.Discriminator}] {reason}";

            if (AllowedToMod(ctx.Member, target))
            {
                if (AllowedToMod(await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id), target))
                {
                    if (target.Roles.Contains(Muted))
                    {
                        await ctx.RespondAsync($":x: **{target.Username}#{target.Discriminator}** is already muted!");
                        return;
                    }

                    await target.GrantRoleAsync(ctx.Guild.GetRole(132106637614776320), fullReason);

                    if (target.Roles.Contains(NonTestingMute))
                    {
                        await target.RevokeRoleAsync(NonTestingMute, fullReason);
                    }

                    await ctx.RespondAsync($"<:check:314349398811475968> Successfully supermuted **{target.Username}#{target.Discriminator}**!");
                }
                else
                {
                    await ctx.RespondAsync($":x: You aren't allowed to supermute **{target.Username}#{target.Discriminator}**!");
                    return;
                }
            }
            else
            {
                await ctx.RespondAsync($":x: I don't have permission to supermute **{target.Username}#{target.Discriminator}**!");
            }
        }


        [Command("unmute")]
        [Dbots, RequireDbotsPerm(Helpers.dbotsPermLevel.Helper)]
        public async Task Unmute(CommandContext ctx, DiscordMember target, [RemainingText] String reason = "No reason provided.")
        {
            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            if (!botMember.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageRoles))
            {
                return;
            }

            if (Helpers.getDbotsPerm(ctx.Member) == Helpers.dbotsPermLevel.Helper && !target.IsBot)
            {
                await ctx.RespondAsync($":x: **{target.Username}#{target.Discriminator}** is not a bot! (Helpers can only unmute bots)");
                return;
            }

            var NonTestingMute = ctx.Guild.GetRole(132106771975110656);
            var Muted = ctx.Guild.GetRole(132106637614776320);
            String fullReason = $"[Unmute by {ctx.User.Username}#{ctx.User.Discriminator}] {reason}";

            if (AllowedToMod(ctx.Member, target))
            {
                if (AllowedToMod(await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id), target))
                {
                    if (target.Roles.Contains(NonTestingMute) || target.Roles.Contains(Muted))
                    {
                        if (target.Roles.Contains(NonTestingMute))
                        {
                            await target.RevokeRoleAsync(NonTestingMute, fullReason);
                        }
                        if (target.Roles.Contains(Muted))
                        {
                            await target.RevokeRoleAsync(Muted, fullReason);
                        }

                        await ctx.RespondAsync($"<:check:314349398811475968> Successfully unmuted **{target.Username}#{target.Discriminator}**!");

                    }
                    else
                    {
                        await ctx.RespondAsync($":x: **{target.Username}#{target.Discriminator}** is not muted!");
                    }
                }
                else
                {
                    await ctx.RespondAsync($":x: You aren't allowed to unmute **{target.Username}#{target.Discriminator}**!");
                    return;
                }
            }
            else
            {
                await ctx.RespondAsync($":x: I don't have permission to unmute **{target.Username}#{target.Discriminator}**!");
            }



        }

    }
}
