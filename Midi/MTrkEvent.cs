namespace MidiPlayer.Midi
{
    internal record MTrkEvent
    {
        public int DeltaTime;
        public Midi.Event Event;

        public MTrkEvent(int dTime, Midi.Event mEvent)
        {
            DeltaTime = dTime;
            Event = mEvent;
        }
    }
}
