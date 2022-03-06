using System.Device.Gpio;

namespace MidiPlayer.OutputModule
{
    internal class PassiveBuzzer : ToneGenerator
    {
        GpioController controller;
        readonly int outputPin;

        public PassiveBuzzer(int Pin) : base()
        {
            // Store output pin number
            outputPin = Pin;
            controller = new GpioController();
            // Set output pin to output mode
            controller.OpenPin(outputPin, PinMode.Output);
        }

        protected override void PlayNote(Note note)
        {
            // Get freq of note
            int freq = GetFrequency(note.NoteNum);
            Console.WriteLine(freq);
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

        void PulseOutputPin()
        {
            controller.Write(outputPin, PinValue.High);
            controller.Write(outputPin, PinValue.Low);
        }
    }
}
