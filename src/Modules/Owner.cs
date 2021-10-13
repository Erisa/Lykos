using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Minio.Exceptions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static Lykos.Modules.Helpers;

namespace Lykos.Modules
{
    partial class Owner : BaseCommandModule
    {

        [Group("debug")]
        [Aliases("d")]
        [RequireOwner]
        class DebugCmds : BaseCommandModule
        {
            [Command("modcheck")]
            [Description("Check whether a user has permisssion to mod another user.")]
            [Aliases("mod")]
            public async Task Modcheck(CommandContext ctx, DiscordMember firstMember, [RemainingText] DiscordMember target = null)
            {
                if (target == null)
                {
                    target = firstMember;
                    firstMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);
                }

                int invoker_hier = Mod.GetHier(firstMember);
                int target_hier = Mod.GetHier(target);

                bool allowed = Mod.AllowedToMod(firstMember, target);

                await ctx.Channel.SendMessageAsync($"According to my calulcations, **{firstMember.Username}#{firstMember.Discriminator}** has a Role Hierachy of `{invoker_hier}`" +
                    $"and **{target.Username}#{target.Discriminator}** has `{target_hier}`.\nFrom this, I can conclude that the answer is `{allowed}`.");
            }

            [Command("sysinfo")]
            [Description("Where am I running? Lets find out together!")]
            public async Task Sysinfo(CommandContext ctx)
            {
                await ctx.Channel.SendMessageAsync($"🤔 Hmm, based on my research it seems that:\n" +
                    $"- This device is calling itself `{System.Environment.MachineName}`\n" +
                    $"- The OS platform is `{Helpers.GetOSPlatform()}`\n" +
                    $"- The OS describes itself as `{RuntimeInformation.OSDescription}`\n" +
                    $"- The OS architecture appears to be `{RuntimeInformation.OSArchitecture}`\n" +
                    $"- The framework I'm running from is `{RuntimeInformation.FrameworkDescription}`");
            }
        }

        [Group("system")]
        [Aliases("s", "sys")]
        [RequireOwner]
        [Hidden]
        class SystemCmds : BaseCommandModule
        {
            [Command("reconnect"), Aliases("rc", "re")]
            [Description("Goodbye, hello! This will reconnect my websocket connection.")]
            public async Task Reconnect(CommandContext ctx)
            {
                DiscordMessage msg = await ctx.Channel.SendMessageAsync("Reconnecting to websocket...");
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                await ctx.Client.ReconnectAsync();
                watch.Stop();
                await msg.ModifyAsync($"Reconnected to websocket!\n- This took `{watch.ElapsedMilliseconds}ms` to complete!");
            }

            [Command("shutdown"), Aliases("shut", "sd", "s", "kill")]
            [Description("A soft exit. I will disconnect and then end my process.")]
            public async Task Shutdown(CommandContext ctx)
            {
                DiscordMessage msg = await ctx.Channel.SendMessageAsync("Disonnecting from websocket...");
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                await ctx.Client.DisconnectAsync();
                watch.Stop();
                await msg.ModifyAsync($"Disconnected from websocket!\n" +
                    $"- This took `{watch.ElapsedMilliseconds}ms` to complete!\n" +
                    $"Now exiting main process. Goodbye!");
                Environment.Exit(0);
            }

            [Command("die")]
            [Description("A more permanent goodbye! I will try to end my own service.")]
            public async Task Die(CommandContext ctx, string target = null)
            {
                if (target != null && System.Environment.MachineName.ToLower() != target.ToLower())
                {
                    return;
                }

                DiscordMessage msg = await ctx.Channel.SendMessageAsync("Disonnecting from websocket...");
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                await ctx.Client.DisconnectAsync();
                watch.Stop();
                await msg.ModifyAsync($"Disconnected from websocket!\n" +
                    $"- This took `{watch.ElapsedMilliseconds}ms` to complete!\n" +
                    $"Now stopping main service. If that doesn't work, I'll just end my process!");

                ShellResult finishedShell = Helpers.RunShellCommand("pm2 stop lykos");

                if (finishedShell.proc.ExitCode != 0)
                {
                    Environment.Exit(1);
                }

            }

