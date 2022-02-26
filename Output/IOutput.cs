namespace MidiPlayer.Output
{
    internal interface IOutput
    {
        public void Output(MidiPlayer.ParsedTrack track);
    }
}
