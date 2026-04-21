namespace SeedWave.Core.AudioComposition
{
    public record CompositionPlan
   (
       int TempoBpm,
       int Bars,
       IReadOnlyList<NoteEvent> Notes
   );
}