            [Command("sh")]
            [Aliases("cmd")]
            [Description("Run shell commands! Bash for Linux/macOS, batch for Windows!")]
            public async Task Shell(CommandContext ctx, [RemainingText] string command)
            {
                DiscordMessage msg = await ctx.Channel.SendMessageAsync("executing..");

                ShellResult finishedShell = Helpers.RunShellCommand(command);

                if (finishedShell.result.Length > 1947)
                {
                    HasteBinResult hasteURL = await Program.hasteUploader.Post(finishedShell.result);
                    if (hasteURL.IsSuccess)
                    {
                        await msg.ModifyAsync($"Done, but output exceeded character limit! (`{finishedShell.result.Length}`/`1947`)\n" +
                            $"Full output can be viewed here: https://paste.erisa.moe/raw/{hasteURL.Key}\nProcess exited with code `{finishedShell.proc.ExitCode}`.");
                    }
                    else
                    {
                        Console.WriteLine(finishedShell.result);
                        await msg.ModifyAsync($"Error occured during upload to Hastebin.\nAction was executed regardless, shell exit code was `{finishedShell.proc.ExitCode}`. Hastebin status code is `{hasteURL.StatusCode}`.\nPlease check the console/log for the command output.");
                    }
                }
                else
                {
                    await msg.ModifyAsync($"Done, output: ```\n" +
                        $"{finishedShell.result}```Process exited with code `{finishedShell.proc.ExitCode}`.");
                }
            }

            [Command("say"), Aliases("echo")]
            public async Task Say(CommandContext ctx, [RemainingText] string input)
            {
                await ctx.Channel.SendMessageAsync(Helpers.SanitiseEveryone(input));
            }

        }

        [Group("eri")]
        [RequireOwner]
        [Description("Commands that manage data across Erisas things and stuff.")]
        partial class Eri : BaseCommandModule
        {
            [Command("gibinvite")]
            [Description("???")]
            public async Task Gibinvite(CommandContext ctx, int max_uses = 1, int age = 0)
            {
                DiscordChannel channel = await ctx.Client.GetChannelAsync(230004550973521932);
                DiscordInvite inv = await channel.CreateInviteAsync(age, max_uses, false, true, $"gibinvite command used in {ctx.Channel.Id}");

                DiscordDmChannel chan = await ctx.Member.CreateDmChannelAsync();
                await chan.SendMessageAsync($"Here's the invite you asked for: https://discord.gg/{inv.Code}");
                await ctx.Channel.SendMessageAsync($"{Program.cfgjson.Emoji.Check} I've DMed you an invite to **Erisa's Corner** with `{max_uses}` use(s) and an age of `{age}`!");

            }

            [Group("update")]
            partial class Update : BaseCommandModule
            {

