using System.Device.Gpio;

namespace MidiPlayer.Output
{
    internal class FloppyDrive : ToneGenerator
    {
        static private bool Forward = false;
        static private bool Back = true;

        GpioController controller;

        int directionPin;
        int stepPin;
        int location;
        bool direction;

        public FloppyDrive(int dPin, int sPin) : base()
        {
            directionPin = dPin;
            stepPin = sPin;
            controller = new GpioController();

            controller.OpenPin(directionPin, PinMode.Output);
            controller.OpenPin(stepPin, PinMode.Output);
            controller.Write(directionPin, Back);

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
            int length = (int)note.Length;
            double PPMS = freq / 1000.0;
            if (PPMS >= 1)
            {
                int time = 0;
                while (time < length)
                {
                    for (int pulse = 0; pulse < PPMS; pulse++) PulseStepPin();
                    time++;
                    Thread.Sleep(1);
                }
            }
            else
            {
                int MSperPulse = (int)(1 / PPMS);
                int time = 0;
                while (time < length)
                {
                    PulseStepPin();
                    time += MSperPulse;
                    Thread.Sleep(MSperPulse);
                }
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
                controller.Write(directionPin, Forward);
            }
            else if (location == 0)
            {
                direction = Back;
                controller.Write(directionPin, Back);
            }
        }
    }
}
