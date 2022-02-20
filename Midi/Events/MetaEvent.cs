namespace MidiPlayer.Midi
{
    internal record MetaEvent : Midi.Event
    {

        public byte Type;
        public int Length;

        public MetaEvent(byte id, byte type, int length, byte[] data) : base(id, data)
        {
            Type = type;
            Length = length;
        }
    }
}
