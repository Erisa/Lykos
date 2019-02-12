using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Newtonsoft.Json;
using Google.Cloud.Storage.V1;
using System.Net;
using System.IO;

namespace Lykos.Modules
{
    class Owner
    {

        [Group("system")]
        [Aliases("s", "sys")]
        [RequireOwner]
        [Hidden]
        class SystemCmds
        {
            [Command("reconnect")]
            [Aliases("rc", "re")]
            public async Task Reconnect(CommandContext ctx)
            {
                var msg = await ctx.RespondAsync("Reconnecting to websocket...");
                var watch = System.Diagnostics.Stopwatch.StartNew();
                await ctx.Client.ReconnectAsync();
                watch.Stop();
                await msg.ModifyAsync($"Reconnected to websocket!\n- This took `{watch.ElapsedMilliseconds}ms` to complete!");
            }

            [Command("shutdown")]
            [Aliases("shut", "sd", "s", "kill", "die")]
            public async Task Shutdown(CommandContext ctx)
            {
                var msg = await ctx.RespondAsync("Disonnecting from websocket...");
                var watch = System.Diagnostics.Stopwatch.StartNew();
                await ctx.Client.DisconnectAsync();
                watch.Stop();
                await msg.ModifyAsync($"Disconnected from websocket!\n- This took `{watch.ElapsedMilliseconds}ms` to complete!\nNow exiting main process. Goodbye!");
                Environment.Exit(0);
            }

        }

        [Group("eri")]
        [Description("Commands that manage data across Erisas things and stuff.")]
        [RequireOwner]
        [Hidden]
        class Eri {
            [Group("update")]
            [RequireOwner]
            class Update
            {
                [Command("avatar")]
                public async Task Avatar(CommandContext ctx)
                {
                    var msg = await ctx.RespondAsync("Updating network avatar.. if no status message is displayed soon, assume a fatal error occured.");
                    string avatarUrl = $"https://cdn.discordapp.com/avatars/{ctx.User.Id}/{ctx.User.AvatarHash}.png?size=1024";
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(avatarUrl, "AVATAR.png");
                    }
                    var objectName = "avatars/current.png";

                    Google.Apis.Storage.v1.Data.Object storageObject;
                    using (var f = File.OpenRead("AVATAR.png"))
                    {
                        objectName = objectName ?? Path.GetFileName("AVATAR.png");
                        try
                        {
                            storageObject = await Program.storageClient.UploadObjectAsync(Program.bucketName, objectName, "image/png", f);
                        } catch (Google.GoogleApiException e)
                        {
                            await msg.ModifyAsync($"<:xmark:314349398824058880> A Google Cloud API error occured during upload! ```\n{e.Message}```");
                            storageObject = null;
                            return;
                        }
                        Console.WriteLine($"Uploaded {objectName}.");
                    }

                    try
                    {
                        storageObject = Program.storageClient.GetObject(Program.bucketName, objectName, new GetObjectOptions() { Projection = Projection.Full });
                    }
                    catch (Google.GoogleApiException e)
                    {
                        await msg.ModifyAsync($"<:xmark:314349398824058880>  A Google Cloud API error occured during object access! ```\n{e.Message}```");
                        storageObject = null;
                        return;
                    }

                    storageObject.Acl.Add(new Google.Apis.Storage.v1.Data.ObjectAccessControl()
                    {
                        Bucket = Program.bucketName,
                        Entity = "allUsers",
                        Role = "READER"
                    });

                    try
                    {
                        var updatedObject = await Program.storageClient.UpdateObjectAsync(storageObject, new UpdateObjectOptions()
                        {
                            IfMetagenerationMatch = storageObject.Metageneration
                        });
                    } catch(Google.GoogleApiException e)
                    {
                        await msg.ModifyAsync($"<:xmark:314349398824058880> A Google Cloud API error occured during object updating! ```\n{e.Message}```");
                        return;
                    }

                    await msg.ModifyAsync("<:check:314349398811475968> Should be done!!");

                }
            }
        }

    }
}
