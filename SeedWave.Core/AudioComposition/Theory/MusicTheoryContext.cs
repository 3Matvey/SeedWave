using Melanchall.DryWetMidi.MusicTheory;
using SeedWave.Core.Generation;
using MusicNote = Melanchall.DryWetMidi.MusicTheory.Note;

namespace SeedWave.Core.AudioComposition.Theory
{
    public class MusicTheoryContext
    {
        private readonly NoteName _rootNoteName;
        private readonly ScaleMode _mode;

        public MusicTheoryContext(AudioProfile profile)
        {
            ArgumentNullException.ThrowIfNull(profile);

            _rootNoteName = ToNoteName(profile.Key);
            _mode = profile.Mode;

            var scaleName = profile.Mode == ScaleMode.Minor
                ? "minor"
                : "major";

            Scale = Scale.Parse($"{ToScaleRootString(profile.Key)} {scaleName}");
        }

        public Scale Scale { get; }

        public MusicNote GetScaleNote(int degree, int octave)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(degree);

            var tonic = MusicNote.Get(_rootNoteName, octave);

            return Scale
                .GetAscendingNotes(tonic)
                .Skip(degree)
                .First();
        }

        public IReadOnlyList<MusicNote> GetTriadNotes(int degree, int octave)
        {
            var root = GetScaleNote(degree, octave);
            var quality = GetChordQuality(degree);

            var thirdInterval = quality switch
            {
                ChordQuality.Major => Interval.Four,
                ChordQuality.Minor => Interval.Three,
                ChordQuality.Diminished => Interval.Three,
                _ => Interval.Four
            };

            var fifthInterval = quality switch
            {
                ChordQuality.Diminished => Interval.Six,
                _ => Interval.Seven
            };

            return
            [
                root,
                root + thirdInterval,
                root + fifthInterval
            ];
        }

        public Chord GetTriadChord(int degree)
        {
            var root = GetScaleNote(degree, 4);
            var quality = GetChordQuality(degree);

            return Chord.GetByTriad(root.NoteName, quality);
        }

        public MusicNote GetPreviousScaleNote(MusicNote note)
        {
            ArgumentNullException.ThrowIfNull(note);
            return Scale.GetPreviousNote(note);
        }

        public MusicNote GetNextScaleNote(MusicNote note)
        {
            ArgumentNullException.ThrowIfNull(note);
            return Scale.GetNextNote(note);
        }

        public MusicNote GetApproachNoteToDegree(int degree, int octave)
        {
            var target = GetScaleNote(degree, octave);
            return GetPreviousScaleNote(target);
        }

        private ChordQuality GetChordQuality(int degree)
        {
            var normalizedDegree = degree % 7;

            return _mode switch
            {
                ScaleMode.Major => normalizedDegree switch
                {
                    0 => ChordQuality.Major,
                    1 => ChordQuality.Minor,
                    2 => ChordQuality.Minor,
                    3 => ChordQuality.Major,
                    4 => ChordQuality.Major,
                    5 => ChordQuality.Minor,
                    6 => ChordQuality.Diminished,
                    _ => ChordQuality.Major
                },

                ScaleMode.Minor => normalizedDegree switch
                {
                    0 => ChordQuality.Minor,
                    1 => ChordQuality.Diminished,
                    2 => ChordQuality.Major,
                    3 => ChordQuality.Minor,
                    4 => ChordQuality.Minor,
                    5 => ChordQuality.Major,
                    6 => ChordQuality.Major,
                    _ => ChordQuality.Minor
                },

                _ => ChordQuality.Major
            };
        }

        private static NoteName ToNoteName(MusicalKey key)
        {
            return key switch
            {
                MusicalKey.C => NoteName.C,
                MusicalKey.CSharp => NoteName.CSharp,
                MusicalKey.D => NoteName.D,
                MusicalKey.DSharp => NoteName.DSharp,
                MusicalKey.E => NoteName.E,
                MusicalKey.F => NoteName.F,
                MusicalKey.FSharp => NoteName.FSharp,
                MusicalKey.G => NoteName.G,
                MusicalKey.GSharp => NoteName.GSharp,
                MusicalKey.A => NoteName.A,
                MusicalKey.ASharp => NoteName.ASharp,
                MusicalKey.B => NoteName.B,
                _ => NoteName.C
            };
        }

        private static string ToScaleRootString(MusicalKey key)
        {
            return key switch
            {
                MusicalKey.C => "C",
                MusicalKey.CSharp => "C#",
                MusicalKey.D => "D",
                MusicalKey.DSharp => "D#",
                MusicalKey.E => "E",
                MusicalKey.F => "F",
                MusicalKey.FSharp => "F#",
                MusicalKey.G => "G",
                MusicalKey.GSharp => "G#",
                MusicalKey.A => "A",
                MusicalKey.ASharp => "A#",
                MusicalKey.B => "B",
                _ => "C"
            };
        }
    }
}