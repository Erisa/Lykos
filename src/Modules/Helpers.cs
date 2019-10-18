using DSharpPlus.Entities;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace Lykos.Modules
{
    public class Helpers
    {
        private static readonly ulong dbotsGuildId = 110373943822540800;

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
                // doesnt function correctly
                // TODO: make it function correctly
                fileName = "C:/Windows/system32/cmd.exe";
                arguments = $"/C {escapedArgs} 2>&1";
            }
            else
            {
                // if you dont have bash i apologise
                fileName = "/bin/bash";
                arguments = $"-c \"{escapedArgs} 2>&1\"";
            }


            var proc = new Process
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

        public enum DbotsPermLevel { nothing, botDev, Helper, siteHelper, mod, owner = int.MaxValue }

        public static DbotsPermLevel GetDbotsPerm(DiscordMember target)
        {
            if (target.Guild.Id != dbotsGuildId)
            {
                return DbotsPermLevel.nothing;
            }

            var modRole = target.Guild.GetRole(113379036524212224);
            var fakeMod = target.Guild.GetRole(366668416058130432);
            var helperRole = target.Guild.GetRole(407326634819977217);
            var siteHelperRole = target.Guild.GetRole(598574793712992286);
            var botDevRole = target.Guild.GetRole(110375768374136832);


            if (target.IsOwner)
            {
                return DbotsPermLevel.owner;
            }
            else if (target.Roles.Contains(modRole) || target.Roles.Contains(fakeMod))
            {
                return DbotsPermLevel.mod;
            }
            else if (target.Roles.Contains(siteHelperRole))
            {
                return DbotsPermLevel.siteHelper;
            }
            else if (target.Roles.Contains(helperRole))
            {
                return DbotsPermLevel.Helper;
            }
            else if (target.Roles.Contains(botDevRole))
            {
                return DbotsPermLevel.botDev;
            }
            else
            {
                return DbotsPermLevel.nothing;
            }
        }

        public static Boolean IsDbotsBooster(DiscordMember target)
        {
            var boosterRole = target.Guild.GetRole(585535347753222157);
            return target.Roles.Contains(boosterRole);
        }

        public static string sanitiseEveryone(string input)
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
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("x2"));
            }
            return sb.ToString();
        }

    }
}
