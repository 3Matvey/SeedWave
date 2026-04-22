namespace SeedWave.Core.Generation
{
    /// <summary>
    /// Builds a deterministic visual profile for cover rendering from the audio profile and seed.
    /// </summary>
    public sealed class CoverProfileBuilder
    {
        public CoverProfile Build(AudioProfile audioProfile, int seed)
        {
            ArgumentNullException.ThrowIfNull(audioProfile);

            var random = new Random(HashCode.Combine(
                audioProfile.TempoBpm,
                audioProfile.Bars,
                (int)audioProfile.Key,
                (int)audioProfile.Mode,
                seed));

            var layout = PickLayout(audioProfile, random);
            var palette = PickPalette(audioProfile, random);
            var shapeSeed = HashCode.Combine(seed, audioProfile.TempoBpm, audioProfile.Bars, 13);
            var useGrainOverlay = PickUseGrainOverlay(audioProfile, random);

            return new CoverProfile(
                layout,
                palette.BackgroundColorHex,
                palette.AccentColorHex,
                palette.TextColorHex,
                shapeSeed,
                useGrainOverlay);
        }

        private static CoverLayoutKind PickLayout(AudioProfile audioProfile, Random random)
        {
            if (audioProfile.Bars <= 4)
            {
                return random.Next(2) == 0
                    ? CoverLayoutKind.CenteredTitle
                    : CoverLayoutKind.BottomBand;
            }

            return random.Next(4) switch
            {
                0 => CoverLayoutKind.CenteredTitle,
                1 => CoverLayoutKind.TopLeftStack,
                2 => CoverLayoutKind.BottomBand,
                _ => CoverLayoutKind.SplitVertical
            };
        }

        private static bool PickUseGrainOverlay(AudioProfile audioProfile, Random random)
        {
            if (audioProfile.Mode == ScaleMode.Minor)
            {
                return random.Next(100) < 85;
            }

            return random.Next(100) < 60;
        }

        private static CoverPalette PickPalette(AudioProfile audioProfile, Random random)
        {
            var energetic = audioProfile.TempoBpm >= 125;
            var minor = audioProfile.Mode == ScaleMode.Minor;
            var variant = random.Next(4);

            if (minor && energetic)
            {
                return PickMinorEnergeticPalette(variant);
            }

            if (minor)
            {
                return PickMinorPalette(variant);
            }

            if (energetic)
            {
                return PickEnergeticPalette(variant);
            }

            return PickCalmPalette(variant);
        }

        private static CoverPalette PickMinorEnergeticPalette(int variant)
        {
            return variant switch
            {
                0 => new CoverPalette("#0B0D12", "#7C3AED", "#F3F4F6"),
                1 => new CoverPalette("#090A0F", "#2563EB", "#F5F7FA"),
                2 => new CoverPalette("#0D0B10", "#DB2777", "#FAF7FF"),
                _ => new CoverPalette("#0B0E0E", "#10B981", "#F3FAF8")
            };
        }

        private static CoverPalette PickMinorPalette(int variant)
        {
            return variant switch
            {
                0 => new CoverPalette("#101114", "#6D28D9", "#F4F4F5"),
                1 => new CoverPalette("#121214", "#0891B2", "#F5F5F5"),
                2 => new CoverPalette("#151210", "#EA580C", "#FFF8F1"),
                _ => new CoverPalette("#111215", "#4F46E5", "#F4F7FB")
            };
        }

        private static CoverPalette PickEnergeticPalette(int variant)
        {
            return variant switch
            {
                0 => new CoverPalette("#0D1117", "#EAB308", "#F9FAFB"),
                1 => new CoverPalette("#0A1010", "#14B8A6", "#F3FBFB"),
                2 => new CoverPalette("#120D0F", "#EF4444", "#FFF7F7"),
                _ => new CoverPalette("#11100D", "#F59E0B", "#FFFDF5")
            };
        }

        private static CoverPalette PickCalmPalette(int variant)
        {
            return variant switch
            {
                0 => new CoverPalette("#F5F1EA", "#2563EB", "#111827"),
                1 => new CoverPalette("#F7F7F5", "#16A34A", "#111111"),
                2 => new CoverPalette("#FBF7F0", "#D97706", "#161616"),
                _ => new CoverPalette("#F7F4FA", "#7C3AED", "#15131A")
            };
        }

        private sealed record CoverPalette(
            string BackgroundColorHex,
            string AccentColorHex,
            string TextColorHex);
    }
}