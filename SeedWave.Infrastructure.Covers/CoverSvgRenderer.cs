using System.Globalization;
using System.Text;
using SeedWave.Core.Generation;

namespace SeedWave.Infrastructure.Covers
{
    public sealed class CoverSvgRenderer : ICoverRenderer
    {
        private const int Width = 1400;
        private const int Height = 1400;

        private const int Margin = 110;
        private const int SafeTextWidth = 900;
        private const int ContourCount = 6;
        private const int GrainDotCount = 140;
        private const int FocalPointCount = 18;

        public GeneratedCover Render(
            CoverProfile coverProfile,
            AudioProfile audioProfile,
            int seed,
            string title,
            string artist,
            string fileNameWithoutExtension)
        {
            ArgumentNullException.ThrowIfNull(coverProfile);
            ArgumentNullException.ThrowIfNull(audioProfile);

            var safeTitle = title ?? "Untitled";
            var safeArtist = artist ?? "SeedWave";
            var safeFileName = NormalizeFileName(fileNameWithoutExtension);

            var palette = BuildPalette(coverProfile);
            var svg = BuildSvg(coverProfile, audioProfile, seed, safeTitle, safeArtist, palette);

            return new GeneratedCover(
                Encoding.UTF8.GetBytes(svg),
                "image/svg+xml",
                $"{safeFileName}.svg");
        }

        private static string NormalizeFileName(string fileNameWithoutExtension)
        {
            return string.IsNullOrWhiteSpace(fileNameWithoutExtension)
                ? "cover"
                : fileNameWithoutExtension;
        }

        private static string BuildSvg(
            CoverProfile coverProfile,
            AudioProfile audioProfile,
            int seed,
            string title,
            string artist,
            CoverPalette palette)
        {
            var wavePath = BuildWavePath(audioProfile, seed);
            var contours = BuildContourPaths(seed, ContourCount);
            var focalShape = BuildFocalShape(audioProfile, coverProfile.ShapeSeed);
            var grainDots = coverProfile.UseGrainOverlay
                ? BuildGrainDots(seed, GrainDotCount)
                : [];

            var sb = new StringBuilder();
            AppendSvgOpen(sb);
            AppendBackground(sb, palette);
            AppendDefinitions(sb, palette);
            AppendGlow(sb, coverProfile.Layout);
            AppendGrain(sb, grainDots, palette);
            AppendContours(sb, contours, palette);
            AppendWave(sb, wavePath);
            AppendFocalShape(sb, focalShape, palette);
            AppendTextLayout(sb, coverProfile, title, artist, audioProfile, seed, palette);
            AppendSvgClose(sb);

            return sb.ToString();
        }

        private static CoverPalette BuildPalette(CoverProfile coverProfile)
        {
            var background = coverProfile.BackgroundColorHex;
            var accent = coverProfile.AccentColorHex;
            var foreground = coverProfile.TextColorHex;

            return new CoverPalette(
                Background: background,
                Foreground: foreground,
                ForegroundSoft: WithOpacityHex(foreground, "CC"),
                Accent: accent,
                AccentSoft: WithOpacityHex(accent, "CC"),
                ShapeFill: BlendHex(background, accent, 0.18),
                ShapeStroke: BlendHex(foreground, accent, 0.40),
                Line: BlendHex(foreground, accent, 0.28));
        }

        private static void AppendSvgOpen(StringBuilder sb)
        {
            sb.AppendLine($"""
                <svg xmlns="http://www.w3.org/2000/svg" width="{Width}" height="{Height}" viewBox="0 0 {Width} {Height}" fill="none">
                """);
        }

        private static void AppendSvgClose(StringBuilder sb)
        {
            sb.AppendLine("</svg>");
        }

        private static void AppendBackground(StringBuilder sb, CoverPalette palette)
        {
            sb.AppendLine($"""
                <rect width="{Width}" height="{Height}" fill="{palette.Background}" />
                """);
        }

        private static void AppendDefinitions(StringBuilder sb, CoverPalette palette)
        {
            sb.AppendLine($"""
                <defs>
                  <linearGradient id="bgGlow" x1="0" y1="0" x2="1" y2="1">
                    <stop offset="0%" stop-color="{palette.AccentSoft}" stop-opacity="0.18" />
                    <stop offset="100%" stop-color="{palette.Accent}" stop-opacity="0.04" />
                  </linearGradient>
                  <linearGradient id="waveGradient" x1="0" y1="0" x2="1" y2="1">
                    <stop offset="0%" stop-color="{palette.Accent}" stop-opacity="0.92" />
                    <stop offset="100%" stop-color="{palette.AccentSoft}" stop-opacity="0.68" />
                  </linearGradient>
                  <filter id="softBlur">
                    <feGaussianBlur stdDeviation="16" />
                  </filter>
                </defs>
                """);
        }

