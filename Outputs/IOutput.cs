namespace MidiPlayer.Outputs
{
    internal interface IOutput
    {
        public void Output(MidiPlayer.ParsedTrack track);
    }
}
