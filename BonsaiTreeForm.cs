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
    /// Maximized window with proper sizing and bonsai stats
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
        private const int MAX_GENERATION_TIME_MS = 10000; // 10 seconds timeout
        
        // Threading and state management
        private CancellationTokenSource cancellationTokenSource = new();
        private volatile bool isGenerating = false;
        private volatile bool isFormReady = false;
        private bool disposed = false;
        
        // Current tree data
        private char[,]? currentTree;
        private Dictionary<char, Color>? colorMapping;
        private BonsaiStats? currentStats;
        
        // Rain effect
        private Timer? rainTimer;
        private List<RainDrop>? rainDrops;
        private int rainFrameCount = 0;
        
        #endregion

        #region Constructor and Form Setup

        public BonsaiTreeForm()
        {
            try
            {
                treeGenerator = new BonsaiTreeGenerator(random);
                colorMapping = treeGenerator.GetColorMapping();
                
                InitializeComponent();
                SetupUserInterface();
                
                // Enable double buffering for smooth rendering
                SetStyle(ControlStyles.AllPaintingInWmPaint | 
                        ControlStyles.UserPaint | 
                        ControlStyles.DoubleBuffer | 
                        ControlStyles.ResizeRedraw, true);
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
                Text = "Realistic Procedural 2D Bonsai Tree Generator - Interactive Pet Edition";
                
                // Start maximized
                WindowState = FormWindowState.Maximized;
                StartPosition = FormStartPosition.CenterScreen;
                BackColor = Color.FromArgb(240, 240, 235);
                
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
                BackColor = Color.FromArgb(240, 240, 235),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.None
            };

            // Configure column styles - tree takes more space
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70F)); // Tree area
            mainContainer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F)); // Stats area

            // Configure row styles
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 90F)); // Main content
            mainContainer.RowStyles.Add(new RowStyle(SizeType.Percent, 10F));  // Controls

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
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10),
                AutoScroll = false // No scroll bars as requested
            };

            // Create the main tree display using RichTextBox with no scroll bars
            treeDisplay?.Dispose();
            treeDisplay = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                Font = new Font("Consolas", 8F, FontStyle.Regular), // Smaller font to fit without scrolling
                BackColor = Color.Black,
                ForeColor = Color.White,
                BorderStyle = BorderStyle.None,
                ScrollBars = RichTextBoxScrollBars.None, // No scroll bars
                WordWrap = false,
                DetectUrls = false,
                EnableAutoDragDrop = false,
                HideSelection = false
            };

            // Create tree title
            var treeTitle = new Label
            {
                Text = "🌳 YOUR INTERACTIVE BONSAI PET 🌳",
                Font = new Font("Segoe UI", 14F, FontStyle.Bold),
                ForeColor = Color.FromArgb(101, 67, 33),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(245, 245, 240)
            };

            // Create progress bar
            progressBar?.Dispose();
            progressBar = new ProgressBar
            {
                Dock = DockStyle.Bottom,
                Height = 8,
                Style = ProgressBarStyle.Continuous,
                Visible = false,
                ForeColor = Color.FromArgb(34, 139, 34)
            };

            treePanel.Controls.AddRange([treeDisplay, progressBar, treeTitle]);
            mainContainer.Controls.Add(treePanel, 0, 0);
        }

        private void CreateStatsPanel()
        {
            if (mainContainer == null) return;

            statsPanel?.Dispose();
            
            statsPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(248, 246, 243),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(5),
                AutoScroll = true
            };

            var statsTitle = new Label
            {
                Text = "🎋 YOUR BONSAI'S STATS",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(101, 67, 33),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Top,
                Height = 40,
                BackColor = Color.FromArgb(240, 235, 230)
            };

            statsDisplay = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.FromArgb(248, 246, 243),
                BorderStyle = BorderStyle.None,
                Font = new Font("Segoe UI", 10F),
                DetectUrls = false,
                EnableAutoDragDrop = false,
                Margin = new Padding(10, 10, 10, 100) // Space for buttons at bottom
            };

            // Create Water button
            waterButton?.Dispose();
            waterButton = new Button
            {
                Text = "💧 Water Your Bonsai",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(10, statsPanel.Height - 90)
            };
            waterButton.FlatAppearance.BorderColor = Color.FromArgb(0, 100, 0);
            waterButton.Click += WaterButton_Click;

            // Create Kill button
            killButton?.Dispose();
            killButton = new Button
            {
                Text = "💀 Kill Your Bonsai",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                Size = new Size(200, 40),
                BackColor = Color.FromArgb(139, 34, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false,
                Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                Location = new Point(10, statsPanel.Height - 45)
            };
            killButton.FlatAppearance.BorderColor = Color.FromArgb(100, 0, 0);
            killButton.Click += KillButton_Click;

            statsPanel.Controls.AddRange([statsDisplay, waterButton, killButton, statsTitle]);
            
            // Adjust button positions when panel resizes
            statsPanel.Resize += (s, e) => {
                if (waterButton != null && killButton != null)
                {
                    waterButton.Location = new Point(10, statsPanel.Height - 95);
                    waterButton.Size = new Size(statsPanel.Width - 25, 40);
                    killButton.Location = new Point(10, statsPanel.Height - 50);
                    killButton.Size = new Size(statsPanel.Width - 25, 40);
                }
            };
            
            mainContainer.Controls.Add(statsPanel, 1, 0);
        }

        private void CreateControlPanel()
        {
            if (mainContainer == null) return;

            var controlPanel = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(235, 235, 230),
                BorderStyle = BorderStyle.FixedSingle,
                Margin = new Padding(10, 5, 10, 10)
            };

            // Generate Tree button
            generateButton?.Dispose();
            generateButton = new Button
            {
                Text = "🌱 Adopt New Bonsai",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Size = new Size(220, 45),
                Location = new Point(20, 15),
                BackColor = Color.FromArgb(34, 139, 34),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            generateButton.FlatAppearance.BorderColor = Color.FromArgb(0, 100, 0);
            generateButton.Click += GenerateButton_Click;

            // Save Tree button
            saveButton?.Dispose();
            saveButton = new Button
            {
                Text = "💾 Save Bonsai",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                Size = new Size(160, 45),
                Location = new Point(260, 15),
                BackColor = Color.FromArgb(101, 67, 33),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            saveButton.FlatAppearance.BorderColor = Color.FromArgb(83, 53, 20);
            saveButton.Click += SaveButton_Click;

            // Status label with updated timestamp
            statusLabel?.Dispose();
            statusLabel = new Label
            {
                Text = "Generated on: 2025-06-23 01:16:31 UTC for user: cs121287",
                Font = new Font("Segoe UI", 10F, FontStyle.Italic),
                ForeColor = Color.FromArgb(105, 105, 105),
                Location = new Point(450, 25),
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
                
                // Generate the bonsai tree
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
                ShowError("Failed to generate bonsai tree", ex);
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
                        // Convert tree array to string
                        var treeText = ConvertTreeToString(tree);
                        
                        treeDisplay.Clear();
                        treeDisplay.Text = treeText;
                        ApplyColorFormatting();
                    }
                });
            }, cancellationToken);
        }

        private string ConvertTreeToString(char[,] tree)
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
                
                // Apply corrected bonsai colors to individual characters
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

        #endregion

        #region Stats Display

        private void UpdateStatsDisplay()
        {
            try
            {
                if (statsDisplay == null || statsDisplay.IsDisposed || currentStats == null) return;
                
                statsDisplay.Clear();
                
                // Bonsai name
                statsDisplay.SelectionFont = new Font("Segoe UI", 16F, FontStyle.Bold);
                statsDisplay.SelectionColor = Color.FromArgb(101, 67, 33);
                statsDisplay.AppendText($"🌳 {currentStats.Name}\n\n");
                
                // Age
                statsDisplay.SelectionFont = new Font("Segoe UI", 12F, FontStyle.Bold);
                statsDisplay.SelectionColor = Color.FromArgb(139, 90, 43);
                statsDisplay.AppendText("Age:\n");
                statsDisplay.SelectionFont = new Font("Segoe UI", 11F, FontStyle.Regular);
                statsDisplay.SelectionColor = Color.Black;
                statsDisplay.AppendText($"{currentStats.Age}\n\n");
                
                // Likes
                statsDisplay.SelectionFont = new Font("Segoe UI", 12F, FontStyle.Bold);
                statsDisplay.SelectionColor = Color.FromArgb(34, 139, 34);
                statsDisplay.AppendText("💚 Likes:\n");
                statsDisplay.SelectionFont = new Font("Segoe UI", 11F, FontStyle.Regular);
                statsDisplay.SelectionColor = Color.Black;
                foreach (string like in currentStats.Likes)
                {
                    statsDisplay.AppendText($"• {like}\n");
                }
                statsDisplay.AppendText("\n");
                
                // Dislikes
                statsDisplay.SelectionFont = new Font("Segoe UI", 12F, FontStyle.Bold);
                statsDisplay.SelectionColor = Color.FromArgb(139, 34, 34);
                statsDisplay.AppendText("💔 Dislikes:\n");
                statsDisplay.SelectionFont = new Font("Segoe UI", 11F, FontStyle.Regular);
                statsDisplay.SelectionColor = Color.Black;
                foreach (string dislike in currentStats.Dislikes)
                {
                    statsDisplay.AppendText($"• {dislike}\n");
                }
                statsDisplay.AppendText("\n");
                
                // Care instructions
                statsDisplay.SelectionFont = new Font("Segoe UI", 10F, FontStyle.Italic);
                statsDisplay.SelectionColor = Color.FromArgb(105, 105, 105);
                statsDisplay.AppendText("💡 Take good care of your bonsai!\nWater it regularly and avoid doing anything harmful...");
            }
            catch (Exception ex)
            {
                ShowError("Failed to update stats display", ex);
            }
        }

        #endregion

        #region Rain Effect

        private void StartRainEffect()
        {
            try
            {
                if (treeDisplay == null || treeDisplay.IsDisposed) return;
                
                // Initialize rain drops
                rainDrops = new List<RainDrop>();
                for (int i = 0; i < 30; i++)
                {
                    rainDrops.Add(new RainDrop
                    {
                        X = random.Next(TREE_WIDTH),
                        Y = random.Next(-10, 0),
                        Speed = random.Next(1, 3)
                    });
                }
                
                rainFrameCount = 0;
                
                // Start rain timer
                rainTimer?.Dispose();
                rainTimer = new Timer();
                rainTimer.Interval = 100; // 100ms for smooth animation
                rainTimer.Tick += RainTimer_Tick;
                rainTimer.Start();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Rain effect error: {ex.Message}");
            }
        }

        private void RainTimer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (rainDrops == null || currentTree == null) return;
                
                rainFrameCount++;
                
                // Stop rain after 1 second (10 frames at 100ms each)
                if (rainFrameCount >= 10)
                {
                    rainTimer?.Stop();
                    rainTimer?.Dispose();
                    rainTimer = null;
                    
                    // Restore original tree
                    if (currentTree != null)
                    {
                        var treeText = ConvertTreeToString(currentTree);
                        SafeInvoke(() =>
                        {
                            if (treeDisplay != null && !treeDisplay.IsDisposed)
                            {
                                treeDisplay.Clear();
                                treeDisplay.Text = treeText;
                                ApplyColorFormatting();
                            }
                        });
                    }
                    return;
                }
                
                // Create rain frame
                var rainCanvas = (char[,])currentTree.Clone();
                
                // Update and draw rain drops
                foreach (var drop in rainDrops)
                {
                    drop.Y += drop.Speed;
                    
                    // Reset drop if it goes off screen
                    if (drop.Y >= TREE_HEIGHT)
                    {
                        drop.Y = random.Next(-10, -1);
                        drop.X = random.Next(TREE_WIDTH);
                    }
                    
                    // Draw drop
                    if (drop.Y >= 0 && drop.Y < TREE_HEIGHT && drop.X >= 0 && drop.X < TREE_WIDTH)
                    {
                        if (rainCanvas[drop.Y, drop.X] == ' ')
                        {
                            rainCanvas[drop.Y, drop.X] = '|';
                        }
                    }
                }
                
                // Update display
                var rainText = ConvertTreeToString(rainCanvas);
                SafeInvoke(() =>
                {
                    if (treeDisplay != null && !treeDisplay.IsDisposed)
                    {
                        treeDisplay.Clear();
                        treeDisplay.Text = rainText;
                        ApplyColorFormatting();
                        
                        // Color rain drops blue
                        string text = treeDisplay.Text;
                        for (int i = 0; i < text.Length; i++)
                        {
                            if (text[i] == '|')
                            {
                                treeDisplay.SelectionStart = i;
                                treeDisplay.SelectionLength = 1;
                                treeDisplay.SelectionColor = Color.FromArgb(100, 149, 237);
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Rain timer error: {ex.Message}");
            }
        }

        private class RainDrop
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Speed { get; set; }
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
                MessageBox.Show($"{currentStats?.Name ?? "Your bonsai"} loves the water! 💧🌱\n\nYour bonsai is happy and healthy!", 
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
                
                MessageBox.Show($"{warning}\n\n😢 {currentStats?.Name ?? "Your bonsai"} doesn't want to die!", 
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
            content.AppendLine("=" + new string('=', 40));
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
            content.AppendLine("INTERACTIVE FEATURES:");
            content.AppendLine("=" + new string('=', 20));
            content.AppendLine("• Water your bonsai to see a beautiful rain effect");
            content.AppendLine("• Your bonsai has unique personality traits");
            content.AppendLine("• Each bonsai is procedurally generated with authentic characteristics");
            content.AppendLine("• Proper brown trunk colors and natural green foliage");
            content.AppendLine("• Horizontal branch layers creating authentic bonsai structure");
            content.AppendLine("• Dense foliage pads at branch endpoints");
            content.AppendLine("• Traditional bonsai proportions and styling");
            content.AppendLine();
            content.AppendLine("TECHNICAL FEATURES:");
            content.AppendLine("=" + new string('=', 18));
            content.AppendLine("• Corrected color mapping for realistic appearance");
            content.AppendLine("• Maximized window with proper sizing and no scroll bars");
            content.AppendLine("• Interactive pet simulation with stats and personality");
            content.AppendLine("• ASCII rain effect animation for watering");
            content.AppendLine("• Random name generation from authentic Japanese names");
            content.AppendLine("• Age generation from 1 month to 10 years");
            content.AppendLine("• Personality traits with likes and dislikes");
            content.AppendLine("• Emotional responses to user interactions");
            content.AppendLine();
            content.AppendLine("BONSAI CARE GUIDE:");
            content.AppendLine("=" + new string('=', 18));
            content.AppendLine("• Water regularly to keep your bonsai healthy");
            content.AppendLine("• Pay attention to your bonsai's likes and dislikes");
            content.AppendLine("• Each bonsai has a unique personality");
            content.AppendLine("• Treat your bonsai with love and respect");
            content.AppendLine("• Enjoy the peaceful art of bonsai cultivation");
            content.AppendLine();
            content.AppendLine("TRADITIONAL BONSAI STYLES:");
            content.AppendLine("=" + new string('=', 26));
            content.AppendLine("• Formal Upright (Chokkan) - Straight, symmetrical");
            content.AppendLine("• Informal Upright (Moyogi) - Natural curves");
            content.AppendLine("• Windswept (Fukinagashi) - Wind-blown appearance");
            content.AppendLine("• Cascade (Kengai) - Waterfall style");
            content.AppendLine("• Slanting (Shakan) - Leaning trunk");
            content.AppendLine();
            content.AppendLine("Generated on: 2025-06-23 01:16:31 UTC for user: cs121287");
            content.AppendLine("Created with Interactive Bonsai Pet Generator");
            content.AppendLine("Featuring corrected colors, proper sizing, and interactive features");
            content.AppendLine("Your virtual bonsai companion with personality and charm");
            content.AppendLine("Experience the zen of digital bonsai cultivation");
            
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
                    generateButton.Text = generating ? "🌱 Growing..." : "🌱 Adopt New Bonsai";
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
                    statusLabel.Text = "Generated on: 2025-06-23 01:16:31 UTC for user: cs121287";
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
                if (components != null)
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