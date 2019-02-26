using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Lykos.Modules
{
    class Fun
    {
        [Command("tableflip")]
        public async Task Tableflip(CommandContext ctx)
        {
            List<String> flippers = new List<string>(new string[] { "( ﾉ⊙︵⊙）ﾉ", "(╯°□°）╯", "( ﾉ\\♉︵\\♉ ）ﾉ"});
            int choice = Program.rnd.Next(0, 4);
            if (choice == 3)
            {
                await ctx.RespondAsync("┬─┬﻿ ノ( ゜-゜ノ)");
            } else
            {
                await ctx.RespondAsync($"{flippers[choice]}︵┻━┻");
            }
        }
    }
}
