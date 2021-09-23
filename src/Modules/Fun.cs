using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykos.Modules
{
    class Fun : BaseCommandModule
    {
        readonly List<String> flips = new(new string[] {
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
            await ctx.Channel.SendMessageAsync(flips[Program.rnd.Next(0, flips.Count)]);
        }

        [Command("headpat")]
        [Description("Give someone a good headpat, or treat yourself to one!")]
        [Aliases("pat")]
        public async Task Headpat(CommandContext ctx, [Description("The person to give a headpat to!"), RemainingText] string target = "me")
        {
            target = Helpers.SanitiseEveryone(target);

            if (target == null || target == "me" || target == ctx.User.Username || target == ctx.Member.Nickname || target == ctx.Member.Mention)
            {
                await ctx.Channel.SendMessageAsync($"{Program.cfgjson.Emoji.BlobPats} \\*gives a big headpat to {ctx.User.Mention}\\*");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"{Program.cfgjson.Emoji.BlobPats} {target} was given a big headpat by {ctx.User.Username}!");
            }
        }

        [Command("hug")]
        [Description("Give someone a good hug, or treat yourself to one!")]
        public async Task Hug(CommandContext ctx, [Description("The person to give a hug to!"), RemainingText] string target = "me")
        {
            target = Helpers.SanitiseEveryone(target);

            if (target == null || target == "me" || target == ctx.User.Username || target == ctx.Member.Nickname || target == ctx.Member.Mention)
            {
                await ctx.Channel.SendMessageAsync($"{Program.cfgjson.Emoji.BlobHug} \\*gives a tight hug to {ctx.User.Mention}\\*");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"{Program.cfgjson.Emoji.BlobHug} {target} was given a tight hug by {ctx.User.Username}!");
            }
        }

        [Command("kiss")]
        [Description("Give someone a good kiss!")]
        public async Task Kiss(CommandContext ctx, [Description("The person to give a kiss to!"), RemainingText] string target = "me")
        {
            target = Helpers.SanitiseEveryone(target);

            if (target == null || target == "me" || target == ctx.User.Username || target == ctx.Member.Nickname || target == ctx.Member.Mention)
            {
                await ctx.Channel.SendMessageAsync("😳");
            }
            else
            {
                await ctx.Channel.SendMessageAsync($"{Program.cfgjson.Emoji.Kiss} {target} was given a deep and meaningful kiss by {ctx.User.Username}! ❤️");
            }
        }

        [Command("pingeri")]
        [Description("Pong!")]
        public async Task Pingeri(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("<@228574821590499329>");
        }

        [Command("pingfleuron")]
        public async Task Pingfleuron(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("<@188482204601548800>");
        }

        [Command("pingtodd")]
        public async Task Pingtodd(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync("<@461265486655520788>");
        }

        [Command("whenissarahsbirthday")]
        public async Task Whenissarahsbirthday(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync($"Today.");
        }

        [Command("cotd")]
        public async Task Cotd(CommandContext ctx)
        {
            DSharpPlus.Entities.DiscordUser scat = await ctx.Client.GetUserAsync(103347843934212096);
            await ctx.Channel.SendMessageAsync($"Todays cat of the day is **{scat.Username}#{scat.Discriminator}**!");
        }

        [Command("gotd")]
        public async Task Gotd(CommandContext ctx)
        {
            DSharpPlus.Entities.DiscordUser circ = await ctx.Client.GetUserAsync(255114091360681986);
            await ctx.Channel.SendMessageAsync($"Todays genius of the day is **{circ.Username}#{circ.Discriminator}**!");
        }

        [Command("zotd")]
        public async Task Zotd(CommandContext ctx)
        {
            DSharpPlus.Entities.DiscordUser zeta = await ctx.Client.GetUserAsync(94129005791281152);
            await ctx.Channel.SendMessageAsync($"Todays Zeta of the day is **{zeta.Username}#{zeta.Discriminator}**!");
        }

    }
}
