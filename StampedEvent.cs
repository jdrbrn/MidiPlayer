namespace MidiPlayer
{
    internal record StampedEvent
    {
        public int TickStamp;
        public Midi.Event Event;

        // TickStamp is the timestamp for the Midi Event in Midi ticks
        // Only is accurate to a single Midi file
        public StampedEvent(int tStamp, Midi.Event mEvent)
        {
            TickStamp = tStamp;
            Event = mEvent;
        }
    }
}
