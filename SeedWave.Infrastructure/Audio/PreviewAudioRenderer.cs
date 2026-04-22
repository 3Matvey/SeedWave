using MeltySynth;
using SeedWave.Core.AudioComposition;
using SeedWave.Core.Generation;
using IAudioRenderer = SeedWave.Core.Generation.IAudioRenderer;
using MeltyMidiFile = MeltySynth.MidiFile;

namespace SeedWave.Infrastructure.Audio
{
    public sealed class PreviewAudioRenderer : IAudioRenderer
    {
        private const int SampleRate = 44_100;
        private const int ChannelCount = 2;
        private const int BeatsPerBar = 4;
        private const int TailSeconds = 2;

        private readonly IMidiRenderer _midiRenderer;
        private readonly WavEncoder _wavEncoder;
        private readonly string _soundFontPath;

        public PreviewAudioRenderer(
            IMidiRenderer midiRenderer,
            WavEncoder wavEncoder,
            AudioSettings settings)
        {
            _midiRenderer = midiRenderer;
            _wavEncoder = wavEncoder;
            _soundFontPath = settings.SoundFontPath;
        }

        public GeneratedAudio Render(CompositionPlan plan, string fileName)
        {
            ArgumentNullException.ThrowIfNull(plan);

            var midiBytes = RenderMidi(plan);
            var samples = RenderSamples(plan, midiBytes);
            var wav = _wavEncoder.Encode(samples, SampleRate);

            return new GeneratedAudio(wav, "audio/wav", fileName);
        }

        private GeneratedMidi RenderMidi(CompositionPlan plan)
        {
            return _midiRenderer.Render(plan, "preview.mid");
        }

        private float[] RenderSamples(CompositionPlan plan, GeneratedMidi midi)
        {
            using var soundFontStream = OpenSoundFont();
            using var midiStream = OpenMidi(midi);

            var synthesizer = CreateSynth(soundFontStream);
            var midiFile = ReadMidiFile(midiStream);
            var sequencer = CreateSequencer(synthesizer);

            sequencer.Play(midiFile, false);

            var stereo = CreateStereoBuffer(plan);
            sequencer.RenderInterleaved(stereo);

            return MixToMono(stereo);
        }

        private FileStream OpenSoundFont()
        {
            return File.OpenRead(_soundFontPath);
        }

        private static MemoryStream OpenMidi(GeneratedMidi midi)
        {
            return new MemoryStream(midi.Content, writable: false);
        }

        private static Synthesizer CreateSynth(Stream soundFontStream)
        {
            var soundFont = new SoundFont(soundFontStream);
            var settings = new SynthesizerSettings(SampleRate);

            return new Synthesizer(soundFont, settings);
        }

        private static MeltyMidiFile ReadMidiFile(Stream midiStream)
        {
            return new MeltyMidiFile(midiStream);
        }

        private static MidiFileSequencer CreateSequencer(Synthesizer synthesizer)
        {
            return new MidiFileSequencer(synthesizer);
        }

        private static float[] CreateStereoBuffer(CompositionPlan plan)
        {
            var sampleCount = EstimateSampleCount(plan);
            return new float[sampleCount * ChannelCount];
        }

        private static int EstimateSampleCount(CompositionPlan plan)
        {
            var seconds = EstimateDurationSeconds(plan);
            return (int)Math.Ceiling(seconds * SampleRate);
        }

        private static double EstimateDurationSeconds(CompositionPlan plan)
        {
            var beats = plan.Bars * BeatsPerBar;
            var songSeconds = beats * 60.0 / plan.TempoBpm;

            return songSeconds + TailSeconds;
        }

        private static float[] MixToMono(float[] stereo)
        {
            var mono = new float[stereo.Length / ChannelCount];

            for (var i = 0; i < mono.Length; i++)
            {
                mono[i] = (stereo[i * 2] + stereo[(i * 2) + 1]) * 0.5f;
            }

            return mono;
        }
    }
}