using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BonsaiTreeGenerator
{
    /// <summary>
    /// Main form for the Realistic Procedural 2D Bonsai Tree Generator
    /// Professional UI with clean layout and consistent font sizing
    /// Using improved 3-step process with wavy S-shaped trunk and branches
    /// </summary>
    public partial class BonsaiTreeForm : Form
    {
        #region Private Fields

        // Core components
        private readonly Random random = new();
        private readonly BonsaiTreeGenerator treeGenerator;
        private readonly object lockObject = new();

        // UI Components
        private TableLayoutPanel? mainContainer;
        private Panel? treePanel;
        private Panel? statsPanel;
        private Panel? buttonPanel;
        private RichTextBox? treeDisplay;
        private RichTextBox? statsDisplay;
        private Button? generateButton;
        private Button? saveButton;
        private Button? waterButton;
        private Button? killButton;
        private Label? statusLabel;
        private ProgressBar? progressBar;

        // Configuration constants
        private const int TREE_WIDTH = 90;
        private const int TREE_HEIGHT = 35;
        private const int MAX_GENERATION_TIME_MS = 100000; // 100 seconds timeout

        // Threading and state management
        private CancellationTokenSource cancellationTokenSource = new();
        private volatile bool isGenerating = false;
        private volatile bool isFormReady = false;
        private bool disposed = false;

        // Current tree data
        private char[,]? currentTree;
        private readonly Dictionary<char, Color>? colorMapping;
        private BonsaiStats? currentStats;

        // Enhanced rain effect
        private System.Windows.Forms.Timer? rainTimer;
        private List<RainDrop>? rainDrops;
        private int rainFrameCount = 0;
        private readonly int RAIN_DROP_COUNT = 120; // Doubled for more dramatic effect
        private readonly int RAIN_INTERVAL = 50; // Faster animation - reduced from 100ms to 50ms
        private readonly int RAIN_DURATION_FRAMES = 10; // Longer duration - increased from 10 to 40 frames

        #endregion

        #region Constructor and Form Setup

        public BonsaiTreeForm()
        {
            try
            {
                treeGenerator = new BonsaiTreeGenerator(random);
                colorMapping = BonsaiTreeGenerator.GetColorMapping();

                InitializeComponent();
                SetupUserInterface();

                // Enable double buffering for smooth rendering and reduce flicker
                SetStyle(ControlStyles.AllPaintingInWmPaint |
                        ControlStyles.UserPaint |
                        ControlStyles.DoubleBuffer |
                        ControlStyles.ResizeRedraw |
                        ControlStyles.OptimizedDoubleBuffer, true);

                // Reduce flicker during updates
                typeof(Control).InvokeMember("DoubleBuffered",
                    System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                    null, this, [true]);
            }
            catch (Exception ex)
            {
                ShowError("Failed to initialize Realistic Bonsai Tree Generator", ex);
            }
        }

        private void SetupUserInterface()
        {
            try
            {
                Text = "Personal Bonsai Tree Generator - The Only Friends You Have Left!";

                // Start maximized
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;
                BackColor = Color.FromArgb(248, 249, 250);

                CreateMainLayout();
                CreateTreeDisplay();
                CreateStatsPanel();
                CreateControlPanel();
            }
            catch (Exception ex)
            {
                ShowError("Failed to setup user interface", ex);
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                isFormReady = true;
                _ = GenerateTreeAsync();
            }
            catch (Exception ex)
            {
                ShowError("Failed to load initial tree", ex);
            }
        }

        #endregion

        #region UI Creation Methods

        private void CreateMainLayout()
        {
            mainContainer?.Dispose();

            mainContainer = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 2,
                BackColor = Color.FromArgb(248, 249, 250),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None,
                Padding = new Padding(12)
            };

            // Configure column styles - tree takes more space
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F)); // Tree area
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // Stats area

            // Configure row styles
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 88F)); // Main content
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 12F));  // Controls

            Controls.Add(mainContainer);
        }

        private void CreateTreeDisplay()
        {
            if (mainContainer == null) return;

            treePanel?.Dispose();

            treePanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0, 0, 6, 0),
                AutoScroll = false,
                Padding = new Padding(2)
            };

            // Add subtle shadow effect
            treePanel.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, treePanel.ClientRectangle,
                    Color.FromArgb(200, 200, 200), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(200, 200, 200), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(200, 200, 200), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(200, 200, 200), 1, ButtonBorderStyle.Solid);
            };

            // Create the main tree display using RichTextBox with enhanced double buffering
            treeDisplay?.Dispose();
            treeDisplay = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 9F, FontStyle.Regular),
                BackColor = Color.Black,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.None,
                WordWrap = false,
                DetectUrls = false,
                EnableAutoDragDrop = false,
                HideSelection = false,
                Margin = new Padding(8)
            };

            // Enable double buffering for the tree display to reduce flicker
            typeof(Control).InvokeMember("DoubleBuffered",
                System.Reflection.BindingFlags.SetProperty | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
                null, treeDisplay, [true]);

            // Create tree title - removed emoji
            var treeTitle = new Label
            {
                Text = "YOUR PERSONAL BONSAI TREE",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(0, 12, 0, 0)
            };

            // Create progress bar
            progressBar?.Dispose();
            progressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = 6,
                Style = ProgressBarStyle.Continuous,
                Visible = false,
                ForeColor = Color.FromArgb(46, 204, 113)
            };

            treePanel.Controls.AddRange([treeDisplay, progressBar, treeTitle]);
            mainContainer.Controls.Add(treePanel, 0, 0);
        }

        private void CreateStatsPanel()
        {
            if (mainContainer == null) return;

            var statsContainer = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent,
                Margin = new Padding(6, 0, 0, 0)
            };

            // Stats display panel
            statsPanel?.Dispose();
            statsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Padding = new Padding(2)
            };

            // Add subtle border
            statsPanel.Paint += (s, e) =>
            {
                ControlPaint.DrawBorder(e.Graphics, statsPanel.ClientRectangle,
                    Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid,
                    Color.FromArgb(220, 220, 220), 1, ButtonBorderStyle.Solid);
            };

            // Stats title - removed emoji
            var statsTitle = new Label
            {
                Text = "BONSAI STATS",
                Font = new Font("Segoe UI", 22F, FontStyle.Bold),
                ForeColor = Color.FromArgb(52, 73, 94),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 55,
                BackColor = Color.FromArgb(236, 240, 241),
                Padding = new Padding(0, 12, 0, 0)
            };

            statsDisplay?.Dispose();
            statsDisplay = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 13F), // Increased font size from 10F to 13F
                DetectUrls = false,
                EnableAutoDragDrop = false,
                Padding = new Padding(16, 12, 16, 12)
            };

            // Create button panel below stats
            buttonPanel?.Dispose();
            buttonPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 120,
                BackColor = Color.White,
                Padding = new Padding(16, 8, 16, 16)
            };

            // Create Water button - removed emoji
            waterButton?.Dispose();
            waterButton = new Button
            {
                Text = "Water Your Bonsai",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Size = new Size(0, 40),
                Dock = DockStyle.Top,
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Margin = new Padding(0, 0, 0, 8)
            };
            waterButton.FlatAppearance.BorderSize = 0;
            waterButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 174, 96);
            waterButton.Click += WaterButton_Click;

            // Create Kill button
            killButton?.Dispose();
            killButton = new Button
            {
                Text = "Kill Your Bonsai",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Size = new Size(0, 40),
                Dock = DockStyle.Bottom,
                BackColor = Color.FromArgb(231, 76, 60),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Margin = new Padding(0, 8, 0, 0)
            };
            killButton.FlatAppearance.BorderSize = 0;
            killButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(192, 57, 43);
            killButton.Click += KillButton_Click;

            buttonPanel.Controls.AddRange([waterButton, killButton]);
            statsPanel.Controls.AddRange([statsDisplay, buttonPanel, statsTitle]);
            statsContainer.Controls.Add(statsPanel);

            mainContainer.Controls.Add(statsContainer, 1, 0);
        }

        private void CreateControlPanel()
        {
            if (mainContainer == null) return;

            var controlPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(236, 240, 241),
                BorderStyle = BorderStyle.None,
                Margin = new Padding(0, 8, 0, 0),
                Padding = new Padding(16, 12, 16, 12)
            };

            // Add subtle top border
            controlPanel.Paint += (s, e) =>
            {
                using var pen = new Pen(Color.FromArgb(189, 195, 199), 1);
                e.Graphics.DrawLine(pen, 0, 0, controlPanel.Width, 0);
            };

            // Generate Tree button - removed emoji
            generateButton?.Dispose();
            generateButton = new Button
            {
                Text = "Adopt New Bonsai",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Size = new Size(200, 45),
                Location = new Point(16, 12),
                BackColor = Color.FromArgb(46, 204, 113),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            generateButton.FlatAppearance.BorderSize = 0;
            generateButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(39, 174, 96);
            generateButton.Click += GenerateButton_Click;

            // Save Tree button - removed emoji
            saveButton?.Dispose();
            saveButton = new Button
            {
                Text = "Save Bonsai",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Size = new Size(160, 45),
                Location = new Point(232, 12),
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            saveButton.FlatAppearance.BorderSize = 0;
            saveButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(41, 128, 185);
            saveButton.Click += SaveButton_Click;

            // Status label with updated timestamp
            statusLabel?.Dispose();
            statusLabel = new Label
            {
                Text = "C. Saunders - 2025",
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                ForeColor = Color.FromArgb(127, 140, 141),
                Location = new Point(420, 20),
                Size = new Size(500, 25),
                TextAlign = ContentAlignment.MiddleLeft
            };

            controlPanel.Controls.AddRange([generateButton, saveButton, statusLabel]);
            mainContainer.Controls.Add(controlPanel, 0, 1);
            mainContainer.SetColumnSpan(controlPanel, 2);
        }

        #endregion

        #region Tree Generation Engine

        private async Task GenerateTreeAsync()
        {
            if (!isFormReady || isGenerating) return;

            try
            {
                lock (lockObject)
                {
                    if (isGenerating) return;
                    isGenerating = true;
                }

                // Cancel any existing generation
                cancellationTokenSource?.Cancel();
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = new CancellationTokenSource();

                SetUIGenerating(true);

                using var timeoutCts = new CancellationTokenSource(MAX_GENERATION_TIME_MS);
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(
                    cancellationTokenSource.Token, timeoutCts.Token);

                // Create progress reporter
                var progress = new Progress<int>(UpdateProgress);

                // Generate new stats for the bonsai
                currentStats = new BonsaiStats(random);

                // Generate the realistic bonsai tree using improved 3-step wavy process
                currentTree = await treeGenerator.GenerateTreeAsync(
                    TREE_WIDTH, TREE_HEIGHT, progress, combinedCts.Token);

                if (!combinedCts.Token.IsCancellationRequested && !IsDisposed && isFormReady)
                {
                    await ApplyTreeToUI(currentTree, combinedCts.Token);
                    UpdateStatsDisplay();
                    UpdateStatusLabel();
                }
            }
            catch (OperationCanceledException)
            {
                // Normal cancellation, no error message needed
            }
            catch (Exception ex)
            {
                ShowError("Failed to generate realistic bonsai tree using improved 3-step wavy process", ex);
            }
            finally
            {
                SetUIGenerating(false);
                lock (lockObject)
                {
                    isGenerating = false;
                }
            }
        }

        private async Task ApplyTreeToUI(char[,] tree, CancellationToken cancellationToken)
        {
            if (!isFormReady || treeDisplay == null || treeDisplay.IsDisposed) return;

            await Task.Run(() =>
            {
                SafeInvoke(() =>
                {
                    if (isFormReady && treeDisplay != null && !treeDisplay.IsDisposed && treeDisplay.IsHandleCreated)
                    {
                        // Suspend layout to prevent flicker during updates
                        treeDisplay.SuspendLayout();

                        try
                        {
                            // Convert tree array to string
                            var treeText = ConvertTreeToString(tree);

                            // Clear and set text in one operation
                            treeDisplay.Clear();
                            treeDisplay.Text = treeText;

                            // Apply color formatting efficiently
                            ApplyColorFormattingOptimized();
                        }
                        finally
                        {
                            // Resume layout and refresh
                            treeDisplay.ResumeLayout(true);
                            treeDisplay.Refresh();
                        }
                    }
                });
            }, cancellationToken);
        }

        private static string ConvertTreeToString(char[,] tree)
        {
            var treeText = new StringBuilder();
            int height = tree.GetLength(0);
            int width = tree.GetLength(1);

            for (int row = 0; row < height; row++)
            {
                for (int col = 0; col < width; col++)
                {
                    treeText.Append(tree[row, col]);
                }
                if (row < height - 1)
                {
                    treeText.AppendLine();
                }
            }

            return treeText.ToString();
        }

        private void ApplyColorFormatting()
        {
            try
            {
                if (!isFormReady || treeDisplay == null || treeDisplay.IsDisposed ||
                    !treeDisplay.IsHandleCreated || colorMapping == null) return;

                // Set default color (black background)
                treeDisplay.SelectAll();
                treeDisplay.SelectionBackColor = Color.Black;
                treeDisplay.SelectionStart = 0;

                // Apply realistic bonsai colors using improved 3-step wavy process color mapping
                string text = treeDisplay.Text;
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    if (colorMapping.TryGetValue(c, out Color color))
                    {
                        treeDisplay.SelectionStart = i;
                        treeDisplay.SelectionLength = 1;
                        treeDisplay.SelectionColor = color;
                    }
                }

                // Reset selection
                treeDisplay.SelectionStart = 0;
                treeDisplay.SelectionLength = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Color formatting error: {ex.Message}");
            }
        }

        private void ApplyColorFormattingOptimized()
        {
            try
            {
                if (!isFormReady || treeDisplay == null || treeDisplay.IsDisposed ||
                    !treeDisplay.IsHandleCreated || colorMapping == null) return;

                // Set default color (black background)
                treeDisplay.SelectAll();
                treeDisplay.SelectionBackColor = Color.Black;
                treeDisplay.SelectionStart = 0;

                // Apply realistic bonsai colors in optimized batches
                string text = treeDisplay.Text;
                var colorRanges = new Dictionary<Color, List<(int start, int length)>>();

                // Group consecutive characters of the same color
                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    if (colorMapping.TryGetValue(c, out Color color))
                    {
                        if (!colorRanges.ContainsKey(color))
                            colorRanges[color] = [];

                        colorRanges[color].Add((i, 1));
                    }
                }

                // Apply colors in batches
                foreach (var kvp in colorRanges)
                {
                    Color color = kvp.Key;
                    foreach (var (start, length) in kvp.Value)
                    {
                        treeDisplay.SelectionStart = start;
                        treeDisplay.SelectionLength = length;
                        treeDisplay.SelectionColor = color;
                    }
                }

                // Reset selection
                treeDisplay.SelectionStart = 0;
                treeDisplay.SelectionLength = 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Optimized color formatting error: {ex.Message}");
            }
        }

        #endregion

        #region Stats Display

        private void UpdateStatsDisplay()
        {
            try
            {
                if (statsDisplay == null || statsDisplay.IsDisposed || currentStats == null) return;

                // Suspend layout to prevent flicker
                statsDisplay.SuspendLayout();

                try
                {
                    statsDisplay.Clear();

                    // Center alignment for all stats text
                    statsDisplay.SelectionAlignment = HorizontalAlignment.Center;

                    // Bonsai name - larger font size and centered
                    statsDisplay.SelectionFont = new Font("Segoe UI", 18F, FontStyle.Bold); // Increased from 16F
                    statsDisplay.SelectionColor = Color.FromArgb(52, 73, 94);
                    statsDisplay.AppendText($"Your Bonsai's Name is {currentStats.Name}\n\n");

                    // Age section - larger font and centered
                    statsDisplay.SelectionFont = new Font("Segoe UI", 14F, FontStyle.Bold); // Increased from 11F
                    statsDisplay.SelectionColor = Color.FromArgb(149, 165, 166);
                    statsDisplay.AppendText("AGE\n");
                    statsDisplay.SelectionFont = new Font("Segoe UI", 13F, FontStyle.Regular); // Increased from 10F
                    statsDisplay.SelectionColor = Color.FromArgb(44, 62, 80);
                    statsDisplay.AppendText($"{currentStats.Age}\n\n");

                    // Likes section - larger font, centered, removed emoji
                    statsDisplay.SelectionFont = new Font("Segoe UI", 14F, FontStyle.Bold); // Increased from 11F
                    statsDisplay.SelectionColor = Color.FromArgb(46, 204, 113);
                    statsDisplay.AppendText("LIKES\n");
                    statsDisplay.SelectionFont = new Font("Segoe UI", 13F, FontStyle.Regular); // Increased from 10F
                    statsDisplay.SelectionColor = Color.FromArgb(44, 62, 80);
                    foreach (string like in currentStats.Likes)
                    {
                        // Remove bullet point for cleaner centered look
                        statsDisplay.AppendText($"{like}\n");
                    }
                    statsDisplay.AppendText("\n");

                    // Dislikes section - larger font, centered, removed emoji
                    statsDisplay.SelectionFont = new Font("Segoe UI", 14F, FontStyle.Bold); // Increased from 11F
                    statsDisplay.SelectionColor = Color.FromArgb(231, 76, 60);
                    statsDisplay.AppendText("DISLIKES\n");
                    statsDisplay.SelectionFont = new Font("Segoe UI", 13F, FontStyle.Regular); // Increased from 10F
                    statsDisplay.SelectionColor = Color.FromArgb(44, 62, 80);
                    foreach (string dislike in currentStats.Dislikes)
                    {
                        // Remove bullet point for cleaner centered look
                        statsDisplay.AppendText($"{dislike}\n");
                    }
                    statsDisplay.AppendText("\n");

                    // Care instructions - larger font, centered, removed emoji
                    statsDisplay.SelectionFont = new Font("Segoe UI", 12F, FontStyle.Italic); // Increased from 9F
                    statsDisplay.SelectionColor = Color.FromArgb(127, 140, 141);
                    statsDisplay.AppendText("Take good care of your bonsai! Water it regularly and treat it with love.");
                }
                finally
                {
                    statsDisplay.ResumeLayout(true);
                    statsDisplay.Refresh();
                }
            }
            catch (Exception ex)
            {
                ShowError("Failed to update stats display", ex);
            }
        }

        #endregion

        #region Enhanced Rain Effect

        private void StartRainEffect()
        {
            try
            {
                if (treeDisplay == null || treeDisplay.IsDisposed) return;

                // Initialize dramatic rain drops - doubled the count
                rainDrops = [];
                for (int i = 0; i < RAIN_DROP_COUNT; i++)
                {
                    rainDrops.Add(new RainDrop
                    {
                        X = random.Next(TREE_WIDTH),
                        Y = random.Next(-20, 0), // Start higher for more dramatic effect
                        Speed = random.Next(2, 5), // Faster speeds (was 1-3, now 2-5)
                        Character = GetRandomRainCharacter(), // Varied rain characters
                        Intensity = random.NextDouble() // For varied visual intensity
                    });
                }

                rainFrameCount = 0;

                // Start enhanced rain timer - faster animation
                rainTimer?.Dispose();
                rainTimer = new System.Windows.Forms.Timer
                {
                    Interval = RAIN_INTERVAL // 50ms for quicker, more dramatic animation
                };
                rainTimer.Tick += RainTimer_Tick;
                rainTimer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Enhanced rain effect error: {ex.Message}");
            }
        }

        private char GetRandomRainCharacter()
        {
            // Various rain drop characters for more dramatic effect
            char[] rainChars = ['|', '¦', '│', '┃', '║', '∣', '⎮', '❘'];
            return rainChars[random.Next(rainChars.Length)];
        }

        private void RainTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (rainDrops == null || currentTree == null) return;

                rainFrameCount++;

                // Longer dramatic rain duration - 40 frames at 50ms each = 2 seconds
                if (rainFrameCount >= RAIN_DURATION_FRAMES)
                {
                    rainTimer?.Stop();
                    rainTimer?.Dispose();
                    rainTimer = null;

                    // Restore original tree with optimized rendering
                    if (currentTree != null)
                    {
                        var treeText = ConvertTreeToString(currentTree);
                        SafeInvoke(() =>
                        {
                            if (treeDisplay != null && !treeDisplay.IsDisposed)
                            {
                                treeDisplay.SuspendLayout();
                                try
                                {
                                    treeDisplay.Clear();
                                    treeDisplay.Text = treeText;
                                    ApplyColorFormattingOptimized();
                                }
                                finally
                                {
                                    treeDisplay.ResumeLayout(true);
                                    treeDisplay.Refresh();
                                }
                            }
                        });
                    }
                    return;
                }

                // Create dramatic rain frame
                var rainCanvas = (char[,])currentTree.Clone();

                // Update and draw enhanced rain drops
                foreach (var drop in rainDrops)
                {
                    drop.Y += drop.Speed;

                    // Reset drop if it goes off screen - with varied starting positions
                    if (drop.Y >= TREE_HEIGHT)
                    {
                        drop.Y = random.Next(-20, -5); // Higher starting position
                        drop.X = random.Next(TREE_WIDTH);
                        drop.Speed = random.Next(2, 5); // Re-randomize speed
                        drop.Character = GetRandomRainCharacter(); // New character
                        drop.Intensity = random.NextDouble(); // New intensity
                    }

                    // Draw multiple drops per position for intensity
                    for (int offset = 0; offset < 3; offset++)
                    {
                        int dropY = drop.Y - offset;
                        if (dropY >= 0 && dropY < TREE_HEIGHT && drop.X >= 0 && drop.X < TREE_WIDTH)
                        {
                            if (rainCanvas[dropY, drop.X] == ' ')
                            {
                                // Use different characters based on position for trail effect
                                char rainChar = offset == 0 ? drop.Character :
                                              offset == 1 ? '˙' :
                                              '·';
                                rainCanvas[dropY, drop.X] = rainChar;
                            }
                        }
                    }
                }

                // Add splash effects at ground level
                for (int x = 0; x < TREE_WIDTH; x++)
                {
                    if (random.NextDouble() > 0.7) // 30% chance of splash
                    {
                        int splashY = TREE_HEIGHT - 1;
                        if (splashY >= 0 && rainCanvas[splashY, x] == ' ')
                        {
                            rainCanvas[splashY, x] = random.NextDouble() > 0.5 ? '∶' : '˙';
                        }
                    }
                }

                // Update display with enhanced rendering
                var rainText = ConvertTreeToString(rainCanvas);
                SafeInvoke(() =>
                {
                    if (treeDisplay != null && !treeDisplay.IsDisposed)
                    {
                        treeDisplay.SuspendLayout();
                        try
                        {
                            treeDisplay.Clear();
                            treeDisplay.Text = rainText;

                            // Apply original tree colors first
                            ApplyColorFormattingOptimized();

                            // Then apply enhanced rain colors
                            ApplyEnhancedRainColors();
                        }
                        finally
                        {
                            treeDisplay.ResumeLayout(true);
                            treeDisplay.Refresh();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Enhanced rain timer error: {ex.Message}");
            }
        }

        private void ApplyEnhancedRainColors()
        {
            try
            {
                if (treeDisplay == null || treeDisplay.IsDisposed) return;

                string text = treeDisplay.Text;
                char[] rainChars = ['|', '¦', '│', '┃', '║', '∣', '⎮', '❘', '˙', '·', '∶'];

                for (int i = 0; i < text.Length; i++)
                {
                    char c = text[i];
                    if (rainChars.Contains(c))
                    {
                        treeDisplay.SelectionStart = i;
                        treeDisplay.SelectionLength = 1;

                        // Enhanced rain colors - blues with intensity variation
                        Color rainColor = c switch
                        {
                            '|' or '¦' or '│' or '┃' or '║' or '∣' or '⎮' or '❘' => Color.FromArgb(52, 152, 219), // Main rain blue
                            '˙' or '·' => Color.FromArgb(174, 214, 241), // Light splash blue
                            '∶' => Color.FromArgb(93, 173, 226), // Medium splash blue
                            _ => Color.FromArgb(52, 152, 219)
                        };

                        treeDisplay.SelectionColor = rainColor;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Enhanced rain color error: {ex.Message}");
            }
        }

        private class RainDrop
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Speed { get; set; }
            public char Character { get; set; } = '|';
            public double Intensity { get; set; } = 1.0;
        }

        #endregion

        #region Event Handlers

        private async void GenerateButton_Click(object? sender, EventArgs e)
        {
            await GenerateTreeAsync();
        }

        private void WaterButton_Click(object? sender, EventArgs e)
        {
            try
            {
                StartRainEffect();

                // Show positive message
                MessageBox.Show($"{currentStats?.Name ?? "Your bonsai"} loves the water! \n\nYour bonsai is happy and healthy!",
                    "Watering Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                ShowError("Failed to water bonsai", ex);
            }
        }

        private void KillButton_Click(object? sender, EventArgs e)
        {
            try
            {
                string[] killWarnings = [
                    "NO.", "PAPA WHYYYYY", "What is wrong with you", "Don't you dare!",
                    "Why would you do this?!", "I trusted you!", "This is cruel!",
                    "Please reconsider!", "Have mercy!", "Think of the children!",
                    "I had so much to live for!", "But we were just getting to know each other!",
                    "I thought we were friends!", "This is not the way!"
                ];

                string warning = killWarnings[random.Next(killWarnings.Length)];

                MessageBox.Show($"{warning}\n\n {currentStats?.Name ?? "Your bonsai"} doesn't want to die!",
                    "Bonsai Plea for Life", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                ShowError("Kill button error", ex);
            }
        }

        private async void SaveButton_Click(object? sender, EventArgs e)
        {
            try
            {
                if (!isFormReady || treeDisplay?.Text == null || string.IsNullOrEmpty(treeDisplay.Text))
                {
                    MessageBox.Show("No bonsai tree to save. Please adopt a bonsai first.", "Save Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                using var saveDialog = new SaveFileDialog
                {
                    Filter = "Text files (*.txt)|*.txt|Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"Bonsai_{currentStats?.Name ?? "Tree"}_{DateTime.Now:yyyyMMdd_HHmmss}.txt",
                    Title = "Save Your Bonsai"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    SetUIGenerating(true);

                    await Task.Run(() =>
                    {
                        var content = CreateSaveContent();
                        File.WriteAllText(saveDialog.FileName, content, Encoding.UTF8);
                    });

                    MessageBox.Show($"Your bonsai {currentStats?.Name ?? "tree"} saved successfully to:\n{saveDialog.FileName}",
                        "Save Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                ShowError("Failed to save bonsai tree", ex);
            }
            finally
            {
                SetUIGenerating(false);
            }
        }

        #endregion

        #region Helper Methods

        private string CreateSaveContent()
        {
            var content = new StringBuilder();
            content.AppendLine($"YOUR BONSAI PET: {currentStats?.Name ?? "Unknown"}");
            content.AppendLine("=" + new string('=', 105));
            content.AppendLine();
            content.AppendLine(treeDisplay?.Text ?? "");
            content.AppendLine();
            content.AppendLine("BONSAI STATS:");
            content.AppendLine("=" + new string('=', 15));
            content.AppendLine($"Name: {currentStats?.Name ?? "Unknown"}");
            content.AppendLine($"Age: {currentStats?.Age ?? "Unknown"}");
            content.AppendLine();
            content.AppendLine("Likes:");
            if (currentStats?.Likes != null)
            {
                foreach (string like in currentStats.Likes)
                {
                    content.AppendLine($"• {like}");
                }
            }
            content.AppendLine();
            content.AppendLine("Dislikes:");
            if (currentStats?.Dislikes != null)
            {
                foreach (string dislike in currentStats.Dislikes)
                {
                    content.AppendLine($"• {dislike}");
                }
            }
            content.AppendLine();
            content.AppendLine("C. Saunders - 2025");
            content.AppendLine("personal Bonsai Tree Generator");

            return content.ToString();
        }

        private void UpdateProgress(int progress)
        {
            SafeInvoke(() =>
            {
                if (progressBar != null && !progressBar.IsDisposed && progressBar.IsHandleCreated)
                {
                    progressBar.Value = Math.Min(progress, 100);
                }
            });
        }

        private void SetUIGenerating(bool generating)
        {
            SafeInvoke(() =>
            {
                if (generateButton != null && !generateButton.IsDisposed && generateButton.IsHandleCreated)
                {
                    generateButton.Enabled = !generating;
                    generateButton.Text = generating ? "Growing..." : "Adopt New Bonsai";
                }

                if (saveButton != null && !saveButton.IsDisposed && saveButton.IsHandleCreated)
                {
                    saveButton.Enabled = !generating;
                }

                if (progressBar != null && !progressBar.IsDisposed && progressBar.IsHandleCreated)
                {
                    progressBar.Visible = generating;
                    if (generating)
                    {
                        progressBar.Value = 0;
                    }
                }
            });
        }

        private void UpdateStatusLabel()
        {
            SafeInvoke(() =>
            {
                if (statusLabel != null && !statusLabel.IsDisposed && statusLabel.IsHandleCreated)
                {
                    statusLabel.Text = "C. Saunders - 2025";
                }
            });
        }

        private void SafeInvoke(Action action)
        {
            try
            {
                if (!isFormReady) return;

                if (InvokeRequired)
                {
                    try
                    {
                        BeginInvoke(action);
                    }
                    catch (InvalidOperationException)
                    {
                        // Handle may not be created yet, ignore
                    }
                }
                else
                {
                    action();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SafeInvoke error: {ex.Message}");
            }
        }

        private void ShowError(string message, Exception ex)
        {
            SafeInvoke(() =>
            {
                var errorMessage = $"{message}\n\nError: {ex.Message}";
                if (ex.InnerException != null)
                {
                    errorMessage += $"\nInner Exception: {ex.InnerException.Message}";
                }

                MessageBox.Show(errorMessage, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                System.Diagnostics.Debug.WriteLine($"Error: {message} - {ex}");
            });
        }

        #endregion

        #region Resource Management

        private void ReleaseFormResources()
        {
            try
            {
                isFormReady = false;

                // Stop rain timer
                rainTimer?.Stop();
                rainTimer?.Dispose();

                cancellationTokenSource?.Cancel();
                Thread.Sleep(100);

                cancellationTokenSource?.Dispose();

                // Dispose UI components
                treeDisplay?.Dispose();
                treePanel?.Dispose();
                statsPanel?.Dispose();
                statsDisplay?.Dispose();
                buttonPanel?.Dispose();
                generateButton?.Dispose();
                saveButton?.Dispose();
                waterButton?.Dispose();
                killButton?.Dispose();
                statusLabel?.Dispose();
                progressBar?.Dispose();
                mainContainer?.Dispose();

                disposed = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Resource cleanup error: {ex.Message}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!disposed && disposing)
            {
                if (components == null)
                {
                }
                else
                {
                    components.Dispose();
                }
                ReleaseFormResources();
            }
            base.Dispose(disposing);
        }

        #endregion
    }
}