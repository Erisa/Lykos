using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykos.Modules
{
    class Fun
    {
        List<String> flips = new List<string>(new string[] {
            "( ﾉ⊙︵⊙）ﾉ︵┻━┻",
            "(╯°□°）╯︵┻━┻",
            "( ﾉ\\♉︵\\♉ ）ﾉ︵┻━┻",
            "┬─┬﻿ ノ( ゜-゜ノ)",
            "┬─┬﻿ ノ( °□°  ノ)"
        });

        [Command("tableflip")]
        public async Task Tableflip(CommandContext ctx)
        {
            await ctx.RespondAsync(flips[Program.rnd.Next(0, flips.Count)]);
        }
    }
}
