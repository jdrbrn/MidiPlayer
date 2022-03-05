string inputFile;
MidiPlayer.Config config;

// Dictionary acts as a converter from string to an Output Module
// (Eventually move out to a different class)
// Each delegate function also checks for the arguments needed for each Output Module
Dictionary<string, Func<string[], MidiPlayer.OutputModule.IOutputModule>> outputs = new()
{
    // Takes one argument as the name of the output file
    ["MidiFile"] = new Func<string[], MidiPlayer.OutputModule.IOutputModule>((string[] args) => {
        string outputFile;
        if (args.Length > 1) Console.WriteLine("Warning: Extraneous arguments passed to output module MidiFile, ignoring...");
        if (args.Length > 0) outputFile = args[0];
        else
        {
            Console.WriteLine("Warning: No argument passed to output module MidiFile, using default output file(output.mid)");
            outputFile = "output.mid";
        }
        return new MidiPlayer.OutputModule.MidiFile(outputFile);
    }),
    // Takes 0 arguments
    // Also check if on Windows as ConsoleBeep isn't supported on any other platforms
    ["ConsoleBeep"] = new Func<string[], MidiPlayer.OutputModule.IOutputModule>((string[] args) => {
        if (args.Length > 0) Console.WriteLine("Warning: Extraneous arguments passed to output module ConsoleBeep, ignoring...");
        // Check if running on Windows
        if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)) throw new Exception("Error: Can only use ConsoleBeep output module on Windows");
        return new MidiPlayer.OutputModule.ConsoleBeep();
    }),
    // Needs one argmuent for the GPIO Pin number
    ["PassiveBuzzer"] = new Func<string[], MidiPlayer.OutputModule.IOutputModule>((string[] args) => {
        int pin;
        if (args.Length > 1) Console.WriteLine("Warning: Extraneous arguments passed to output module PassiveBuzzer, ignoring...");
        if (args.Length > 0)
        {
            if (!Int32.TryParse(args[0], out pin)) throw new Exception("Error: invalid arugment \"" + args[0] + "\" for output module PassiveBuzzer");
        }
        else
        {
            throw new Exception("Error: No pin number passed for PassiveBuzzer output module");
        }
        return new MidiPlayer.OutputModule.PassiveBuzzer(pin);
    }),
};

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
        Console.WriteLine("Config File " + args[0] + " not found");
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

// Create output module from the data found in the config file
MidiPlayer.OutputModule.IOutputModule outputModule = outputs[config.OutputChannels[0].OutputModule](config.OutputChannels[0].Args);

Console.WriteLine("Opening file " + inputFile);
byte[] inputData = System.IO.File.ReadAllBytes(inputFile);
Console.WriteLine("Parsing file");
MidiPlayer.MidiParser midiParser = new();
MidiPlayer.ParsedTrack output = midiParser.ParseData(inputData);
Console.WriteLine("Outputting data");
outputModule.Output(output);