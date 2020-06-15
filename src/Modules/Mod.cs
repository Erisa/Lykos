using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
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
