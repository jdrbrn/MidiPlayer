using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MidiPlayer
{
    internal struct PlayNote
    {
        // Time is the time in milliseconds since starting to play the note
        // Note is the number of the note w/ 60 being Middle C per Midi standard
        // Length is the time in milliseconds to hold the note
        public double TimeStamp;
        public int Note;
        public double Length;

        public PlayNote(double time, int note, double len)
        {
            TimeStamp = time;
            Note = note;
            Length = len;
        }
    }
}
