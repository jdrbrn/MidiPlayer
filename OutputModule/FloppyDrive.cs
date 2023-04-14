using System.Device.Gpio;

namespace MidiPlayer.OutputModule
{
    internal class FloppyDrive : GpioToneGenerator
    {
        // Makes it easier to understand the controls
        const bool Forward = false;
        const bool Back = true;

        // Keep track of the pins
        int directionPin;
        int stepPin;
        
        // Keep track of where the head/motor is at
        int location;
        bool direction;

        public FloppyDrive(int dPin, int sPin) : base()
        {
            // Set the direction and setp pin and then init the GPIO for use
            directionPin = dPin;
            stepPin = sPin;
            controller.OpenPin(directionPin, PinMode.Output);
            controller.OpenPin(stepPin, PinMode.Output);
            controller.Write(directionPin, Back);

            // Reset the drive/motor to be at the end of the track
            // 3.5in floppies have 79 tracks/steps
            for (int i = 0; i < 80; i++)
            {
                controller.Write(stepPin, PinValue.High);
                Thread.Sleep(1);
                controller.Write(stepPin, PinValue.Low);
                Thread.Sleep(3);
            }

            // Set the home'd in location and setup for use
            location = 0;
            direction = Forward;
            controller.Write(directionPin, Forward);
            Thread.Sleep(1000);
        }

        protected override void PulseOutputPin()
        {
            // Pulse the step pin to move the drive
            controller.Write(stepPin, PinValue.High);
            controller.Write(stepPin, PinValue.Low);

            // Inc/Dec the location based on direction
            if (direction == Forward) location++;
            else location--;

            // If at the end (or start) of the track then set movement to the other direction
            if (location == 79)
            {
                direction = Back;
                controller.Write(directionPin, Back);
            }
            else if (location == 0)
            {
                direction = Forward;
                controller.Write(directionPin, Forward);
            }
        }
    }
}
