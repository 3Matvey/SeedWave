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
        private const byte DrumChannel = 9;

        private const byte LeadProgram = 80;
        private const byte BassProgram = 33;

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
            var midiFile = CreateMidiFile(plan);
            var tempoMap = BuildTempoMap(plan);

            midiFile.ReplaceTempoMap(tempoMap);

            return midiFile;
        }

        private static MidiFile CreateMidiFile(CompositionPlan plan)
        {
            return new MidiFile(
                BuildLeadTrack(plan),
                BuildBassTrack(plan),
                BuildDrumTrack(plan))
            {
                TimeDivision = new TicksPerQuarterNoteTimeDivision(TicksPerBeat)
            };
        }

        private static TempoMap BuildTempoMap(CompositionPlan plan)
        {
            return TempoMap.Create(Tempo.FromBeatsPerMinute(plan.TempoBpm));
        }

        private static TrackChunk BuildLeadTrack(CompositionPlan plan)
        {
            return BuildInstrumentTrack(
                plan,
                TrackKind.Lead,
                "Lead",
                LeadChannel,
                LeadProgram);
        }

        private static TrackChunk BuildBassTrack(CompositionPlan plan)
        {
            return BuildInstrumentTrack(
                plan,
                TrackKind.Bass,
                "Bass",
                BassChannel,
                BassProgram);
        }

        private static TrackChunk BuildDrumTrack(CompositionPlan plan)
        {
            var chunk = new TrackChunk();

            AddTrackName(chunk, "Drums");
            AddNotes(chunk, GetNotes(plan, TrackKind.Drums), DrumChannel);

            return chunk;
        }

        private static TrackChunk BuildInstrumentTrack(
            CompositionPlan plan,
            TrackKind trackKind,
            string name,
            byte channel,
            byte program)
        {
            var chunk = new TrackChunk();

            AddTrackName(chunk, name);
            AddProgramChange(chunk, channel, program);
            AddNotes(chunk, GetNotes(plan, trackKind), channel);

            return chunk;
        }

        private static IEnumerable<CoreNote> GetNotes(
            CompositionPlan plan,
            TrackKind trackKind)
        {
            return plan.Notes
                .Where(note => note.Track == trackKind)
                .OrderBy(note => note.StartBeat);
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
                CreateProgramChange(channel, program),
                0));
        }

        private static ProgramChangeEvent CreateProgramChange(
            byte channel,
            byte program)
        {
            return new ProgramChangeEvent((SevenBitNumber)program)
            {
                Channel = (FourBitNumber)channel
            };
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
                Length = ToTicks(note.DurationBeats),
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
    }
}