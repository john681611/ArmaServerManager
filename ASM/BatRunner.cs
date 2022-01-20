using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ASM.Lib
{   
    class BatRunner
    {
        
        public static void RunServer(List<string> modIds, string activeServerId, ASMConfig Config, List<string> logStream)
        {   var server = Config.Servers[activeServerId];
            var mods = server.Mods.Where(x => modIds.Contains(x.Key)).Select(x => x.Value).ToList();
            string modsString = string.Join(";", mods.Where(x => !x.ServerSide).Select(x => x.Path));
            string modsServerString = string.Join(";", mods.Where(x => x.ServerSide).Select(x => x.Path));
            var lines = new List<string>{
                $"start {Config.Servers[activeServerId].ServerPath}\\arma3server_x64.exe -mod={modsString} -serverMod={modsServerString} -config={server.ConfigPath} -bepath={server.BattleEyePath} -cfg={server.NetworkConfig} {server.ExtraArgs}"
            };
            RunBat(lines, logStream);
        }

        public static void RunSteamModsUpdate(List<string> modIds, string activeServerId, ASMConfig Config, List<string> logStream)
        {
            var server = Config.Servers[activeServerId];
            var lines = new List<string>();
            foreach (var modId in modIds)
            {
                lines.Add($"{Config.SteamPath}\\steamcmd.exe \"+force_install_dir {server.ServerPath}\\mods\" +login {Config.SteamLogin} +\"workshop_download_item {server.ServerBranch}\" {modId} validate +quit");
            }
            RunBat(lines, logStream);
        }
         public static void RunSteamModInstall(string modId, string folderName, string activeServerId, ASMConfig Config, List<string> logStream)
        {
            var server = Config.Servers[activeServerId];
            var lines = new List<string>{
                $"{Config.SteamPath}\\steamcmd.exe \"+force_install_dir {server.ServerPath}\\mods\" +login {Config.SteamLogin} +\"workshop_download_item {server.ServerBranch}\" {modId} validate +quit"
            };
            RunBat(lines, logStream);
        }

        public static void RunSteamServerUpdate(string activeServerId, ASMConfig Config, List<string> logStream)
        {
            var server = Config.Servers[activeServerId];
            var lines = new List<string>{
                $"{Config.SteamPath}\\steamcmd.exe \"+force_install_dir {server.ServerPath}\" +login {Config.SteamLogin}  +\"app_update {server.ServerBranch}\" validate +quit"
            };
            RunBat(lines, logStream);
        }

        private static async void RunBat(List<string> lines, List<string> logStream)
        {
            string tempFilename = Path.ChangeExtension(Path.GetTempFileName(), ".bat");
            using (StreamWriter writer = new StreamWriter(tempFilename))
            {
                foreach (var line in lines)
                {
                writer.WriteLine(line);
                    
                }
                writer.WriteLine("exit");
            }
            Process process = new Process();
            process.StartInfo.FileName = tempFilename;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.ErrorDataReceived += new DataReceivedEventHandler((sender, e) => logStream.Add($"Error: {e.Data}"));
            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                logStream.Add(await process.StandardOutput.ReadLineAsync());
                var error = await process.StandardError.ReadLineAsync();
                if(!string.IsNullOrEmpty(error))
                    logStream.Add(error);
            }
            await process.WaitForExitAsync();
            File.Delete(tempFilename);
        }
    }
}