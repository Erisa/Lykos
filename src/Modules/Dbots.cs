using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using static Lykos.Modules.Helpers;

namespace Lykos.Modules
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RequireDbotsPermAttribute : CheckBaseAttribute
    {
        public dbotsPermLevel TargetLvl { get; set; }

        public RequireDbotsPermAttribute(dbotsPermLevel targetLvl )
        {
            this.TargetLvl = targetLvl;
        }

        public override async Task<bool> CanExecute(CommandContext ctx, bool help)
        {
            return getDbotsPerm(ctx.Member) >= this.TargetLvl;
        }

    }

    class Dbots
    {

        public class DbotsMod : CheckBaseAttribute
        {
            public override Task<bool> CanExecute(CommandContext ctx, bool help = false)
            {
                if (ctx.Guild.Id != 110373943822540800)
                {
                    return Task.FromResult(false);
                }

                // Ugly workaround because I can't be bothered working out semantics of how it works.
                if (ctx.Command.Name == "help")
                {
                    return Task.FromResult(true);
                }

                if (getDbotsPerm(ctx.Member) >= dbotsPermLevel.Mod)
                {
                    return Task.FromResult(true);
                }
                else
                {
                    dbotsPermLevel level = getDbotsPerm(ctx.Member);
                    ctx.RespondAsync($"<:xmark:314349398824058880> You're not a Verification Helper on **Discord Bots**! ```\n" +
                        $"Required permission level:{dbotsPermLevel.Mod.ToString("d")} ({dbotsPermLevel.Mod.ToString().ToUpper()})" +
                        $"Your permission level:    {level.ToString("d")} ({level.ToString().ToUpper()})\n```");
                    return Task.FromResult(false);
                }
            }
        }

        public class DbotsHelper : CheckBaseAttribute
        {
            public override Task<bool> CanExecute(CommandContext ctx, bool help = false)
            {
                if (ctx.Guild.Id != 110373943822540800)
                {
                    return Task.FromResult(false);
                }

                // Ugly workaround because I can't be bothered working out semantics of how it works.
                if (ctx.Command.Name == "help")
                {
                    return Task.FromResult(true);
                }

                if (getDbotsPerm(ctx.Member) >= dbotsPermLevel.Helper)
                {
                    return Task.FromResult(true);
                }
                else
                {
                    dbotsPermLevel level = getDbotsPerm(ctx.Member);
                    ctx.RespondAsync($"<:xmark:314349398824058880> You're not a Verification Helper on **Discord Bots**! ```\n" +
                        $"Required permission level:  {dbotsPermLevel.Helper.ToString("d")} ({dbotsPermLevel.Helper.ToString().ToUpper()})\n" +
                        $"Your permission level:      {level.ToString("d")} ({level.ToString().ToUpper()})```");
                    return Task.FromResult(false);
                }
            }
        }

        [Command("dbotsowner")]
        public async Task dbotsOwner(CommandContext ctx)
        {
            await ctx.RespondAsync($"Everyone knows the secret owner of **Discord Bots** is **{ctx.User.Username}#{ctx.User.Discriminator}**.");
        }

        [Command("whenissarahsbirthday")]
        public async Task whenissarahsbirthday(CommandContext ctx)
        {
            await ctx.RespondAsync($"Today.");
        }

        [Command("cotd")]
        public async Task cotd(CommandContext ctx)
        {
            var scat = await ctx.Client.GetUserAsync(103347843934212096);
            await ctx.RespondAsync($"Todays cat of the day is **{scat.Username}#{scat.Discriminator}**!");
        }

        [Command("zotd")]
        public async Task zotd(CommandContext ctx)
        {
            var zeta = await ctx.Client.GetUserAsync(94129005791281152);
            await ctx.RespondAsync($"Todays Zeta of the day is **{zeta.Username}#{zeta.Discriminator}**!");
        }


        ulong BotDevID = 110375768374136832;
        ulong unlistedID = 479762844720824320;

        [
            Command("undev"),
            Aliases("takedev"),
            RequireDbotsPerm(dbotsPermLevel.Mod),
            Description("Removes Bot Developer from a user. Only usable in Discord Bots.")
        ]
        public async Task Undev(CommandContext ctx, [Description("The user to remove a botdev role from.")] DiscordMember target, [Description("The reason for removing the role.")] params string[] reason)
        {
            String streason;
            if (reason.Length == 0)
                streason = $"[Undev by {ctx.User.Username}#{ctx.User.Discriminator}] No reason specified.";
            else
                streason = $"[Undev by {ctx.User.Username}#{ctx.User.Discriminator}] " + String.Join(" ", reason);

            var role = ctx.Guild.GetRole(BotDevID);
            if (target.Roles.Contains(role))
            {
                await target.RevokeRoleAsync(role, streason);
                await ctx.RespondAsync($"<:check:314349398811475968> Bot Developer taken from **{target.Username}#{target.Discriminator}**!");
            }
            else
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** is not a Bot Developer!");
            }

        }

        [Command("givedev")]
        [RequireDbotsPerm(dbotsPermLevel.Mod)]
        [Description("Gives Bot Developer to a user. Only usable in Discord Bots.")]
        public async Task Givedev(CommandContext ctx, [Description("The user to give a botdev role to.")] DiscordMember target, [Description("The reason for adding the role.")] params string[] reason)
        {
            String streason;
            if (reason.Length == 0)
                streason = $"[Givedev by {ctx.User.Username}#{ctx.User.Discriminator}] No reason specified.";
            else
                streason = $"[Givedev by {ctx.User.Username}#{ctx.User.Discriminator}] " + String.Join(" ", reason);

            var role = ctx.Guild.GetRole(BotDevID);
            if (target.Roles.Contains(role))
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** is already a Bot Developer!");
            }
            else
            {
                await target.GrantRoleAsync(role, streason);
                await ctx.RespondAsync($"<:check:314349398811475968> Bot Developer given to **{target.Username}#{target.Discriminator}**!");
            }

        }

        [Command("list")]
        [RequireDbotsPerm(dbotsPermLevel.Helper)]
        [Aliases("listed", "takeunlisted")]
        [Description("Removes Bot Developer from a user. Only usable in Discord Bots.")]
        public async Task List(CommandContext ctx, [Description("The user to remove a botdev role from.")] DiscordMember target, [Description("The reason for removing the role.")] params string[] reason)
        {
            String streason;
            if (reason.Length == 0)
                streason = $"[List by {ctx.User.Username}#{ctx.User.Discriminator}] No reason specified.";
            else
                streason = $"[List by {ctx.User.Username}#{ctx.User.Discriminator}] " + String.Join(" ", reason);

            var role = ctx.Guild.GetRole(unlistedID);
            if (!target.IsBot)
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** is not a bot!");
            }
            else if (target.Roles.Contains(role))
            {
                await target.RevokeRoleAsync(role, streason);
                await ctx.RespondAsync($"<:check:314349398811475968> Unlisted taken from **{target.Username}#{target.Discriminator}**!");
            }
            else
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** doesn't have **Unlisted** role!");
            }

        }

        [Command("unlist")]
        [RequireDbotsPerm(dbotsPermLevel.Helper)]
        [Description("Gives Bot Developer to a user. Only usable in Discord Bots.")]
        public async Task Unlist(CommandContext ctx, [Description("The user to give a botdev role to.")] DiscordMember target, [Description("The reason for adding the role.")] params string[] reason)
        {
            String streason;
            if (reason.Length == 0)
                streason = $"[Unlist by {ctx.User.Username}#{ctx.User.Discriminator}] No reason specified.";
            else
                streason = $"[Unlist by {ctx.User.Username}#{ctx.User.Discriminator}] " + String.Join(" ", reason);

            var role = ctx.Guild.GetRole(unlistedID);

            if (!target.IsBot)
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** is not a bot!");
            }
            else if (target.Roles.Contains(role))
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> **{target.Username}#{target.Discriminator}** already has **Unlisted** role!");
            }
            else
            {
                await target.GrantRoleAsync(role, streason);
                await ctx.RespondAsync($"<:check:314349398811475968> Unlisted given to **{target.Username}#{target.Discriminator}**!");
            }

        }

        [Command("checkperms")]
        public async Task Checkperms(CommandContext ctx, [RemainingText] DiscordMember target = null)
        {
            if (target == null)
                target = ctx.Member;

            dbotsPermLevel level = Helpers.getDbotsPerm(target);
            String msg = $"The permission level of **{target.Username}#{target.Discriminator}** is `{level.ToString("d")}` (`{level.ToString().ToUpper()}`)";
            if (level >= dbotsPermLevel.botDev)
            {
                msg = msg + "\n- <:check:314349398811475968> User is a Bot Developer or higher.";
            }
            else
            {
                msg = msg + "\n- <:xmark:314349398824058880> User is not a Bot Developer.";
            }

            if (level >= dbotsPermLevel.Helper)
            {
                msg = msg + "\n- <:check:314349398811475968> User has access to Helper commands.";
            }
            else
            {
                msg = msg + "\n- <:xmark:314349398824058880> User does not have access to Helper commands.";
            }

            if (level >= dbotsPermLevel.Mod)
            {
                msg = msg + "\n- <:check:314349398811475968> User has access to Moderator commands.";
            }
            else
            {
                msg = msg + "\n- <:xmark:314349398824058880> User does not have access to Moderator commands.";
            }

            if (level >= dbotsPermLevel.Owner)
            {
                msg = msg + "\n- <:check:314349398811475968> User is the owner of Discord Bots.";
            }
            else
            {
                msg = msg + "\n- <:xmark:314349398824058880> User is not the owner of Discord Bots.";
            }

            await ctx.RespondAsync(msg);


        }



    }
}
