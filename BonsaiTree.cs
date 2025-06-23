using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BonsaiTreeGenerator
{
    /// <summary>
    /// Bonsai Tree generator with corrected colors and proper ASCII representation
    /// Focus: Natural brown trunk, realistic green foliage, proper character mapping
    /// </summary>
    public class BonsaiTree
    {
        private readonly Random random;
        private readonly List<TrunkSegment> trunkSegments = [];
        private readonly List<HorizontalBranch> horizontalBranches = [];
        private readonly List<FoliagePad> foliagePads = [];
        private readonly List<Root> surfaceRoots = [];
        private Pot pot;
        private int width;
        private int height;
        private BonsaiStyle style;
        private Point trunkBase;
        
        public enum BonsaiStyle
        {
            FormalUpright,    // Chokkan - straight, formal
            InformalUpright,  // Moyogi - curved, natural
            Windswept,        // Fukinagashi - wind-blown
            Cascade,          // Kengai - waterfall style
            Slanting          // Shakan - leaning
        }
        
        public BonsaiTree(Random random)
        {
            this.random = random;
        }
        
        /// <summary>
        /// Generates an authentic bonsai tree with corrected colors
        /// </summary>
        public char[,] GenerateTree(int canvasWidth, int canvasHeight)
        {
            width = canvasWidth;
            height = canvasHeight;
            
            // Clear previous generation
            trunkSegments.Clear();
            horizontalBranches.Clear();
            foliagePads.Clear();
            surfaceRoots.Clear();
            
            // Select random bonsai style
            style = (BonsaiStyle)random.Next(Enum.GetValues<BonsaiStyle>().Length);
            
            // Create the canvas
            var canvas = new char[height, width];
            FillBackground(canvas);
            
            // Generate components in proper order for authentic bonsai appearance
            pot = GenerateAuthenticPot();
            DrawPot(canvas);
            
            GenerateThickCurvedTrunk();
            DrawTrunk(canvas);
            
            GenerateSurfaceRoots();
            DrawSurfaceRoots(canvas);
            
            GenerateHorizontalBranchLayers();
            DrawBranches(canvas);
            
            GenerateDenseFoliagePads();
            DrawFoliagePads(canvas);
            
            return canvas;
        }
        
        public Dictionary<char, Color> GetColorMapping()
        {
            return new Dictionary<char, Color>
            {
                [' '] = Color.Black,
                
                // Pot - earth tone ceramics
                ['P'] = Color.FromArgb(139, 117, 102),       // Pot main body
                ['p'] = Color.FromArgb(118, 99, 87),         // Pot shadow
                ['◘'] = Color.FromArgb(160, 135, 118),       // Pot highlight
                ['◙'] = Color.FromArgb(101, 85, 75),         // Pot edge
                
                // Surface roots - natural brown
                ['═'] = Color.FromArgb(139, 90, 43),         // Main surface roots
                ['─'] = Color.FromArgb(160, 110, 60),        // Secondary roots
                
                // Trunk - natural brown tones
                ['T'] = Color.FromArgb(101, 67, 33),         // Main trunk body
                ['t'] = Color.FromArgb(83, 53, 20),          // Trunk shadow
                ['Θ'] = Color.FromArgb(139, 90, 43),         // Trunk highlight
                ['Ω'] = Color.FromArgb(120, 80, 40),         // Trunk mid-tone
                ['∞'] = Color.FromArgb(95, 65, 30),          // Trunk core
                ['φ'] = Color.FromArgb(110, 75, 35),         // Trunk texture
                ['ψ'] = Color.FromArgb(130, 85, 45),         // Trunk surface
                ['ω'] = Color.FromArgb(90, 60, 30),          // Trunk edge
                
                // Branches - brown hierarchy
                ['B'] = Color.FromArgb(120, 80, 40),         // Main horizontal branches
                ['b'] = Color.FromArgb(100, 70, 35),         // Secondary branches
                ['|'] = Color.FromArgb(140, 95, 50),         // Vertical connectors
                ['I'] = Color.FromArgb(90, 60, 30),          // Thick connectors
                ['/'] = Color.FromArgb(110, 75, 40),         // Diagonal branches
                ['\\'] = Color.FromArgb(115, 78, 42),        // Diagonal branches
                
                // Foliage - natural green tones
                ['F'] = Color.FromArgb(34, 139, 34),         // Dense foliage
                ['f'] = Color.FromArgb(46, 125, 46),         // Medium foliage
                ['G'] = Color.FromArgb(60, 140, 60),         // Light foliage
                ['g'] = Color.FromArgb(80, 160, 80),         // Bright foliage
                ['L'] = Color.FromArgb(20, 100, 20),         // Dark green
                ['l'] = Color.FromArgb(40, 120, 40),         // Mid green
                ['*'] = Color.FromArgb(70, 150, 70),         // Accent green
                ['o'] = Color.FromArgb(85, 165, 85),         // Light accent
            };
        }
        
        private void FillBackground(char[,] canvas)
        {
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    canvas[r, c] = ' ';
                }
            }
        }
        
        private Pot GenerateAuthenticPot()
        {
            // Proper bonsai pot proportions - wider and shallower
            int potHeight = Math.Max(3, height / 12);
            int potWidth = (width * 2) / 3;
            
            int potBottom = height - 1;
            int potLeft = (width - potWidth) / 2;
            
            return new Pot
            {
                Left = potLeft,
                Right = potLeft + potWidth,
                Top = potBottom - potHeight,
                Bottom = potBottom,
                Width = potWidth,
                Height = potHeight
            };
        }
        
        private void DrawPot(char[,] canvas)
        {
            for (int r = pot.Top; r <= pot.Bottom; r++)
            {
                for (int c = pot.Left; c <= pot.Right; c++)
                {
                    if (r >= 0 && r < height && c >= 0 && c < width)
                    {
                        bool isTopEdge = r == pot.Top;
                        bool isBottomEdge = r == pot.Bottom;
                        bool isLeftEdge = c == pot.Left;
                        bool isRightEdge = c == pot.Right;
                        
                        if (isTopEdge) canvas[r, c] = '◘';
                        else if (isBottomEdge) canvas[r, c] = 'p';
                        else if (isLeftEdge) canvas[r, c] = '◙';
                        else if (isRightEdge) canvas[r, c] = 'p';
                        else canvas[r, c] = 'P';
                    }
                }
            }
        }
        
        private void GenerateThickCurvedTrunk()
        {
            // Create a substantial, curved trunk
            trunkBase = new Point((pot.Left + pot.Right) / 2, pot.Top - 1);
            int trunkHeight = (height * 3) / 5;
            
            // Generate trunk segments with substantial thickness
            int segments = 10;
            int currentX = trunkBase.X;
            int currentY = trunkBase.Y;
            
            for (int i = 0; i <= segments; i++)
            {
                float progress = (float)i / segments;
                int targetY = trunkBase.Y - (int)(trunkHeight * progress);
                
                // Create natural S-curve movement
                int deltaX = GetTrunkCurveMovement(progress);
                currentX += deltaX;
                currentX = Math.Clamp(currentX, 6, width - 6);
                
                // Substantial thickness that tapers naturally
                int thickness = GetTrunkThickness(progress);
                
                var segment = new TrunkSegment
                {
                    X = currentX,
                    Y = targetY,
                    Thickness = thickness,
                    Progress = progress
                };
                
                trunkSegments.Add(segment);
            }
        }
        
        private int GetTrunkCurveMovement(float progress)
        {
            return style switch
            {
                BonsaiStyle.FormalUpright => random.Next(-1, 2),
                BonsaiStyle.InformalUpright => (int)(Math.Sin(progress * Math.PI * 2.5) * 4),
                BonsaiStyle.Windswept => (int)(progress * 12) + random.Next(-1, 2),
                BonsaiStyle.Cascade => (int)(progress * -8) + random.Next(-1, 2),
                BonsaiStyle.Slanting => (int)(progress * 6) + random.Next(-1, 2),
                _ => random.Next(-2, 3)
            };
        }
        
        private int GetTrunkThickness(float progress)
        {
            // Substantial trunk thickness that tapers naturally
            int baseThickness = 8;
            int currentThickness = (int)(baseThickness * (1 - progress * 0.7f));
            return Math.Max(2, currentThickness);
        }
        
        private void DrawTrunk(char[,] canvas)
        {
            // Draw trunk segments with substantial presence
            for (int i = 0; i < trunkSegments.Count - 1; i++)
            {
                var current = trunkSegments[i];
                var next = trunkSegments[i + 1];
                
                DrawThickTrunkSegment(canvas, current, next);
            }
        }
        
        private void DrawThickTrunkSegment(char[,] canvas, TrunkSegment start, TrunkSegment end)
        {
            // Bresenham line with substantial thickness
            int x0 = start.X, y0 = start.Y;
            int x1 = end.X, y1 = end.Y;
            
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            
            int x = x0, y = y0;
            int steps = 0;
            int totalSteps = dx + dy;
            
            while (true)
            {
                // Interpolate thickness
                float progress = totalSteps > 0 ? (float)steps / totalSteps : 0;
                int currentThickness = (int)(start.Thickness * (1 - progress) + end.Thickness * progress);
                
                DrawTrunkCross(canvas, x, y, currentThickness);
                
                if (x == x1 && y == y1) break;
                
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x += sx; }
                if (e2 < dx) { err += dx; y += sy; }
                steps++;
            }
        }
        
        private void DrawTrunkCross(char[,] canvas, int centerX, int centerY, int thickness)
        {
            int radius = thickness / 2;
            
            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int px = centerX + dx;
                    int py = centerY + dy;
                    
                    if (px >= 0 && px < width && py >= 0 && py < height)
                    {
                        double distance = Math.Sqrt(dx * dx + dy * dy);
                        if (distance <= radius)
                        {
                            char trunkChar = GetTrunkTextureCharacter(dx, dy, radius);
                            canvas[py, px] = trunkChar;
                        }
                    }
                }
            }
        }
        
        private char GetTrunkTextureCharacter(int offsetX, int offsetY, double radius)
        {
            double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
            double ratio = distance / radius;
            
            if (ratio < 0.2) return 'T';  // Dense core
            if (ratio < 0.4) return 't';  // Inner
            if (ratio < 0.6) return 'Θ';  // Mid-inner
            if (ratio < 0.8) return 'Ω';  // Mid-outer
            return '∞';  // Edge
        }
        
        private void GenerateSurfaceRoots()
        {
            int numRoots = 4 + random.Next(0, 3);
            
            for (int i = 0; i < numRoots; i++)
            {
                double angle = (Math.PI * 2 * i / numRoots) + random.NextDouble() * 0.3 - 0.15;
                int rootLength = 3 + random.Next(0, 3);
                
                var root = new Root
                {
                    StartX = trunkBase.X,
                    StartY = trunkBase.Y,
                    EndX = trunkBase.X + (int)(Math.Cos(angle) * rootLength),
                    EndY = trunkBase.Y,
                    Thickness = 1 + (i < 2 ? 1 : 0)
                };
                
                surfaceRoots.Add(root);
            }
        }
        
        private void DrawSurfaceRoots(char[,] canvas)
        {
            foreach (var root in surfaceRoots)
            {
                for (int x = Math.Min(root.StartX, root.EndX); x <= Math.Max(root.StartX, root.EndX); x++)
                {
                    if (x >= 0 && x < width && root.StartY >= 0 && root.StartY < height)
                    {
                        char rootChar = root.Thickness > 1 ? '═' : '─';
                        canvas[root.StartY, x] = rootChar;
                    }
                }
            }
        }
        
        private void GenerateHorizontalBranchLayers()
        {
            // Generate 3-5 distinct horizontal layers
            int numLayers = 3 + random.Next(0, 3);
            
            for (int layer = 0; layer < numLayers; layer++)
            {
                // Position layers at different heights along trunk
                float heightRatio = 0.2f + (layer * 0.18f) + (float)random.NextDouble() * 0.08f;
                if (heightRatio > 0.85f) continue;
                
                var trunkPoint = GetTrunkAtHeight(heightRatio);
                
                // Create left and right extending branches
                bool hasLeft = random.NextDouble() > 0.25;
                bool hasRight = random.NextDouble() > 0.25;
                
                if (hasLeft)
                {
                    var leftBranch = CreateHorizontalBranch(trunkPoint, true, layer, heightRatio);
                    horizontalBranches.Add(leftBranch);
                }
                
                if (hasRight)
                {
                    var rightBranch = CreateHorizontalBranch(trunkPoint, false, layer, heightRatio);
                    horizontalBranches.Add(rightBranch);
                }
            }
        }
        
        private TrunkSegment GetTrunkAtHeight(float heightRatio)
        {
            if (trunkSegments.Count == 0) return new TrunkSegment { X = width / 2, Y = height / 2, Thickness = 6 };
            
            int index = (int)(heightRatio * (trunkSegments.Count - 1));
            index = Math.Clamp(index, 0, trunkSegments.Count - 1);
            return trunkSegments[index];
        }
        
        private HorizontalBranch CreateHorizontalBranch(TrunkSegment trunkPoint, bool isLeft, int layer, float heightRatio)
        {
            int direction = isLeft ? -1 : 1;
            int branchLength = 12 - layer * 2 + random.Next(-2, 3);
            branchLength = Math.Max(6, branchLength);
            
            // Slight upward angle for natural look
            int verticalDrop = random.Next(0, 3);
            
            return new HorizontalBranch
            {
                StartX = trunkPoint.X,
                StartY = trunkPoint.Y,
                EndX = trunkPoint.X + (direction * branchLength),
                EndY = trunkPoint.Y + verticalDrop,
                IsLeft = isLeft,
                Layer = layer,
                Length = branchLength,
                HeightRatio = heightRatio
            };
        }
        
        private void DrawBranches(char[,] canvas)
        {
            foreach (var branch in horizontalBranches)
            {
                DrawHorizontalBranch(canvas, branch);
            }
        }
        
        private void DrawHorizontalBranch(char[,] canvas, HorizontalBranch branch)
        {
            // Draw main horizontal extending branch
            DrawBranchLine(canvas, branch.StartX, branch.StartY, branch.EndX, branch.EndY, 'B');
            
            // Add some secondary branching for fullness
            if (branch.Length > 8)
            {
                int midX = (branch.StartX + branch.EndX) / 2;
                int midY = (branch.StartY + branch.EndY) / 2;
                
                // Add small upward and downward secondary branches
                DrawBranchLine(canvas, midX, midY, midX + random.Next(-2, 3), midY - random.Next(1, 3), 'b');
                DrawBranchLine(canvas, midX, midY, midX + random.Next(-2, 3), midY + random.Next(1, 3), 'b');
            }
        }
        
        private void DrawBranchLine(char[,] canvas, int x0, int y0, int x1, int y1, char branchChar)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;
            
            int x = x0, y = y0;
            
            while (true)
            {
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    canvas[y, x] = branchChar;
                }
                
                if (x == x1 && y == y1) break;
                
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x += sx; }
                if (e2 < dx) { err += dx; y += sy; }
            }
        }
        
        private void GenerateDenseFoliagePads()
        {
            // Create dense foliage masses at the end of each horizontal branch
            foreach (var branch in horizontalBranches)
            {
                int padSize = GetFoliagePadSize(branch.Layer);
                
                var pad = new FoliagePad
                {
                    CenterX = branch.EndX,
                    CenterY = branch.EndY,
                    Width = padSize + random.Next(-1, 2),
                    Height = padSize - random.Next(0, 2),
                    Density = 0.8f - (branch.Layer * 0.1f),
                    Layer = branch.Layer
                };
                
                foliagePads.Add(pad);
            }
            
            // Add apex foliage
            if (trunkSegments.Count > 0)
            {
                var apex = trunkSegments.Last();
                var apexPad = new FoliagePad
                {
                    CenterX = apex.X,
                    CenterY = apex.Y - 1,
                    Width = 5,
                    Height = 3,
                    Density = 0.75f,
                    Layer = -1
                };
                
                foliagePads.Add(apexPad);
            }
        }
        
        private int GetFoliagePadSize(int layer)
        {
            // Lower layers have larger foliage pads
            return Math.Max(4, 9 - layer * 2);
        }
        
        private void DrawFoliagePads(char[,] canvas)
        {
            foreach (var pad in foliagePads)
            {
                DrawDenseFoliagePad(canvas, pad);
            }
        }
        
        private void DrawDenseFoliagePad(char[,] canvas, FoliagePad pad)
        {
            // Create solid, dense foliage masses
            char[] foliageChars = ['F', 'f', 'G', 'g', 'L', 'l', '*', 'o'];
            
            int halfWidth = pad.Width / 2;
            int halfHeight = pad.Height / 2;
            
            for (int dy = -halfHeight; dy <= halfHeight; dy++)
            {
                for (int dx = -halfWidth; dx <= halfWidth; dx++)
                {
                    int leafX = pad.CenterX + dx;
                    int leafY = pad.CenterY + dy;
                    
                    if (leafX >= 0 && leafX < width && leafY >= 0 && leafY < height)
                    {
                        // Create elliptical, solid foliage shape
                        double distanceX = (double)dx / halfWidth;
                        double distanceY = (double)dy / halfHeight;
                        double ellipseDistance = Math.Sqrt(distanceX * distanceX + distanceY * distanceY);
                        
                        if (ellipseDistance <= 1.0)
                        {
                            double probability = pad.Density * (1 - ellipseDistance * 0.2);
                            
                            if (random.NextDouble() < probability)
                            {
                                // Choose character based on position in pad for solid appearance
                                int charIndex;
                                if (ellipseDistance < 0.3) charIndex = 0; // Dense center
                                else if (ellipseDistance < 0.6) charIndex = 1; // Mid density
                                else if (ellipseDistance < 0.8) charIndex = 2; // Medium
                                else charIndex = 3; // Edges
                                
                                char foliageChar = foliageChars[charIndex];
                                
                                // Only draw if background or lower priority
                                if (canvas[leafY, leafX] == ' ')
                                {
                                    canvas[leafY, leafX] = foliageChar;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    
    public class TrunkSegment
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Thickness { get; set; }
        public float Progress { get; set; }
    }
    
    public class HorizontalBranch
    {
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int EndX { get; set; }
        public int EndY { get; set; }
        public bool IsLeft { get; set; }
        public int Layer { get; set; }
        public int Length { get; set; }
        public float HeightRatio { get; set; }
    }
    
    public class FoliagePad
    {
        public int CenterX { get; set; }
        public int CenterY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Density { get; set; }
        public int Layer { get; set; }
    }
    
    public class Root
    {
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int EndX { get; set; }
        public int EndY { get; set; }
        public int Thickness { get; set; }
    }
    
    public class Pot
    {
        public int Left { get; set; }
        public int Right { get; set; }
        public int Top { get; set; }
        public int Bottom { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
}