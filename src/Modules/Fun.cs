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
            await ctx.Channel.SendMessageAsync($"Today's cat of the day is **{UniqueUsername(await ctx.Client.GetUserAsync(103347843934212096))}**!");
        }

        [Command("gotd")]
        public async Task Gotd(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync($"Today's genius of the day is **{UniqueUsername(await ctx.Client.GetUserAsync(255114091360681986))}**!");
        }

        [Command("zotd")]
        public async Task Zotd(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync($"Today's Zeta of the day is **{UniqueUsername(await ctx.Client.GetUserAsync(94129005791281152))}**!");
        }

        [Command("kotd")]
        public async Task Kotd(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync($"Today's Kot of the day is **{UniqueUsername(await ctx.Client.GetUserAsync(621929840710516736))}**!");
        }

        [Command("motd")]
        public async Task Motd(CommandContext ctx)
        {
            await ctx.Channel.SendMessageAsync($"Today's Megumin of the day is **{UniqueUsername(await ctx.Client.GetUserAsync(545581357812678656))}**!");
        }

    }
}
