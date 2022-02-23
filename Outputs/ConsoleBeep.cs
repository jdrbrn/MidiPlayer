using System.Timers;

namespace MidiPlayer.Outputs
{
    internal class ConsoleBeep : ToneGenerator
    {
        public ConsoleBeep() : base()
        {
            // Check if running on Windows
            if (!System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Windows)) throw new Exception("Error: Can only use system beep output on Windows");
        }

        protected override void PlayNote(MidiPlayer.Note note)
        {
            Console.Beep(GetFrequency(note.NoteNum), Convert.ToInt32(note.Length));
        }
    }
}
