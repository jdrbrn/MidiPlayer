using System.Device.Pwm.Drivers;
// Requires libgpiod-dev package

namespace MidiPlayer.Outputs
{
    internal class PassiveBuzzer : ToneGenerator
    {

        SoftwarePwmChannel softwarePwmChannel;

        public PassiveBuzzer(int pin) : base()
        {
            softwarePwmChannel = new SoftwarePwmChannel(pin);
        }

        protected override void PlayNote(Note note)
        {
            softwarePwmChannel.Frequency = GetFrequency(note.NoteNum);
            softwarePwmChannel.Start();
            Wait(note.Length);
            softwarePwmChannel.Stop();
        }
    }
}
