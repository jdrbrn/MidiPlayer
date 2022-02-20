namespace MidiPlayer.Midi
{
    internal class Chunk
    {
        public string Header;
        public uint Length;
        public byte[] Data;

        public Chunk(string header, uint length, byte[] data)
        {
            if (header.Length == 4) Header = header;
            else throw new ArgumentException("Invalid header size");
            Length = length;
            if (data.Length == Length) Data = data;
            else throw new ArgumentException("Invalid chunk data size(Doesn't match Length)");
        }
    }
}
