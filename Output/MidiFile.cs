namespace MidiPlayer.Output
{
    internal class MidiFile : IOutput
    {
        readonly string outputFile;

        public MidiFile(string outputLocation)
        {
            outputFile = outputLocation;
        }

        public void Output(ParsedTrack track)
        {
            List<byte> outputData = new();

            // Keep track of timing
            double time;
            // Midi's default tempo so we shouldn't need to specify in the file
            int tempo = 500000;
            // Division value we're setting in the header
            int division = 96;

            // Magic Header data for a type 0 Midi file with 1 track and setting division to 16
            outputData.AddRange(new byte[] { 0x4d, 0x54, 0x68, 0x64, 0x00, 0x00, 0x00, 0x06, 0x00, 0x00, 0x00, 0x01, 0x00, (byte)division });

            // Create the main segment of the track based off the note data
            List<byte> trackData = new();

            // Add info to play first note
            // Send Delta-Time of when to start
            // Need to convert from milliseconds to delta-t ticks reversing the equation in Midi File specs and then encode to variable length
            trackData.AddRange(MidiFile.EncodeVaribleLength((uint)(track.Notes[0].TimeStamp * 1000.0 * division / tempo)));
            // Advance time to playback timestamp
            time = track.Notes[0].TimeStamp;
            // Add Note On for channel 1, Note ID, Velocity of 64
            trackData.AddRange(new byte[] { 0x90, (byte)track.Notes[0].NoteNum, 64 });
            // status IDs can be skipped now by using running status to play/stop notes
            // Turn off first note
            // Add time note is held to clock
            time += track.Notes[0].Length;
            // Add length from data transformed into Delta-T ticks and encoded into variable length
            trackData.AddRange(MidiFile.EncodeVaribleLength((uint)(track.Notes[0].Length * 1000.0 * division / tempo)));
            // Add Note ID, and velocity of 0
            trackData.AddRange(new byte[] { (byte)track.Notes[0].NoteNum, 0 });

            // Add info to play rest of notes using running status
            for (int i = 1; i < track.Notes.Count; i++)
            {
                // Calculate delay before playing next note
                double delay = 0;
                // If current time hasn't reached time to play note need to set deltaT, otherwise leave at 0
                if (time < track.Notes[i].TimeStamp) delay = track.Notes[i].TimeStamp - time;
                // Advance time to playback timestamp
                time = track.Notes[i].TimeStamp;

                // Add delay as Delta-T ticks for time to play
                trackData.AddRange(MidiFile.EncodeVaribleLength((uint)(delay * 1000.0 * division / tempo)));
                // Add Note ID, Velocity of 64
                trackData.AddRange(new byte[] { (byte)track.Notes[i].NoteNum, 64 });
                // Add length from data transformed into Delta-T ticks and encoded into variable length
                trackData.AddRange(MidiFile.EncodeVaribleLength((uint)(track.Notes[i].Length * 1000.0 * division / tempo)));
                // Add Note ID, and velocity of 0
                trackData.AddRange(new byte[] { (byte)track.Notes[i].NoteNum, 0 });
                // Add time held to clock
                time += track.Notes[i].Length;
            }
            // Add end of track marker
            trackData.AddRange(new byte[] { 0x00, 0xff, 0x2f, 0x00 });

            // Create Header data for track
            List<byte> trackHeader = new();
            // Add header ID
            trackHeader.AddRange(new byte[] { (byte)'M', (byte)'T', (byte)'r', (byte)'k' });
            // Get data length
            byte[] trackLength = BitConverter.GetBytes(trackData.Count);
            // Reverse byte order if little endian (files are stored big)
            if (BitConverter.IsLittleEndian) Array.Reverse(trackLength);
            // Add Length to header
            trackHeader.AddRange(trackLength);

            // Combine all the data
            outputData.AddRange(trackHeader);
            outputData.AddRange(trackData);


            // Write to file
            Console.WriteLine("Writing file " + outputFile);
            System.IO.File.WriteAllBytes(outputFile, outputData.ToArray());
        }

        private static byte[] EncodeVaribleLength(uint input)
        {
            List<byte> result = new();
            do
            {
                byte temp = (byte)input;
                // Clear most significant bit for signature bit
                temp &= (byte)0b_0111_1111;
                // Add signature bit if not least significant byte
                if (result.Count > 0) temp |= (byte)0b_1000_0000;
                // Add byte to result
                result.Add(temp);
                // Shift down 7 bits to get next segment
                input >>= 7;
            } while (input > 0);

            //Reverse to have proper significance
            result.Reverse();
            return result.ToArray();
        }
    }
}