        private static void AppendGlow(StringBuilder sb, CoverLayoutKind layout)
        {
            var glow = ResolveGlowPosition(layout);

            sb.AppendLine($"""
                <circle cx="{F(glow.X)}" cy="{F(glow.Y)}" r="{F(glow.Radius)}" fill="url(#bgGlow)" filter="url(#softBlur)" />
                """);
        }

        private static GlowPosition ResolveGlowPosition(CoverLayoutKind layout)
        {
            return layout switch
            {
                CoverLayoutKind.CenteredTitle => new GlowPosition(Width * 0.50, Height * 0.28, 260),
                CoverLayoutKind.TopLeftStack => new GlowPosition(Width * 0.72, Height * 0.30, 280),
                CoverLayoutKind.BottomBand => new GlowPosition(Width * 0.68, Height * 0.26, 280),
                CoverLayoutKind.SplitVertical => new GlowPosition(Width * 0.72, Height * 0.50, 300),
                _ => new GlowPosition(Width * 0.68, Height * 0.30, 280)
            };
        }

        private static void AppendGrain(
            StringBuilder sb,
            IReadOnlyList<GrainDot> grainDots,
            CoverPalette palette)
        {
            foreach (var dot in grainDots)
            {
                AppendGrainDot(sb, dot, palette);
            }
        }

        private static void AppendGrainDot(
            StringBuilder sb,
            GrainDot dot,
            CoverPalette palette)
        {
            sb.AppendLine($"""
                <circle cx="{F(dot.X)}" cy="{F(dot.Y)}" r="{F(dot.Radius)}" fill="{palette.Foreground}" opacity="{F(dot.Opacity)}" />
                """);
        }

        private static void AppendContours(
            StringBuilder sb,
            IReadOnlyList<string> contourPaths,
            CoverPalette palette)
        {
            foreach (var contour in contourPaths)
            {
                AppendContour(sb, contour, palette);
            }
        }

        private static void AppendContour(
            StringBuilder sb,
            string contour,
            CoverPalette palette)
        {
            sb.AppendLine($"""
                <path d="{contour}" stroke="{palette.Line}" stroke-opacity="0.32" stroke-width="2.2" fill="none" />
                """);
        }

        private static void AppendWave(StringBuilder sb, string wavePath)
        {
            sb.AppendLine($"""
                <path d="{wavePath}" fill="none" stroke="url(#waveGradient)" stroke-width="10" stroke-linecap="round" />
                """);
        }

        private static void AppendFocalShape(
            StringBuilder sb,
            string focalShape,
            CoverPalette palette)
        {
            sb.AppendLine($"""
                <path d="{focalShape}" fill="{palette.ShapeFill}" fill-opacity="0.92" stroke="{palette.ShapeStroke}" stroke-width="4" />
                """);
        }

        private static void AppendTextLayout(
            StringBuilder sb,
            CoverProfile coverProfile,
            string title,
            string artist,
            AudioProfile audioProfile,
            int seed,
            CoverPalette palette)
        {
            var layout = ResolveTextLayout(coverProfile.Layout);

            AppendTextBackdrop(sb, layout, palette);
            AppendTitleText(sb, layout, title, palette);
            AppendArtistText(sb, layout, artist, palette);
            AppendMetaText(sb, layout, audioProfile, seed, palette);
        }

