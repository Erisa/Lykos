using DSharpPlus.Entities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Lykos.Modules
{
    public class Helpers
    {
        readonly static string[] validExts = { "default", "png or gif", "gif", "png", "jpg", "webp" };

        public static OSPlatform GetOSPlatform()
        {
            // Default to "Unknown" platform.
            OSPlatform osPlatform = OSPlatform.Create("Unknown");

            // Check if it's windows 
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            osPlatform = isWindows ? OSPlatform.Windows : osPlatform;
            // Check if it's osx 
            bool isOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            osPlatform = isOSX ? OSPlatform.OSX : osPlatform;
            // Check if it's Linux 
            bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            osPlatform = isLinux ? OSPlatform.Linux : osPlatform;
            // Check if it's FreeBSD
            bool isBSD = RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD);
            osPlatform = isBSD ? OSPlatform.FreeBSD : osPlatform;
            return osPlatform;
        }

        public static ShellResult RunShellCommand(String command)
        {
            string fileName;
            string arguments;

            string escapedArgs = command.Replace("\"", "\\\"");
            if (GetOSPlatform() == OSPlatform.Windows)
            {
                fileName = Environment.GetEnvironmentVariable("COMSPEC");
                // this shouldnt ever fail but so many people have cursed setups
                if (!System.IO.File.Exists(fileName))
                {
                    fileName = "C:\\Windows\\system32\\cmd.exe";
                }
                arguments = $"/C \"{escapedArgs}\"";
            }
            else
            {
                fileName = Environment.GetEnvironmentVariable("SHELL");
                if (!System.IO.File.Exists(fileName))
                {
                    fileName = "/bin/sh";
                }
                arguments = $"-c \"{escapedArgs} 2>&1\"";
            }


            Process proc = new()
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                }
            };

            proc.Start();
            string result = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();

            return new ShellResult(proc, result);

        }

        public struct ShellResult
        {
            public Process proc;
            public String result;

            public ShellResult(Process proce, String res)
            {
                proc = proce;
                result = res;
            }
        }

        public static string SanitiseEveryone(string input)
        {
            return input.Replace("@everyone", "@\u200Beveryone").Replace("@here", "@\u200Bhere");
        }

        public static string CalculateMD5Hash(string input)
        {
            // step 1, calculate MD5 hash from input
            MD5 md5 = System.Security.Cryptography.MD5.Create();
            byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // step 2, convert byte array to hex string
            StringBuilder sb = new();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

        public static string MemberAvatarURL(DiscordMember member, string format = "default", int size = 4096)
        {
            var hash = member.GuildAvatarHash;
            if (hash == null)
                return member.DefaultAvatarUrl;

            if (format == "default" || format == "png or gif")
            {
                format = hash.StartsWith("a_") ? "gif" : "png";
            }
            else if (!validExts.Any(format.Contains))
            {
                throw new ArgumentException("You supplied an invalid format, " +
                    "either give none or one of the following: `gif`, `png`, `jpg`, `webp`");
            }
            else if (format == "gif" && !hash.StartsWith("a_"))
            {
                throw new ArgumentException("The format of `gif` only applies to animated avatars.\n" +
                    "The user you are trying to lookup does not have an animated avatar.");
            }

            if (member.GuildAvatarHash != member.AvatarHash)
                return $"https://cdn.discordapp.com/guilds/{member.Guild.Id}/users/{member.Id}/avatars/{hash}.{format}?size=4096";
            else
                return $"https://cdn.discordapp.com/avatars/{member.Id}/{member.AvatarHash}.{format}?size=4096";
        }

        public static async Task<string> UserOrMemberAvatarURL(DiscordUser user, DiscordGuild guild, string format = "default", int size = 4096)
        {
            if (!validExts.Any(format.Contains))
            {
                throw new ArgumentException("You supplied an invalid format, " +
                    "either give none or one of the following: `gif`, `png`, `jpg`, `webp`");
            }

            try
            {
                return MemberAvatarURL(await guild.GetMemberAsync(user.Id), format);
            } catch (DSharpPlus.Exceptions.NotFoundException)
            {
                string hash = user.AvatarHash;

                if (hash == null)
                    return user.DefaultAvatarUrl;

                if (format == "default" || format == "png or gif")
                {
                    format = hash.StartsWith("a_") ? "gif" : "png";
                }
                else if (format == "gif" && !hash.StartsWith("a_"))
                {
                    throw new ArgumentException("The format of `gif` only applies to animated avatars.\n" +
                        "The user you are trying to lookup does not have an animated avatar.");
                }

                return $"https://cdn.discordapp.com/avatars/{user.Id}/{user.AvatarHash}.{format}?size=4096";
            }

        }

    }
}
