using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ASM.Lib
{
    public class ASMConfig
    {
        public string SteamPath { get; set; }
        public string SteamLogin { get; set; }
        public Dictionary<string, Server> Servers { get; set; }
        public string PBOMinify { get; set; }

        [JsonIgnore]
        internal string filePath { get; set; }

        public static ASMConfig Load()
        {
            var path = ASMCore.FindFile("ASMconfig.json");
            using var streamReader = new StreamReader(path);
            string json = streamReader.ReadToEnd();
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
            using var streamReader = new StreamReader(path.Replace("ASMconfig", "ASMTemplates"));
            string json = streamReader.ReadToEnd();
            var generatedServers = JsonConvert.DeserializeObject<Dictionary<string, GeneratedServer>>(json);
            foreach (var server in generatedServers)
            {
                if (!config.Servers.ContainsKey(server.Key))
                    continue;
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
            File.WriteAllText(filePath.Replace("ASMconfig", "ASMTemplates"), json);
        }
    }
}