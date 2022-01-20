using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ASM.Lib
{
    public class ASMConfig
    {
        public string SteamPath { get; set; }
        public string SteamLogin { get; set; }
        public Dictionary<string, Server> Servers { get; set; }

        [JsonIgnore]
        internal string filePath { get; set; }

        public static ASMConfig Load(string path = "")
        {
            if (string.IsNullOrEmpty(path))
                path = AppDomain.CurrentDomain.BaseDirectory;
            path = Path.GetFullPath(path);
            DirectoryInfo di = new DirectoryInfo(path);
            if (!di.GetFiles().Any(x => x.Name == "ASMconfig.json"))
                return Load(path + "/..");
            path = $"{path}/ASMconfig.json";
            StreamReader r = new StreamReader(path);
            string json = r.ReadToEnd();
            var config = JsonConvert.DeserializeObject<ASMConfig>(json);
            config.filePath = path;

            config = LoadGenerated(path, config);
            foreach (var server in config.Servers)
            {
                server.Value.Load();
            }
            return config;

        }

        private static ASMConfig LoadGenerated(string path, ASMConfig config)
        {
            StreamReader r = new StreamReader(path.Replace("ASMconfig", "ASMgenerated"));
            string json = r.ReadToEnd();
            var generatedServers = JsonConvert.DeserializeObject<Dictionary<string, GeneratedServer>>(json);
            foreach (var server in generatedServers)
            {
                if (config.Servers.ContainsKey(server.Key))
                    continue;
                config.Servers[server.Key].Missions = server.Value.Missions;
                config.Servers[server.Key].Mods = server.Value.Mods;
                config.Servers[server.Key].Templates = server.Value.Templates;
            }
            return config;
        }

        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, formatting: Formatting.Indented);
            File.WriteAllText(filePath, json);
            SaveGenerated();
        }

        private void SaveGenerated()
        {
            var generatedServers = new Dictionary<string, GeneratedServer>();
            foreach (var server in Servers)
            {
                generatedServers.Add(server.Key, server.Value.GetGeneratedServer());
            }
            var json = JsonConvert.SerializeObject(generatedServers, formatting: Formatting.Indented);
            File.WriteAllText(filePath.Replace("ASMconfig", "ASMgenerated"), json);
        }

    }
}