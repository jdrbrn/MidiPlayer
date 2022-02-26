string inputFile;
MidiPlayer.Output.IOutput outputModule;

Dictionary<string, Func<MidiPlayer.Output.IOutput>> outputs = new Dictionary<string, Func<MidiPlayer.Output.IOutput>>
{
    ["MidiFile"] = new Func<MidiPlayer.Output.IOutput>(() => new MidiPlayer.Output.MidiFile("output.mid")),
    ["ConsoleBeep"] = new Func<MidiPlayer.Output.IOutput>(() => new MidiPlayer.Output.ConsoleBeep()),
    ["PassiveBuzzer"] = new Func<MidiPlayer.Output.IOutput>(() => new MidiPlayer.Output.PassiveBuzzer(18))
};

if (args.Count() < 2)
{
    Console.WriteLine("Usage: MidiPlayer.dll [output] [path to midi]");
    return;
}
else if (!outputs.ContainsKey(args[0]))
{
    Console.WriteLine("Invalid output");
    Console.Write("Options are: ");
    foreach (string option in outputs.Keys) Console.Write(option + " ");
    Console.WriteLine();
    return;
}
else
{
    inputFile = args[1];
    outputModule = outputs[args[0]]();
}

Console.WriteLine("Opening file input.mid");
if (!File.Exists(inputFile))
{
    Console.WriteLine("File " + inputFile + " not found");
    return;
}
Console.WriteLine("File Found");
Console.WriteLine("Reading File");
byte[] inputData = System.IO.File.ReadAllBytes(inputFile);
Console.WriteLine("File Read");
Console.WriteLine("Initializing IO");
MidiPlayer.MidiParser midiParser = new();
MidiPlayer.ParsedTrack output = midiParser.ParseData(inputData);
outputModule.Output(output);