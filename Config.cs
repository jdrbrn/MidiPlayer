using System.Text.Json;

namespace MidiPlayer
{
    internal class Config
    {
        public OutputChannel[] OutputChannels { get; set; }

        public Config(OutputChannel[] outputChannels) 
        {
            OutputChannels = outputChannels;
        }

        // Returns a default config based around a single channel MidiFile output
        public static Config DefaultConfig()
        {
            return new Config
            (
                new OutputChannel[]
                {
                    new OutputChannel()
                    {
                        OutputModule = "MidiFile",
                        Args = new string[] { "output.mid" }
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

        public struct OutputChannel
        {
            public string OutputModule { get; set; }
            public string[] Args { get; set; }
        }
    }
}
