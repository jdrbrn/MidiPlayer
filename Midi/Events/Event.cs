namespace MidiPlayer.Midi
{
    internal record Event
    {
        public byte StatusID { get;  }
        public byte[] Data { get; }

        public Event(byte id, byte[] data)
        {
            StatusID = id;
            Data = data;
        }
    }
}
