
using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Google.Apis.YouTube.v3;
using Google.Apis.Services;
using System.Threading.Tasks;
using System.Linq;
using DSharpPlus.Interactivity;
using DSharpPlus.Entities;
using DSharpPlus;
using System.Net;

namespace Lykos.Modules
{
    class YouTube
    {

        [Command("yt")]
        public async Task Yt(CommandContext ctx, [RemainingText] String searchTerm)
        {
            var curMember = await ctx.Guild.GetMemberAsync(ctx.Client.CurrentUser.Id);

            if (!Program.usingYoutube)
            {
                await ctx.RespondAsync("No YouTube API key in config. Command cannot run.");
                return;
            }

            var searchListRequest = Program.youtubeService.Search.List("snippet");
            searchListRequest.Q = searchTerm; // Replace with your search term.
            searchListRequest.Type = "video";
            searchListRequest.MaxResults = 5;

            var searchListResponse = await searchListRequest.ExecuteAsync();
            if (searchListResponse.Items.Count() == 0)
            {
                await ctx.RespondAsync("No results found!");
                return;
            } else if (searchListResponse.Items.Count() == 1)
            {
                await ctx.RespondAsync($"**{ctx.User.Username}**, I only found one video. Here it is:\nhttps://www.youtube.com/watch?v={searchListResponse.Items[0].Id.VideoId}");
                return;
            }

            String res = $"**{ctx.User.Username}**, I found these videos. Which one did you want?```\n";

            foreach(var searchResult in searchListResponse.Items)
            {
                res += $"{searchListResponse.Items.IndexOf(searchResult) + 1} - {WebUtility.HtmlDecode(searchResult.Snippet.Title)} | {WebUtility.HtmlDecode(searchResult.Snippet.ChannelTitle)}\n";
            }

            res += "```Respond with the number to select, anything else will cancel.";

            var msg_results = await ctx.RespondAsync(res);

            var ctx_second = await Program.interactivity.WaitForMessageAsync(xm => xm.Author.Id == ctx.User.Id && xm.Channel.Id == ctx.Channel.Id, TimeSpan.FromMinutes(1));
            int numOut;

            if (ctx_second != null)
            {
                if (int.TryParse(ctx_second.Message.Content, out numOut))
                {
                    if (numOut > searchListResponse.Items.Count() || numOut < 1)
                    {
                        await msg_results.ModifyAsync("Unknown response, canceled.");

                        if (curMember.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageMessages))
                        {
                            await ctx_second.Message.DeleteAsync();
                            await ctx.Message.DeleteAsync();
                        }

                        System.Threading.Thread.Sleep(1000);
                        await msg_results.DeleteAsync();
                    } else
                    {
                        var yt_res = await msg_results.ModifyAsync($"**{ctx.User.Username}**, heres your video. Reply with `del` to remove it.\nhttps://www.youtube.com/watch?v={searchListResponse.Items[numOut - 1].Id.VideoId}");
                        await ctx_second.Message.DeleteAsync();
                        if (curMember.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageMessages))
                        {
                            // await msg_results.DeleteAsync();
                        }
                        var deleteme = await Program.interactivity.WaitForMessageAsync(xm => xm.Author.Id == ctx.User.Id && xm.Channel.Id == ctx.Channel.Id && (xm.Content == "delete" || xm.Content == "del"), TimeSpan.FromMinutes(1));
                        if (deleteme != null)
                        {
                            if (curMember.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageMessages))
                            {
                                ctx.Message.DeleteAsync();
                                deleteme.Message.DeleteAsync();
                            }
                            await msg_results.DeleteAsync();
                            
                        }
                    }
                } else
                {
                    await msg_results.ModifyAsync("Unknown response, canceled.");
                   
                    if (curMember.PermissionsIn(ctx.Channel).HasPermission(Permissions.ManageMessages))
                    {
                        await ctx_second.Message.DeleteAsync();
                    }
                    
                    System.Threading.Thread.Sleep(1000);
                    await msg_results.DeleteAsync();
                }
            } else
            {
                await msg_results.ModifyAsync("Timed out.");
            }


        }

    }
}