                [Command("avatar")]
                [Description("Updates cdn.erisa.moe/avatars/current.png or any other filename.")]
                public async Task Avatar(CommandContext ctx, string name = "current")
                {
                    if (ctx.User.Id == 202122613118468097 && name == "current")
                    {
                        name = "esumi";
                    }

                    DiscordMessage msg;
                    string objectName;

                    msg = await ctx.RespondAsync($"Selected name: `{name}`\n{Program.cfgjson.Emoji.Loading} - Uploading to {Program.cfgjson.S3.DisplayName}...\n" +
                        $"🔲 - Waiting to purge Cloudflare cache.");
                    objectName = $"avatars/{name}.png";

                    string avatarUrl;
                    if (ctx.Member.GuildAvatarHash != ctx.Member.AvatarHash)
                        avatarUrl = $"https://cdn.discordapp.com/guilds/{ctx.Guild.Id}/users/{ctx.Member.Id}/avatars/{ctx.Member.GuildAvatarHash}.png?size=4096";
                    else
                        avatarUrl = $"https://cdn.discordapp.com/avatars/{ctx.Member.Id}/{ctx.Member.AvatarHash}.png?size=4096";

                    MemoryStream memStream;
                    using (WebClient client = new())
                    {
                        memStream = new MemoryStream(client.DownloadData(avatarUrl));
                    }

                    try
                    {
                        Dictionary<string, string> meta = new() { };

                        if (Program.cfgjson.S3.PublicReadAcl)
                        {
                            meta["x-amz-acl"] = "public-read";
                        }

                        await Program.minio.PutObjectAsync(Program.cfgjson.S3.Bucket, objectName, memStream, memStream.Length, "image/png", meta);
                    }
                    catch (MinioException e)
                    {
                        await msg.ModifyAsync($"{Program.cfgjson.Emoji.Xmark} An API error occured while uploading to {Program.cfgjson.S3.DisplayName}:```\n" +
                            $"{e.Message}```");
                        return;
                    }
                    catch (Exception e)
                    {
                        await msg.ModifyAsync($"{Program.cfgjson.Emoji.Xmark} An unexpected error occured while uploading to {Program.cfgjson.S3.DisplayName}:```\n" +
                            $"{e.Message}```");
                        return;
                    }

                    await msg.ModifyAsync($"Selected name: `{name}`\n" +
                        $"{Program.cfgjson.Emoji.Check} - Uploaded `{objectName}` to {Program.cfgjson.S3.DisplayName}!\n" +
                        $"{Program.cfgjson.Emoji.Loading} Purging the Cloudflare cache...");

                    // https://github.com/Sankra/cloudflare-cache-purger/blob/master/main.csx#L113
                    CloudflareContent content = new(new List<string>() { Program.cfgjson.Cloudflare.UrlPrefix + objectName });
                    string cloudflareContentString = JsonConvert.SerializeObject(content);
                    try
                    {
                        using HttpClient httpClient = new()
                        {
                            BaseAddress = new Uri("https://api.cloudflare.com/")
                        };

                        HttpRequestMessage request = new(HttpMethod.Delete, "client/v4/zones/" + Program.cfgjson.Cloudflare.ZoneID + "/purge_cache")
                        {
                            Content = new StringContent(cloudflareContentString, Encoding.UTF8, "application/json")
                        };
                        request.Headers.Add("Authorization", $"Bearer {Program.cfgjson.Cloudflare.Token}");

                        HttpResponseMessage response = await httpClient.SendAsync(request);
                        string responseText = await response.Content.ReadAsStringAsync();

                        if (response.IsSuccessStatusCode)
                        {
                            await msg.ModifyAsync($"{Program.cfgjson.Emoji.Check} - Uploaded `{objectName}` to {Program.cfgjson.S3.DisplayName}!" +
                                $"\n{Program.cfgjson.Emoji.Check} - Successsfully purged the Cloudflare cache for `{objectName}`!");
                        }
                        else
                        {
                            await msg.ModifyAsync($"{Program.cfgjson.Emoji.Check} - Uploaded `{objectName}` to {Program.cfgjson.S3.DisplayName}!" +
                                $"\n{Program.cfgjson.Emoji.Xmark} - An API error occured when purging the Cloudflare cache: ```json\n{responseText}```");
                        }
                    }
                    catch (Exception e)
                    {
                        await msg.ModifyAsync($"{Program.cfgjson.Emoji.Check} - Uploaded `{objectName}` to {Program.cfgjson.S3.DisplayName}!\n" +
                                $"{Program.cfgjson.Emoji.Xmark} - An unexpected error occured when purging the Cloudflare cache: ```json\n" +
                                $"{e.Message}```");
                    }

                }
            }

            [Command("link")]
            public async Task Link(CommandContext ctx, string key, string url)
            {
                using HttpClient httpClient = new()
                {
                    BaseAddress = new Uri(Program.cfgjson.WorkerLinks.BaseUrl)
                };

                HttpRequestMessage request;

                if (key == "null" || key == "random" || key == "gen")
                {
                    request = new HttpRequestMessage(HttpMethod.Post, "") { };
                }
                else
                {
                    request = new HttpRequestMessage(HttpMethod.Put, key) { };
                }


                request.Headers.Add("Authorization", Program.cfgjson.WorkerLinks.Secret);
                request.Headers.Add("URL", url);

                HttpResponseMessage response = await httpClient.SendAsync(request);
                int httpStatusCode = (int)response.StatusCode;
                string httpStatus = response.StatusCode.ToString();
                string responseText = await response.Content.ReadAsStringAsync();
                if (responseText.Length > 1940)
                {
                    HasteBinResult hasteURL = await Program.hasteUploader.Post(responseText);
                    if (hasteURL.IsSuccess)
                    {
                        // responseText = hasteURL.FullUrl;
                        await ctx.Channel.SendMessageAsync($"Worker responded with code: `{httpStatusCode}` (`{httpStatus}`)\n{hasteURL.FullUrl}");
                        return;
                    }
                    else
                    {
                        Console.WriteLine(responseText);
                        responseText = "Error occured during upload to Hastebin. Please check the console/logs for the output.";
                    }
                }
                await ctx.Channel.SendMessageAsync($"Worker responded with code: `{httpStatusCode}` (`{httpStatus}`)\n```json\n{responseText}\n```");
            }


            // https://github.com/Sankra/cloudflare-cache-purger/blob/master/main.csx#L197
            readonly struct CloudflareContent
            {
                public CloudflareContent(List<string> urls)
                {
                    Files = urls;
                }

                public List<string> Files { get; }
            }

        }


    }
}
