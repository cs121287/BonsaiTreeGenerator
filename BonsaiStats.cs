using System;
using System.Collections.Generic;

namespace BonsaiTreeGenerator
{
    /// <summary>
    /// Generates random stats for each realistic wavy bonsai tree created with improved 3-step process
    /// </summary>
    public class BonsaiStats
    {
        private readonly Random random;

        public string Name { get; private set; }
        public string Age { get; private set; }
        public List<string> Likes { get; private set; }
        public List<string> Dislikes { get; private set; }

        private readonly string[] names = [
            "Rupert", "Lester", "Jimbo", "Chester", "Bubba", "Milt", "Goober", "Zeke", "Bozo", "Cletus",
            "Rufus", "Jethro", "Buck", "Moe", "Binky", "Elmer", "Bertie", "Scooter", "Otis", "Hobart",
            "Waldo", "Barney", "Clovis", "Norbert", "Floyd", "Gomer", "Corky", "Roscoe", "Milford", "Nubs",
            "Eustace", "Buford", "Pip", "Boris", "Hank", "Earl", "Tad", "Mort",
            "Pickle", "Duke", "Snuffy", "Hershel", "Gus", "Toby", "Willy", "Ziggy"
        ];

        private readonly string[] likeNouns = [
            "kids", "dogs", "hotdogs", "sunshine", "rain", "music", "books", "flowers", "cookies",
            "birds", "butterflies", "pizza", "ice cream", "cats", "hugs", "laughter", "games",
            "movies", "dancing", "singing", "art", "nature", "beaches", "mountains", "stars",
            "pancakes", "chocolate", "tea", "coffee", "naps", "gardens", "rainbows", "friends",
            "meditation", "zen", "peace", "harmony", "balance", "tranquility", "wisdom", "growth",
            "wavy curves", "gentle breezes", "morning dew", "sunset colors", "flowing water"
        ];

        private readonly string[] dislikeNouns = [
            "heat", "ants", "talk radio", "loud noises", "spiders", "vegetables", "homework",
            "traffic", "waiting", "mondays", "dentists", "scary movies", "brussels sprouts",
            "wasps", "mosquitoes", "cold showers", "alarm clocks", "taxes", "bullies", "liars",
            "storms", "darkness", "crowds", "pollution", "arguing", "stress", "drama", "gossip",
            "chaos", "impatience", "rushing", "neglect", "overwatering", "harsh winds", "frost",
            "straight lines", "rigid structures", "artificial shapes", "sharp angles"
        ];

        public BonsaiStats(Random random)
        {
            this.random = random;
            GenerateRandomStats();
        }

        private void GenerateRandomStats()
        {
            // Generate random name from authentic Japanese names
            Name = names[random.Next(names.Length)];

            // Generate random age between 1 month and 15 years for realistic bonsai range
            int totalMonths = random.Next(1, 181); // 1 month to 180 months (15 years)

            if (totalMonths < 12)
            {
                Age = $"{totalMonths} month{(totalMonths == 1 ? "" : "s")} old";
            }
            else
            {
                int years = totalMonths / 12;
                int months = totalMonths % 12;

                if (months == 0)
                {
                    Age = $"{years} year{(years == 1 ? "" : "s")} old";
                }
                else
                {
                    Age = $"{years} year{(years == 1 ? "" : "s")}, {months} month{(months == 1 ? "" : "s")} old";
                }
            }

            // Generate 3 random likes
            Likes = [];
            var availableLikes = new List<string>(likeNouns);
            for (int i = 0; i < 3; i++)
            {
                int index = random.Next(availableLikes.Count);
                Likes.Add(availableLikes[index]);
                availableLikes.RemoveAt(index);
            }

            // Generate 3 random dislikes
            Dislikes = [];
            var availableDislikes = new List<string>(dislikeNouns);
            for (int i = 0; i < 3; i++)
            {
                int index = random.Next(availableDislikes.Count);
                Dislikes.Add(availableDislikes[index]);
                availableDislikes.RemoveAt(index);
            }
        }

        public void RegenerateStats()
        {
            GenerateRandomStats();
        }
    }
}