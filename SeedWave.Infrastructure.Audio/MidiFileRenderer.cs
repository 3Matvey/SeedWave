using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using SeedWave.Core.AudioComposition;
using SeedWave.Core.Generation;
using CoreNote = SeedWave.Core.AudioComposition.NoteEvent;
using MidiNote = Melanchall.DryWetMidi.Interaction.Note;

namespace SeedWave.Infrastructure.Audio
{
    public sealed class MidiFileRenderer : IMidiRenderer
    {
        private const short TicksPerBeat = 480;

        private const byte LeadChannel = 0;
        private const byte BassChannel = 1;
        private const byte PadChannel = 2;
        private const byte DrumChannel = 9;

        private const byte ControllerVolume = 7;
        private const byte ControllerPan = 10;
        private const byte ControllerExpression = 11;
        private const byte ControllerReverb = 91;
        private const byte ControllerChorus = 93;

        public GeneratedMidi Render(CompositionPlan plan, string fileName)
        {
            ArgumentNullException.ThrowIfNull(plan);

            var midiFile = BuildMidiFile(plan);
            var content = WriteMidiFile(midiFile);
            var midiFileName = BuildMidiFileName(fileName);

            return new GeneratedMidi(content, "audio/midi", midiFileName);
        }

        private static MidiFile BuildMidiFile(CompositionPlan plan)
        {
            var midiFile = new MidiFile(
                BuildLeadTrack(plan),
                BuildBassTrack(plan),
                BuildArpTrack(plan),
                BuildDrumTrack(plan))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(TicksPerBeat)
            };

            midiFile.ReplaceTempoMap(TempoMap.Create(Tempo.FromBeatsPerMinute(plan.TempoBpm)));
            return midiFile;
        }

        private static TrackChunk BuildLeadTrack(CompositionPlan plan)
        {
            var preset = PickLeadPreset(plan);

            return BuildInstrumentTrack(
                plan,
                TrackKind.Lead,
                "Lead",
                LeadChannel,
                preset.Program,
                preset.Volume,
                preset.Pan,
                preset.Expression,
                preset.Reverb,
                preset.Chorus);
        }

        private static TrackChunk BuildBassTrack(CompositionPlan plan)
        {
            var preset = PickBassPreset(plan);

            return BuildInstrumentTrack(
                plan,
                TrackKind.Bass,
                "Bass",
                BassChannel,
                preset.Program,
                preset.Volume,
                preset.Pan,
                preset.Expression,
                preset.Reverb,
                preset.Chorus);
        }

        private static TrackChunk BuildArpTrack(CompositionPlan plan)
        {
            var preset = PickArpPreset(plan);

            return BuildInstrumentTrack(
                plan,
                TrackKind.Pad,
                "Arp",
                PadChannel,
                preset.Program,
                preset.Volume,
                preset.Pan,
                preset.Expression,
                preset.Reverb,
                preset.Chorus);
        }

        private static TrackChunk BuildDrumTrack(CompositionPlan plan)
        {
            var chunk = new TrackChunk();

            AddTrackName(chunk, "Drums");
            AddController(chunk, DrumChannel, ControllerVolume, 112);
            AddController(chunk, DrumChannel, ControllerPan, 64);
            AddController(chunk, DrumChannel, ControllerReverb, 10);
            AddNotes(chunk, GetNotes(plan, TrackKind.Drums), DrumChannel);

            return chunk;
        }

        private static TrackChunk BuildInstrumentTrack(
            CompositionPlan plan,
            TrackKind trackKind,
            string name,
            byte channel,
            byte program,
            byte volume,
            byte pan,
            byte expression,
            byte reverb,
            byte chorus)
        {
            var chunk = new TrackChunk();

            AddTrackName(chunk, name);
            AddProgramChange(chunk, channel, program);
            AddController(chunk, channel, ControllerVolume, volume);
            AddController(chunk, channel, ControllerPan, pan);
            AddController(chunk, channel, ControllerExpression, expression);
            AddController(chunk, channel, ControllerReverb, reverb);
            AddController(chunk, channel, ControllerChorus, chorus);
            AddNotes(chunk, GetNotes(plan, trackKind), channel);

            return chunk;
        }

