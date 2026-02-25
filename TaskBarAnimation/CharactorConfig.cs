using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace DonchanOverlay
{
    public class CharacterEntry
    {
        public int id { get; set; }
        public string name { get; set; } = "";
    }

    public class CharacterConfig
    {
        public List<CharacterEntry> characters { get; set; } = new();

        public static CharacterConfig Load(string baseDir)
        {
            string configPath = Path.Combine(baseDir, "Flames", "config.json");

            if (!File.Exists(configPath))
            {
                return new CharacterConfig();
            }

            string json = File.ReadAllText(configPath);
            var cfg = JsonSerializer.Deserialize<CharacterConfig>(json);
            return cfg ?? new CharacterConfig();
        }
    }
}