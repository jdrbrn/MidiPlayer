using System.Timers;

namespace MidiPlayer.Outputs
{
    internal class ConsoleBeep : IOutput
    {

        private Dictionary<int, int> _frequencies = new Dictionary<int, int>();

        public void Output(ParsedTrack track)
        {
            // Check if running on Windows
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)) throw new Exception("Error: Can only use system beep output on Windows");
            Console.WriteLine("Playing song...");
            // Keep track of time
            int time = 0;
            // Iternate over notes
            for (int i = 0; i<track.Notes.Count; i++)
            {
                // Convert timestamp and length to int for ease of use
                int timestamp = (int)track.Notes[i].TimeStamp;
                int length = (int)track.Notes[i].Length;
                // Sleep until we hit timestamp
                Thread.Sleep(timestamp - time);
                // Update time to be at timestamp
                time = timestamp;
                // Play note if valid
                if (track.Notes[i].Length > 0) Console.Beep(GetFrequency(track.Notes[i].Note), length);
                // Update time to be after playing note
                time += length;
            }
            Console.WriteLine("Done");
        }

        // Return frequency in Hz for a given note by Midi number
        private int GetFrequency(int note)
        {
            // Check if frequency already calculated; If not calculate + store
            if (!_frequencies.ContainsKey(note))
            {
                // Algorithm from https://newt.phys.unsw.edu.au/jw/notes.html
                // f  =  2^((m−69)/12) * (440 Hz) where m = midi note number
                _frequencies[note] = (int)(Math.Pow(2, (note - 69) / 12f) * 440);

            }
            return _frequencies[note];
        }
    }
}