        private static TextLayout ResolveTextLayout(CoverLayoutKind layout)
        {
            return layout switch
            {
                CoverLayoutKind.CenteredTitle => new TextLayout(
                    PanelX: 170,
                    PanelY: 1030,
                    PanelWidth: 1060,
                    PanelHeight: 210,
                    TitleX: 700,
                    TitleY: 1110,
                    ArtistX: 700,
                    ArtistY: 1170,
                    MetaX: 700,
                    MetaY: 1215,
                    TitleAnchor: "middle",
                    ArtistAnchor: "middle",
                    MetaAnchor: "middle"),

                CoverLayoutKind.TopLeftStack => new TextLayout(
                    PanelX: Margin,
                    PanelY: 95,
                    PanelWidth: 700,
                    PanelHeight: 210,
                    TitleX: Margin,
                    TitleY: 180,
                    ArtistX: Margin,
                    ArtistY: 240,
                    MetaX: Margin,
                    MetaY: 285,
                    TitleAnchor: "start",
                    ArtistAnchor: "start",
                    MetaAnchor: "start"),

                CoverLayoutKind.BottomBand => new TextLayout(
                    PanelX: 0,
                    PanelY: Height - 250,
                    PanelWidth: Width,
                    PanelHeight: 250,
                    TitleX: Margin,
                    TitleY: Height - 145,
                    ArtistX: Margin,
                    ArtistY: Height - 90,
                    MetaX: Margin,
                    MetaY: Height - 45,
                    TitleAnchor: "start",
                    ArtistAnchor: "start",
                    MetaAnchor: "start"),

                CoverLayoutKind.SplitVertical => new TextLayout(
                    PanelX: 80,
                    PanelY: 120,
                    PanelWidth: 480,
                    PanelHeight: Height - 240,
                    TitleX: 140,
                    TitleY: 260,
                    ArtistX: 140,
                    ArtistY: 320,
                    MetaX: 140,
                    MetaY: 380,
                    TitleAnchor: "start",
                    ArtistAnchor: "start",
                    MetaAnchor: "start"),

                _ => new TextLayout(
                    PanelX: Margin,
                    PanelY: Height - 350,
                    PanelWidth: SafeTextWidth,
                    PanelHeight: 220,
                    TitleX: Margin,
                    TitleY: Height - 220,
                    ArtistX: Margin,
                    ArtistY: Height - 155,
                    MetaX: Margin,
                    MetaY: Height - 95,
                    TitleAnchor: "start",
                    ArtistAnchor: "start",
                    MetaAnchor: "start")
            };
        }

        private static void AppendTextBackdrop(
            StringBuilder sb,
            TextLayout layout,
            CoverPalette palette)
        {
            sb.AppendLine($"""
                <rect x="{F(layout.PanelX)}" y="{F(layout.PanelY)}" width="{F(layout.PanelWidth)}" height="{F(layout.PanelHeight)}" fill="{palette.Background}" fill-opacity="0.16" />
                """);
        }

        private static void AppendTitleText(
            StringBuilder sb,
            TextLayout layout,
            string title,
            CoverPalette palette)
        {
            sb.AppendLine($"""
                <text x="{F(layout.TitleX)}" y="{F(layout.TitleY)}" text-anchor="{layout.TitleAnchor}" fill="{palette.Foreground}" font-family="Inter, Segoe UI, Arial, sans-serif" font-size="84" font-weight="800" letter-spacing="-2">
                  {EscapeXml(ClampText(title, 26))}
                </text>
                """);
        }

        private static void AppendArtistText(
            StringBuilder sb,
            TextLayout layout,
            string artist,
            CoverPalette palette)
        {
            sb.AppendLine($"""
                <text x="{F(layout.ArtistX)}" y="{F(layout.ArtistY)}" text-anchor="{layout.ArtistAnchor}" fill="{palette.ForegroundSoft}" font-family="Inter, Segoe UI, Arial, sans-serif" font-size="34" font-weight="600" letter-spacing="1.5">
                  {EscapeXml(ClampText(artist.ToUpperInvariant(), 32))}
                </text>
                """);
        }

        private static void AppendMetaText(
            StringBuilder sb,
            TextLayout layout,
            AudioProfile audioProfile,
            int seed,
            CoverPalette palette)
        {
            var meta = BuildMetaLine(audioProfile, seed);

            sb.AppendLine($"""
                <text x="{F(layout.MetaX)}" y="{F(layout.MetaY)}" text-anchor="{layout.MetaAnchor}" fill="{palette.ForegroundSoft}" font-family="Inter, Segoe UI, Arial, sans-serif" font-size="24" font-weight="500" letter-spacing="2">
                  {EscapeXml(meta)}
                </text>
                """);
        }

        private static string BuildMetaLine(AudioProfile profile, int seed)
        {
            return $"BPM {profile.TempoBpm} · {profile.Mode.ToString().ToUpperInvariant()} · {profile.Key.ToString().ToUpperInvariant()} · SEED {seed}";
        }

        private static string BuildWavePath(AudioProfile profile, int seed)
        {
            var random = new Random(seed * 17 + 3);
            var config = BuildWaveConfig(profile, random);
            var sb = new StringBuilder();

            for (var i = 0; i <= 220; i++)
            {
                AppendWavePoint(sb, config, i);
            }

            return sb.ToString();
        }

