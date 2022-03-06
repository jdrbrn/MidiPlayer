using System.Text.Json;

namespace MidiPlayer
{
    internal class Config
    {
        public ChannelConfiguration[] OutputChannelConfiguration { get; set; }

        public Config(ChannelConfiguration[] outputChannelConfiguration) 
        {
            OutputChannelConfiguration = outputChannelConfiguration;
        }

        // Returns a default config based around a single channel MidiFile output
        public static Config DefaultConfig()
        {
            return new Config
            (
                new ChannelConfiguration[]
                {
                    new ChannelConfiguration()
                    {
                        OutputModule = "MidiFile",
                        Args = new Dictionary<string, string> { ["FileName"] = "output.mid" }
                    }
                }
            );
        }

        public static void SaveConfig(Config config, string file)
        {
            string jsonString = JsonSerializer.Serialize(config);
            File.WriteAllText(file, jsonString);
        }

        public static Config LoadConfig(string file)
        {
            if (!File.Exists(file)) throw new FileNotFoundException(file);

            string jsonString = File.ReadAllText(file);
            return JsonSerializer.Deserialize<Config>(jsonString)!;
        }
    }
}
