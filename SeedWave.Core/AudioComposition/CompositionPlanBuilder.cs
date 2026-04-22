using SeedWave.Core.AudioComposition.Theory;
using SeedWave.Core.Generation;
using MusicNote = Melanchall.DryWetMidi.MusicTheory.Note;
using MusicInterval = Melanchall.DryWetMidi.MusicTheory.Interval;

namespace SeedWave.Core.AudioComposition
{
    public enum SectionType
    {
        Intro,
        Main,
        Variation,
        Break,
        Outro
    }

    public sealed class CompositionPlanBuilder
    {
        private const int BeatsPerBar = 4;
        private const int EighthStepsPerBar = 8;
        private const int SixteenthStepsPerBar = 16;
        private const int LoopBars = 2;

        private const double EighthBeat = 0.5;
        private const double SixteenthBeat = 0.25;

        private const double LeadDuration = 0.22;
        private const double LeadAccentDuration = 0.34;
        private const double ArpDuration = 0.18;
        private const double BassDuration = 0.22;
        private const double BassAccentDuration = 0.34;

        private const double DrumHitDuration = 0.14;
        private const double HiHatDuration = 0.06;

        private const int KickMidiNote = 36;
        private const int SnareMidiNote = 38;
        private const int ClosedHiHatMidiNote = 42;
        private const int OpenHiHatMidiNote = 46;

        private const double LeadVelocity = 0.72;
        private const double LeadAccentVelocity = 0.86;
        private const double ArpVelocity = 0.42;
        private const double ArpAccentVelocity = 0.54;
        private const double BassVelocity = 0.80;
        private const double BassAccentVelocity = 0.92;
        private const double KickVelocity = 0.98;
        private const double KickSecondaryVelocity = 0.88;
        private const double SnareVelocity = 0.78;
        private const double HiHatVelocity = 0.28;
        private const double OpenHiHatVelocity = 0.46;

        private readonly HarmonyPlanner _harmonyPlanner = new();

        public CompositionPlan Build(AudioProfile audioProfile, int seed)
        {
            ArgumentNullException.ThrowIfNull(audioProfile);

            var random = new Random(seed);
            var notes = new List<NoteEvent>();

            var theory = new MusicTheoryContext(audioProfile);
            var harmony = _harmonyPlanner.Build(audioProfile.Bars, theory, seed);
            var sections = BuildSectionPlan(audioProfile.Bars);

            var leadLoop = BuildLeadLoop(random);
            var arpLoop = BuildArpLoop(random);
            var bassLoop = BuildBassLoop(random);
            var drumLoop = BuildDrumLoop(random);

            for (var bar = 0; bar < audioProfile.Bars; bar++)
            {
                var section = sections[bar];
                var harmonicBar = harmony[bar];
                var nextDegree = harmony[Math.Min(bar + 1, harmony.Count - 1)].Degree;
                var barStartBeat = bar * BeatsPerBar;
                var barInLoop = bar % LoopBars;
                var isVariantBar = section == SectionType.Variation || ((bar + 1) % 4 == 0);

                AddArpLayer(
                    notes,
                    theory,
                    harmonicBar.Degree,
                    audioProfile,
                    section,
                    arpLoop[barInLoop],
                    barStartBeat,
                    isVariantBar);

                AddBassLayer(
                    notes,
                    theory,
                    harmonicBar.Degree,
                    nextDegree,
                    audioProfile,
                    section,
                    bassLoop[barInLoop],
                    barStartBeat,
                    random,
                    isVariantBar);

                AddLeadLayer(
                    notes,
                    theory,
                    harmonicBar.Degree,
                    audioProfile,
                    section,
                    leadLoop[barInLoop],
                    barStartBeat,
                    random,
                    isVariantBar);

                AddDrumLayer(
                    notes,
                    section,
                    drumLoop[barInLoop],
                    barStartBeat,
                    addFill: (bar + 1) % 4 == 0 && section is not SectionType.Outro);
            }

            return new CompositionPlan(
                audioProfile.TempoBpm,
                audioProfile.Bars,
                notes);
        }

