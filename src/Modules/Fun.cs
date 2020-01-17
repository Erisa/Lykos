using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykos.Modules
{
    class Fun : BaseCommandModule
    {
        readonly List<String> flips = new List<string>(new string[] {
            "( ﾉ⊙︵⊙）ﾉ︵┻━┻",
            "(╯°□°）╯︵┻━┻",
            "( ﾉ\\♉︵\\♉ ）ﾉ︵┻━┻",
            "┬─┬﻿ ノ( ゜-゜ノ)",
            "┬─┬﻿ ノ( °□°  ノ)"
        });

        [Command("tableflip")]
        [Description("( ﾉ⊙︵⊙）ﾉ︵┻━┻")]
        public async Task Tableflip(CommandContext ctx)
        {
            await ctx.RespondAsync(flips[Program.rnd.Next(0, flips.Count)]);
        }

        [Command("headpat")]
        [Description("Give someone a good headpat, or treat yourself to one!")]
        [Aliases("pat")]
        public async Task Headpat(CommandContext ctx, [Description("The person to give a headpat to!"), RemainingText] string target = "me")
        {
            target = Helpers.sanitiseEveryone(target);

            if (target == null || target == "me" || target == ctx.User.Username || target == ctx.Member.Nickname || target == ctx.Member.Mention)
            {
                await ctx.RespondAsync($"<:blobpats:585804188735504435> \\*gives a big headpat to {ctx.User.Mention}\\*");
            } else
            {
                await ctx.RespondAsync($"<:blobpats:585804188735504435> {target} was given a big headpat by {ctx.User.Username}!");
            }
        }

        [Command("pingeri")]
        [Description("Pong!")]
        public async Task Pingeri(CommandContext ctx)
        {
            await ctx.RespondAsync("<@228574821590499329>");
        }
    }
}
