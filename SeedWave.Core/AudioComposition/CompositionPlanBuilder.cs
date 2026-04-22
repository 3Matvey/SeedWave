using SeedWave.Core.Generation;

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

    public enum MotifVariationType
    {
        Repeat,
        TailVariation,
        RhythmicVariation,
        Simplify,
        DirectionFlip
    }

    public enum BassPatternType
    {
        HoldRoot,
        PulseRoot,
        ApproachNextRoot
    }

    public enum DrumPatternType
    {
        Basic,
        Sparse,
        Driving
    }

    internal sealed record LeadRhythmPattern(bool[] Steps);

    internal sealed record MotifStep(int DegreeOffset, bool IsRest = false);

    internal sealed record LeadMotif(IReadOnlyList<MotifStep> Steps);

    /// <summary>
    /// Builds a deterministic generated arrangement with:
    /// - section-based song structure;
    /// - lead motifs with rhythmic variation;
    /// - bass patterns with simple movement;
    /// - multiple drum grooves and small fills.
    /// </summary>
    public class CompositionPlanBuilder
    {
        private const int SemitonesPerOctave = 12;
        private const int MidiOctaveOffset = 1;

        private const int BeatsPerBar = 4;
        private const int LeadStepsPerBar = 8;
        private const int HiHatStepsPerBar = 8;

        private const double QuarterNoteDuration = 1.0;
        private const double EighthNoteDuration = 0.5;
        private const double HalfBarDuration = 2.0;

        private const double DrumHitDuration = 0.20;
        private const double HiHatHitDuration = 0.10;
        private const double HiHatStepSize = 0.5;

        private const double LeadBaseVelocity = 0.62;
        private const double LeadVariationVelocity = 0.74;
        private const double LeadVelocityRange = 0.10;

        private const double BassHoldVelocity = 0.68;
        private const double BassPulseVelocity = 0.60;
        private const double BassApproachVelocity = 0.58;

        private const double KickPrimaryVelocity = 0.95;
        private const double KickSecondaryVelocity = 0.82;
        private const double SnareVelocity = 0.76;
        private const double FillSnareVelocity = 0.80;

        private const double HiHatMinVelocity = 0.26;
        private const double HiHatVelocityRange = 0.16;

        private const int KickMidiNote = 36;
        private const int SnareMidiNote = 38;
        private const int ClosedHiHatMidiNote = 42;
        private const int OpenHiHatMidiNote = 46;

        private static readonly int[] MinorScaleIntervals = [0, 2, 3, 5, 7, 8, 10];
        private static readonly int[] MajorScaleIntervals = [0, 2, 4, 5, 7, 9, 11];

        private static readonly LeadRhythmPattern[] LeadRhythmPatterns =
        [
            new([true, false, true, false, true, false, true, false]),
            new([true, true, false, true, false, true, false, true]),
            new([true, false, false, true, true, false, true, false]),
            new([true, true, true, false, true, false, false, true]),
            new([true, false, true, true, false, true, false, false])
        ];

        public CompositionPlan Build(AudioProfile audioProfile, int seed)
        {
            ArgumentNullException.ThrowIfNull(audioProfile);

            var random = new Random(seed);
            var notes = new List<NoteEvent>();

            var scaleIntervals = GetScaleIntervals(audioProfile.Mode);
            var leadRoot = GetMidiRoot(audioProfile.Key, audioProfile.LeadOctave);
            var bassRoot = GetMidiRoot(audioProfile.Key, audioProfile.BassOctave);

            var sections = BuildSectionPlan(audioProfile.Bars);
            var bassRoots = BuildBassRoots(scaleIntervals, bassRoot, audioProfile.Bars, random);

            var baseMotif = CreateBaseLeadMotif(random);
            var previousMotif = baseMotif;

            for (var bar = 0; bar < audioProfile.Bars; bar++)
            {
                var section = sections[bar];
                var currentBassRoot = bassRoots[bar];
                var nextBassRoot = bassRoots[Math.Min(bar + 1, bassRoots.Length - 1)];
                var barStartBeat = bar * BeatsPerBar;

                var motif = bar == 0
                    ? baseMotif
                    : CreateMotifForBar(previousMotif, section, random);

                AddLeadNotes(notes, scaleIntervals, leadRoot, motif, section, barStartBeat, random);
                AddBassNotes(notes, scaleIntervals, currentBassRoot, nextBassRoot, section, barStartBeat, random);
                AddDrumNotes(notes, section, bar, barStartBeat, random);

                previousMotif = motif;
            }

            return new CompositionPlan(
                audioProfile.TempoBpm,
                audioProfile.Bars,
                notes);
        }

        private static int[] GetScaleIntervals(ScaleMode mode)
        {
            return mode == ScaleMode.Minor
                ? MinorScaleIntervals
                : MajorScaleIntervals;
        }

        private static int GetMidiRoot(MusicalKey key, int octave)
        {
            return ((int)key) + (octave + MidiOctaveOffset) * SemitonesPerOctave;
        }

        private static SectionType[] BuildSectionPlan(int bars)
        {
            var sections = new SectionType[bars];

            if (bars == 1)
            {
                sections[0] = SectionType.Main;
                return sections;
            }

            if (bars <= 4)
            {
                sections[0] = SectionType.Intro;

                for (var i = 1; i < bars - 1; i++)
                {
                    sections[i] = i == bars - 2
                        ? SectionType.Variation
                        : SectionType.Main;
                }

                sections[^1] = SectionType.Outro;
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

        private static int[] BuildBassRoots(
            IReadOnlyList<int> scaleIntervals,
            int tonicBassRoot,
            int bars,
            Random random)
        {
            var progressionDegrees = random.Next(3) switch
            {
                0 => new[] { 0, 4, 5, 3 },
                1 => new[] { 0, 3, 4, 3 },
                _ => new[] { 0, 5, 3, 4 }
            };

            return
            [
                .. Enumerable.Range(0, bars)
                    .Select(index =>
                    {
                        var degree = progressionDegrees[index % progressionDegrees.Length];
                        return ScaleDegreeToMidi(tonicBassRoot, scaleIntervals, degree, 0);
                    })
            ];
        }

        private static LeadMotif CreateBaseLeadMotif(Random random)
        {
            var rhythm = LeadRhythmPatterns[random.Next(LeadRhythmPatterns.Length)];
            var steps = new List<MotifStep>(LeadStepsPerBar);

            var direction = random.Next(2) == 0 ? 1 : -1;
            var currentOffset = 0;

            for (var i = 0; i < LeadStepsPerBar; i++)
            {
                if (!rhythm.Steps[i])
                {
                    steps.Add(new MotifStep(0, true));
                    continue;
                }

                var move = random.Next(100) switch
                {
                    < 45 => 0,
                    < 72 => direction,
                    < 87 => direction * 2,
                    _ => -direction
                };

                currentOffset = Math.Clamp(currentOffset + move, -1, 4);
                steps.Add(new MotifStep(currentOffset));
            }

            return new LeadMotif(steps);
        }

        private static LeadMotif CreateMotifForBar(
            LeadMotif previousMotif,
            SectionType section,
            Random random)
        {
            var variation = PickVariation(section, random);

            return variation switch
            {
                MotifVariationType.Repeat => previousMotif,
                MotifVariationType.TailVariation => ApplyTailVariation(previousMotif, random),
                MotifVariationType.RhythmicVariation => ApplyRhythmicVariation(previousMotif, random),
                MotifVariationType.Simplify => ApplySimplifyVariation(previousMotif),
                MotifVariationType.DirectionFlip => ApplyDirectionFlip(previousMotif),
                _ => previousMotif
            };
        }

        private static MotifVariationType PickVariation(SectionType section, Random random)
        {
            return section switch
            {
                SectionType.Intro => MotifVariationType.Simplify,
                SectionType.Main => random.Next(100) < 70
                    ? MotifVariationType.Repeat
                    : MotifVariationType.TailVariation,
                SectionType.Variation => random.Next(2) == 0
                    ? MotifVariationType.RhythmicVariation
                    : MotifVariationType.DirectionFlip,
                SectionType.Break => MotifVariationType.Simplify,
                SectionType.Outro => MotifVariationType.Simplify,
                _ => MotifVariationType.Repeat
            };
        }

        private static LeadMotif ApplyTailVariation(LeadMotif motif, Random random)
        {
            var steps = motif.Steps
                .Select(step => new MotifStep(step.DegreeOffset, step.IsRest))
                .ToArray();

            for (var i = Math.Max(steps.Length - 2, 0); i < steps.Length; i++)
            {
                if (steps[i].IsRest)
                    continue;

                var delta = random.Next(-1, 2);
                steps[i] = steps[i] with
                {
                    DegreeOffset = Math.Clamp(steps[i].DegreeOffset + delta, -1, 5)
                };
            }

            return new LeadMotif(steps);
        }

        private static LeadMotif ApplyRhythmicVariation(LeadMotif motif, Random random)
        {
            var steps = motif.Steps
                .Select(step => new MotifStep(step.DegreeOffset, step.IsRest))
                .ToArray();

            var index = random.Next(1, steps.Length - 1);

            if (steps[index].IsRest && !steps[index - 1].IsRest)
            {
                steps[index] = new MotifStep(steps[index - 1].DegreeOffset, false);
            }
            else
            {
                steps[index] = steps[index] with { IsRest = true };
            }

            return new LeadMotif(steps);
        }

        private static LeadMotif ApplySimplifyVariation(LeadMotif motif)
        {
            var steps = motif.Steps
                .Select((step, index) =>
                    index % 2 == 1 && !step.IsRest
                        ? step with { IsRest = true }
                        : step)
                .ToArray();

            return new LeadMotif(steps);
        }

        private static LeadMotif ApplyDirectionFlip(LeadMotif motif)
        {
            var steps = motif.Steps
                .Select(step =>
                    step.IsRest
                        ? step
                        : step with { DegreeOffset = -step.DegreeOffset })
                .ToArray();

            return new LeadMotif(steps);
        }

        private static void AddLeadNotes(
            List<NoteEvent> notes,
            IReadOnlyList<int> scaleIntervals,
            int leadRoot,
            LeadMotif motif,
            SectionType section,
            double barStartBeat,
            Random random)
        {
            for (var stepIndex = 0; stepIndex < motif.Steps.Count; stepIndex++)
            {
                var step = motif.Steps[stepIndex];
                if (step.IsRest)
                    continue;

                if (section == SectionType.Break && stepIndex < 4)
                    continue;

                var startBeat = barStartBeat + (stepIndex * EighthNoteDuration);
                var duration = ResolveLeadDuration(motif, stepIndex, section);
                var velocityBase = section == SectionType.Variation
                    ? LeadVariationVelocity
                    : LeadBaseVelocity;
                var velocity = velocityBase + random.NextDouble() * LeadVelocityRange;

                var degree = Math.Max(0, step.DegreeOffset);
                var octaveOffset = step.DegreeOffset < 0 ? -1 : 0;
                var midiNote = ScaleDegreeToMidi(leadRoot, scaleIntervals, degree, octaveOffset);

                notes.Add(new NoteEvent(
                    TrackKind.Lead,
                    midiNote,
                    startBeat,
                    duration,
                    velocity));
            }
        }

        private static double ResolveLeadDuration(
            LeadMotif motif,
            int stepIndex,
            SectionType section)
        {
            if (section == SectionType.Intro || section == SectionType.Outro)
            {
                return 0.75;
            }

            var nextIndex = stepIndex + 1;
            if (nextIndex < motif.Steps.Count && motif.Steps[nextIndex].IsRest)
            {
                return 0.75;
            }

            return 0.45;
        }

        private static void AddBassNotes(
            List<NoteEvent> notes,
            IReadOnlyList<int> scaleIntervals,
            int currentRoot,
            int nextRoot,
            SectionType section,
            double barStartBeat,
            Random random)
        {
            var pattern = PickBassPattern(section, random);

            switch (pattern)
            {
                case BassPatternType.HoldRoot:
                    notes.Add(new NoteEvent(
                        TrackKind.Bass,
                        currentRoot,
                        barStartBeat,
                        BeatsPerBar,
                        BassHoldVelocity));
                    break;

                case BassPatternType.PulseRoot:
                    notes.Add(new NoteEvent(
                        TrackKind.Bass,
                        currentRoot,
                        barStartBeat,
                        HalfBarDuration,
                        BassPulseVelocity + 0.05));

                    notes.Add(new NoteEvent(
                        TrackKind.Bass,
                        currentRoot,
                        barStartBeat + 2.0,
                        HalfBarDuration,
                        BassPulseVelocity));
                    break;

                case BassPatternType.ApproachNextRoot:
                    notes.Add(new NoteEvent(
                        TrackKind.Bass,
                        currentRoot,
                        barStartBeat,
                        3.0,
                        BassHoldVelocity));

                    var approachNote = BuildApproachNote(scaleIntervals, nextRoot);
                    notes.Add(new NoteEvent(
                        TrackKind.Bass,
                        approachNote,
                        barStartBeat + 3.5,
                        0.4,
                        BassApproachVelocity));
                    break;
            }
        }

        private static BassPatternType PickBassPattern(SectionType section, Random random)
        {
            return section switch
            {
                SectionType.Intro => BassPatternType.HoldRoot,
                SectionType.Main => random.Next(100) < 65
                    ? BassPatternType.PulseRoot
                    : BassPatternType.HoldRoot,
                SectionType.Variation => BassPatternType.ApproachNextRoot,
                SectionType.Break => BassPatternType.HoldRoot,
                SectionType.Outro => BassPatternType.HoldRoot,
                _ => BassPatternType.PulseRoot
            };
        }

        private static int BuildApproachNote(
            IReadOnlyList<int> scaleIntervals,
            int nextRoot)
        {
            var pitchClass = nextRoot % SemitonesPerOctave;
            var octave = (nextRoot / SemitonesPerOctave) - MidiOctaveOffset;

            var scalePitchClasses = scaleIntervals
                .Select(interval => interval % SemitonesPerOctave)
                .OrderBy(x => x)
                .ToArray();

            var lowerScalePitchClass = scalePitchClasses
                .LastOrDefault(pc => pc < pitchClass);

            if (lowerScalePitchClass == 0 && !scalePitchClasses.Contains(0) && pitchClass != 0)
            {
                lowerScalePitchClass = scalePitchClasses[^1];
                octave--;
            }
            else if (lowerScalePitchClass == 0 && pitchClass == 0)
            {
                lowerScalePitchClass = scalePitchClasses[^1];
                octave--;
            }

            return lowerScalePitchClass + ((octave + MidiOctaveOffset) * SemitonesPerOctave);
        }

        private static void AddDrumNotes(
            List<NoteEvent> notes,
            SectionType section,
            int barIndex,
            double barStartBeat,
            Random random)
        {
            var pattern = PickDrumPattern(section, random);

            AddKick(notes, pattern, barStartBeat);
            AddSnare(notes, pattern, barStartBeat);
            AddHiHats(notes, pattern, section, barStartBeat, random);

            if ((barIndex + 1) % 4 == 0 && section is not SectionType.Outro)
            {
                AddMiniFill(notes, barStartBeat);
            }
        }

        private static DrumPatternType PickDrumPattern(SectionType section, Random random)
        {
            return section switch
            {
                SectionType.Intro => DrumPatternType.Sparse,
                SectionType.Main => random.Next(100) < 70
                    ? DrumPatternType.Basic
                    : DrumPatternType.Driving,
                SectionType.Variation => DrumPatternType.Driving,
                SectionType.Break => DrumPatternType.Sparse,
                SectionType.Outro => DrumPatternType.Sparse,
                _ => DrumPatternType.Basic
            };
        }

        private static void AddKick(
            List<NoteEvent> notes,
            DrumPatternType pattern,
            double beatOffset)
        {
            var beats = pattern switch
            {
                DrumPatternType.Basic => new[] { 0.0, 2.0 },
                DrumPatternType.Sparse => new[] { 0.0 },
                DrumPatternType.Driving => new[] { 0.0, 1.5, 2.0, 3.0 },
                _ => new[] { 0.0, 2.0 }
            };

            for (var i = 0; i < beats.Length; i++)
            {
                notes.Add(new NoteEvent(
                    TrackKind.Drums,
                    KickMidiNote,
                    beatOffset + beats[i],
                    DrumHitDuration,
                    i == 0 ? KickPrimaryVelocity : KickSecondaryVelocity));
            }
        }

        private static void AddSnare(
            List<NoteEvent> notes,
            DrumPatternType pattern,
            double beatOffset)
        {
            var beats = pattern switch
            {
                DrumPatternType.Basic => new[] { 1.0, 3.0 },
                DrumPatternType.Sparse => new[] { 3.0 },
                DrumPatternType.Driving => new[] { 1.0, 3.0 },
                _ => new[] { 1.0, 3.0 }
            };

            foreach (var beat in beats)
            {
                notes.Add(new NoteEvent(
                    TrackKind.Drums,
                    SnareMidiNote,
                    beatOffset + beat,
                    DrumHitDuration,
                    SnareVelocity));
            }
        }

        private static void AddHiHats(
            List<NoteEvent> notes,
            DrumPatternType pattern,
            SectionType section,
            double beatOffset,
            Random random)
        {
            for (var step = 0; step < HiHatStepsPerBar; step++)
            {
                if (pattern == DrumPatternType.Sparse && step % 2 == 1)
                {
                    continue;
                }

                var startBeat = beatOffset + (step * HiHatStepSize);
                var midiNote = section == SectionType.Variation && step == HiHatStepsPerBar - 1
                    ? OpenHiHatMidiNote
                    : ClosedHiHatMidiNote;
                var velocity = HiHatMinVelocity + random.NextDouble() * HiHatVelocityRange;

                notes.Add(new NoteEvent(
                    TrackKind.Drums,
                    midiNote,
                    startBeat,
                    HiHatHitDuration,
                    velocity));
            }
        }

        private static void AddMiniFill(List<NoteEvent> notes, double barStartBeat)
        {
            var fillStart = barStartBeat + 3.0;

            notes.Add(new NoteEvent(
                TrackKind.Drums,
                SnareMidiNote,
                fillStart + 0.00,
                0.10,
                FillSnareVelocity));

            notes.Add(new NoteEvent(
                TrackKind.Drums,
                SnareMidiNote,
                fillStart + 0.25,
                0.10,
                FillSnareVelocity + 0.04));

            notes.Add(new NoteEvent(
                TrackKind.Drums,
                SnareMidiNote,
                fillStart + 0.50,
                0.10,
                FillSnareVelocity + 0.08));

            notes.Add(new NoteEvent(
                TrackKind.Drums,
                OpenHiHatMidiNote,
                fillStart + 0.75,
                0.12,
                0.66));
        }

        private static int ScaleDegreeToMidi(
            int baseMidi,
            IReadOnlyList<int> scaleIntervals,
            int degree,
            int octaveOffset)
        {
            var scaleLength = scaleIntervals.Count;
            var octaveShift = degree / scaleLength;
            var scaleIndex = degree % scaleLength;

            return baseMidi
                   + scaleIntervals[scaleIndex]
                   + ((octaveShift + octaveOffset) * SemitonesPerOctave);
        }
    }
}