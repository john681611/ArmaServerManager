using System.Collections.Generic;

namespace ASL.Lib
{
    public class Config {
        public Dictionary<string, Mod> Mods {get; set;}
        public string ServerExe {get; set;}
        public string ConfigPath {get; set;}
        public string NetworkConfig {get; set;}
        public string BattleEyePath {get; set;}
        public string ExtraArgs {get; set;}

        public string SteamPath {get; set;}
        public string SteamLogin { get; internal set; }
        public string ServerBranch { get; internal set; }
    }
}