namespace MidiPlayer
{
    internal record StampedEvent
    {
        public int TickStamp;
        public Midi.Event Event;

        public StampedEvent(int tStamp, Midi.Event mEvent)
        {
            TickStamp = tStamp;
            Event = mEvent;
        }
    }
}
