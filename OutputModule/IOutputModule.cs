namespace MidiPlayer.OutputModule
{
    internal interface IOutputModule
    {
        public void Output(MidiPlayer.ParsedTrack track);
    }
}
