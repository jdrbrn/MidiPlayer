using MidiPlayer.Outputs;

namespace MidiPlayer
{
    internal class MidiParser
    {
        public ParsedTrack ParseData(byte[] input)
        {
            MidiData midiData = new MidiData(input);
            // Only deal with one music track right now
            List<StampedEvent> flatTrack = new List<StampedEvent>();

            // If more than one track combine the first + second since first is likely tempo/pacing data
            if (midiData.TrackChunks.Count > 1)
            {
                // Flatten tempo data to tickstamped track
                List<StampedEvent> tempoTrack = FlattenTrackChunk(midiData.TrackChunks[0]);
                // Flatten music data to tickstamped track
                List<StampedEvent> musicTrack = FlattenTrackChunk(midiData.TrackChunks[1]);
                // Combine tracks
                flatTrack = MergeFlatTrack(new List<List<StampedEvent>> { tempoTrack, musicTrack });
            }
            // Otherwise flatten the single track to a tickstamped track
            else flatTrack = FlattenTrackChunk(midiData.TrackChunks[0]);

            return ParseFlatTrack(midiData.Header , flatTrack);
        }

        private List<StampedEvent> FlattenTrackChunk(Midi.TrackChunk track)
        {
            // Get list of mTrkEvnts
            List<Midi.MTrkEvent> midiEvents = track.mTrkEvents;
            // Create list for output
            List<StampedEvent> stampedEvents = new List<StampedEvent>();

            // Keep track of cumulative ticks
            int tickCount = 0;

            // For each midiEvent calculate the cumulative tick count and then add to absolute timeline
            foreach (Midi.MTrkEvent midiEvent in midiEvents)
            {
                tickCount += midiEvent.DeltaTime;
                stampedEvents.Add(new StampedEvent(tickCount, midiEvent.Event));
            }

            return stampedEvents;
        }

        private List<StampedEvent> MergeFlatTrack(List<List<StampedEvent>> tracks)
        {
            List<StampedEvent> output = new List<StampedEvent> ();

            foreach (var track in tracks)
            {
                output.AddRange(track);
            }

            output.Sort(CompareStampedEvents);
            return output;
        }

        private static int CompareStampedEvents(StampedEvent a, StampedEvent b)
        {
            int tickstampA = a.TickStamp;
            int tickstampB = b.TickStamp;
            // Return Values
            // 0 - a==b
            // <0 - a<b
            // >0 - a>b
            if (tickstampA == tickstampB) return 0;
            if (tickstampA < tickstampB) return -1;
            else return 1;
        }

