/*
==========================================================================
This file is part of Briefing Room for DCS World, a mission
generator for DCS World, by @akaAgar (https://github.com/akaAgar/briefing-room-for-dcs)

Briefing Room for DCS World is free software: you can redistribute it
and/or modify it under the terms of the GNU General Public License
as published by the Free Software Foundation, either version 3 of
the License, or (at your option) any later version.

Briefing Room for DCS World is distributed in the hope that it will
be useful, but WITHOUT ANY WARRANTY; without even the implied warranty
of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Briefing Room for DCS World. If not, see https://www.gnu.org/licenses/
==========================================================================
*/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ASL.Lib
{
    public sealed class ASLCore
    {
        public const string VERSION = "0.0.1";
        public const string REPO_URL = "https://github.com/john681611/ArmaServerLauncher";
        public Config Config {get; private set;}

        public const string BUILD_VERSION = "~BUILD_VERSION~";
        public ASLCore()
        {
            Config = LoadConfig();
        }

        private Config LoadConfig()
        {
            using (StreamReader r = new StreamReader("../config.json"))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<Config>(json);
            }
        }

        public void RunServer(List<Mod> mods)
        {
            string modsString = string.Join(";", mods.Where(x => !x.ServerSide ).Select(x => x.Path));
             string modsServerString = string.Join(";", mods.Where(x => x.ServerSide ).Select(x => x.Path));
            string tempFilename = Path.ChangeExtension(Path.GetTempFileName(), ".bat");
            using (StreamWriter writer = new StreamWriter(tempFilename))
            {
                writer.WriteLine($"start {Config.ServerPath}\\arma3server_x64.exe -mod={modsString} -serverMod={modsServerString} -config={Config.ConfigPath} -bepath={Config.BattleEyePath} -cfg={Config.NetworkConfig} {Config.ExtraArgs}");
                writer.WriteLine("exit");
            }
            Process process = Process.Start(tempFilename);
            process.WaitForExit();
            File.Delete(tempFilename);
        }

        public void RunSteamModsUpdate(List<Mod> mods)
        {
            string tempFilename = Path.ChangeExtension(Path.GetTempFileName(), ".bat");
            using (StreamWriter writer = new StreamWriter(tempFilename))
            {
                foreach (var mod in mods)
                {
                    writer.WriteLine($"{Config.SteamPath}\\steamcmd.exe \"+force_install_dir {Config.ServerPath}\\mods\" +login {Config.SteamLogin} +\"workshop_download_item {Config.ServerBranch}\" {mod.SteamId} validate +quit");
                }
                writer.WriteLine("exit");
            }
            Process process = Process.Start(tempFilename);
            process.WaitForExit();
            File.Delete(tempFilename);
        }

        public void RunSteamServerUpdate()
        {
            string tempFilename = Path.ChangeExtension(Path.GetTempFileName(), ".bat");
            using (StreamWriter writer = new StreamWriter(tempFilename))
            {

                writer.WriteLine($"{Config.SteamPath}\\steamcmd.exe \"+force_install_dir {Config.ServerPath}\" +login {Config.SteamLogin}  +\"app_update {Config.ServerBranch}\" validate +quit");
                writer.WriteLine("exit");
            }
            Process process = Process.Start(tempFilename);
            process.WaitForExit();
            File.Delete(tempFilename);
        }
    }
}
