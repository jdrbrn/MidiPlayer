namespace MidiPlayer.OutputModule
{
    internal class ConsoleBeep : ToneGenerator
    {
        protected override void PlayNote(MidiPlayer.Note note)
        {
            Console.Beep(GetFrequency(note.NoteNum), Convert.ToInt32(note.Length));
        }
    }
}