        private static SectionType[] BuildSectionPlan(int bars)
        {
            var sections = new SectionType[bars];

            if (bars == 1)
            {
                sections[0] = SectionType.Main;
                return sections;
            }

            sections[0] = SectionType.Intro;
            sections[^1] = SectionType.Outro;

            for (var i = 1; i < bars - 1; i++)
            {
                sections[i] = i switch
                {
                    var n when n == bars - 2 => SectionType.Break,
                    var n when n % 4 == 3 => SectionType.Variation,
                    _ => SectionType.Main
                };
            }

            return sections;
        }

        private static int[][] BuildLeadLoop(Random random)
        {
            var patterns = new[]
            {
                new[] { 0, 2, 4, 2, 5, 4, 2, 1, 0, 2, 4, 5, 4, 2, 1, 0 },
                new[] { 0, 0, 2, 4, 5, 4, 2, 0, 0, 2, 4, 2, 5, 4, 2, 1 },
                new[] { 0, 2, 4, 5, 4, 2, 1, 0, 0, 2, 0, 4, 5, 4, 2, 0 },
                new[] { 0, 2, 0, 4, 5, 4, 2, 0, 1, 2, 4, 2, 5, 4, 2, 0 }
            };

            var first = patterns[random.Next(patterns.Length)];
            var second = MutateLeadPattern(first, random);

            return [first, second];
        }

        private static int[][] BuildArpLoop(Random random)
        {
            var patterns = new[]
            {
                new[] { 0, 1, 2, 3, 2, 1, 0, 1, 2, 3, 2, 1, 0, 1, 2, 3 },
                new[] { 0, 2, 1, 3, 0, 2, 1, 3, 0, 2, 1, 3, 0, 2, 1, 3 },
                new[] { 0, 1, 2, 1, 3, 2, 1, 0, 0, 1, 2, 1, 3, 2, 1, 0 },
                new[] { 0, 1, 3, 2, 0, 1, 3, 2, 0, 1, 2, 3, 0, 1, 2, 3 }
            };

            var first = patterns[random.Next(patterns.Length)];
            var second = MutateArpPattern(first, random);

            return [first, second];
        }

        private static int[][] BuildBassLoop(Random random)
        {
            var patterns = new[]
            {
                new[] { 0, -1, 0, 0, 0, -1, 0, 1, 0, -1, 0, 0, 0, -1, 1, -1 },
                new[] { 0, -1, 0, 2, 0, -1, 0, 1, 0, -1, 0, 2, 0, -1, 1, -1 },
                new[] { 0, 0, -1, 0, 0, 2, -1, 1, 0, 0, -1, 0, 0, -1, 1, -1 },
                new[] { 0, -1, 0, 0, 0, -1, 2, 1, 0, -1, 0, 0, 0, -1, 1, -1 }
            };

            var first = patterns[random.Next(patterns.Length)];
            var second = MutateBassPattern(first, random);

            return [first, second];
        }

        private static DrumBarPattern[] BuildDrumLoop(Random random)
        {
            var first = random.Next(100) < 50
                ? DrumBarPattern.Basic()
                : DrumBarPattern.Driving();

            var second = random.Next(100) < 50
                ? DrumBarPattern.BasicVariant()
                : DrumBarPattern.DrivingVariant();

            return [first, second];
        }

        private static int[] MutateLeadPattern(int[] source, Random random)
        {
            var clone = (int[])source.Clone();

            for (var i = 0; i < 2; i++)
            {
                var index = random.Next(clone.Length);
                clone[index] = Math.Clamp(clone[index] + (random.Next(100) < 50 ? -1 : 1), 0, 6);
            }

            return clone;
        }

        private static int[] MutateArpPattern(int[] source, Random random)
        {
            var clone = (int[])source.Clone();

            for (var i = 0; i < 2; i++)
            {
                clone[random.Next(clone.Length)] = random.Next(4);
            }

            return clone;
        }

        private static int[] MutateBassPattern(int[] source, Random random)
        {
            var clone = (int[])source.Clone();

            if (random.Next(100) < 70)
            {
                clone[7] = 1;
            }

            if (random.Next(100) < 50)
            {
                clone[11] = 2;
            }

            return clone;
        }

