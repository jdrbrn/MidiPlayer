string inputFile;

if (args.Count() > 0) inputFile = args[0];
else
{
    Console.WriteLine("Run with argument to midi file");
    return;
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
MidiPlayer.Outputs.IOutput outputModule = new MidiPlayer.Outputs.ConsoleBeep();
MidiPlayer.MidiParser midiParser = new();
MidiPlayer.ParsedTrack output = midiParser.ParseData(inputData);
outputModule.Output(output);