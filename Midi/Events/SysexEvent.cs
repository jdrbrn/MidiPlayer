namespace MidiPlayer.Midi
{
    internal record SysexEvent : Midi.Event
    {
        public int Length;

        public SysexEvent(byte id, int length, byte[] data) : base(id, data)
        {
            Length = length;
        }
    }
}