        private static InstrumentPreset PickLeadPreset(CompositionPlan plan)
        {
            if (plan.TempoBpm >= 100)
            {
                return new InstrumentPreset(
                    Program: 80,   // Square lead
                    Volume: 112,
                    Pan: 68,
                    Expression: 120,
                    Reverb: 14,
                    Chorus: 6);
            }

            return new InstrumentPreset(
                Program: 81,   // Saw lead
                Volume: 108,
                Pan: 68,
                Expression: 116,
                Reverb: 18,
                Chorus: 8);
        }

        private static InstrumentPreset PickBassPreset(CompositionPlan plan)
        {
            if (plan.TempoBpm >= 100)
            {
                return new InstrumentPreset(
                    Program: 38,   // Synth bass
                    Volume: 110,
                    Pan: 64,
                    Expression: 116,
                    Reverb: 4,
                    Chorus: 2);
            }

            return new InstrumentPreset(
                Program: 39,   // Synth bass 2
                Volume: 108,
                Pan: 64,
                Expression: 114,
                Reverb: 6,
                Chorus: 2);
        }

        private static InstrumentPreset PickArpPreset(CompositionPlan plan)
        {
            if (plan.TempoBpm >= 100)
            {
                return new InstrumentPreset(
                    Program: 62,   // Synth Brass 1
                    Volume: 90,
                    Pan: 60,
                    Expression: 102,
                    Reverb: 10,
                    Chorus: 4);
            }

            return new InstrumentPreset(
                Program: 87,   // Bass + Lead
                Volume: 88,
                Pan: 60,
                Expression: 100,
                Reverb: 12,
                Chorus: 6);
        }

        private static IEnumerable<CoreNote> GetNotes(
            CompositionPlan plan,
            TrackKind trackKind)
        {
            return plan.Notes
                .Where(note => note.Track == trackKind)
                .OrderBy(note => note.StartBeat)
                .ThenBy(note => note.MidiNote);
        }

        private static void AddTrackName(TrackChunk chunk, string name)
        {
            using var manager = chunk.ManageTimedEvents();

            manager.Objects.Add(new TimedEvent(
                new SequenceTrackNameEvent(name),
                0));
        }

        private static void AddProgramChange(
            TrackChunk chunk,
            byte channel,
            byte program)
        {
            using var manager = chunk.ManageTimedEvents();

            manager.Objects.Add(new TimedEvent(
                new ProgramChangeEvent((SevenBitNumber)program)
                {
                    Channel = (FourBitNumber)channel
                },
                0));
        }

        private static void AddController(
            TrackChunk chunk,
            byte channel,
            byte controller,
            byte value)
        {
            using var manager = chunk.ManageTimedEvents();

            manager.Objects.Add(new TimedEvent(
                new ControlChangeEvent((SevenBitNumber)controller, (SevenBitNumber)value)
                {
                    Channel = (FourBitNumber)channel
                },
                0));
        }

        private static void AddNotes(
            TrackChunk chunk,
            IEnumerable<CoreNote> notes,
            byte channel)
        {
            using var manager = chunk.ManageNotes();

            foreach (var note in notes)
            {
                manager.Objects.Add(CreateMidiNote(note, channel));
            }
        }

        private static MidiNote CreateMidiNote(CoreNote note, byte channel)
        {
            return new MidiNote((SevenBitNumber)note.MidiNote)
            {
                Time = ToTicks(note.StartBeat),
                Length = Math.Max(ToTicks(note.DurationBeats), 1),
                Velocity = (SevenBitNumber)ToVelocity(note.Velocity),
                Channel = (FourBitNumber)channel
            };
        }

        private static long ToTicks(double beats)
        {
            return (long)Math.Round(beats * TicksPerBeat);
        }

        private static byte ToVelocity(double velocity)
        {
            var value = (int)Math.Round(velocity * 127.0);
            return (byte)Math.Clamp(value, 1, 127);
        }

        private static byte[] WriteMidiFile(MidiFile midiFile)
        {
            using var stream = new MemoryStream();
            midiFile.Write(stream);
            return stream.ToArray();
        }

        private static string BuildMidiFileName(string fileName)
        {
            var stem = Path.GetFileNameWithoutExtension(fileName);
            return $"{stem}.mid";
        }

        private sealed record InstrumentPreset(
            byte Program,
            byte Volume,
            byte Pan,
            byte Expression,
            byte Reverb,
            byte Chorus);
    }
}