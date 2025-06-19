using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ASM.Lib
{
    class BatRunner
    {

        public static void RunServer(
            List<string> modIds,
            string activeServerId,
            ASMConfig Config,
            List<string> logStream,
            bool ignoreOptionalKeys,
            bool pauseOnFinish,
            string mission)
        {
            var server = Config.Servers[activeServerId];
            var mods = server.Mods.Where(x => modIds.Contains(x.Key)).Select(x => x.Value).ToList();
            string modsString = string.Join(";", mods.Where(x => !x.ServerSide).Select(x => x.Path));
            string modsServerString = string.Join(";", mods.Where(x => x.ServerSide).Select(x => x.Path));
            var di = new DirectoryInfo($"{server.ServerPath}\\keys");
            foreach (var file in di.GetFiles())
            {
                if (file.Name != "a3.bikey")
                    file.Delete();
            }
            var lines = new List<string> { $"del /q {server.ServerPath}\\keys\\*.*" };
            var keyFolders = FindKeyFolders(mods);
            foreach (var keyFolder in keyFolders)
            {
                lines.Add($"xcopy \"{keyFolder}\" \"{server.ServerPath}\\keys\" /C /y");
            }
            if (!string.IsNullOrEmpty(server.OptKeysPath) && !ignoreOptionalKeys)
                lines.Add($"xcopy  \"{server.OptKeysPath}\" \"{server.ServerPath}\\keys\" /C /y");

            var config = server.ConfigPath;
            if (!string.IsNullOrEmpty(mission))
                config = SetupMissionConfig(mission, server.TemplateConfigPath);
            lines.Add($"start {server.ServerPath}\\arma3server_x64.exe \"-mod={modsString}\" \"-serverMod={modsServerString}\" -config={config} -bepath={server.BattleEyePath} -cfg={server.NetworkConfig} {server.ExtraArgs}");
            RunBat(lines, logStream, pauseOnFinish);
        }

        public static void RunSteamModsUpdate(List<string> modIds, string activeServerId, bool deleteBeforeUpdate, bool pauseOnFinish, ASMConfig Config, List<string> logStream)
        {
            var server = Config.Servers[activeServerId];
            var lines = new List<string>();
            foreach (var modId in modIds)
            {
                if (deleteBeforeUpdate)
                {
                    lines.Add($"rmdir /s /q \"{server.ServerPath}\\Mods\\steamapps\\workshop\\content\\107410\\{modId}\"");
                }
                lines.Add($"{Config.SteamPath}\\steamcmd.exe \"+force_install_dir {server.ServerPath}\\mods\" +login {Config.SteamLogin} +\"workshop_download_item {server.ClientBranch}\" \"{modId}\" validate +quit");
                AddMinifyLines(Config, server, ref lines, modId);
            }
            RunBat(lines, logStream, pauseOnFinish);
        }
        public static void RunSteamModInstall(string modId, string folderName, string activeServerId, bool pauseOnFinish, ASMConfig Config, List<string> logStream)
        {
            var server = Config.Servers[activeServerId];
            var lines = new List<string>{
                $"{Config.SteamPath}\\steamcmd.exe \"+force_install_dir {server.ServerPath}\\mods\" +login {Config.SteamLogin} +\"workshop_download_item {server.ClientBranch}\" \"{modId}\" validate +quit",
                $"mklink /D \"{server.ServerPath}\\mods\\{folderName}\" \"{server.ServerPath}\\Mods\\steamapps\\workshop\\content\\107410\\{modId}\""
            };
            AddMinifyLines(Config, server, ref lines, modId);
            RunBat(lines, logStream, pauseOnFinish);
        }

        public static void RunSteamMultiModInstall(Dictionary<string, string> modMap, string activeServerId, bool pauseOnFinish, ASMConfig Config, List<string> logStream)
         {
            var server = Config.Servers[activeServerId];
            var lines = new List<string>();

            foreach (var mod in modMap)
            {
                lines.Add($"{Config.SteamPath}\\steamcmd.exe \"+force_install_dir {server.ServerPath}\\mods\" +login {Config.SteamLogin} +\"workshop_download_item {server.ClientBranch}\" \"{mod.Key}\" validate +quit");
                lines.Add($"mklink /D \"{server.ServerPath}\\mods\\{mod.Value}\" \"{server.ServerPath}\\Mods\\steamapps\\workshop\\content\\107410\\{mod.Key}\"");
                AddMinifyLines(Config, server, ref lines, mod.Key);
            }
            RunBat(lines, logStream, pauseOnFinish);
        }

        public static void RunSteamServerUpdate(string activeServerId, bool pauseOnFinish, ASMConfig Config, List<string> logStream)
        {
            var server = Config.Servers[activeServerId];
            var lines = new List<string>{
                $"{Config.SteamPath}\\steamcmd.exe \"+force_install_dir {server.ServerPath}\" +login {Config.SteamLogin}  +\"app_update {server.ServerBranch}\" validate +quit"
            };
            RunBat(lines, logStream, pauseOnFinish);
        }

        private static async void RunBat(List<string> lines, List<string> logStream, bool pauseOnFinish)
        {
            logStream.Add("------Running .bat script------");
            logStream.AddRange(lines);
            logStream.Add("------------------------------");
            string tempFilename = Path.ChangeExtension(Path.GetTempFileName(), ".bat");
            using (StreamWriter writer = new StreamWriter(tempFilename))
            {
                foreach (var line in lines)
                {
                    writer.WriteLine(line);
                }
                if (pauseOnFinish)
                    writer.WriteLine("pause");
                writer.WriteLine("exit");
            }
            Process process = new Process();
            process.StartInfo.FileName = tempFilename;
            process.Start();
            await process.WaitForExitAsync();
            File.Delete(tempFilename);
        }

        private static List<string> FindKeyFolders(List<Mod> mods)
        {
            var modFolders = new List<string>();
            foreach (var mod in mods)
            {
                var directories = new DirectoryInfo(mod.Path).GetDirectories();
                if (directories.Any(x => x.Name.ToLower().Contains("key")))
                    modFolders.Add(directories.First(x => x.Name.ToLower().Contains("key")).FullName);
            }
            return modFolders;
        }

        private static string SetupMissionConfig(string mission, string templateFilePath)
        {
            var path = string.IsNullOrEmpty(templateFilePath) ? ASMCore.FindFile("ASMMission.cfg") : templateFilePath;
            using var streamReader = new StreamReader(path);
            string configTemplate = streamReader.ReadToEnd();
            configTemplate = configTemplate.Replace("$TEMPLATE$", mission.Replace(".pbo", ""));

            var filePath = Path.ChangeExtension(Path.GetTempFileName(), ".cfg");
            File.WriteAllText(filePath, configTemplate);
            return filePath;
        }

        private static void AddMinifyLines(ASMConfig Config, Server server, ref List<string> lines, string modId)
        {
            if (!string.IsNullOrEmpty(Config.PBOMinify))
            {
                lines.Add($"""
                    {Config.PBOMinify}\pbo_minify.exe "{server.ServerPath}\Mods\steamapps\workshop\content\107410\{modId}\Addons" "{Config.PBOMinify}\Addons"
                    copy /Y "{Config.PBOMinify}\Addons\*.*" "{server.ServerPath}\Mods\steamapps\workshop\content\107410\{modId}\Addons"
                    del /s /q "{Config.PBOMinify}\Addons\*.*"
                """);
            }
        }
    }
}