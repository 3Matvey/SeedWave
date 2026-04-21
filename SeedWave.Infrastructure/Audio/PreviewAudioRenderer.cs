using SeedWave.Core.AudioComposition;

namespace SeedWave.Infrastructure.Audio
{
    /// <summary>
    /// Renders a <see cref="CompositionPlan"/> into a simple browser-playable WAV preview.
    /// </summary>
    /// <remarks>
    /// This renderer is intentionally lightweight and deterministic.
    /// It does not attempt physically accurate synthesis or realistic instrument emulation.
    /// Instead, it produces a fast musical preview using:
    /// - a sine-based lead tone;
    /// - a bass tone made from a fundamental plus a quiet sub-oscillator;
    /// - procedural drum sounds for kick, snare, and hi-hat;
    /// - final peak normalization to avoid clipping.
    /// </remarks>
    public class PreviewAudioRenderer
    {
        private const int SampleRate = 44_100;

        private const int BeatsPerBar = 4;
        private const int A4MidiNote = 69;
        private const double A4Frequency = 440.0;
        private const double SemitonesPerOctave = 12.0;

        private const double LeadAmplitude = 0.18;
        private const double BassAmplitude = 0.22;
        private const double DrumAmplitude = 0.40;
        private const double HiHatRelativeAmplitude = 0.45;

        private const int KickMidiNote = 36;
        private const int SnareMidiNote = 38;
        private const int ClosedHiHatMidiNote = 42;

        private const double LeadMaxAttackSeconds = 0.01;
        private const double LeadAttackFraction = 0.15;
        private const double LeadMaxReleaseSeconds = 0.08;
        private const double LeadReleaseFraction = 0.35;

        private const double BassAttackSeconds = 0.01;
        private const double BassMaxReleaseSeconds = 0.12;
        private const double BassReleaseFraction = 0.25;
        private const double BassSustainLevel = 0.90;

        private const double BassFundamentalMix = 0.75;
        private const double BassSubOscillatorMix = 0.35;
        private const double BassSubFrequencyRatio = 0.5;

        private const double KickStartFrequency = 120.0;
        private const double KickFrequencyDropPerSecond = 70.0;
        private const double KickDecayRate = 18.0;

        private const double SnareToneFrequency = 180.0;
        private const double SnareToneMix = 0.25;
        private const double SnareNoiseMix = 0.80;
        private const double SnareDecayRate = 22.0;

        private const double HiHatDecayRate = 60.0;

        private const float NormalizePeakTarget = 0.92f;

        private readonly WavEncoder _wavEncoder;

        public PreviewAudioRenderer(WavEncoder wavEncoder)
        {
            _wavEncoder = wavEncoder ?? throw new ArgumentNullException(nameof(wavEncoder));
        }

        public RenderedAudio Render(CompositionPlan plan, string fileName)
        {
            ArgumentNullException.ThrowIfNull(plan);

            var totalBeats = plan.Bars * BeatsPerBar;
            var totalSeconds = BeatsToSeconds(totalBeats, plan.TempoBpm);
            var sampleCount = (int)Math.Ceiling(totalSeconds * SampleRate);

            var buffer = new float[sampleCount];

            foreach (var note in plan.Notes)
            {
                RenderNote(buffer, note, plan.TempoBpm);
            }

            Normalize(buffer);

            var content = _wavEncoder.Encode(buffer, SampleRate);

            return new RenderedAudio(
                content,
                "audio/wav",
                fileName);
        }

        private static void RenderNote(float[] buffer, NoteEvent note, int tempoBpm)
        {
            var startSeconds = BeatsToSeconds(note.StartBeat, tempoBpm);
            var durationSeconds = BeatsToSeconds(note.DurationBeats, tempoBpm);

            var startSample = (int)(startSeconds * SampleRate);
            var endSample = Math.Min(
                buffer.Length,
                (int)((startSeconds + durationSeconds) * SampleRate));

            if (startSample >= endSample)
            {
                return;
            }

            switch (note.Track)
            {
                case TrackKind.Lead:
                    RenderLead(buffer, note, startSample, endSample, durationSeconds);
                    break;

                case TrackKind.Bass:
                    RenderBass(buffer, note, startSample, endSample, durationSeconds);
                    break;

                case TrackKind.Drums:
                    RenderDrum(buffer, note, startSample, endSample);
                    break;
            }
        }

        /// <summary>
        /// Renders the lead as a plain sine wave shaped by a short attack/release envelope.
        /// This keeps the preview clean and avoids hard clicks at note boundaries.
        /// </summary>
        private static void RenderLead(
            float[] buffer,
            NoteEvent note,
            int startSample,
            int endSample,
            double durationSeconds)
        {
            var frequency = MidiToFrequency(note.MidiNote);

            for (var i = startSample; i < endSample; i++)
            {
                var time = (double)(i - startSample) / SampleRate;
                var envelope = ComputeLeadEnvelope(time, durationSeconds);
                var sample = Math.Sin(2 * Math.PI * frequency * time);

                buffer[i] += (float)(sample * envelope * note.Velocity * LeadAmplitude);
            }
        }

        /// <summary>
        /// Renders the bass as a weighted mix of:
        /// - the main oscillator at the target pitch;
        /// - a quieter sub-oscillator one octave lower.
        /// This produces a fuller low end without requiring a complex synth model.
        /// </summary>
        private static void RenderBass(
            float[] buffer,
            NoteEvent note,
            int startSample,
            int endSample,
            double durationSeconds)
        {
            var frequency = MidiToFrequency(note.MidiNote);

            for (var i = startSample; i < endSample; i++)
            {
                var time = (double)(i - startSample) / SampleRate;
                var envelope = ComputeBassEnvelope(time, durationSeconds);

                var fundamental = Math.Sin(2 * Math.PI * frequency * time);
                var sub = Math.Sin(2 * Math.PI * frequency * BassSubFrequencyRatio * time) * BassSubOscillatorMix;
                var sample = (fundamental * BassFundamentalMix) + sub;

                buffer[i] += (float)(sample * envelope * note.Velocity * BassAmplitude);
            }
        }

