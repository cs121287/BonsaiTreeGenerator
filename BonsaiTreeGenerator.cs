using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;

namespace BonsaiTreeGenerator
{
    /// <summary>
    /// Improved realistic bonsai tree generator using enhanced 3-step wavy process:
    /// STEP 1: Create pot and wavy S-shaped main trunk
    /// STEP 2: Add 1-4 wavy S-shaped branches in upper 3/4 of trunk only
    /// STEP 3: Add mushroom cap shaped leaf canopies with proportional sizing
    /// </summary>
    public class BonsaiTreeGenerator(Random random)
    {
        private readonly Random random = random;
        private readonly BonsaiTree bonsaiTree = new(random);

        /// <summary>
        /// Generates a realistic bonsai tree using the improved 3-step wavy process
        /// </summary>
        public async Task<char[,]> GenerateTreeAsync(int width, int height,
            IProgress<int>? progress = null, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() =>
            {
                progress?.Report(15);
                cancellationToken.ThrowIfCancellationRequested();

                progress?.Report(50);
                var tree = bonsaiTree.GenerateTree(width, height);

                progress?.Report(85);
                cancellationToken.ThrowIfCancellationRequested();

                progress?.Report(100);
                return tree;
            }, cancellationToken);
        }

        /// <summary>
        /// Gets the realistic color mapping for the improved 3-step wavy process
        /// </summary>
        public static Dictionary<char, Color> GetColorMapping()
        {
            return BonsaiTree.GetColorMapping();
        }
    }
}