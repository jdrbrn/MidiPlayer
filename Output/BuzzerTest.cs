using System.Device.Gpio;

namespace MidiPlayer.Output
{
    internal class BuzzerTest : ToneGenerator
    {
        static private bool Forward = false;
        static private bool Back = true;

        GpioController controller;

        int stepPin;
        int location;
        bool direction;

        public BuzzerTest(int sPin) : base()
        {
            stepPin = sPin;
            controller = new GpioController();

            //controller.OpenPin(directionPin, PinMode.Output);
            controller.OpenPin(stepPin, PinMode.Output);
            //controller.Write(directionPin, Back);

            for (int i = 0; i < 80; i++)
            {
                controller.Write(stepPin, PinValue.High);
                controller.Write(stepPin, PinValue.Low);
            }

            location = 0;
            direction = Forward;
        }

        protected override void PlayNote(Note note)
        {
            int freq = GetFrequency(note.NoteNum);
            int length = (int)(note.Length * ticksPerMS);
            double PPMS = freq / 1000.0;
            int TicksPerPulse = (int)(ticksPerMS / PPMS);
            int time = 0;
            while (time < length)
            {
                PulseStepPin();
                time += TicksPerPulse;
                Wait(TicksPerPulse);
            }
        }

        void PulseStepPin()
        {
            controller.Write(stepPin, PinValue.High);
            controller.Write(stepPin, PinValue.Low);

            if (direction == Forward) location++;
            else location--;

            if (location == 80)
            {
                direction = Forward;
                //controller.Write(directionPin, Forward);
            }
            else if (location == 0)
            {
                direction = Back;
                //controller.Write(directionPin, Back);
            }
        }
    }
}
