using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace Lykos.Modules
{
    public class Helpers
    {
        static ulong dbotsGuildId = 110373943822540800;

        public static OSPlatform GetOSPlatform()
        {
            OSPlatform osPlatform = OSPlatform.Create("Other Platform");
            // Check if it's windows 
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            osPlatform = isWindows ? OSPlatform.Windows : osPlatform;
            // Check if it's osx 
            bool isOSX = RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
            osPlatform = isOSX ? OSPlatform.OSX : osPlatform;
            // Check if it's Linux 
            bool isLinux = RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
            osPlatform = isLinux ? OSPlatform.Linux : osPlatform;
            return osPlatform;
        }

        public static ShellResult runShellCommand(String command)
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

        public enum dbotsPermLevel { nothing, botDev, Helper, SiteHelper, Mod, Owner = int.MaxValue }

        public static dbotsPermLevel getDbotsPerm(DiscordMember target)
        {
            if (target.Guild.Id != dbotsGuildId)
            {
                return dbotsPermLevel.nothing;
            }

            var modRole = target.Guild.GetRole(113379036524212224);
            var fakeMod = target.Guild.GetRole(366668416058130432);
            var helperRole = target.Guild.GetRole(407326634819977217);
            var siteHelperRole = target.Guild.GetRole(598574793712992286);
            var botDevRole = target.Guild.GetRole(110375768374136832);

            if (target.IsOwner)
            {
                return dbotsPermLevel.Owner;
            }
            else if (target.Roles.Contains(modRole) || target.Roles.Contains(fakeMod))
            {
                return dbotsPermLevel.Mod;
            }
            else if (target.Roles.Contains(siteHelperRole))
            {
                return dbotsPermLevel.SiteHelper;
            }
            else if (target.Roles.Contains(helperRole))
            {
                return dbotsPermLevel.Helper;
            }
            else if (target.Roles.Contains(botDevRole))
            {
                return dbotsPermLevel.botDev;
            }
            else
            {
                return dbotsPermLevel.nothing;
            }
        }

        public static Boolean isDbotsBooster(DiscordMember target)
        {
            var boosterRole = target.Guild.GetRole(585535347753222157);
            return target.Roles.Contains(boosterRole);
        }

    }
}
