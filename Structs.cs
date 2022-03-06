namespace MidiPlayer
{
    internal struct Note
    {
        // Time is the time in milliseconds since starting to play the note
        // Note is the number of the note w/ 60 being Middle C per Midi standard
        // Length is the time in milliseconds to hold the note
        public double TimeStamp;
        public int NoteNum;
        public double Length;

        public Note(double time, int note, double len)
        {
            TimeStamp = time;
            NoteNum = note;
            Length = len;
        }
    }

    internal struct ChannelConfiguration
    {
        public string OutputModule { get; set; }
        public Dictionary<string, string> Args { get; set; }
    }
}
