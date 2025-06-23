using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Drawing;

namespace BonsaiTreeGenerator
{
    /// <summary>
    /// Bonsai tree generator with corrected colors and enhanced functionality
    /// </summary>
    public class BonsaiTreeGenerator
    {
        private readonly Random random;
        private readonly BonsaiTree bonsaiTree;
        
        public BonsaiTreeGenerator(Random random)
        {
            this.random = random;
            this.bonsaiTree = new BonsaiTree(random);
        }
        
        /// <summary>
        /// Generates an authentic bonsai tree with proper colors
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
        /// Gets the corrected color mapping for realistic bonsai rendering
        /// </summary>
        public Dictionary<char, Color> GetColorMapping()
        {
            return bonsaiTree.GetColorMapping();
        }
    }
}