        private static void AddArpLayer(
            List<NoteEvent> notes,
            MusicTheoryContext theory,
            int chordDegree,
            AudioProfile profile,
            SectionType section,
            int[] arpPattern,
            double barStartBeat,
            bool isVariantBar)
        {
            var chord = theory.GetTriadNotes(
                chordDegree,
                Math.Max(profile.BassOctave + 2, profile.LeadOctave - 1));

            var arpNotes = BuildArpNotes(chord);

            for (var step = 0; step < SixteenthStepsPerBar; step++)
            {
                if (section == SectionType.Break && step < 4)
                {
                    continue;
                }

                var note = arpNotes[arpPattern[step] % arpNotes.Count];
                var velocity = step % 4 == 0
                    ? ArpAccentVelocity
                    : ArpVelocity;

                if (section == SectionType.Intro)
                {
                    velocity *= 0.90;
                }

                if (isVariantBar && step is 14 or 15)
                {
                    velocity += 0.04;
                }

                notes.Add(new NoteEvent(
                    TrackKind.Pad,
                    note.NoteNumber,
                    barStartBeat + (step * SixteenthBeat),
                    ArpDuration,
                    velocity));
            }
        }

        private static IReadOnlyList<MusicNote> BuildArpNotes(IReadOnlyList<MusicNote> chord)
        {
            var root = chord[0];
            var third = chord[1];
            var fifth = chord[2];

            return
            [
                root,
                third,
                fifth,
                root + MusicInterval.Twelve
            ];
        }

        private static void AddBassLayer(
            List<NoteEvent> notes,
            MusicTheoryContext theory,
            int chordDegree,
            int nextChordDegree,
            AudioProfile profile,
            SectionType section,
            int[] bassPattern,
            double barStartBeat,
            Random random,
            bool isVariantBar)
        {
            for (var step = 0; step < SixteenthStepsPerBar; step++)
            {
                var patternValue = bassPattern[step];
                if (patternValue < 0)
                {
                    continue;
                }

                if (section == SectionType.Break && step is not 0 and not 4 and not 8 and not 12 and not 14)
                {
                    continue;
                }

                var degree = step >= 14
                    ? nextChordDegree + patternValue
                    : chordDegree + patternValue;

                degree = Math.Clamp(degree, 0, 6);

                var note = theory.GetScaleNote(degree, profile.BassOctave);
                var velocity = step % 4 == 0
                    ? BassAccentVelocity
                    : BassVelocity;
                var duration = step % 4 == 0
                    ? BassAccentDuration
                    : BassDuration;

                if (isVariantBar && step == 14)
                {
                    velocity += 0.04;
                }

                notes.Add(new NoteEvent(
                    TrackKind.Bass,
                    note.NoteNumber,
                    barStartBeat + (step * SixteenthBeat),
                    duration,
                    velocity + (random.NextDouble() * 0.03)));
            }
        }

        private static void AddLeadLayer(
            List<NoteEvent> notes,
            MusicTheoryContext theory,
            int chordDegree,
            AudioProfile profile,
            SectionType section,
            int[] leadPattern,
            double barStartBeat,
            Random random,
            bool isVariantBar)
        {
            var rootBias = chordDegree;

            for (var step = 0; step < SixteenthStepsPerBar; step++)
            {
                if (section == SectionType.Break && step < 8)
                {
                    continue;
                }

                if (step % 2 == 1 && random.Next(100) < 35 && section != SectionType.Variation)
                {
                    continue;
                }

                var degree = Math.Clamp(rootBias + leadPattern[step], 0, 6);

                var octave = Math.Max(3, profile.LeadOctave - 1);
                if (step >= 8 && random.Next(100) < 12)
                {
                    octave += 1;
                }

                var note = theory.GetScaleNote(degree, octave);
                var velocity = step % 4 == 0
                    ? LeadAccentVelocity
                    : LeadVelocity;
                var duration = step % 4 == 0
                    ? LeadAccentDuration
                    : LeadDuration;

                if (section == SectionType.Intro)
                {
                    velocity *= 0.88;
                }

                if (isVariantBar && step is 12 or 15)
                {
                    velocity += 0.05;
                }

                notes.Add(new NoteEvent(
                    TrackKind.Lead,
                    note.NoteNumber,
                    barStartBeat + (step * SixteenthBeat),
                    duration,
                    velocity + (random.NextDouble() * 0.04)));

                if (step % 8 == 0 && section is not SectionType.Intro)
                {
                    notes.Add(new NoteEvent(
                        TrackKind.Lead,
                        note.NoteNumber + 12,
                        barStartBeat + (step * SixteenthBeat),
                        duration * 0.85,
                        (velocity * 0.68) + (random.NextDouble() * 0.03)));
                }
            }
        }