        /// <summary>
        /// Dispatches procedural drum rendering based on the General MIDI note used in the composition plan.
        /// </summary>
        private static void RenderDrum(float[] buffer, NoteEvent note, int startSample, int endSample)
        {
            switch (note.MidiNote)
            {
                case KickMidiNote:
                    RenderKick(buffer, note, startSample, endSample);
                    break;

                case SnareMidiNote:
                    RenderSnare(buffer, note, startSample, endSample);
                    break;

                case ClosedHiHatMidiNote:
                    RenderHiHat(buffer, note, startSample, endSample);
                    break;
            }
        }

        /// <summary>
        /// Renders a kick as a decaying sine tone with a downward pitch sweep.
        /// The sweep gives the hit its punchy "thump" character.
        /// </summary>
        private static void RenderKick(float[] buffer, NoteEvent note, int startSample, int endSample)
        {
            for (var i = startSample; i < endSample; i++)
            {
                var time = (double)(i - startSample) / SampleRate;
                var envelope = Math.Exp(-time * KickDecayRate);
                var frequency = KickStartFrequency - (time * KickFrequencyDropPerSecond);
                var sample = Math.Sin(2 * Math.PI * frequency * time);

                buffer[i] += (float)(sample * envelope * note.Velocity * DrumAmplitude);
            }
        }

        /// <summary>
        /// Renders a snare as filtered-looking synthetic noise plus a quiet tonal body.
        /// The random source is seeded from the note start so the preview stays deterministic.
        /// </summary>
        private static void RenderSnare(float[] buffer, NoteEvent note, int startSample, int endSample)
        {
            var random = new Random(startSample);

            for (var i = startSample; i < endSample; i++)
            {
                var time = (double)(i - startSample) / SampleRate;
                var envelope = Math.Exp(-time * SnareDecayRate);
                var noise = (random.NextDouble() * 2.0) - 1.0;
                var tone = Math.Sin(2 * Math.PI * SnareToneFrequency * time) * SnareToneMix;
                var sample = (noise * SnareNoiseMix) + tone;

                buffer[i] += (float)(sample * envelope * note.Velocity * DrumAmplitude);
            }
        }

        /// <summary>
        /// Renders a hi-hat as short, sharply decaying noise.
        /// The random source is seeded deterministically from the note position.
        /// </summary>
        private static void RenderHiHat(float[] buffer, NoteEvent note, int startSample, int endSample)
        {
            var random = new Random(startSample * 17);

            for (var i = startSample; i < endSample; i++)
            {
                var time = (double)(i - startSample) / SampleRate;
                var envelope = Math.Exp(-time * HiHatDecayRate);
                var noise = (random.NextDouble() * 2.0) - 1.0;

                buffer[i] += (float)(noise * envelope * note.Velocity * DrumAmplitude * HiHatRelativeAmplitude);
            }
        }

        /// <summary>
        /// Computes a simple attack/release envelope for lead notes.
        /// Both attack and release are capped so very long notes do not become too soft or too slow.
        /// </summary>
        private static double ComputeLeadEnvelope(double time, double durationSeconds)
        {
            var attack = Math.Min(LeadMaxAttackSeconds, durationSeconds * LeadAttackFraction);
            var release = Math.Min(LeadMaxReleaseSeconds, durationSeconds * LeadReleaseFraction);

            if (time < attack)
            {
                return attack <= 0 ? 1.0 : time / attack;
            }

            if (time > durationSeconds - release)
            {
                var releaseTime = durationSeconds - time;
                return release <= 0 ? 0.0 : Math.Max(0, releaseTime / release);
            }

            return 1.0;
        }

        /// <summary>
        /// Computes a bass envelope with a short attack, a slightly longer release,
        /// and a sustain level below 1.0 for a softer held tone.
        /// </summary>
        private static double ComputeBassEnvelope(double time, double durationSeconds)
        {
            var release = Math.Min(BassMaxReleaseSeconds, durationSeconds * BassReleaseFraction);

            if (time < BassAttackSeconds)
                return time / BassAttackSeconds;

            if (time > durationSeconds - release)
            {
                var releaseTime = durationSeconds - time;
                return release <= 0 ? 0.0 : Math.Max(0, releaseTime / release);
            }

            return BassSustainLevel;
        }

        /// <summary>
        /// Converts a MIDI note number to frequency in hertz using equal temperament with A4 = 440 Hz.
        /// </summary>
        private static double MidiToFrequency(int midiNote)
        {
            return A4Frequency * Math.Pow(2.0, (midiNote - A4MidiNote) / SemitonesPerOctave);
        }

        /// <summary>
        /// Converts beat positions into time in seconds using the composition tempo.
        /// </summary>
        private static double BeatsToSeconds(double beats, int tempoBpm)
        {
            return beats * 60.0 / tempoBpm;
        }

        /// <summary>
        /// Applies peak normalization so the loudest absolute sample reaches the configured target level.
        /// This prevents clipping while keeping the preview reasonably loud.
        /// </summary>
        private static void Normalize(float[] buffer)
        {
            var peak = buffer.Max(sample => Math.Abs(sample));

            if (peak <= 0f)
                return;

            var gain = NormalizePeakTarget / peak;

            for (var i = 0; i < buffer.Length; i++)
                buffer[i] *= gain;
        }
    }
}