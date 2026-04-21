using SeedWave.Core.Regions;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeedWave.Core.Generation
{
    public class ReviewTextGenerator
    {
        public string Generate(LocaleProfile localeProfile, int seed)
        {
            ArgumentNullException.ThrowIfNull(localeProfile);

            ValidateLexicon(localeProfile.Reviews);

            var random = new Random(seed);

            return BuildReview(localeProfile.Reviews, random);
        }

        private static void ValidateLexicon(ReviewLexicon reviews)
        {
            if (reviews.Openings.Count == 0)
                throw new InvalidOperationException("Review opening lexicon cannot be empty.");

            if (reviews.MoodDescriptors.Count == 0)
                throw new InvalidOperationException("Review mood lexicon cannot be empty.");

            if (reviews.ArrangementDescriptors.Count == 0)
                throw new InvalidOperationException("Review arrangement lexicon cannot be empty.");

            if (reviews.Closings.Count == 0)
                throw new InvalidOperationException("Review closing lexicon cannot be empty.");
        }

        private static string BuildReview(ReviewLexicon reviews, Random random)
        {
            var opening = reviews.Openings[random.Next(reviews.Openings.Count)];
            var mood = reviews.MoodDescriptors[random.Next(reviews.MoodDescriptors.Count)];
            var arrangement = reviews.ArrangementDescriptors[random.Next(reviews.ArrangementDescriptors.Count)];
            var closing = reviews.Closings[random.Next(reviews.Closings.Count)];

            return $"{opening} {mood}, {arrangement}. {closing}";
        }
    }
}
