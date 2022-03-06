string inputFile;
MidiPlayer.Config config;

// If either too few or too many arguments print help test
if (args.Length < 1 || args.Length > 2)
{
    Console.WriteLine("Usage: MidiPlayer.dll midi_path [config_path]");
    return;
}

// Check if Midi input file exists
if (!File.Exists(args[0]))
{
    Console.WriteLine("Midi File " + args[0] + " not found");
    return;
}
else inputFile = args[0];

// Check if config file passed via argument
if (args.Length==2)
{
    // Check if config file exists
    if (!File.Exists(args[1]))
    {
        Console.WriteLine("Config File " + args[1] + " not found");
        return;
    }
    else config = MidiPlayer.Config.LoadConfig(args[1]);
}
// If no config file argument check for a config.json in same directory
else
{
    // If config file doesn't exist then output some help text
    if (!File.Exists("config.json"))
    {
        Console.WriteLine("No config file passed in and config file not found!");
        Console.WriteLine("Creating config.json with default values.");
        Console.WriteLine("Please edit and restart program");
        MidiPlayer.Config.SaveConfig(MidiPlayer.Config.DefaultConfig(), "config.json");
        return;
    }
    else config = MidiPlayer.Config.LoadConfig("config.json");
}
Console.WriteLine("Opening file " + inputFile);
byte[] inputData = System.IO.File.ReadAllBytes(inputFile);
Console.WriteLine("Parsing file");
MidiPlayer.MidiParser midiParser = new();
MidiPlayer.ParsedTrack output = midiParser.ParseData(inputData);
Console.WriteLine("Outputting data");
var chan = new MidiPlayer.OutputChannel(config.OutputChannelConfiguration[0], output);
chan.Output();