        private static void AddDrumLayer(
            List<NoteEvent> notes,
            SectionType section,
            DrumBarPattern pattern,
            double barStartBeat,
            bool addFill)
        {
            AddKick(notes, pattern.KickSteps, barStartBeat, section);
            AddSnare(notes, pattern.SnareSteps, barStartBeat, section);
            AddHiHats(notes, pattern.HiHatSteps, barStartBeat, section);

            if (addFill && section is not SectionType.Outro)
            {
                AddMiniFill(notes, barStartBeat);
            }
        }

        private static void AddKick(
            List<NoteEvent> notes,
            IReadOnlyList<int> steps,
            double barStartBeat,
            SectionType section)
        {
            foreach (var step in steps)
            {
                if (section == SectionType.Intro && step != 0)
                {
                    continue;
                }

                var velocity = step == 0
                    ? KickVelocity
                    : KickSecondaryVelocity;

                notes.Add(new NoteEvent(
                    TrackKind.Drums,
                    KickMidiNote,
                    barStartBeat + (step * SixteenthBeat),
                    DrumHitDuration,
                    velocity));
            }
        }

        private static void AddSnare(
            List<NoteEvent> notes,
            IReadOnlyList<int> steps,
            double barStartBeat,
            SectionType section)
        {
            foreach (var step in steps)
            {
                if (section == SectionType.Intro && step < 8)
                {
                    continue;
                }

                notes.Add(new NoteEvent(
                    TrackKind.Drums,
                    SnareMidiNote,
                    barStartBeat + (step * SixteenthBeat),
                    DrumHitDuration,
                    SnareVelocity));
            }
        }

        private static void AddHiHats(
            List<NoteEvent> notes,
            IReadOnlyList<int> steps,
            double barStartBeat,
            SectionType section)
        {
            foreach (var step in steps)
            {
                if (section == SectionType.Break && step % 2 == 1)
                {
                    continue;
                }

                if (section == SectionType.Intro && step % 4 == 3)
                {
                    continue;
                }

                var midiNote = step == 15 && section == SectionType.Variation
                    ? OpenHiHatMidiNote
                    : ClosedHiHatMidiNote;

                var velocity = midiNote == OpenHiHatMidiNote
                    ? OpenHiHatVelocity
                    : HiHatVelocity + ((step % 4 == 0) ? 0.04 : 0.0);

                notes.Add(new NoteEvent(
                    TrackKind.Drums,
                    midiNote,
                    barStartBeat + (step * SixteenthBeat),
                    HiHatDuration,
                    velocity));
            }
        }

        private static void AddMiniFill(List<NoteEvent> notes, double barStartBeat)
        {
            var fillStart = barStartBeat + 3.0;

            notes.Add(new NoteEvent(TrackKind.Drums, SnareMidiNote, fillStart + 0.00, 0.08, 0.78));
            notes.Add(new NoteEvent(TrackKind.Drums, SnareMidiNote, fillStart + 0.25, 0.08, 0.82));
            notes.Add(new NoteEvent(TrackKind.Drums, SnareMidiNote, fillStart + 0.50, 0.08, 0.86));
            notes.Add(new NoteEvent(TrackKind.Drums, OpenHiHatMidiNote, fillStart + 0.75, 0.10, 0.64));
        }

        private sealed record DrumBarPattern(
            IReadOnlyList<int> KickSteps,
            IReadOnlyList<int> SnareSteps,
            IReadOnlyList<int> HiHatSteps)
        {
            public static DrumBarPattern Basic()
            {
                return new DrumBarPattern(
                    KickSteps: [0, 8],
                    SnareSteps: [4, 12],
                    HiHatSteps: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
            }

            public static DrumBarPattern BasicVariant()
            {
                return new DrumBarPattern(
                    KickSteps: [0, 6, 8],
                    SnareSteps: [4, 12],
                    HiHatSteps: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
            }

            public static DrumBarPattern Driving()
            {
                return new DrumBarPattern(
                    KickSteps: [0, 6, 8, 12],
                    SnareSteps: [4, 12],
                    HiHatSteps: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
            }

            public static DrumBarPattern DrivingVariant()
            {
                return new DrumBarPattern(
                    KickSteps: [0, 4, 8, 12],
                    SnareSteps: [4, 12],
                    HiHatSteps: [0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15]);
            }
        }
    }
}