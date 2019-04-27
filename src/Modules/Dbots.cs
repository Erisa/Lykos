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

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RequireDbotsPermAttribute : CheckBaseAttribute
    {
        public dbotsPermLevel TargetLvl { get; set; }

        public RequireDbotsPermAttribute(dbotsPermLevel targetLvl )
        {
            this.TargetLvl = targetLvl;
        }

        public override async Task<bool> CanExecute(CommandContext ctx, bool help)
        {
            if (ctx.Command.Name == "help")
                return false;

            var level = getDbotsPerm(ctx.Member);
            if (level >= this.TargetLvl)
            {
                return true;
            } else
            {
                await ctx.RespondAsync($"<:xmark:314349398824058880> You don't have permission to access this command! ```\n" +
                        $"Required permission level:  {this.TargetLvl.ToString("d")} ({this.TargetLvl.ToString().ToUpper()})\n" +
                        $"Your permission level:      {level.ToString("d")} ({level.ToString().ToUpper()})\n```");
                return false;
            }
        }


    }

    public class DbotsAttribute : CheckBaseAttribute
    {
        public override async Task<bool> CanExecute(CommandContext ctx, bool help)
        {
            if (ctx.Guild.Id == 110373943822540800)
            {
                return true;
            } else
            {
                await ctx.RespondAsync("<:xmark:314349398824058880> This command only works in **Discord Bots**!");
                return false;
            }
        }
    }

    class Dbots
    {

        [Command("dbotsowner")]
        [Dbots]
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
            Dbots,
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
        [Dbots]
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
        [Dbots]
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
        [Dbots]
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
        [Dbots]
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
