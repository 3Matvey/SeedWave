using SeedWave.Core.Generation;

namespace SeedWave.Core.AudioComposition
{
    /// <summary>
    /// Builds a deterministic note plan for a simple generated arrangement.
    /// The arrangement currently consists of:
    /// - lead notes selected from the chosen scale;
    /// - bass notes following a fixed repeating progression;
    /// - a basic 4/4 drum groove with kick, snare and hi-hat.
    /// </summary>
    public class CompositionPlanBuilder
    {
        private const int SemitonesPerOctave = 12;
        private const int MidiOctaveOffset = 1;

        private const int BeatsPerBar = 4;
        private const int LeadStepsPerBar = 4;
        private const int HiHatStepsPerBar = 8;

        private const double QuarterNoteDuration = 1.0;
        private const double EighthNoteDuration = 0.5;
        private const double HalfBarBassDuration = 2.0;

        private const double ShortLeadNoteChance = 0.25;

        private const double LeadMinVelocity = 0.45;
        private const double LeadVelocityRange = 0.25;

        private const double BassFirstHitVelocity = 0.55;
        private const double BassSecondHitVelocity = 0.50;

        private const double KickPrimaryVelocity = 0.95;
        private const double KickSecondaryVelocity = 0.80;
        private const double SnareVelocity = 0.75;

        private const double HiHatMinVelocity = 0.25;
        private const double HiHatVelocityRange = 0.15;

        private const double DrumHitDuration = 0.20;
        private const double HiHatHitDuration = 0.10;
        private const double HiHatStepSize = 0.5;

        private const int KickMidiNote = 36;
        private const int SnareMidiNote = 38;
        private const int ClosedHiHatMidiNote = 42;

        /// <summary>
        /// Natural minor scale intervals in semitones from the tonic.
        /// </summary>
        private static readonly int[] MinorScaleIntervals = [0, 2, 3, 5, 7, 8, 10];

        /// <summary>
        /// Major scale intervals in semitones from the tonic.
        /// </summary>
        private static readonly int[] MajorScaleIntervals = [0, 2, 4, 5, 7, 9, 11];

        /// <summary>
        /// Bass progression for minor mode, expressed as semitone offsets from the tonic.
        /// This is a simple looping game-like progression.
        /// </summary>
        private static readonly int[] MinorBassProgression = [0, 5, 3, 4];

        /// <summary>
        /// Bass progression for major mode, expressed as semitone offsets from the tonic.
        /// This is a simple looping game-like progression.
        /// </summary>
        private static readonly int[] MajorBassProgression = [0, 4, 5, 3];

        public CompositionPlan Build(AudioProfile audioProfile, int seed)
        {
            ArgumentNullException.ThrowIfNull(audioProfile);

            var random = new Random(seed);
            var notes = new List<NoteEvent>();

            var scaleNotes = BuildScale(audioProfile.Key, audioProfile.Mode, audioProfile.LeadOctave);
            var bassRoots = BuildBassRoots(audioProfile.Key, audioProfile.Mode, audioProfile.BassOctave, audioProfile.Bars);

            AddLeadNotes(notes, scaleNotes, audioProfile.Bars, random);
            AddBassNotes(notes, bassRoots, audioProfile.Bars);
            AddDrumNotes(notes, audioProfile.Bars, random);

            return new CompositionPlan(
                audioProfile.TempoBpm,
                audioProfile.Bars,
                notes);
        }

        private static int[] BuildScale(MusicalKey key, ScaleMode mode, int octave)
        {
            var root = GetMidiRoot(key, octave);
            var intervals = mode == ScaleMode.Minor
                ? MinorScaleIntervals
                : MajorScaleIntervals;

            return [.. intervals.Select(interval => root + interval)];
        }

        private static int[] BuildBassRoots(MusicalKey key, ScaleMode mode, int octave, int bars)
        {
            var root = GetMidiRoot(key, octave);
            var progression = mode == ScaleMode.Minor
                ? MinorBassProgression
                : MajorBassProgression;

            return [.. Enumerable.Range(0, bars)
                .Select(index => root + progression[index % progression.Length])];
        }