        private static WaveConfig BuildWaveConfig(AudioProfile profile, Random random)
        {
            var left = 90.0;
            var right = Width - 90.0;
            var centerY = Height * 0.42;
            var span = right - left;

            var tempoFactor = Math.Clamp((profile.TempoBpm - 70.0) / 90.0, 0.0, 1.0);
            var barsFactor = Math.Clamp((profile.Bars - 4.0) / 12.0, 0.0, 1.0);
            var amplitude = 120 + (tempoFactor * 90) + (barsFactor * 40);

            return new WaveConfig(
                left,
                span,
                centerY,
                amplitude,
                1.1 + random.NextDouble() * 1.8,
                2.2 + random.NextDouble() * 2.6,
                4.5 + random.NextDouble() * 2.0,
                random.NextDouble() * Math.PI * 2,
                random.NextDouble() * Math.PI * 2,
                random.NextDouble() * Math.PI * 2);
        }

        private static void AppendWavePoint(StringBuilder sb, WaveConfig config, int index)
        {
            var t = index / 220.0;
            var x = config.Left + (config.Span * t);
            var y = ComputeWaveY(config, t);
            var command = index == 0 ? "M" : "L";

            sb.Append($"{command} {F(x)} {F(y)} ");
        }

        private static double ComputeWaveY(WaveConfig config, double t)
        {
            var y = config.CenterY
                    + Math.Sin((t * Math.PI * 2 * config.F1) + config.P1) * config.Amplitude * 0.48
                    + Math.Sin((t * Math.PI * 2 * config.F2) + config.P2) * config.Amplitude * 0.22
                    + Math.Sin((t * Math.PI * 2 * config.F3) + config.P3) * config.Amplitude * 0.10;

            return y + Math.Sin((t * Math.PI * 2) - Math.PI / 2) * 40;
        }

        private static List<string> BuildContourPaths(int seed, int count)
        {
            var paths = new List<string>(count);
            var baseSeed = seed * 31 + 9;

            for (var layer = 0; layer < count; layer++)
            {
                paths.Add(BuildContourPath(baseSeed, layer));
            }

            return paths;
        }

        private static string BuildContourPath(int baseSeed, int layer)
        {
            var random = new Random(baseSeed + layer * 101);
            var config = BuildContourConfig(layer, random);
            var sb = new StringBuilder();

            for (var i = 0; i <= 120; i++)
            {
                AppendContourPoint(sb, config, i);
            }

            return sb.ToString();
        }

        private static ContourConfig BuildContourConfig(int layer, Random random)
        {
            return new ContourConfig(
                70.0,
                Width - 140.0,
                (Height * 0.24) + (layer * 95),
                24 + (layer * 6),
                1.4 + (random.NextDouble() * 2.8),
                random.NextDouble() * Math.PI * 2);
        }

        private static void AppendContourPoint(StringBuilder sb, ContourConfig config, int index)
        {
            var t = index / 120.0;
            var x = config.Left + (t * config.Width);
            var y = ComputeContourY(config, t);
            var command = index == 0 ? "M" : "L";

            sb.Append($"{command} {F(x)} {F(y)} ");
        }

        private static double ComputeContourY(ContourConfig config, double t)
        {
            var primary = Math.Sin((t * Math.PI * 2 * config.Frequency) + config.Phase) * config.Amplitude;
            var secondary = Math.Sin((t * Math.PI * 2 * (config.Frequency * 0.5)) + config.Phase * 0.7) * (config.Amplitude * 0.35);

            return config.CenterY + primary + secondary;
        }

        private static string BuildFocalShape(AudioProfile profile, int shapeSeed)
        {
            var random = new Random(shapeSeed);
            var config = BuildFocalShapeConfig(profile);
            var sb = new StringBuilder();

            for (var i = 0; i < FocalPointCount; i++)
            {
                AppendFocalPoint(sb, config, random, i, FocalPointCount);
            }

            sb.Append('Z');
            return sb.ToString();
        }

        private static FocalShapeConfig BuildFocalShapeConfig(AudioProfile profile)
        {
            var centerX = Width * (profile.Mode == ScaleMode.Minor ? 0.68 : 0.64);
            var centerY = Height * (profile.TempoBpm >= 120 ? 0.38 : 0.42);
            var baseRadius = profile.TempoBpm >= 120 ? 180 : 210;

            return new FocalShapeConfig(centerX, centerY, baseRadius);
        }

