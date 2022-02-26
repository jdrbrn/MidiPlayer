namespace MidiPlayer.Midi
{
    internal class TrackChunk : Chunk
    {
        public List<Midi.MTrkEvent> mTrkEvents;

        public TrackChunk(string header, uint length, byte[] data) : base(header, length, data)
        {
            if (header != "MTrk") throw new Exception("Invalid: Setting non-track chunk to track chunk");
            mTrkEvents = new List<Midi.MTrkEvent>();
        }

        public void ParseData()
        {
            int index = 0;
            // While index is in bounds, parse events
            // ParseEvent returns the index of the next byte after the MTrkEvent it parsed
            while (index < Data.Length) index = ParseEvent(index);
        }

        // Parses an event starting at index and adds it to the mTrkEvents list
        // Returns index of byte after mTrkEvent
        private int ParseEvent(int index)
        {
            // get delta-time of event
            int deltaTLength = 0;
            // delta-time stored as variable length
            // gets length of delta-time by checking if byte > 0 (signature bit is missing)
            while ((sbyte)Data[index + deltaTLength] < 0) deltaTLength++;
            // Add one to get length vs index delta
            deltaTLength++;
            // copy variable length data to array for parsing
            byte[] deltaTVaribleLength = new byte[deltaTLength];
            Array.Copy(Data, index, deltaTVaribleLength, 0, deltaTLength);
            int deltaT = MidiParser.decodeVariableLength(deltaTVaribleLength);

            // Move index to after dt
            index += deltaTLength;

            // Get event data
            // Status bytes are < 0; Data bytes >=0
            // If byte after dt is Data then running status (Status is taken from last event)
            byte status;
            if ((sbyte)Data[index] < 0)
            {
                // Copy status byte
                status = Data[index];
                // inc index to get to data bytes
                index++;
            }
            // If running status pull status from last event
            else
            {
                // Make sure a last event exists
                if (mTrkEvents.Count == 0) throw new Exception("Malformed Midi file; Using running status without a previous event");
                // Copy last status
                status = mTrkEvents[^1].Event.StatusID;
                // Don't increment index bc already at data bytes
            }

            // Grab data bytes depending on type of event
            // See http://www.music.mcgill.ca/~ich/classes/mumt306/StandardMIDIfileformat.html#BMA1_ for status ID + data lengths

            // Sysex and Meta events handled seperately because special

            // sysex event id is 0xF0 or 0xF7
            if (status == 0xF0 || status == 0xF7)
            {
                // Get length of sysex message
                int sysexlengthLength = 0;
                // length stored as variable length
                // see if variable length over by checking if byte > 0 (signature bit is missing)
                while ((sbyte)Data[index + sysexlengthLength] < 0) sysexlengthLength++;
                // Add one to get length vs index delta
                sysexlengthLength++;

                // copy variable length data to array for parsing
                byte[] sysexDataLengthVaribleLength = new byte[sysexlengthLength];
                Array.Copy(Data, index, sysexDataLengthVaribleLength, 0, sysexlengthLength);
                int sysexDataLength = MidiParser.decodeVariableLength(sysexDataLengthVaribleLength);

                // move index to start of sysex data
                index += sysexlengthLength;

                // copy sysex data to an array
                byte[] sysexData = new byte[sysexDataLength];
                Array.Copy(Data, index, sysexData, 0, sysexDataLength);

                mTrkEvents.Add(new MTrkEvent(deltaT, new SysexEvent(status, sysexDataLength, sysexData)));

                // Move index to after the MTrkEvent and return new value
                return index += sysexDataLength;
            }

            // meta-event
            if (status == 0xFF)
            {
                // Get type of meta event
                byte metaType = Data[index];
                index++;

                // Get length of metadata
                int metadatalengthLength = 0;
                // length stored as variable length
                // see if variable length over by checking if byte > 0 (signature bit is missing)
                while ((sbyte)Data[index + metadatalengthLength] < 0) metadatalengthLength++;
                // Add one to get length vs index delta
                metadatalengthLength++;

                // copy variable length data to array for parsing
                byte[] metadataLengthVaribleLength = new byte[metadatalengthLength];
                Array.Copy(Data, index, metadataLengthVaribleLength, 0, metadatalengthLength);
                int metadataLength = MidiParser.decodeVariableLength(metadataLengthVaribleLength);

                // move index to start of sysex data
                index += metadatalengthLength;

                // Copy meta data to array if length > 0
                // Else send empty array
                byte[] metaData = new byte[metadataLength];
                if (metadataLength > 0)
                {
                    // copy meta data to an array
                    Array.Copy(Data, index, metaData, 0, metadataLength);

                    mTrkEvents.Add(new MTrkEvent(deltaT, new MetaEvent(status, metaType, metadataLength, metaData)));
                }

                // Move index to after the MTrkEvent and return new value
                return index += metadataLength;
            }

            // Get length of data bytes (based off spec)
            int dataLength = 0;
            // The bound check is for the status id of the next type of status
            // Had to convert from binary to dec for sbyte check to work nicely w/ negatives
            // Note Off
            if ((sbyte)status < -112) dataLength = 2;
            // Note On
            else if ((sbyte)status < -96) dataLength = 2;
            // Key Pressure
            else if ((sbyte)status < -80) dataLength = 2;
            // Control Change
            else if ((sbyte)status < -64) dataLength = 2;
            // Program Change
            else if ((sbyte)status < -48) dataLength = 1;
            // Channel Pressure
            else if ((sbyte)status < -32) dataLength = 1;
            // Pitch Wheel Change
            else if ((sbyte)status < -16) dataLength = 2;

            // Copy event data
            byte[] eventData = new byte[dataLength];
            Array.Copy(Data, index, eventData, 0, eventData.Length);

            // Add Event to list
            mTrkEvents.Add(new MTrkEvent(deltaT, new Event(status, eventData)));

            // Move index to after the MTrkEvent and return
            return index += dataLength;
        }
    }
}