        private static int GetMidiRoot(MusicalKey key, int octave)
        {
            return ((int)key) + (octave + MidiOctaveOffset) * SemitonesPerOctave;
        }

        /// <summary>
        /// Adds one lead note per beat.
        /// Most notes are quarter notes; some are shortened to eighth notes
        /// to make the melody less mechanically uniform.
        /// </summary>
        private static void AddLeadNotes(List<NoteEvent> notes, int[] scaleNotes, int bars, Random random)
        {
            for (var bar = 0; bar < bars; bar++)
            {
                for (var step = 0; step < LeadStepsPerBar; step++)
                {
                    var startBeat = (bar * BeatsPerBar) + step;
                    var midiNote = scaleNotes[random.Next(scaleNotes.Length)];
                    var duration = random.NextDouble() < ShortLeadNoteChance
                        ? EighthNoteDuration
                        : QuarterNoteDuration;
                    var velocity = LeadMinVelocity + random.NextDouble() * LeadVelocityRange;

                    notes.Add(new NoteEvent(
                        TrackKind.Lead,
                        midiNote,
                        startBeat,
                        duration,
                        velocity));
                }
            }
        }

        /// <summary>
        /// Adds two sustained bass notes per bar, each spanning half a bar.
        /// </summary>
        private static void AddBassNotes(List<NoteEvent> notes, int[] bassRoots, int bars)
        {
            for (var bar = 0; bar < bars; bar++)
            {
                var root = bassRoots[bar];
                var startBeat = bar * BeatsPerBar;

                notes.Add(new NoteEvent(
                    TrackKind.Bass,
                    root,
                    startBeat,
                    HalfBarBassDuration,
                    BassFirstHitVelocity));

                notes.Add(new NoteEvent(
                    TrackKind.Bass,
                    root,
                    startBeat + 2,
                    HalfBarBassDuration,
                    BassSecondHitVelocity));
            }
        }

        /// <summary>
        /// Adds a simple 4/4 groove:
        /// - kick on beats 1 and 3;
        /// - snare on beats 2 and 4;
        /// - closed hi-hat in eighth notes.
        /// </summary>
        private static void AddDrumNotes(List<NoteEvent> notes, int bars, Random random)
        {
            for (var bar = 0; bar < bars; bar++)
            {
                var beatOffset = bar * BeatsPerBar;

                AddKick(notes, beatOffset);
                AddSnare(notes, beatOffset);
                AddHiHats(notes, beatOffset, random);
            }
        }

        private static void AddKick(List<NoteEvent> notes, double beatOffset)
        {
            notes.Add(new NoteEvent(
                TrackKind.Drums,
                KickMidiNote,
                beatOffset + 0,
                DrumHitDuration,
                KickPrimaryVelocity));

            notes.Add(new NoteEvent(
                TrackKind.Drums,
                KickMidiNote,
                beatOffset + 2,
                DrumHitDuration,
                KickSecondaryVelocity));
        }

        private static void AddSnare(List<NoteEvent> notes, double beatOffset)
        {
            notes.Add(new NoteEvent(
                TrackKind.Drums,
                SnareMidiNote,
                beatOffset + 1,
                DrumHitDuration,
                SnareVelocity));

            notes.Add(new NoteEvent(
                TrackKind.Drums,
                SnareMidiNote,
                beatOffset + 3,
                DrumHitDuration,
                SnareVelocity));
        }

        private static void AddHiHats(List<NoteEvent> notes, double beatOffset, Random random)
        {
            for (var step = 0; step < HiHatStepsPerBar; step++)
            {
                var startBeat = beatOffset + (step * HiHatStepSize);
                var velocity = HiHatMinVelocity + random.NextDouble() * HiHatVelocityRange;

                notes.Add(new NoteEvent(
                    TrackKind.Drums,
                    ClosedHiHatMidiNote,
                    startBeat,
                    HiHatHitDuration,
                    velocity));
            }
        }
    }
}