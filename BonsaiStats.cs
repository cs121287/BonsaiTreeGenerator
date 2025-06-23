using System;
using System.Collections.Generic;

namespace BonsaiTreeGenerator
{
    /// <summary>
    /// Generates random stats for each bonsai tree
    /// </summary>
    public class BonsaiStats
    {
        private readonly Random random;
        
        public string Name { get; private set; }
        public string Age { get; private set; }
        public List<string> Likes { get; private set; }
        public List<string> Dislikes { get; private set; }
        
        private readonly string[] names = [
            "Zen", "Akira", "Sakura", "Hana", "Kiko", "Yuki", "Momo", "Taro", "Keshi", "Bumi",
            "Roku", "Niko", "Suki", "Mika", "Roko", "Kira", "Nami", "Sora", "Yori", "Koda",
            "Shiro", "Kuro", "Midori", "Aoi", "Kiku", "Ume", "Matsu", "Take", "Ishi", "Mizu"
        ];
        
        private readonly string[] likeNouns = [
            "kids", "dogs", "hotdogs", "sunshine", "rain", "music", "books", "flowers", "cookies", 
            "birds", "butterflies", "pizza", "ice cream", "cats", "hugs", "laughter", "games", 
            "movies", "dancing", "singing", "art", "nature", "beaches", "mountains", "stars",
            "pancakes", "chocolate", "tea", "coffee", "naps", "gardens", "rainbows", "friends"
        ];
        
        private readonly string[] dislikeNouns = [
            "heat", "ants", "talk radio", "loud noises", "spiders", "vegetables", "homework", 
            "traffic", "waiting", "mondays", "dentists", "scary movies", "brussels sprouts",
            "wasps", "mosquitoes", "cold showers", "alarm clocks", "taxes", "bullies", "liars",
            "storms", "darkness", "crowds", "pollution", "arguing", "stress", "drama", "gossip"
        ];
        
        public BonsaiStats(Random random)
        {
            this.random = random;
            GenerateRandomStats();
        }
        
        private void GenerateRandomStats()
        {
            // Generate random name
            Name = names[random.Next(names.Length)];
            
            // Generate random age between 1 month and 10 years
            int totalMonths = random.Next(1, 121); // 1 month to 120 months (10 years)
            
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
            Likes = new List<string>();
            var availableLikes = new List<string>(likeNouns);
            for (int i = 0; i < 3; i++)
            {
                int index = random.Next(availableLikes.Count);
                Likes.Add(availableLikes[index]);
                availableLikes.RemoveAt(index);
            }
            
            // Generate 3 random dislikes
            Dislikes = new List<string>();
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