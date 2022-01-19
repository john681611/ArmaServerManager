using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ASM.Lib
{   
    class BatRunner
    {
        
        public static void RunServer(List<string> modIds, ASMConfig Config, List<string> logStream)
        {   
            var mods = Config.Mods.Where(x => modIds.Contains(x.Key)).Select(x => x.Value).ToList();
            string modsString = string.Join(";", mods.Where(x => !x.ServerSide).Select(x => x.Path));
            string modsServerString = string.Join(";", mods.Where(x => x.ServerSide).Select(x => x.Path));
            var lines = new List<string>{
                $"start {Config.ServerPath}\\arma3server_x64.exe -mod={modsString} -serverMod={modsServerString} -config={Config.ConfigPath} -bepath={Config.BattleEyePath} -cfg={Config.NetworkConfig} {Config.ExtraArgs}"
            };
            RunBat(lines, logStream);
        }

        public static void RunSteamModsUpdate(List<string> modIds, ASMConfig Config, List<string> logStream)
        {
            var mods = Config.Mods.Where(x => modIds.Contains(x.Key)).Select(x => x.Value).ToList();

            var lines = new List<string>();
            foreach (var mod in mods)
            {
                lines.Add($"{Config.SteamPath}\\steamcmd.exe \"+force_install_dir {Config.ServerPath}\\mods\" +login {Config.SteamLogin} +\"workshop_download_item {Config.ServerBranch}\" {mod.SteamId} validate +quit");
            }
            RunBat(lines, logStream);
        }

        public static void RunSteamServerUpdate(ASMConfig Config, List<string> logStream)
        {
            var lines = new List<string>{
                $"{Config.SteamPath}\\steamcmd.exe \"+force_install_dir {Config.ServerPath}\" +login {Config.SteamLogin}  +\"app_update {Config.ServerBranch}\" validate +quit"
            };
            RunBat(lines, logStream);
        }

        private static void RunBat(List<string> lines, List<string> logStream)
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
            process.Start();
            while (!process.StandardOutput.EndOfStream)
            {
                logStream.Add(process.StandardOutput.ReadLine());
            }
            process.WaitForExit();
            File.Delete(tempFilename);
        }
    }
}