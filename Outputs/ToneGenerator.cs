﻿using System.Diagnostics;

namespace MidiPlayer.Outputs
{
    internal abstract class ToneGenerator : IOutput
    {
        protected Dictionary<int, int> _frequencies = new Dictionary<int, int>();
        protected Stopwatch _stopwatch = new Stopwatch();
        // Stopwatch Frequency in ticks/second  / 1000 ms = ticks/ms
        protected double ticksPerMS = Stopwatch.Frequency / 1000.0;

        public void Output(ParsedTrack track)
        {
            Console.WriteLine("Playing song...");
            // Keep track of time
            int time = 0;
            // Iternate over notes
            for (int i = 0; i < track.Notes.Count; i++)
            {
                // Convert timestamp and length to int for ease of use
                int timestamp = (int)track.Notes[i].TimeStamp;
                int length = (int)track.Notes[i].Length;
                // Sleep until we hit timestamp
                Thread.Sleep(timestamp - time);
                // Update time to be at timestamp
                time = timestamp;
                // Play note if valid
                if (track.Notes[i].Length > 0) PlayNote(track.Notes[i]);
                // Update time to be after playing note
                time += length;
            }
            Console.WriteLine("Done");
        }

        protected abstract void PlayNote(MidiPlayer.Note note);

        // Return frequency in Hz for a given note by Midi number
        protected int GetFrequency(int note)
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

        // Wait for time in ms
        protected void Wait(double time)
        {
            Wait((int)(ticksPerMS * time));
        }
        
        // Wait for time in ticks
        protected void Wait(int ticks)
        {
            _stopwatch.Start();
            // Do nothing while waiting
            while (_stopwatch.ElapsedTicks < ticks) ;
            _stopwatch.Reset();
        }
    }
}
