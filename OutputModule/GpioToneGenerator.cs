using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Text;

namespace MidiPlayer.OutputModule
{
    internal abstract class GpioToneGenerator : ToneGenerator
    {
        // Static GPIO Controller to use for all tone generator outputs
        protected static GpioController controller = new();

        protected override void PlayNote(Note note)
        {
            // Get freq of note
            int freq = GetFrequency(note.NoteNum);
            // Get length of playback in ticks
            int length = (int)(note.Length * ticksPerMS);
            // Convert freq (pulse/second) to pulse per ms
            double PPMS = freq / 1000.0;
            // Invert to ms/pulse
            double MSPP = 1 / PPMS;
            // Convert to Ticks per Pulse
            int TicksPerPulse = (int)(ticksPerMS * MSPP);
            // Keep track of how many ticks have elapsed
            int ticks = 0;
            // While tick count is less than the length of the note play the note
            while (ticks < length)
            {
                PulseOutputPin();
                Wait(TicksPerPulse);
                ticks += TicksPerPulse;
            }
        }

        protected abstract void PulseOutputPin();
    }
}
