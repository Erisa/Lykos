using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykos.Modules
{
    class Mod : BaseCommandModule
    {
        [Command("delete")]
        [Description("Owner only, delete a message.")]
        [RequireOwner, RequireBotPermissions(Permissions.ManageMessages)]
        public async Task Delete(CommandContext ctx, [Description("ID of the message to delete")] ulong messageId)
        {
            await ctx.Message.DeleteAsync();
            DiscordMessage msg = await ctx.Channel.GetMessageAsync(messageId);
            await msg.DeleteAsync();
        }

        [Command("yeet")]
        [Description("Deletes the embed on a given message.")]
        [RequirePermissions(DSharpPlus.Permissions.ManageMessages)]
        public async Task Yeet(CommandContext ctx, [Description("ID of the message to delete an embed on")] ulong messageId)
        {
            await ctx.Message.DeleteAsync();
            DiscordMessage msg = await ctx.Channel.GetMessageAsync(messageId);
            await msg.ModifyEmbedSuppressionAsync(true);
        }

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
                        await member.BanAsync(0, $"[Ban by {ctx.User.Username}#{ctx.User.Discriminator}] {reason}");
                        await ctx.RespondAsync($"🔨 Succesfully bent **{target.Username}#{target.Discriminator} (`{target.Id}`)**");
                        return;
                    }
                    else
                    {
                        await ctx.RespondAsync($"{Program.cfgjson.Emoji.Xmark} I don't have permission to ban **{target.Username}#{target.Discriminator}**!");
                        return;

                    }
                }
                else
                {
                    await ctx.RespondAsync($"{Program.cfgjson.Emoji.Xmark} You aren't allowed to ban **{target.Username}#{target.Discriminator}**!");
                    return;
                }
            }
        }

        [Command("unban")]
        [Description("Unban a user. If you can. Do it, I dare you.")]
        [RequirePermissions(Permissions.BanMembers)]
        public async Task Unban(CommandContext ctx, DiscordUser target, string reason = "No reason provided.")
        {
            await target.UnbanAsync(ctx.Guild, $"[Unban by {ctx.User.Username}#{ctx.User.Discriminator}] {reason}");
            await ctx.RespondAsync($"Succesfully unbanned **{target.Username}#{target.Discriminator}** (`{target.Id}`)");
        }

        [Command("kick")]
        [Description("Kick a user. If you can. Do it, I dare you.")]
        [RequirePermissions(Permissions.KickMembers)]
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
                    await ctx.RespondAsync($"{Program.cfgjson.Emoji.Xmark} I don't have permission to kick **{target.Username}#{target.Discriminator}**!");
                    return;
                }
            }
            else
            {
                await ctx.RespondAsync($"{Program.cfgjson.Emoji.Xmark} You aren't allowed to kick **{target.Username}#{target.Discriminator}**!");
                return;
            }
        }

        [Command("prune")]
        [Description("prune some messages or something")]
        public async Task Prune(CommandContext ctx, DiscordUser targetUser = null)
        {
            var helperRole = ctx.Guild.GetRole(407326634819977217);
            var modRole = ctx.Guild.GetRole(113379036524212224);
            DiscordMember targetMember = null;
            if (targetUser != null)
            {
                targetMember = await ctx.Guild.GetMemberAsync(targetUser.Id);
            }
            else
            {

            }

            System.Collections.ObjectModel.Collection<DiscordMessage> messagesToDelete = new System.Collections.ObjectModel.Collection<DiscordMessage> { };
            var messagesToConsider = await ctx.Channel.GetMessagesAsync(50);
            if (helperRole == null)
            {
                return;
            }

            if (ctx.Member.Roles.Contains(helperRole))
            {

                if (targetUser == null)
                {

                    foreach (var msg in messagesToConsider)
                    {
                        if (msg.Author.IsBot)
                        {
                            messagesToDelete.Add(msg);
                        }
                    }
                }
                else if (!targetUser.IsBot)
                {
                    await ctx.RespondAsync("That target is not a bot!");
                    return;
                }
                else if (targetMember != null && targetMember.Roles.Contains(modRole))
                {
                    await ctx.RespondAsync("You can't take action on a Moderator bot!");
                    return;
                }
                else
                {
                    foreach (var msg in messagesToConsider)
                    {
                        if (msg.Author == targetUser)
                        {
                            messagesToDelete.Add(msg);
                        }
                    }
                }
                if (messagesToDelete.Count == 0)
                {
                    await ctx.RespondAsync("There were no messages that matched! Please don't waste my time :(");
                }
                else
                {
                    await ctx.Channel.DeleteMessagesAsync((IEnumerable<DiscordMessage>)messagesToDelete, $"Prune by {ctx.User.Id}");
                    var resp = await ctx.RespondAsync($"Okay! I deleted {messagesToDelete.Count} messages! Hope they were the right ones!");
                    System.Threading.Thread.Sleep(6000);
                    await resp.DeleteAsync();
                }
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

    }

}
