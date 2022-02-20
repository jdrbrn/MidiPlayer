using MidiPlayer.Midi;

namespace MidiPlayer
{
    internal class MidiData
    {
        public Midi.HeaderChunk Header;
        public List<Midi.TrackChunk> TrackChunks;
        public List<Midi.Chunk> OtherChunks;

        public MidiData(byte[] data)
        {
            TrackChunks = new List<Midi.TrackChunk>();
            OtherChunks = new List<Midi.Chunk>();
            Header = GetHeader(data);
            GetChunks(data);
            HeaderCheck();
            ParseTracks();
        }

        private Midi.HeaderChunk GetHeader(byte[] input)
        {
            // Get the chunk header from the raw bytes
            char[] chunkHeaderBytes = new char[4];
            Array.Copy(input, 0, chunkHeaderBytes, 0, 4);
            string chunkHeader = new string(chunkHeaderBytes);
            // Get the bytes representing the length of the chunk
            byte[] lengthBytes = new byte[4];
            Array.Copy(input, 0 + 4, lengthBytes, 0, 4);
            // Reverse byte order if little endian (files are stored big)
            if (BitConverter.IsLittleEndian) Array.Reverse(lengthBytes);
            // Convert bytes to useable uint
            uint length = BitConverter.ToUInt32(lengthBytes);
            // Copy the chunk data
            byte[] data = new byte[length];
            Array.Copy(input, 0 + 4 + 4, data, 0, (int)length);
            // Create the appropriate type of chunk
            if (chunkHeader == "MThd") return new Midi.HeaderChunk(chunkHeader, length, data);
            else throw new Exception("Error: No Header found in Midi File");
        }

        private void GetChunks(byte[] input)
        {
            // Parse chunks out of the data
            // Start after the header chunk (header, length, data)
            for (int i = 4 + 4 + (int)Header.Length; i < input.Length;)
            {
                // Get the chunk header from the raw bytes
                char[] chunkHeaderBytes = new char[4];
                Array.Copy(input, i, chunkHeaderBytes, 0, 4);
                string chunkHeader = new string(chunkHeaderBytes);
                // Get the bytes representing the length of the chunk
                byte[] lengthBytes = new byte[4];
                Array.Copy(input, i + 4, lengthBytes, 0, 4);
                // Reverse byte order if little endian (files are stored big)
                if (BitConverter.IsLittleEndian) Array.Reverse(lengthBytes);
                // Convert bytes to useable uint
                uint length = BitConverter.ToUInt32(lengthBytes);
                // Copy the chunk data
                byte[] data = new byte[length];
                Array.Copy(input, i + 4 + 4, data, 0, (int)length);
                // Create the appropriate type of chunk
                if (chunkHeader == "MThd")
                {
                    if (Header == null) Header = new Midi.HeaderChunk(chunkHeader, length, data);
                    else throw new Exception("Invalid Midi file; Multiple header chunks found");
                }
                else if (chunkHeader == "MTrk") TrackChunks.Add(new Midi.TrackChunk(chunkHeader, length, data));
                else OtherChunks!.Add(new Midi.Chunk(chunkHeader, length, data));
                // Move to next chunk (index + header length + size length + data length)
                i = i + 4 + 4 + (int)length;
            }
            Console.WriteLine("Parsed Chunks");
        }

        private void HeaderCheck()
        {
            Midi.HeaderChunk header = Header!;
            if (header.Ntrks != TrackChunks.Count) Console.WriteLine($"Warning: Header declared {header.Ntrks} tracks, but found {TrackChunks.Count}");
            if (header.Format == Midi.MidiFileFormat.FileFormat2) throw new Exception("File Format 2 not supported");
        }

        private void ParseTracks()
        {
            foreach (TrackChunk track in TrackChunks) track.ParseData();
        }
    }
}