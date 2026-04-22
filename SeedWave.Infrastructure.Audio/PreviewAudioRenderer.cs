using MeltySynth;
using SeedWave.Core.AudioComposition;
using SeedWave.Core.Generation;
using IAudioRenderer = SeedWave.Core.Generation.IAudioRenderer;
using MeltyMidiFile = MeltySynth.MidiFile;

namespace SeedWave.Infrastructure.Audio
{
    public sealed class PreviewAudioRenderer(
        IMidiRenderer midiRenderer,
        WavEncoder wavEncoder,
        AudioSettings settings) : IAudioRenderer
    {
        private const int ChannelCount = 2;
        private const int BeatsPerBar = 4;

        public GeneratedAudio Render(CompositionPlan plan, string fileName)
        {
            ArgumentNullException.ThrowIfNull(plan);

            var midiBytes = RenderMidi(plan);
            var stereoSamples = RenderStereoSamples(plan, midiBytes);

            PostProcess(
                stereoSamples,
                settings.SampleRate,
                ChannelCount,
                settings.TargetPeak,
                settings.SoftClipDrive);

            var wav = wavEncoder.EncodeInterleaved(
                stereoSamples,
                settings.SampleRate,
                ChannelCount);

            return new GeneratedAudio(wav, "audio/wav", fileName);
        }

        private GeneratedMidi RenderMidi(CompositionPlan plan)
        {
            return midiRenderer.Render(plan, "preview.mid");
        }

        private float[] RenderStereoSamples(CompositionPlan plan, GeneratedMidi midi)
        {
            using var soundFontStream = File.OpenRead(settings.SoundFontPath);
            using var midiStream = new MemoryStream(midi.Content, writable: false);

            var soundFont = new SoundFont(soundFontStream);
            var synthSettings = CreateSynthSettings(settings.SampleRate);
            var synthesizer = new Synthesizer(soundFont, synthSettings);
            var midiFile = new MeltyMidiFile(midiStream);
            var sequencer = new MidiFileSequencer(synthesizer);

            sequencer.Play(midiFile, false);

            var stereo = new float[EstimateSampleCount(plan) * ChannelCount];
            sequencer.RenderInterleaved(stereo);

            return stereo;
        }

        private SynthesizerSettings CreateSynthSettings(int sampleRate)
        {
            return new SynthesizerSettings(sampleRate)
            {
                EnableReverbAndChorus = true
            };
        }

        private int EstimateSampleCount(CompositionPlan plan)
        {
            var beats = plan.Bars * BeatsPerBar;
            var songSeconds = beats * 60.0 / plan.TempoBpm;
            var totalSeconds = songSeconds + settings.TailSeconds;

            return (int)Math.Ceiling(totalSeconds * settings.SampleRate);
        }

        private static void PostProcess(
            float[] interleaved,
            int sampleRate,
            int channelCount,
            float targetPeak,
            float softClipDrive)
        {
            if (interleaved.Length == 0)
            {
                return;
            }

            ApplyFadeIn(interleaved, sampleRate, channelCount, 0.012);
            ApplyFadeOut(interleaved, sampleRate, channelCount, 1.20);
            ApplyStereoWidthTrim(interleaved, 0.94f);
            NormalizeToPeak(interleaved, targetPeak);
            ApplySoftClip(interleaved, softClipDrive);
        }

        private static void ApplyFadeIn(
            float[] interleaved,
            int sampleRate,
            int channelCount,
            double seconds)
        {
            var frames = Math.Min(
                interleaved.Length / channelCount,
                (int)(sampleRate * seconds));

            for (var frame = 0; frame < frames; frame++)
            {
                var gain = (float)frame / Math.Max(frames, 1);

                for (var channel = 0; channel < channelCount; channel++)
                {
                    var index = (frame * channelCount) + channel;
                    interleaved[index] *= gain;
                }
            }
        }

        private static void ApplyFadeOut(
            float[] interleaved,
            int sampleRate,
            int channelCount,
            double seconds)
        {
            var totalFrames = interleaved.Length / channelCount;
            var fadeFrames = Math.Min(totalFrames, (int)(sampleRate * seconds));
            var fadeStart = totalFrames - fadeFrames;

            for (var frame = fadeStart; frame < totalFrames; frame++)
            {
                var t = (float)(frame - fadeStart) / Math.Max(fadeFrames, 1);
                var gain = 1.0f - t;

                for (var channel = 0; channel < channelCount; channel++)
                {
                    var index = (frame * channelCount) + channel;
                    interleaved[index] *= gain;
                }
            }
        }

        private static void ApplyStereoWidthTrim(float[] interleaved, float sideScale)
        {
            for (var i = 0; i < interleaved.Length - 1; i += 2)
            {
                var left = interleaved[i];
                var right = interleaved[i + 1];

                var mid = (left + right) * 0.5f;
                var side = (left - right) * 0.5f * sideScale;

                interleaved[i] = mid + side;
                interleaved[i + 1] = mid - side;
            }
        }

        private static void NormalizeToPeak(float[] samples, float targetPeak)
        {
            var peak = 0f;

            for (var i = 0; i < samples.Length; i++)
            {
                var value = Math.Abs(samples[i]);
                if (value > peak)
                {
                    peak = value;
                }
            }

            if (peak < 0.0001f)
            {
                return;
            }

            var gain = targetPeak / peak;

            for (var i = 0; i < samples.Length; i++)
            {
                samples[i] *= gain;
            }
        }

        private static void ApplySoftClip(float[] samples, float drive)
        {
            var normalizer = MathF.Tanh(drive);

            for (var i = 0; i < samples.Length; i++)
            {
                var x = samples[i] * drive;
                samples[i] = MathF.Tanh(x) / normalizer;
            }
        }
    }
}