        private ParsedTrack ParseFlatTrack(Midi.HeaderChunk header, List<StampedEvent> events)
        {
            // Time in milliseconds since start
            double time = 0;

            // Get 'division' value from header chunk
            // Ignoring the option for it to be SMPTE based
            // Start with raw data
            byte[] rawDivision = header.RawDivision;
            // Swap bytes around for endianness if needed
            if (BitConverter.IsLittleEndian) Array.Reverse(rawDivision);
            // Get int value
            int division = BitConverter.ToInt16( rawDivision, 0);

            // Set tempo to default of 500000/120BPM
            int tempo = 500000;

            // Keep track of which key is currently pressed and time it was pressed;
            // Use -1 as invalid key
            (int key, double time) pressedKey = (-1, 0);

            ParsedTrack parsedTrack = new ParsedTrack();

            for (int i = 0; i < events.Count; i++)
            {
                // Calculate delta-time ticks between tickstamped events
                // Need ticks, because tracking time in milliseconds
                // Can't just convert -> set time because ticks aren't a constant measure as tempo can change
                int deltaTime = events[i].TickStamp;
                if (i > 0) deltaTime -= events[i - 1].TickStamp;

                // Convert delta-time ticks to milliseconds for time tracking (equation from MIDI File spec)
                double deltaTMS = deltaTime * (tempo / division) / 1000.0;
                // Add deltaT to get updated timestamp of when event occurs
                time += deltaTMS;
                //Console.Write("Time " + time + ": ");
                // If event id >=-96 then it's not a Note On/Off event so process it differntly
                if ((sbyte)events[i].Event.StatusID >= -96)
                {
                    // Check if Meta-Event
                    if (events[i].Event.StatusID == 0xFF)
                    {
                        Midi.MetaEvent metaEvent = (Midi.MetaEvent)events[i].Event;
                        // Check if Meta-Event w/ type 51 - Set Tempo
                        // Otherwise ignore
                        if (metaEvent.Type == 0x51)
                        {
                            // Get 'division' value from header chunk
                            // Ignoring the option for it to be SMPTE based
                            // Start with raw data
                            byte[] rawTempo = new byte[4];
                            Array.Copy(metaEvent.Data, 0, rawTempo, 1, 3);
                            // Swap bytes around for endianness if needed
                            if (BitConverter.IsLittleEndian) Array.Reverse(rawTempo);
                            // Get int value
                            tempo = BitConverter.ToInt32(rawTempo, 0);
                        }
                        else Console.WriteLine("Ignoring Meta Event Type " + Convert.ToString(((Midi.MetaEvent)events[i].Event).Type, toBase: 16));
                    }
                    else
                    {
                        Console.WriteLine("Ignoring non-Meta Event w/ ID " + Convert.ToString(events[i].Event.StatusID, toBase: 16));
                        Console.Write("Data: ");
                        foreach (byte b in events[i].Event.Data) Console.Write(Convert.ToString(b, toBase: 16)+" ");
                        Console.WriteLine();
                    }
                    continue;
                }


                // Otherwise check if event is Note On or Note Off
                // Note Off events are below -112
                if ((sbyte)events[i].Event.StatusID < -112)
                {
                    // Check if key is being pressed and release if is
                    // Otherwise ignore, must've been released to play another key
                    if (pressedKey.key == events[i].Event.Data[0])
                    {
                        // Add note to track
                        parsedTrack.Notes.Add(new PlayNote(pressedKey.time, events[i].Event.Data[0], time - pressedKey.time));
                        // Key is no longer down
                        // Set to arbitrary info
                        pressedKey = (-1, 0);
                    }
                }
                // Note On event
                else
                {
                    // Check if velocity (byte 2) is 0; If so key is being released
                    if (events[i].Event.Data[1] == 0)
                    {
                        // Check if key is being pressed and release if is
                        // Otherwise ignore, must've been released to play another key
                        if (pressedKey.key == events[i].Event.Data[0])
                        {
                            // Add note to track
                            parsedTrack.Notes.Add(new PlayNote(pressedKey.time, events[i].Event.Data[0], time - pressedKey.time));
                            // Key is no longer down
                            // Set to arbitrary info
                            pressedKey = (-1, 0);
                        }
                    }
                    else
                    {
                        // If no key is being pressed, press key
                        if (pressedKey.key == -1) pressedKey = (events[i].Event.Data[0], time);
                        // Otherwise release key and press new key
                        else
                        {
                            parsedTrack.Notes.Add(new PlayNote(pressedKey.time, pressedKey.key, time - pressedKey.time));
                            pressedKey = (events[i].Event.Data[0], time);
                        }
                    }
                }

            }
            return parsedTrack;
        }

        public static int decodeVariableLength(byte[] input)
        {
            // Temp value to store concat result that will be split to bytes later
            int concatResult = 0;
            // Start w/ least significant bits/bytes
            Array.Reverse(input);
            for (int i = 0; i < input.Length; i++)
            {
                // Get rid of highest bit in the byte as it's a signature
                int temp = (input[i] & (byte)0b_0111_1111);
                // Shift up 7 bits per byte to get original posiion and add to result
                concatResult |= temp << (7 * i);
            }
            return concatResult;
        }
    }
}
