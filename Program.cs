string inputFile = @"../../../input.mid";
Console.WriteLine("Opening file input.mid");
if (!File.Exists(inputFile))
{
    Console.WriteLine("File doesn't exist");
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