using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ASM.Lib
{   
    class BatRunner
    {
        
        public static void RunServer(List<string> modIds, ASMConfig Config)
        {   
            var mods = Config.Mods.Where(x => modIds.Contains(x.Key)).Select(x => x.Value).ToList();
            string modsString = string.Join(";", mods.Where(x => !x.ServerSide).Select(x => x.Path));
            string modsServerString = string.Join(";", mods.Where(x => x.ServerSide).Select(x => x.Path));
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

        public static void RunSteamModsUpdate(List<string> modIds, ASMConfig Config)
        {
            var mods = Config.Mods.Where(x => modIds.Contains(x.Key)).Select(x => x.Value).ToList();
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

        public static void RunSteamServerUpdate(ASMConfig Config)
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