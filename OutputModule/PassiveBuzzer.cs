using System.Device.Gpio;

namespace MidiPlayer.OutputModule
{
    internal class PassiveBuzzer : GpioToneGenerator
    {
        readonly int outputPin;

        public PassiveBuzzer(int Pin) : base()
        {
            // Store output pin number
            outputPin = Pin;
            // Set output pin to output mode
            controller.OpenPin(outputPin, PinMode.Output);
        }

        protected override void PulseOutputPin()
        {
            controller.Write(outputPin, PinValue.High);
            controller.Write(outputPin, PinValue.Low);
        }
    }
}
