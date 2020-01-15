using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Lykos.Modules.Helpers;

namespace Lykos.Modules
{

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class RequireDbotsPermAttribute : CheckBaseAttribute
    {
        public DbotsPermLevel TargetLvl { get; set; }

        public RequireDbotsPermAttribute(DbotsPermLevel targetLvl)
        {
            this.TargetLvl = targetLvl;
        }

        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            if (ctx.Command.Name == "help")
                return false;

            if (ctx.Guild.Id != 110373943822540800)
                return false;

            var level = GetDbotsPerm(ctx.Member);
            if (level >= this.TargetLvl)
            {
                return true;
            }
            else
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
        public override async Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
        {
            return ctx.Guild.Id == 110373943822540800;
        }
    }

    class Dbots : BaseCommandModule
    {

        [Command("dbotsowner")]
        [Dbots]
        public async Task DbotsOwner(CommandContext ctx)
        {
            await ctx.RespondAsync($"Everyone knows the secret owner of **Discord Bots** is **{ctx.User.Username}#{ctx.User.Discriminator}**.");
        }

        [Command("whenissarahsbirthday")]
        public async Task Whenissarahsbirthday(CommandContext ctx)
        {
            await ctx.RespondAsync($"Today.");
        }

        [Command("cotd")]
        public async Task Cotd(CommandContext ctx)
        {
            var scat = await ctx.Client.GetUserAsync(103347843934212096);
            await ctx.RespondAsync($"Todays cat of the day is **{scat.Username}#{scat.Discriminator}**!");
        }

        [Command("gotd")]
        public async Task Gotd(CommandContext ctx)
        {
            var circ = await ctx.Client.GetUserAsync(255114091360681986);
            await ctx.RespondAsync($"Todays genius of the day is **{circ.Username}#{circ.Discriminator}**!");
        }
		
	[Command("zotd")]
        public async Task zotd(CommandContext ctx)
        {
            var zeta = await ctx.Client.GetUserAsync(94129005791281152);
            await ctx.RespondAsync($"Todays Zeta of the day is **{zeta.Username}#{zeta.Discriminator}**!");
        }
		
	[Command("coty")]
        public async Task Zotd(CommandContext ctx)
        {
            var cutie = await ctx.Client.GetUserAsync(155659573515124736);
            await ctx.RespondAsync($"The cute of the year is **{cutie.Username}#{cutie.Discriminator}**!");
        }

        readonly ulong BotDevID = 110375768374136832;
        readonly ulong unlistedID = 479762844720824320;

        [
            Command("undev"),
            Aliases("takedev"),
            Dbots,
            RequireDbotsPerm(DbotsPermLevel.mod),
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
        [RequireDbotsPerm(DbotsPermLevel.mod)]
        [Description("Gives Bot Developer to a user. Only usable in Discord Bots.")]
        public async Task Givedev(CommandContext ctx, [Description("The user to give a botdev role to.")] DiscordMember target, [Description("The reason for adding the role.")] params string[] reason)
        {
            String streason;
            if (reason.Length == 0)
                streason = $"[Givedev by {Encoding.UTF8.GetBytes(ctx.User.Username)}#{ctx.User.Discriminator}] No reason specified.";
            else
                streason = $"[Givedev by {Encoding.UTF8.GetBytes(ctx.User.Username)}#{ctx.User.Discriminator}] " + String.Join(" ", reason);

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
        [RequireDbotsPerm(DbotsPermLevel.Helper)]
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
        [RequireDbotsPerm(DbotsPermLevel.Helper)]
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

            DbotsPermLevel level = Helpers.GetDbotsPerm(target);
            String msg = $"The permission level of **{target.Username}#{target.Discriminator}** is `{level.ToString("d")}` (`{level.ToString().ToUpper()}`)";
            if (level >= DbotsPermLevel.botDev)
            {
                msg += "\n- <:check:314349398811475968> User is a Bot Developer or higher.";
            }
            else
            {
                msg += "\n- <:xmark:314349398824058880> User is not a Bot Developer.";
            }

            if (level >= DbotsPermLevel.Helper)
            {
                msg += "\n- <:check:314349398811475968> User has access to Verification Helper commands.";
            }
            else
            {
                msg += "\n- <:xmark:314349398824058880> User does not have access to Verification Helper commands.";
            }

            if (level >= DbotsPermLevel.siteHelper)
            {
                msg += "\n- <:check:314349398811475968> User has access to Site Helper commands.";
            }
            else
            {
                msg += "\n- <:xmark:314349398824058880> User does not have access to Site Helper commands.";
            }

            if (level >= DbotsPermLevel.mod)
            {
                msg += "\n- <:check:314349398811475968> User has access to Moderator commands.";
            }
            else
            {
                msg += "\n- <:xmark:314349398824058880> User does not have access to Moderator commands.";
            }

            if (level >= DbotsPermLevel.owner)
            {
                msg += "\n- <:check:314349398811475968> User is the owner of Discord Bots.";
            }
            else
            {
                msg += "\n- <:xmark:314349398824058880> User is not the owner of Discord Bots.";
            }
            msg += "\nExtras:\n";
            if (Helpers.IsDbotsBooster(target))
            {
                msg += "- <:check:314349398811475968> User is boosting Discord Bots.";
            }
            else
            {
                msg += "- <:xmark:314349398824058880> User is not boosting Discord Bots.";
            }
            msg += $"\n- `{Mod.GetHier(target)}` is this users Role Hierarchy.";


            await ctx.RespondAsync(msg);


        }



    }
}
