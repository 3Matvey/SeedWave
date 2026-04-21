using SeedWave.Core.Generation;
using SeedWave.Core.Regions;

namespace SeedWave.Core.Catalog
{
    /// <summary>
    /// Generates a complete song record by combining deterministic seeds with specialized generators.
    /// </summary>
    public class SongGenerationService(
        ISeedDeriver seedDeriver,
        SongTitleGenerator songTitleGenerator,
        ArtistNameGenerator artistNameGenerator,
        AlbumTitleGenerator albumTitleGenerator,
        GenreGenerator genreGenerator,
        LikesGenerator likesGenerator,
        ReviewTextGenerator reviewTextGenerator
        )
    {
        public SongRecord Generate(SongIdentity identity, double likesAverage, LocaleProfile localeProfile)
        {
            ArgumentNullException.ThrowIfNull(identity);
            ArgumentNullException.ThrowIfNull(localeProfile);

            var title = GenerateTitle(identity, localeProfile);
            var artist = GenerateArtist(identity, localeProfile);
            var album = GenerateAlbum(identity, localeProfile);
            var genre = GenerateGenre(identity, localeProfile);
            var likes = GenerateLikes(identity, likesAverage);
            var reviewText = GenerateReview(identity, localeProfile);

            return CreateSongRecord(identity, title, artist, album, genre, likes, reviewText);
        }

        private string GenerateReview(SongIdentity identity, LocaleProfile localeProfile)
        {
            var seed = seedDeriver.Derive(identity, SeedPurpose.Review);

            return reviewTextGenerator.Generate(localeProfile, seed);
        }

        private string GenerateTitle(SongIdentity identity, LocaleProfile localeProfile)
        {
            var seed = seedDeriver.Derive(identity, SeedPurpose.Title);

            return songTitleGenerator.Generate(localeProfile, seed);
        }

        private string GenerateArtist(SongIdentity identity, LocaleProfile localeProfile)
        {
            var seed = seedDeriver.Derive(identity, SeedPurpose.Artist);

            return artistNameGenerator.Generate(localeProfile, seed);
        }

        private AlbumGenerationResult GenerateAlbum(SongIdentity identity, LocaleProfile localeProfile)
        {
            var seed = seedDeriver.Derive(identity, SeedPurpose.Album);

            return albumTitleGenerator.Generate(localeProfile, seed);
        }

        private string GenerateGenre(SongIdentity identity, LocaleProfile localeProfile)
        {
            var seed = seedDeriver.Derive(identity, SeedPurpose.Genre);

            return genreGenerator.Generate(localeProfile, seed);
        }

        private int GenerateLikes(SongIdentity identity, double likesAverage)
        {
            var seed = seedDeriver.Derive(identity, SeedPurpose.Likes);

            return likesGenerator.Generate(likesAverage, seed);
        }

        private static SongRecord CreateSongRecord
        (
            SongIdentity identity,
            string title,
            string artist,
            AlbumGenerationResult album,
            string genre,
            int likes,
            string reviewText
        )
        {
            var core = new SongCore
            (
                title,
                artist,
                album.Title,
                genre,
                album.IsSingle
            );

            return new SongRecord
            (
                identity,
                core,
                likes,
                reviewText
            );
        }
    }
}