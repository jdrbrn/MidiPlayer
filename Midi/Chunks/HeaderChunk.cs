namespace MidiPlayer.Midi
{
    internal class HeaderChunk : Chunk
    {
        public MidiFileFormat Format;
        public short Ntrks;
        public byte[] RawDivision;


        public HeaderChunk(string header, uint length, byte[] data) : base(header, length, data)
        {
            if (header != "MThd") throw new Exception("Invalid: Setting non-header chunk to header chunk");

            // Format, ntrks, division stored as 16 bit words, in that order, in data section

            // Get Format
            // Get the bytes representing the format of the chunk
            byte[] formatBytes = new byte[2];
            Array.Copy(data, 0, formatBytes, 0, 2);
            // Reverse byte order if little endian (files are stored big)
            if (BitConverter.IsLittleEndian) Array.Reverse(formatBytes);
            // Convert bytes to useable uint
            short rawFormat = BitConverter.ToInt16(formatBytes);
            if (rawFormat >= 0 && rawFormat <= 3) Format = (MidiFileFormat)rawFormat;
            else Format = MidiFileFormat.Unknown;

            // Get ntrks
            // Get the bytes representing ntrks
            byte[] ntrksBytes = new byte[2];
            Array.Copy(data, 2, ntrksBytes, 0, 2);
            // Reverse byte order if little endian (files are stored big)
            if (BitConverter.IsLittleEndian) Array.Reverse(ntrksBytes);
            // Convert bytes to useable uint
            Ntrks = BitConverter.ToInt16(ntrksBytes);

            // Get division
            // Just copies raw data for now
            RawDivision = new byte[2];
            Array.Copy(data, 4, RawDivision, 0, 2);
        }
    }
}