        private static void AppendFocalPoint(
            StringBuilder sb,
            FocalShapeConfig config,
            Random random,
            int index,
            int totalPoints)
        {
            var angle = (Math.PI * 2 * index) / totalPoints;
            var radius = ComputeFocalRadius(random, config.BaseRadius, angle);
            var x = config.CenterX + Math.Cos(angle) * radius;
            var y = config.CenterY + Math.Sin(angle) * radius * 0.88;
            var command = index == 0 ? "M" : "L";

            sb.Append($"{command} {F(x)} {F(y)} ");
        }

        private static double ComputeFocalRadius(Random random, double baseRadius, double angle)
        {
            var wave = Math.Sin(angle * 3 + random.NextDouble()) * 18;
            var wave2 = Math.Cos(angle * 5 + random.NextDouble()) * 12;
            var jitter = (random.NextDouble() * 22) - 11;

            return baseRadius + wave + wave2 + jitter;
        }

        private static List<GrainDot> BuildGrainDots(int seed, int count)
        {
            var random = new Random(seed * 43 + 1);
            var dots = new List<GrainDot>(count);

            for (var i = 0; i < count; i++)
            {
                dots.Add(CreateGrainDot(random));
            }

            return dots;
        }

        private static GrainDot CreateGrainDot(Random random)
        {
            return new GrainDot(
                random.NextDouble() * Width,
                random.NextDouble() * Height,
                0.6 + random.NextDouble() * 1.8,
                0.04 + random.NextDouble() * 0.10);
        }

        private static string BlendHex(string baseHex, string accentHex, double accentWeight)
        {
            var baseRgb = ParseRgb(baseHex);
            var accentRgb = ParseRgb(accentHex);

            var r = Lerp(baseRgb.R, accentRgb.R, accentWeight);
            var g = Lerp(baseRgb.G, accentRgb.G, accentWeight);
            var b = Lerp(baseRgb.B, accentRgb.B, accentWeight);

            return ToHex(r, g, b);
        }

        private static string WithOpacityHex(string hex, string alpha)
        {
            return NormalizeHex(hex) + alpha;
        }

        private static RgbColor ParseRgb(string hex)
        {
            var normalized = NormalizeHex(hex);

            var r = Convert.ToInt32(normalized[1..3], 16);
            var g = Convert.ToInt32(normalized[3..5], 16);
            var b = Convert.ToInt32(normalized[5..7], 16);

            return new RgbColor(r, g, b);
        }

        private static string NormalizeHex(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                return "#000000";
            }

            var value = hex.Trim();

            if (!value.StartsWith('#'))
            {
                value = "#" + value;
            }

            return value.Length == 7 ? value : "#000000";
        }

        private static int Lerp(int from, int to, double weight)
        {
            return (int)Math.Round(from + ((to - from) * weight));
        }

        private static string ToHex(int r, int g, int b)
        {
            return $"#{ClampByte(r):X2}{ClampByte(g):X2}{ClampByte(b):X2}";
        }

        private static int ClampByte(int value)
        {
            return Math.Clamp(value, 0, 255);
        }

        private static string ClampText(string text, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var trimmed = text.Trim();
            return trimmed.Length <= maxLength
                ? trimmed
                : trimmed[..(maxLength - 1)] + "…";
        }

        private static string EscapeXml(string value)
        {
            return value
                .Replace("&", "&amp;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal)
                .Replace("\"", "&quot;", StringComparison.Ordinal)
                .Replace("'", "&apos;", StringComparison.Ordinal);
        }

        private static string F(double value)
        {
            return value.ToString("0.###", CultureInfo.InvariantCulture);
        }

        private record CoverPalette(
            string Background,
            string Foreground,
            string ForegroundSoft,
            string Accent,
            string AccentSoft,
            string ShapeFill,
            string ShapeStroke,
            string Line);

        private record GrainDot(
            double X,
            double Y,
            double Radius,
            double Opacity);

        private record WaveConfig(
            double Left,
            double Span,
            double CenterY,
            double Amplitude,
            double F1,
            double F2,
            double F3,
            double P1,
            double P2,
            double P3);

        private record ContourConfig(
            double Left,
            double Width,
            double CenterY,
            double Amplitude,
            double Frequency,
            double Phase);

        private record FocalShapeConfig(
            double CenterX,
            double CenterY,
            double BaseRadius);

        private record GlowPosition(
            double X,
            double Y,
            double Radius);

        private record TextLayout(
            double PanelX,
            double PanelY,
            double PanelWidth,
            double PanelHeight,
            double TitleX,
            double TitleY,
            double ArtistX,
            double ArtistY,
            double MetaX,
            double MetaY,
            string TitleAnchor,
            string ArtistAnchor,
            string MetaAnchor);

        private record RgbColor(
            int R,
            int G,
            int B);
    }
}