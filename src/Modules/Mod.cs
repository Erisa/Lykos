using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Net.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Lykos.Modules
{
    class Mod : BaseCommandModule
    {

        [Command("ban")]
        [Description("Ban a user. If you can. Do it, I dare you.")]
        [Dbots, RequireDbotsPerm(Helpers.DbotsPermLevel.mod)]
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
                        await member.BanAsync(0, $"[Ban by {ctx.User.Username}#{ctx.User.Discriminator}] {reason}");
                        await ctx.RespondAsync($"🔨 Succesfully bent **{target.Username}#{target.Discriminator} (`{target.Id}`)**");
                        return;
                    }
                    else
                    {
                        await ctx.RespondAsync($"<:xmark:314349398824058880> I don't have permission to ban **{target.Username}#{target.Discriminator}**!");
                        return;

                    }
                }
                else
                {
                    await ctx.RespondAsync($"<:xmark:314349398824058880> You aren't allowed to ban **{target.Username}#{target.Discriminator}**!");
                    return;
                }
            }
        }

        [Command("unban")]
        [Description("Unban a user. If you can. Do it, I dare you.")]
        [Dbots, RequireDbotsPerm(Helpers.DbotsPermLevel.mod)]
        public async Task Unban(CommandContext ctx, DiscordUser target, string reason = "No reason provided.")
        {
            await target.UnbanAsync(ctx.Guild, $"[Unban by {ctx.User.Username}#{ctx.User.Discriminator}] ${reason}");
            await ctx.RespondAsync($"Succesfully unbanned **{target.Username}#{target.Discriminator}** (`{target.Id}`)");
        }

        [Command("kick")]
        [Description("Kick a user. If you can. Do it, I dare you.")]
        [Dbots, RequireDbotsPerm(Helpers.DbotsPermLevel.mod)]
        public async Task Kick(CommandContext ctx, DiscordMember target, string reason = "No reason provided.")
        {
            DiscordMember member = await ctx.Guild.GetMemberAsync(target.Id);

            if (AllowedToMod(ctx.Member, member))
            {
                if (AllowedToMod(await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id), member))
                {
                    await member.RemoveAsync($"[Kick by {ctx.User.Username}#{ctx.User.Discriminator}] {reason}");
                    await ctx.RespondAsync($"\U0001f462 Succesfully ejected **{target.Username}#{target.Discriminator} (`{target.Id}`)**");
                    return;
                }
                else
                {
                    await ctx.RespondAsync($"<:xmark:314349398824058880> I don't have permission to kick **{target.Username}#{target.Discriminator}**!");
                    return;
                }
            }
            else
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> You aren't allowed to kick **{target.Username}#{target.Discriminator}**!");
                return;
            }
        }

        // If invoker is allowed to mod target.
        public static bool AllowedToMod(DiscordMember invoker, DiscordMember target)
        {
            return GetHier(invoker) > GetHier(target);
        }

        public static int GetHier(DiscordMember target)
        {
            return target.IsOwner ? int.MaxValue : (target.Roles.Count() == 0 ? 0 : target.Roles.Max(x => x.Position));
        }


        [Command("mute")]
        [Dbots, RequireDbotsPerm(Helpers.DbotsPermLevel.Helper)]
        public async Task Mute(CommandContext ctx, DiscordMember target, [RemainingText] string reason = "No reason provided.")
        {
            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            if (!botMember.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageRoles))
            {
                return;
            }

            if (Helpers.GetDbotsPerm(ctx.Member) < Helpers.DbotsPermLevel.mod && !target.IsBot)
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** is not a bot! (Helpers can only mute bots)");
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
                        await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** is already muted!");
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
                    await ctx.RespondAsync($"<:xmark:314349398824058880>  I don't have permission to mute **{target.Username}#{target.Discriminator}**!");
                    return;

                }
            }
            else
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> You aren't allowed to mute **{target.Username}#{target.Discriminator}**!");
                return;
            }

        }

        [Command("nickreset")]
        [Dbots, RequireDbotsPerm(Helpers.DbotsPermLevel.mod)]
        public async Task NickReset(CommandContext ctx, DiscordMember target, [RemainingText] string reason = "No reason provided.")
        {
            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            if (!botMember.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageNicknames))
            {
                return;
            }

            if (ctx.Member == target)
            {
                await ctx.RespondAsync("<:xmark:314349398824058880> Do it yourself.");
                return;
            }

            String fullReason = $"[Nickreset by {ctx.User.Username}#{ctx.User.Discriminator}] {reason}";

            if (AllowedToMod(ctx.Member, target))
            {
                if (AllowedToMod(await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id), target))
                {
                    if (target.Nickname == null)
                    {
                        await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** doesn't have a nickname!");
                        return;
                    }

                    await target.ModifyAsync(new Action<MemberEditModel>((MemberEditModel) => MemberEditModel.Nickname = null));

                    await ctx.RespondAsync($"<:check:314349398811475968> Reset the nickname of **{target.Username}#{target.Discriminator}**!");
                }
                else
                {
                    await ctx.RespondAsync($"<:xmark:314349398824058880> I don't have permission to nickreset **{target.Username}#{target.Discriminator}**!");
                    return;

                }
            }
            else
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> You aren't allowed to nickreset **{target.Username}#{target.Discriminator}**!");
                return;
            }


        }

        [Command("supermute")]
        [Aliases("megamute")]
        [Dbots, RequireDbotsPerm(Helpers.DbotsPermLevel.Helper)]
        public async Task SuperMute(CommandContext ctx, DiscordMember target, [RemainingText] string reason = "No reason provided.")
        {
            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            if (!botMember.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageRoles))
            {
                return;
            }

            if (Helpers.GetDbotsPerm(ctx.Member) < Helpers.DbotsPermLevel.mod && !target.IsBot)
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** is not a bot! (Helpers can only mute bots)");
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
                        await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** is already muted!");
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
                    await ctx.RespondAsync($"<:xmark:314349398824058880> I don't have permission to supermute **{target.Username}#{target.Discriminator}**!");
                    return;
                }
            }
            else
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> You aren't allowed to supermute **{target.Username}#{target.Discriminator}**!");
            }
        }


        [Command("unmute")]
        [Dbots, RequireDbotsPerm(Helpers.DbotsPermLevel.Helper)]
        public async Task Unmute(CommandContext ctx, DiscordMember target, [RemainingText] String reason = "No reason provided.")
        {
            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            if (!botMember.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageRoles))
            {
                return;
            }

            if (Helpers.GetDbotsPerm(ctx.Member) < Helpers.DbotsPermLevel.mod && !target.IsBot)
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** is not a bot! (Helpers can only unmute bots)");
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
                        await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** is not muted!");
                    }
                }
                else
                {
                    await ctx.RespondAsync($"<:xmark:314349398824058880> I don't have permission to unmute **{target.Username}#{target.Discriminator}**!");
                    return;
                }
            }
            else
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> You aren't allowed to unmute **{target.Username}#{target.Discriminator}**!");
            }
        }


        [Command("noreadgeneral")]
        [Aliases("bully")]
        [Dbots, RequireDbotsPerm(Helpers.DbotsPermLevel.Helper)]
        public async Task Noreadgeneral(CommandContext ctx, DiscordMember target, [RemainingText] string reason = "No reason provided.")
        {
            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            if (!botMember.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageRoles))
            {
                return;
            }

            if (Helpers.GetDbotsPerm(ctx.Member) < Helpers.DbotsPermLevel.mod  && !target.IsBot)
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** is not a bot! (Helpers can only bully bots)");
                return;
            }

            var roleToUse = ctx.Guild.GetRole(638861615445311509);
            String fullReason = System.Uri.EscapeDataString($"[Noreadgeneral by {ctx.User.Username}#{ctx.User.Discriminator}] {reason}");

            if (AllowedToMod(ctx.Member, target))
            {
                if (AllowedToMod(await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id), target))
                {
                    if (target.Roles.Contains(roleToUse))
                    {
                        await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** is already restricted from non-testing channels!");
                        return;
                    }

                    await target.GrantRoleAsync(ctx.Guild.GetRole(638861615445311509), fullReason);

                    await ctx.RespondAsync($"<:check:314349398811475968> Successfully applied role to **{target.Username}#{target.Discriminator}**!");
                }
                else
                {
                    await ctx.RespondAsync($"<:xmark:314349398824058880> I don't have permission to bully **{target.Username}#{target.Discriminator}**!");
                    return;
                }
            }
            else
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> You aren't allowed to bully **{target.Username}#{target.Discriminator}**!");
            }
        }


        [Command("canreadgeneral")]
        [Aliases("unbully")]
        [Dbots, RequireDbotsPerm(Helpers.DbotsPermLevel.Helper)]
        public async Task Canreadgeneral(CommandContext ctx, DiscordMember target, [RemainingText] String reason = "No reason provided.")
        {
            var botMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            if (!botMember.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageRoles))
            {
                return;
            }

            if (Helpers.GetDbotsPerm(ctx.Member) == Helpers.DbotsPermLevel.Helper && !target.IsBot)
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** is not a bot! (Helpers can only unbully bots)");
                return;
            }

            var roleToUse = ctx.Guild.GetRole(638861615445311509);
            String fullReason = System.Uri.EscapeDataString($"[Canreadgeneral by {ctx.User.Username}#{ctx.User.Discriminator}] {reason}");

            if (AllowedToMod(ctx.Member, target))
            {
                if (AllowedToMod(await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id), target))
                {
                    if (target.Roles.Contains(roleToUse))
                    {
                        await target.RevokeRoleAsync(roleToUse, fullReason);

                        await ctx.RespondAsync($"<:check:314349398811475968> Successfully removed the role from **{target.Username}#{target.Discriminator}**!");

                    }
                    else
                    {
                        await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** is not restricted from general!");
                    }
                }
                else
                {
                    await ctx.RespondAsync($"<:xmark:314349398824058880> I don't have permission to unbully **{target.Username}#{target.Discriminator}**!");
                    return;
                }
            }
            else
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> You aren't allowed to unbully **{target.Username}#{target.Discriminator}**!");
            }

        }


    }

}
