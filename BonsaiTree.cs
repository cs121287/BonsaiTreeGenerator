using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace BonsaiTreeGenerator
{
    /// <summary>
    /// Improved Realistic Bonsai Tree generator following enhanced 3-step process:
    /// STEP 1: Create pot and wavy S-shaped main trunk
    /// STEP 2: Add 1-4 wavy S-shaped branches in upper 3/4 of trunk (not lower quarter)
    /// STEP 3: Add mushroom cap shaped leaf canopies with proportional sizing
    /// </summary>
    public class BonsaiTree(Random random)
    {
        private readonly Random random = random;
        private readonly List<TrunkSegment> trunkSegments = [];
        private readonly List<Branch> branches = [];
        private readonly List<LeafCanopy> leafCanopies = [];
        private readonly List<Root> surfaceRoots = [];
        private Pot pot = new();
        private int width;
        private int height;
        private BonsaiStyle style;
        private Point trunkBase;
        private TreeVariation variation;
        private int trunkLength;

        public enum BonsaiStyle
        {
            FormalUpright,    // Chokkan - straight, formal
            InformalUpright,  // Moyogi - curved, natural
            Windswept,        // Fukinagashi - wind-blown
            Cascade,          // Kengai - waterfall style
            Slanting          // Shakan - leaning
        }

        public enum TreeVariation
        {
            Sparse,           // Few branches, minimal foliage
            Balanced,         // Moderate branches and foliage
            Dense,            // Many branches, full foliage
            Asymmetric,       // Uneven branch distribution
            Mature,           // Large, well-developed canopy
            Young,            // Small, developing tree
            Wild,             // Irregular, untamed growth
            Elegant           // Refined, artistic shape
        }

        /// <summary>
        /// Generates a realistic bonsai tree using improved 3-step wavy process
        /// </summary>
        public char[,] GenerateTree(int canvasWidth, int canvasHeight)
        {
            width = canvasWidth;
            height = canvasHeight;

            // Clear previous generation
            trunkSegments.Clear();
            branches.Clear();
            leafCanopies.Clear();
            surfaceRoots.Clear();

            // Select random bonsai style and variation
            style = (BonsaiStyle)random.Next(Enum.GetValues<BonsaiStyle>().Length);
            variation = (TreeVariation)random.Next(Enum.GetValues<TreeVariation>().Length);

            // Create the canvas
            var canvas = new char[height, width];
            FillBackground(canvas);

            // IMPROVED 3-STEP PROCESS FOR WAVY REALISTIC BONSAI GENERATION

            // STEP 1: Create pot and wavy S-shaped main trunk
            Step1_CreatePotAndWavyTrunk(canvas);

            // STEP 2: Add 1-4 wavy S-shaped branches in upper 3/4 of trunk
            Step2_AddWavyBranchesInUpperArea(canvas);

            // STEP 3: Add mushroom cap shaped leaf canopies with proportional sizing
            Step3_AddMushroomCapCanopies(canvas);

            return canvas;
        }

        public static Dictionary<char, Color> GetColorMapping()
        {
            return new Dictionary<char, Color>
            {
                [' '] = Color.Black,

                // POT - PURE GREY COLORS ONLY
                ['█'] = Color.FromArgb(130, 130, 130),       // Pot main body - medium grey
                ['▓'] = Color.FromArgb(110, 110, 110),       // Pot shadow - darker grey
                ['▒'] = Color.FromArgb(150, 150, 150),       // Pot highlight - lighter grey
                ['░'] = Color.FromArgb(90, 90, 90),          // Pot feet/base - darkest grey
                ['■'] = Color.FromArgb(140, 140, 140),       // Pot edge - medium-light grey
                ['□'] = Color.FromArgb(160, 160, 160),       // Pot rim - lightest grey

                // SOIL - dark brown/black for contrast with grey pot
                ['▪'] = Color.FromArgb(40, 30, 20),          // Dark soil
                ['▫'] = Color.FromArgb(60, 45, 30),          // Medium soil

                // WAVY TRUNK - Natural brown tones with wavy texture
                ['║'] = Color.FromArgb(101, 67, 33),         // Main trunk body
                ['╣'] = Color.FromArgb(83, 53, 20),          // Trunk shadow
                ['╠'] = Color.FromArgb(139, 90, 43),         // Trunk highlight
                ['╦'] = Color.FromArgb(120, 80, 40),         // Trunk mid-tone
                ['╩'] = Color.FromArgb(95, 65, 30),          // Trunk core
                ['╬'] = Color.FromArgb(110, 75, 35),         // Trunk texture
                ['╧'] = Color.FromArgb(130, 85, 45),         // Trunk surface
                ['╨'] = Color.FromArgb(90, 60, 30),          // Trunk edge
                ['╤'] = Color.FromArgb(115, 78, 42),         // Wavy trunk detail
                ['╥'] = Color.FromArgb(125, 82, 38),         // Wavy trunk ridge

                // SURFACE ROOTS - brown tones
                ['═'] = Color.FromArgb(139, 90, 43),         // Main surface roots
                ['─'] = Color.FromArgb(160, 110, 60),        // Secondary roots

                // WAVY BRANCHES - brown hierarchy for S-shaped branches with thickness taper
                ['┃'] = Color.FromArgb(120, 80, 40),         // Main S-shaped branches - thick base
                ['┏'] = Color.FromArgb(100, 70, 35),         // Branch curves - medium
                ['┓'] = Color.FromArgb(90, 60, 30),          // Branch endpoints - thin
                ['┗'] = Color.FromArgb(110, 75, 40),         // Branch joints
                ['┛'] = Color.FromArgb(95, 65, 32),          // Small branch curves
                ['┣'] = Color.FromArgb(115, 78, 42),         // Branch connections
                ['┫'] = Color.FromArgb(105, 72, 38),         // Branch bends
                ['┳'] = Color.FromArgb(125, 82, 45),         // Branch nodes
                ['┻'] = Color.FromArgb(85, 58, 28),          // Branch tips

                // MUSHROOM CAP CANOPIES - Multiple green tones for larger upward mushroom shapes
                ['●'] = Color.FromArgb(34, 139, 34),         // Dense mushroom core
                ['○'] = Color.FromArgb(46, 125, 46),         // Medium mushroom body
                ['◆'] = Color.FromArgb(60, 140, 60),         // Light mushroom areas
                ['◇'] = Color.FromArgb(80, 160, 80),         // Mushroom highlights
                ['◈'] = Color.FromArgb(20, 100, 20),         // Dark mushroom center
                ['◉'] = Color.FromArgb(40, 120, 40),         // Medium mushroom density
                ['◊'] = Color.FromArgb(70, 150, 70),         // Mushroom cap edges
                ['⬢'] = Color.FromArgb(85, 165, 85),         // Mushroom cap highlights
                ['⬡'] = Color.FromArgb(25, 95, 25),          // Mushroom shadows
                ['⬟'] = Color.FromArgb(50, 130, 50),         // Mushroom inner layers
                ['⬠'] = Color.FromArgb(90, 170, 90),         // Mushroom outer edges
                ['⬣'] = Color.FromArgb(15, 80, 15),          // Deep mushroom shadows
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

        /// <summary>
        /// STEP 1: Create pot and wavy S-shaped main trunk
        /// </summary>
        private void Step1_CreatePotAndWavyTrunk(char[,] canvas)
        {
            // Create pot with perfect alignment and pure grey colors
            CreateRealisticPot();
            DrawRealisticPot(canvas);

            // Create wavy S-shaped main trunk
            CreateWavySShapedTrunk();
            DrawWavyTrunk(canvas);

            // Add surface roots for authenticity
            CreateSurfaceRoots();
            DrawSurfaceRoots(canvas);
        }

        private void CreateRealisticPot()
        {
            // Authentic bonsai pot proportions - wider than tall, perfectly centered
            int potHeight = Math.Max(4, height / 10);
            int potWidth = (width * 3) / 4;

            // Perfect centering calculation
            int potBottom = height - 1;
            int potLeft = (width - potWidth) / 2;

            pot = new Pot
            {
                Left = potLeft,
                Right = potLeft + potWidth - 1,
                Top = potBottom - potHeight + 1,
                Bottom = potBottom,
                Width = potWidth,
                Height = potHeight
            };
        }

        private void DrawRealisticPot(char[,] canvas)
        {
            // Draw pot body with pure grey colors only
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
                        bool isCorner = (isTopEdge || isBottomEdge) && (isLeftEdge || isRightEdge);

                        // STRICT POT RULE: Only grey characters
                        if (isCorner)
                            canvas[r, c] = '■';       // Medium-light grey corners
                        else if (isTopEdge)
                            canvas[r, c] = '□';       // Lightest grey rim
                        else if (isBottomEdge)
                            canvas[r, c] = '░';       // Darkest grey base
                        else if (isLeftEdge || isRightEdge)
                            canvas[r, c] = '▒';       // Light grey sides
                        else
                            canvas[r, c] = '█';       // Medium grey body
                    }
                }
            }

            // Add pot feet for authentic appearance
            if (pot.Bottom < height - 1)
            {
                int footWidth = 3;
                int leftFootStart = pot.Left + 2;
                int rightFootStart = pot.Right - footWidth - 1;

                for (int i = 0; i < footWidth; i++)
                {
                    if (leftFootStart + i >= 0 && leftFootStart + i < width)
                        canvas[pot.Bottom + 1, leftFootStart + i] = '▓';
                    if (rightFootStart + i >= 0 && rightFootStart + i < width)
                        canvas[pot.Bottom + 1, rightFootStart + i] = '▓';
                }
            }

            // Add soil surface - dark to contrast with grey pot
            int soilSurface = pot.Top + 1;
            if (soilSurface < height)
            {
                for (int c = pot.Left + 1; c < pot.Right; c++)
                {
                    if (c >= 0 && c < width)
                    {
                        canvas[soilSurface, c] = '▪';
                        if (random.NextDouble() > 0.7)
                            canvas[soilSurface, c] = '▫';
                    }
                }
            }
        }

        private void CreateWavySShapedTrunk()
        {
            // Start trunk at soil surface in pot center
            trunkBase = new Point((pot.Left + pot.Right) / 2, pot.Top + 1);

            // Ensure substantial height - minimum 40% of canvas
            int minimumHeight = (height * 2) / 5;
            int baseHeight = (height * 3) / 5;

            trunkLength = variation switch
            {
                TreeVariation.Young => Math.Max(minimumHeight, baseHeight - random.Next(0, 8)),
                TreeVariation.Mature => Math.Max(minimumHeight, baseHeight + random.Next(8, 18)),
                TreeVariation.Dense => Math.Max(minimumHeight, baseHeight + random.Next(5, 12)),
                TreeVariation.Wild => Math.Max(minimumHeight, baseHeight + random.Next(-5, 20)),
                TreeVariation.Elegant => Math.Max(minimumHeight, baseHeight + random.Next(0, 10)),
                _ => Math.Max(minimumHeight, baseHeight + random.Next(-3, 10))
            };

            // Create wavy S-shaped trunk segments
            int segments = 15 + random.Next(0, 10);
            _ = trunkBase.X;
            _ = trunkBase.Y;

            // S-curve parameters for natural wavy trunk
            double waveAmplitude = 8 + random.Next(0, 6);  // How wide the S-curve is
            double waveFrequency = 1.5 + random.NextDouble(); // How many S-curves in trunk
            double phaseShift = random.NextDouble() * Math.PI * 2; // Random starting point

            for (int i = 0; i <= segments; i++)
            {
                float progress = (float)i / segments;
                int targetY = trunkBase.Y - (int)(trunkLength * progress);

                // Create wavy S-shaped movement using sine wave
                double sineValue = Math.Sin(progress * Math.PI * waveFrequency + phaseShift);
                int wavyMovement = (int)(sineValue * waveAmplitude * (1 - progress * 0.3)); // Reduce amplitude toward top

                int currentX = trunkBase.X + wavyMovement;
                currentX = Math.Clamp(currentX, 10, width - 10);

                // Natural taper from thick base to thin top
                int thickness = GetWavyTrunkThickness(progress);

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

        private int GetWavyTrunkThickness(float progress)
        {
            int baseThickness = variation switch
            {
                TreeVariation.Young => 6 + random.Next(0, 2),
                TreeVariation.Mature => 12 + random.Next(0, 4),
                TreeVariation.Dense => 10 + random.Next(0, 3),
                TreeVariation.Wild => 9 + random.Next(0, 5),
                TreeVariation.Elegant => 9 + random.Next(0, 3),
                _ => 8 + random.Next(0, 2)
            };

            // Natural taper for wavy trunk
            float taperRate = 0.75f;
            int currentThickness = (int)(baseThickness * (1 - progress * taperRate));
            return Math.Max(2, currentThickness);
        }

        private void DrawWavyTrunk(char[,] canvas)
        {
            for (int i = 0; i < trunkSegments.Count - 1; i++)
            {
                var current = trunkSegments[i];
                var next = trunkSegments[i + 1];
                DrawWavyTrunkSegment(canvas, current, next);
            }
        }

        private void DrawWavyTrunkSegment(char[,] canvas, TrunkSegment start, TrunkSegment end)
        {
            // Bresenham line algorithm with wavy trunk texture
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
                float progress = totalSteps > 0 ? (float)steps / totalSteps : 0;
                int currentThickness = (int)(start.Thickness * (1 - progress) + end.Thickness * progress);

                DrawWavyTrunkCrossSection(canvas, x, y, currentThickness, start.Progress);

                if (x == x1 && y == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x += sx; }
                if (e2 < dx) { err += dx; y += sy; }
                steps++;
            }
        }

        private void DrawWavyTrunkCrossSection(char[,] canvas, int centerX, int centerY, int thickness, float trunkProgress)
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
                            char trunkChar = GetWavyTrunkTexture(dx, dy, radius, trunkProgress);
                            canvas[py, px] = trunkChar;
                        }
                    }
                }
            }
        }

        private static char GetWavyTrunkTexture(int offsetX, int offsetY, double radius, float trunkProgress)
        {
            double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
            double ratio = distance / radius;

            // Add wavy texture variation
            double wavyNoise = Math.Sin(trunkProgress * 20 + offsetX * 0.5) * 0.1;
            ratio += wavyNoise;

            if (ratio < 0.15) return '║';  // Core
            if (ratio < 0.3) return '╣';   // Inner
            if (ratio < 0.5) return '╠';   // Mid-inner
            if (ratio < 0.7) return '╦';   // Mid-outer
            if (ratio < 0.85) return '╩';  // Outer
            if (ratio < 0.95) return '╬';  // Surface
            return '╧'; // Edge
        }

        private void CreateSurfaceRoots()
        {
            int numRoots = 4 + random.Next(0, 3);

            for (int i = 0; i < numRoots; i++)
            {
                double angle = (Math.PI * 2 * i / numRoots) + random.NextDouble() * 0.5 - 0.25;
                int rootLength = 4 + random.Next(0, 4);

                var root = new Root
                {
                    StartX = trunkBase.X,
                    StartY = trunkBase.Y,
                    EndX = trunkBase.X + (int)(Math.Cos(angle) * rootLength),
                    EndY = trunkBase.Y + random.Next(0, 2),
                    Thickness = 1 + (i < 2 ? 1 : 0)
                };

                surfaceRoots.Add(root);
            }
        }

        private void DrawSurfaceRoots(char[,] canvas)
        {
            foreach (var root in surfaceRoots)
            {
                int dx = Math.Sign(root.EndX - root.StartX);
                int currentX = root.StartX;
                int currentY = root.StartY;

                while (currentX != root.EndX)
                {
                    if (currentX >= 0 && currentX < width && currentY >= 0 && currentY < height)
                    {
                        char rootChar = root.Thickness > 1 ? '═' : '─';
                        if (canvas[currentY, currentX] == ' ' || canvas[currentY, currentX] == '▪' || canvas[currentY, currentX] == '▫')
                        {
                            canvas[currentY, currentX] = rootChar;
                        }
                    }
                    currentX += dx;

                    if (random.NextDouble() > 0.7 && currentY < height - 1)
                        currentY++;
                }
            }
        }

        /// <summary>
        /// STEP 2: Add 1-4 wavy S-shaped branches in upper 3/4 of trunk only (NOT in lower quarter)
        /// Branches now start as thick as trunk and taper, and are much longer
        /// </summary>
        private void Step2_AddWavyBranchesInUpperArea(char[,] canvas)
        {
            // Random number between 1 and 4 branches as specified
            int numBranches = 1 + random.Next(0, 4);  // 1-4 branches

            for (int i = 0; i < numBranches; i++)
            {
                // CRITICAL: Position branches ONLY in upper 3/4 of trunk (NOT in lower quarter)
                // Lower quarter = 0.0 to 0.25, Upper 3/4 = 0.25 to 1.0
                float minHeightRatio = 0.25f;  // Start at 25% height (bottom of upper 3/4)
                float maxHeightRatio = 0.9f;   // End at 90% height (near top)

                float heightRange = maxHeightRatio - minHeightRatio;
                float heightRatio = minHeightRatio + (i * heightRange / numBranches) + random.Next(-5, 10) / 100f;
                heightRatio = Math.Clamp(heightRatio, minHeightRatio, maxHeightRatio);

                var attachmentPoint = GetTrunkPointAtHeight(heightRatio);

                // Determine if branch goes left or right (or both)
                bool goLeft = random.NextDouble() > 0.3;
                bool goRight = random.NextDouble() > 0.3;

                // Ensure at least one direction
                if (!goLeft && !goRight)
                {
                    if (random.NextDouble() > 0.5) goLeft = true;
                    else goRight = true;
                }

                if (goLeft)
                {
                    var leftBranch = CreateWavyBranch(attachmentPoint, true, i, heightRatio);
                    branches.Add(leftBranch);
                }

                if (goRight)
                {
                    var rightBranch = CreateWavyBranch(attachmentPoint, false, i, heightRatio);
                    branches.Add(rightBranch);
                }
            }

            // Draw all wavy S-shaped branches with thickness tapering
            foreach (var branch in branches)
            {
                DrawWavyBranch(canvas, branch);
            }
        }

        private TrunkSegment GetTrunkPointAtHeight(float heightRatio)
        {
            if (trunkSegments.Count == 0)
                return new TrunkSegment { X = width / 2, Y = height / 2, Thickness = 6 };

            int index = (int)(heightRatio * (trunkSegments.Count - 1));
            index = Math.Clamp(index, 0, trunkSegments.Count - 1);
            return trunkSegments[index];
        }

        private Branch CreateWavyBranch(TrunkSegment attachmentPoint, bool isLeft, int branchIndex, float heightRatio)
        {
            int direction = isLeft ? -1 : 1;

            // BRANCH LENGTH: Much longer branches - 2/3 to full trunk length for dramatic effect
            int baseBranchLength = (trunkLength * 2) / 3; // Start with 2/3 trunk length
            int lengthVariation = random.Next(0, trunkLength / 3); // Can extend up to full trunk length
            int branchLength = baseBranchLength + lengthVariation;
            branchLength = Math.Max(20, branchLength); // Minimum much longer length

            // Ensure branch stays within canvas bounds but allow to extend more
            int maxPossibleLength = isLeft ? attachmentPoint.X - 5 : width - attachmentPoint.X - 5;
            branchLength = Math.Min(branchLength, maxPossibleLength);

            // BRANCH THICKNESS: Start as thick as trunk at attachment point and taper
            int startThickness = attachmentPoint.Thickness; // Start thick as trunk
            int endThickness = Math.Max(1, startThickness / 4); // Taper to 1/4 thickness

            return new Branch
            {
                StartX = attachmentPoint.X,
                StartY = attachmentPoint.Y,
                EndX = attachmentPoint.X + (direction * branchLength), // Initial endpoint, will be modified by S-curve
                EndY = attachmentPoint.Y, // Will be modified to point gently upward
                IsLeft = isLeft,
                Length = branchLength,
                BranchIndex = branchIndex,
                HeightRatio = heightRatio,
                StartThickness = startThickness, // NEW: Track start thickness
                EndThickness = endThickness,     // NEW: Track end thickness
                WavePoints = GenerateBranchWavePoints(attachmentPoint, direction, branchLength)
            };
        }

        private List<Point> GenerateBranchWavePoints(TrunkSegment start, int direction, int length)
        {
            var points = new List<Point>();
            int segments = Math.Max(12, length / 4); // More segments for longer smoother S-curve

            // S-curve parameters for branch
            double amplitude = 4 + random.NextDouble() * 4; // Slightly larger amplitude for longer branches
            double frequency = 1.0 + random.NextDouble() * 0.8; // S-curve frequency

            for (int i = 0; i <= segments; i++)
            {
                float progress = (float)i / segments;

                // Horizontal movement with S-curve
                int baseX = start.X + (int)(direction * length * progress);
                double sineValue = Math.Sin(progress * Math.PI * frequency);
                int wavyOffset = (int)(sineValue * amplitude);
                int finalX = baseX + (direction * wavyOffset);

                // Vertical movement - gentle upward curve at the end
                int baseY = start.Y;
                if (progress > 0.6f) // Last 40% of branch curves gently upward for longer branches
                {
                    float upwardProgress = (progress - 0.6f) / 0.4f;
                    baseY -= (int)(upwardProgress * upwardProgress * 6); // Slightly more upward curve
                }

                points.Add(new Point(finalX, baseY));
            }

            return points;
        }

        private void DrawWavyBranch(char[,] canvas, Branch branch)
        {
            // Draw the wavy S-shaped branch using wave points with thickness tapering
            for (int i = 0; i < branch.WavePoints.Count - 1; i++)
            {
                var currentPoint = branch.WavePoints[i];
                var nextPoint = branch.WavePoints[i + 1];

                // Calculate thickness at this point along the branch
                float progress = (float)i / (branch.WavePoints.Count - 1);
                int currentThickness = (int)(branch.StartThickness * (1 - progress) + branch.EndThickness * progress);
                currentThickness = Math.Max(1, currentThickness);

                // Draw thick tapered branch section
                DrawThickBranchLine(canvas, currentPoint.X, currentPoint.Y, nextPoint.X, nextPoint.Y, currentThickness);
            }

            // Update the branch endpoint to the last wave point for canopy positioning
            if (branch.WavePoints.Count > 0)
            {
                var lastPoint = branch.WavePoints.Last();
                branch.EndX = lastPoint.X;
                branch.EndY = lastPoint.Y;
            }
        }

        private void DrawThickBranchLine(char[,] canvas, int x0, int y0, int x1, int y1, int thickness)
        {
            // Draw thick branch with cross-sections like trunk
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            int x = x0, y = y0;

            while (true)
            {
                // Draw thick cross-section at each point
                DrawBranchCrossSection(canvas, x, y, thickness);

                if (x == x1 && y == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x += sx; }
                if (e2 < dx) { err += dx; y += sy; }
            }
        }

        private void DrawBranchCrossSection(char[,] canvas, int centerX, int centerY, int thickness)
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
                            char branchChar = GetBranchTexture(dx, dy, radius, thickness);
                            if (canvas[py, px] == ' ' || canvas[py, px] == '▪' || canvas[py, px] == '▫')
                            {
                                canvas[py, px] = branchChar;
                            }
                        }
                    }
                }
            }
        }

        private static char GetBranchTexture(int offsetX, int offsetY, double radius, int thickness)
        {
            double distance = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
            double ratio = distance / radius;

            // Branch texture based on thickness - thicker branches get more trunk-like texture
            if (thickness >= 6)
            {
                // Thick branch - trunk-like texture
                if (ratio < 0.3) return '┃';   // Core
                if (ratio < 0.6) return '┏';   // Inner
                return '┓';                    // Outer
            }
            else if (thickness >= 3)
            {
                // Medium branch
                if (ratio < 0.5) return '┏';   // Core
                return '┓';                    // Outer
            }
            else
            {
                // Thin branch
                return '┓';                    // Simple thin branch
            }
        }

        private void DrawWavyBranchLine(char[,] canvas, int x0, int y0, int x1, int y1, char branchChar)
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
                    if (canvas[y, x] == ' ' || canvas[y, x] == '▪' || canvas[y, x] == '▫')
                    {
                        canvas[y, x] = branchChar;
                    }
                }

                if (x == x1 && y == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x += sx; }
                if (e2 < dx) { err += dx; y += sy; }
            }
        }

        /// <summary>
        /// STEP 3: Add mushroom cap shaped leaf canopies with much larger proportional sizing
        /// Canopies are now 4-8 times larger on branches for dramatic realistic effect
        /// </summary>
        private void Step3_AddMushroomCapCanopies(char[,] canvas)
        {
            // Add much larger mushroom cap canopies at each branch endpoint
            foreach (var branch in branches)
            {
                var canopy = CreateLargeMushroomCapCanopy(branch);
                leafCanopies.Add(canopy);
            }

            // Add large apex mushroom canopy at trunk top
            if (trunkSegments.Count > 0)
            {
                var apex = trunkSegments.Last();
                var apexCanopy = CreateLargeApexMushroomCanopy(apex);
                leafCanopies.Add(apexCanopy);
            }

            // Draw all large mushroom cap canopies
            foreach (var canopy in leafCanopies)
            {
                DrawMushroomCapCanopy(canvas, canopy);
            }
        }

        private LeafCanopy CreateLargeMushroomCapCanopy(Branch branch)
        {
            // CANOPY SIZE: 4-8 times larger than before - much more dramatic
            float largeSizeRatio = 1.0f + (float)random.NextDouble() * 1.0f; // Random between 1.0 and 2.0 (4-8x larger than 0.25-0.5)
            int canopyWidth = Math.Max(12, (int)(branch.Length * largeSizeRatio)); // Minimum 12 for dramatic effect

            // HEIGHT: Proportional to width for mushroom shape
            float heightRatio = 0.6f + (float)random.NextDouble() * 0.6f; // Random between 0.6 and 1.2
            int canopyHeight = Math.Max(8, (int)(canopyWidth * heightRatio)); // Minimum 8 for substantial height

            // Position large mushroom canopy at branch endpoint, pointing upward
            int canopyX = branch.EndX;
            int canopyY = branch.EndY - (canopyHeight / 2); // Center vertically above endpoint

            // Density variation for natural large mushroom appearance
            float density = 0.70f + random.Next(0, 25) / 100f; // Slightly lower density for larger canopies
            density = Math.Min(0.95f, density);

            return new LeafCanopy
            {
                CenterX = canopyX,
                CenterY = canopyY,
                Width = canopyWidth,
                Height = canopyHeight,
                Density = density,
                CanopyType = LeafCanopy.Type.MushroomCap,
                IsMushroomShaped = true
            };
        }

        private LeafCanopy CreateLargeApexMushroomCanopy(TrunkSegment apex)
        {
            // Large apex mushroom canopy proportional to trunk
            int apexBaseSize = Math.Max(8, trunkLength / 5); // Larger base size
            int apexWidth = apexBaseSize + random.Next(-2, 6);
            int apexHeight = (int)(apexWidth * (0.7f + random.NextDouble() * 0.5f));

            apexWidth = Math.Max(8, apexWidth); // Larger minimum
            apexHeight = Math.Max(6, apexHeight); // Larger minimum

            return new LeafCanopy
            {
                CenterX = apex.X,
                CenterY = apex.Y - (apexHeight / 2),
                Width = apexWidth,
                Height = apexHeight,
                Density = 0.80f,
                CanopyType = LeafCanopy.Type.MushroomCap,
                IsMushroomShaped = true
            };
        }

        private void DrawMushroomCapCanopy(char[,] canvas, LeafCanopy canopy)
        {
            // Mushroom cap characters for upward-pointing canopies
            char[] mushroomChars = ['●', '○', '◆', '◇', '◈', '◉', '◊', '⬢', '⬡', '⬟', '⬠', '⬣'];

            int halfWidth = canopy.Width / 2;
            int halfHeight = canopy.Height / 2;

            for (int dy = -halfHeight; dy <= halfHeight; dy++)
            {
                for (int dx = -halfWidth; dx <= halfWidth; dx++)
                {
                    int leafX = canopy.CenterX + dx;
                    int leafY = canopy.CenterY + dy;

                    if (leafX >= 0 && leafX < width && leafY >= 0 && leafY < height)
                    {
                        // Create MUSHROOM CAP SHAPE - wider at top, narrower at bottom, always pointing upward
                        float heightProgress = (float)(dy + halfHeight) / canopy.Height; // 0 = top, 1 = bottom

                        // Mushroom cap is wider at top (heightProgress near 0) and narrower at bottom (heightProgress near 1)
                        float widthMultiplier = 1.0f - (heightProgress * 0.6f); // Reduces width toward bottom
                        widthMultiplier = Math.Max(0.3f, widthMultiplier); // Minimum width at bottom for larger canopies

                        double adjustedWidthRadius = halfWidth * widthMultiplier;
                        double distance = Math.Sqrt((dx * dx) / (adjustedWidthRadius * adjustedWidthRadius) +
                                                  (dy * dy) / (double)(halfHeight * halfHeight));

                        if (distance <= 1.0)
                        {
                            // Large mushroom cap density - denser at top, lighter toward bottom
                            double probability = canopy.Density * (1.2 - distance) * (1.1 - heightProgress * 0.25);
                            probability = Math.Max(0.0, Math.Min(1.0, probability));

                            if (random.NextDouble() < probability)
                            {
                                // Select mushroom character based on position - denser chars at top
                                int charIndex = heightProgress < 0.3f ?
                                    random.Next(0, 5) :          // Dense top characters
                                    heightProgress < 0.7f ?
                                    random.Next(5, 9) :         // Medium characters
                                    random.Next(9, mushroomChars.Length); // Lighter bottom characters

                                char mushroomChar = mushroomChars[charIndex];

                                // Only draw on empty space
                                if (canvas[leafY, leafX] == ' ')
                                {
                                    canvas[leafY, leafX] = mushroomChar;
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

    public class Branch
    {
        public int StartX { get; set; }
        public int StartY { get; set; }
        public int EndX { get; set; }
        public int EndY { get; set; }
        public bool IsLeft { get; set; }
        public int Length { get; set; }
        public int BranchIndex { get; set; }
        public float HeightRatio { get; set; }
        public int StartThickness { get; set; } // NEW: Track start thickness
        public int EndThickness { get; set; }   // NEW: Track end thickness
        public List<Point> WavePoints { get; set; } = [];
    }

    public class LeafCanopy
    {
        public enum Type { MushroomCap, Apex }

        public int CenterX { get; set; }
        public int CenterY { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public float Density { get; set; }
        public Type CanopyType { get; set; }
        public bool IsMushroomShaped { get; set; } = false;
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