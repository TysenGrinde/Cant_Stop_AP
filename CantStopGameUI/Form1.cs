using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Linq;
using System.Drawing.Imaging;


namespace CantStopGameUI
{
    public partial class Form1 : Form
    {
        private CantStopGame? _game;
        private Label[,]? _boardLabels; // [playerIndex, columnIndex]
        private readonly Dictionary<string, Image> _cardImages = new();
        private System.Collections.Generic.List<IAutomatedPlayer?> _playerControllers = new System.Collections.Generic.List<IAutomatedPlayer?>();

        public Form1()
        {
            InitializeComponent();

            _cardImages["Fear Of Heights!"] = Properties.Resources.FearOfHeights;
            _cardImages["Team2"] = Properties.Resources.Team2;
            _cardImages["Average Intelligence"] = Properties.Resources.Average_Intelligence;
            _cardImages["Slice & Dice"] = Properties.Resources.SliceandDice;
            _cardImages["Six Shooter"] = Properties.Resources.SixShooter;
            _cardImages["Courage the Cowardly Dog"] = Properties.Resources.CourageTheCowardlyDog;
        }


        private async Task MaybeRunAutomatedTurnAsync()
        {
            if (_game == null) return;
            if (_playerControllers.Count == 0) return;

            while (true)
            {
                if (_game == null) return;

                var controller = _playerControllers[_game.CurrentPlayer];
                if (controller == null)
                {
                    // Next player is human – stop the automatic chain.
                    return;
                }

                // Disable human controls while any AI is playing
                btnRoll.Visible = false;
                btnStop.Visible = false;
                btnApplyPair.Enabled = false;

                UpdateCurrentPlayerUI();

                if (controller is BaseAutomatedPlayer baseAi)
                    baseAi.OnTurnStart(_game);

                // Play exactly one full turn for this AI
                bool gameOver = await RunAutomatedTurnAsync(controller);
                if (gameOver)
                    return;

                // At this point, the game logic (HandleBustAndAdvance / StopAndCommit)
                // has already advanced _game.CurrentPlayer to the next player.
                // The while loop repeats and checks if that next player is also an AI.
            }
        }




        private async Task<bool> RunAutomatedTurnAsync(IAutomatedPlayer controller)
        {
            if (_game == null) return true; // treat as game ended

            while (true)
            {
                // === Roll ===
                var roll = _game.Roll();

                if (controller is BaseAutomatedPlayer baseAi)
                    baseAi.OnRoll(_game, roll);

                lblDice.Text = $"Dice: {roll.Dice[0]} {roll.Dice[1]} {roll.Dice[2]} {roll.Dice[3]}";

                lstPairs.Items.Clear();
                for (int i = 0; i < roll.ValidPairs.Count; i++)
                {
                    var p = roll.ValidPairs[i];
                    lstPairs.Items.Add($"{i + 1}: ({p.sum1}, {p.sum2})");
                }

                UpdateBoardUI();
                pnlBoardVisual.Invalidate();

                // 1 second pause so you can see the roll + current board
                await Task.Delay(1000);

                // === Bust on roll ===
                if (roll.IsBust)
                {
                    _game.HandleBustAndAdvance();
                    UpdateCurrentPlayerUI();
                    UpdateBoardUI();
                    pnlBoardVisual.Invalidate();

                    await Task.Delay(1000);

                    // Turn over, game continues
                    return false;
                }

                // === Choose and apply pair ===
                var move = controller.ChooseMove(_game, roll);

                if (move == null || move.PairIndex < 0 || move.PairIndex >= roll.ValidPairs.Count)
                {
                    // Treat invalid move as "stop immediately"
                    var endResultInvalid = _game.StopAndCommit();
                    UpdateBoardUI();
                    pnlBoardVisual.Invalidate();

                    await Task.Delay(1000);

                    if (endResultInvalid.GameWon)
                    {
                        MessageBox.Show($"{_game.PlayerNames[endResultInvalid.WinnerIndex]} wins!");
                        return true; // game over
                    }

                    UpdateCurrentPlayerUI();
                    return false; // turn over, game continues
                }

                var applyResult = _game.ApplyPairChoice(move.PairIndex);
                UpdateBoardUI();
                pnlBoardVisual.Invalidate();

                // pause after moving climbers
                await Task.Delay(1000);

                // === Bust from chosen pair ===
                if (!applyResult.AppliedAny && applyResult.Bust)
                {
                    _game.HandleBustAndAdvance();
                    UpdateCurrentPlayerUI();
                    UpdateBoardUI();
                    pnlBoardVisual.Invalidate();

                    await Task.Delay(1000);

                    return false; // turn over, game continues
                }

                lstPairs.Items.Clear();

                // === Stop after this move ===
                if (move.StopAfter)
                {
                    var endResult = _game.StopAndCommit();
                    UpdateBoardUI();
                    pnlBoardVisual.Invalidate();

                    await Task.Delay(1000);

                    if (endResult.GameWon)
                    {
                        MessageBox.Show($"{_game.PlayerNames[endResult.WinnerIndex]} wins!");
                        return true; // game over
                    }

                    UpdateCurrentPlayerUI();
                    return false; // turn over, game continues
                }

                // Otherwise AI wants to keep rolling -> while(true) continues, and
                // you'll see another roll + 1-second pause.
            }
        }

        // ------------------ Start Game ------------------

        private async void btnStartGame_Click(object sender, EventArgs e)
        {
            var finalNames = new System.Collections.Generic.List<string>();
            _playerControllers.Clear();

            // 1) Human players from txtPlayers (one per line)
            foreach (var line in txtPlayers.Lines)
            {
                var name = line.Trim();
                if (!string.IsNullOrEmpty(name))
                {
                    finalNames.Add(name);
                    _playerControllers.Add(null); // null => human
                }
            }

            // 2) Automated players from lstAutomatedPlayers
            foreach (var item in lstAutomatedPlayers.SelectedItems)
            {
                string aiName = item.ToString() ?? "";
                if (string.IsNullOrWhiteSpace(aiName))
                    continue;

                var ai = AutomatedPlayerFactory.Create(aiName);
                finalNames.Add(ai.DisplayName);
                _playerControllers.Add(ai);
            }

            // Now validate total players (2–4)
            if (finalNames.Count < 2 || finalNames.Count > 4)
            {
                MessageBox.Show("Total players (humans + automated) must be between 2 and 4.");
                return;
            }

            // Create game with combined player list
            _game = new CantStopGame(finalNames);
            SetupBoardTable(finalNames.Count);
            UpdateBoardUI();
            UpdateCurrentPlayerUI();
            pnlBoardVisual.Invalidate();

            lstPairs.Items.Clear();
            lblDice.Text = "Dice: ";

            // Hide Start Game once the game begins
            btnStartGame.Visible = false;

            // Gameplay buttons initial state
            btnRoll.Visible = true;
            btnStop.Visible = true;
            btnApplyPair.Enabled = false;

            // In case the first player is automated, let them start immediately
            await MaybeRunAutomatedTurnAsync();
        }



        // ------------------ Board Setup & Update ------------------

        private void SetupBoardTable(int numPlayers)
        {
            if (_game == null) return;

            tblBoard.SuspendLayout();
            tblBoard.Controls.Clear();
            tblBoard.ColumnStyles.Clear();
            tblBoard.RowStyles.Clear();

            tblBoard.ColumnCount = CantStopGame.NumColumns + 1; // name + 11 columns
            tblBoard.RowCount = numPlayers + 1;                  // header + players

            for (int c = 0; c < tblBoard.ColumnCount; c++)
            {
                tblBoard.ColumnStyles.Add(
                    new ColumnStyle(SizeType.Percent, 100f / tblBoard.ColumnCount));
            }

            for (int r = 0; r < tblBoard.RowCount; r++)
            {
                tblBoard.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            // Header row (blank then 2..12)
            var blank = new Label
            {
                Text = "",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            tblBoard.Controls.Add(blank, 0, 0);

            for (int col = 0; col < CantStopGame.NumColumns; col++)
            {
                var headerLabel = new Label
                {
                    Text = (col + 2).ToString(),
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font(Font, FontStyle.Bold)
                };
                tblBoard.Controls.Add(headerLabel, col + 1, 0);
            }

            _boardLabels = new Label[numPlayers, CantStopGame.NumColumns];

            for (int p = 0; p < numPlayers; p++)
            {
                var nameLabel = new Label
                {
                    Text = _game.PlayerNames[p],
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                tblBoard.Controls.Add(nameLabel, 0, p + 1);

                for (int col = 0; col < CantStopGame.NumColumns; col++)
                {
                    var cellLabel = new Label
                    {
                        Text = "0",
                        Dock = DockStyle.Fill,
                        TextAlign = ContentAlignment.MiddleCenter,
                        BorderStyle = BorderStyle.FixedSingle,
                        BackColor = Color.White
                    };
                    tblBoard.Controls.Add(cellLabel, col + 1, p + 1);
                    _boardLabels[p, col] = cellLabel;
                }
            }

            tblBoard.ResumeLayout();
        }

        private void UpdateBoardUI()
        {
            if (_game == null || _boardLabels == null)
                return;

            int numPlayers = _game.PlayerNames.Count;

            for (int p = 0; p < numPlayers; p++)
            {
                for (int col = 0; col < CantStopGame.NumColumns; col++)
                {
                    var lbl = _boardLabels[p, col];

                    if (_game.ColumnOwner[col] == p)
                    {
                        // Column won by this player
                        lbl.Text = "W";
                        lbl.BackColor = Color.LightGreen;
                    }
                    else
                    {
                        // Base permanent progress
                        int val = _game.PlayerPositions[p, col];
                        Color backColor = Color.White;

                        // Add temporary turn progress for the current player
                        if (p == _game.CurrentPlayer && _game.TurnProgress != null)
                        {
                            int temp = _game.TurnProgress[col];
                            if (temp > 0)
                            {
                                val += temp;
                                backColor = Color.LightSkyBlue; // highlight "climbers" for this turn
                            }
                        }

                        lbl.Text = val.ToString();
                        lbl.BackColor = backColor;
                    }
                }
            }
        }


        private void UpdateCurrentPlayerUI()
        {
            if (_game == null) return;
            lblCurrentPlayer.Text = $"Current Player: {_game.PlayerNames[_game.CurrentPlayer]}";
        }

        // ------------------ Buttons: Roll / Use Pair / Stop ------------------

        private async void btnRoll_Click(object sender, EventArgs e)
        {
            if (_game == null) return;

            var roll = _game.Roll();

            lblDice.Text = $"Dice: {roll.Dice[0]} {roll.Dice[1]} {roll.Dice[2]} {roll.Dice[3]}";

            lstPairs.Items.Clear();
            for (int i = 0; i < roll.ValidPairs.Count; i++)
            {
                var p = roll.ValidPairs[i];
                lstPairs.Items.Add($"{i + 1}: ({p.sum1}, {p.sum2})");
            }

            if (roll.IsBust)
            {
                MessageBox.Show("Bust! No valid pairs. Turn over.");
                _game.HandleBustAndAdvance();
                UpdateCurrentPlayerUI();
                UpdateBoardUI();
                pnlBoardVisual.Invalidate();
                btnApplyPair.Enabled = false;
                btnRoll.Visible = true;
                btnStop.Visible = true;

                // After a human busts, see if next player is an AI
                await MaybeRunAutomatedTurnAsync();
            }
            else
            {
                // Valid pairs → normal human flow: hide Roll/Stop, etc.
                btnApplyPair.Enabled = true;
                btnRoll.Visible = false;
                btnStop.Visible = false;
            }
        }



        private async void btnApplyPair_Click(object sender, EventArgs e)
        {
            if (_game == null) return;

            int index = lstPairs.SelectedIndex;
            if (index < 0)
            {
                MessageBox.Show("Select a pairing from the list.");
                return;
            }

            // ----- Special case: 2 climbers already placed, and both sums are new -----
            var pair = _game.LastValidPairs[index];
            int sum1 = pair.sum1;
            int sum2 = pair.sum2;

            int col1 = sum1 - 2;
            int col2 = sum2 - 2;

            int activeCount = _game.ActiveColumns.Count;
            bool col1Active = _game.ActiveColumns.Contains(col1);
            bool col2Active = _game.ActiveColumns.Contains(col2);

            if (activeCount == 2 && !col1Active && !col2Active)
            {
                var choice = MessageBox.Show(
                    $"You already have two climbers.\n" +
                    $"Where do you want to place your last climber?\n\n" +
                    $"Yes = {sum1},  No = {sum2}",
                    "Choose Column",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (choice == DialogResult.No)
                {
                    // Prioritize sum2: swap in LastValidPairs so engine uses that first.
                    _game.LastValidPairs[index] = (sum2, sum1);
                }
            }

            // ----- Apply pair in game engine -----
            var result = _game.ApplyPairChoice(index);

            if (!result.AppliedAny && result.Bust)
            {
                MessageBox.Show("Bust! No valid columns from that pairing. Turn over.");
                _game.HandleBustAndAdvance();
                UpdateCurrentPlayerUI();
                UpdateBoardUI();
                pnlBoardVisual.Invalidate();

                btnRoll.Visible = true;
                btnStop.Visible = true;
                btnApplyPair.Enabled = false;
                lstPairs.Items.Clear();

                await MaybeRunAutomatedTurnAsync();
            }
            else
            {
                // Successful move this turn...
                UpdateBoardUI();
                pnlBoardVisual.Invalidate();

                lstPairs.Items.Clear();
                btnApplyPair.Enabled = false;

                btnRoll.Visible = true;
                btnStop.Visible = true;
            }
        }



        private async void btnStop_Click(object sender, EventArgs e)
        {
            if (_game == null) return;

            var result = _game.StopAndCommit();
            UpdateBoardUI();
            pnlBoardVisual.Invalidate();

            lstPairs.Items.Clear();
            btnApplyPair.Enabled = false;

            // After stopping, next player will roll
            btnRoll.Visible = true;
            btnStop.Visible = true;

            if (result.GameWon)
            {
                MessageBox.Show($"{_game.PlayerNames[result.WinnerIndex]} wins!");
                // Optionally disable buttons if you don't want to keep playing.
                // btnRoll.Enabled = false;
                // btnStop.Enabled = false;
                // btnApplyPair.Enabled = false;
            }
            else
            {
                UpdateCurrentPlayerUI();
                await MaybeRunAutomatedTurnAsync();
            }
        }


        private void pnlBoardVisual_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);

            // --- First: draw AI cards on the right, even if no game is running ---

            // --- 1) Draw AI cards on the right side, even if there is no game yet ---

            if (_cardImages.Count > 0)
            {
                // Names of currently-selected automated players
                var selectedNames = new HashSet<string>(
                    lstAutomatedPlayers.SelectedItems
                        .Cast<object>()
                        .Select(o => o?.ToString() ?? string.Empty)
                        .Where(s => !string.IsNullOrWhiteSpace(s)));

                int cardWidth = 90;
                int cardHeight = 140;
                int padding = 8;

                int columns = 2; // << two columns
                int totalWidth = columns * cardWidth + (columns + 1) * padding;

                // Base X so the whole 2-column block hugs the right edge
                int xBase = pnlBoardVisual.Width - totalWidth;
                if (xBase < 0) xBase = 0; // just in case panel is very narrow

                using (var fadedAttributes = new ImageAttributes())
                {
                    float alpha = 0.35f; // 35% opacity for non-selected cards
                    var cm = new ColorMatrix(new float[][]
                    {
            new float[] {1, 0, 0, 0, 0},
            new float[] {0, 1, 0, 0, 0},
            new float[] {0, 0, 1, 0, 0},
            new float[] {0, 0, 0, alpha, 0},
            new float[] {0, 0, 0, 0, 1}
                    });
                    fadedAttributes.SetColorMatrix(cm);

                    int index = 0;
                    foreach (var kvp in _cardImages)
                    {
                        var img = kvp.Value;
                        if (img == null) continue;

                        int colIndex = index % columns;      // 0 or 1
                        int rowIndex = index / columns;      // 0,1,2,...

                        int x = xBase + padding + colIndex * (cardWidth + padding);
                        int y = padding + rowIndex * (cardHeight + padding);

                        var dest = new Rectangle(x, y, cardWidth, cardHeight);
                        bool isSelected = selectedNames.Contains(kvp.Key);

                        if (isSelected)
                        {
                            // Full opacity
                            g.DrawImage(img, dest);
                        }
                        else
                        {
                            // Faded
                            g.DrawImage(
                                img,
                                dest,
                                0, 0, img.Width, img.Height,
                                GraphicsUnit.Pixel,
                                fadedAttributes);
                        }

                        index++;
                    }
                }
            }

            // If there is no game yet, we’re done after drawing cards.
            if (_game == null)
                return;

            // --- Now draw the actual board (mountains + tokens) ---

            int cols = CantStopGame.NumColumns;

            // Layout constants for the board portion (leave room on the right for cards)
            int marginLeft = 40;
            int marginBottom = 30;
            int columnSpacing = 35;  // a bit tighter so the board fits left of the cards
            int cellHeight = 18;
            int cellWidth = 30;

            // Draw columns 2..12 as vertical "mountains"
            for (int col = 0; col < cols; col++)
            {
                int sum = col + 2;
                int height = CantStopGame.ColumnHeights[col];

                int colX = marginLeft + col * columnSpacing;

                // Column label at bottom
                using (var font = new Font("Segoe UI", 8, FontStyle.Bold))
                using (var brush = new SolidBrush(Color.Black))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center })
                {
                    g.DrawString(sum.ToString(), font, brush,
                                 colX + cellWidth / 2,
                                 pnlBoardVisual.Height - marginBottom + 2,
                                 sf);
                }

                int owner = _game.ColumnOwner[col];

                for (int step = 0; step < height; step++)
                {
                    int x = colX;
                    int y = pnlBoardVisual.Height - marginBottom - (step + 1) * cellHeight;

                    var rect = new Rectangle(x, y, cellWidth, cellHeight - 2);

                    if (owner != -1)
                    {
                        // Column is owned: fill the whole column green
                        using (var fillBrush = new SolidBrush(Color.LightGreen))
                        {
                            g.FillRectangle(fillBrush, rect);
                        }
                    }

                    g.DrawRectangle(Pens.Gray, rect);
                }
            }

            // Player colors
            Color[] playerColors = { Color.Red, Color.Blue, Color.Green, Color.Orange };

            // Draw camps (saved positions) and climbers (this turn)
            for (int p = 0; p < _game.PlayerNames.Count; p++)
            {
                Color color = playerColors[p % playerColors.Length];

                for (int col = 0; col < cols; col++)
                {
                    int height = CantStopGame.ColumnHeights[col];
                    int colX = marginLeft + col * columnSpacing;

                    int basePos = _game.PlayerPositions[p, col]; // permanent "camp"
                    int temp = (p == _game.CurrentPlayer && _game.TurnProgress != null)
                                    ? _game.TurnProgress[col]
                                    : 0;

                    // Slight horizontal offset per player to avoid exact overlap
                    int offset = (p - _game.PlayerNames.Count / 2) * 4;

                    // 1) Permanent camp token (colored)
                    if (basePos > 0)
                    {
                        int campStep = Math.Min(basePos, height) - 1;
                        int campY = pnlBoardVisual.Height - marginBottom - (campStep + 1) * cellHeight;

                        var campRect = new Rectangle(
                            colX + cellWidth / 2 - 6 + offset,
                            campY + 2,
                            12,
                            cellHeight - 6);

                        using (var brush = new SolidBrush(color))
                        {
                            g.FillEllipse(brush, campRect);
                        }
                        g.DrawEllipse(Pens.Black, campRect);
                    }

                    // 2) Climber token (black) for current player only
                    if (p == _game.CurrentPlayer && temp > 0)
                    {
                        int climberPos = basePos + temp;
                        if (climberPos > height) climberPos = height;

                        int climberStep = climberPos - 1;
                        int climberY = pnlBoardVisual.Height - marginBottom - (climberStep + 1) * cellHeight;

                        var climberRect = new Rectangle(
                            colX + cellWidth / 2 - 6 + offset,
                            climberY + 2,
                            12,
                            cellHeight - 6);

                        using (var brush = new SolidBrush(Color.Black))
                        {
                            g.FillEllipse(brush, climberRect);
                        }
                        g.DrawEllipse(Pens.Black, climberRect);
                    }
                }
            }
        }


        private void lstAutomatedPlayers_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnlBoardVisual.Invalidate(); // redraw board + cards with new selection
        }

        private async void btnFastSim_Click(object sender, EventArgs e)
        {
            // Use the automated-player list box for lineup selection
            var aiNames = lstAutomatedPlayers.SelectedItems
                .Cast<object>()
                .Select(item => item?.ToString())
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();

            if (aiNames.Count < 2 || aiNames.Count > 4)
            {
                MessageBox.Show("For fast simulation, select 2–4 automated players in the list.");
                return;
            }

            int gameCount = (int)numSimGames.Value;

            btnFastSim.Enabled = false;
            txtSimResults.Clear();
            txtSimResults.AppendText($"Running {gameCount} games with:\r\n");
            foreach (var name in aiNames)
                txtSimResults.AppendText($" - {name}\r\n");
            txtSimResults.AppendText("\r\n");

            try
            {
                // Run the simulations off the UI thread
                var result = await Task.Run(() => FastSimulator.RunManyGames(aiNames, gameCount));

                txtSimResults.AppendText($"Finished.\r\n");
                txtSimResults.AppendText($"Total games: {result.TotalGames}\r\n");
                txtSimResults.AppendText($"Average game length (turns): {result.AvgGameLength:F2}\r\n\r\n");

                foreach (var cs in result.PerCard)
                {
                    txtSimResults.AppendText($"{cs.Name}\r\n");

                    // Win stats
                    txtSimResults.AppendText(
                        $"  Games: {cs.GamesPlayed}, Wins: {cs.GamesWon}, WinRate: {cs.WinRate:P1}\r\n");

                    // Rolls / busts
                    txtSimResults.AppendText(
                        $"  Avg rolls / turn: {cs.AvgRollsPerTurn:F2}, Bust rate / roll: {cs.BustRatePerRoll:P1}\r\n");

                    // Column wins (only show columns that were actually claimed)
                    txtSimResults.AppendText("  Columns claimed: ");
                    bool first = true;
                    for (int col = 0; col < CantStopGame.NumColumns; col++)
                    {
                        int count = cs.ColumnsClaimed[col];
                        if (count > 0)
                        {
                            if (!first)
                                txtSimResults.AppendText(", ");
                            int sum = col + 2; // col 0 => sum 2, etc.
                            txtSimResults.AppendText($"{sum}:{count}");
                            first = false;
                        }
                    }
                    if (first)
                    {
                        txtSimResults.AppendText("none");
                    }

                    txtSimResults.AppendText("\r\n\r\n");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Simulation error: " + ex.Message);
            }
            finally
            {
                btnFastSim.Enabled = true;
            }
        }
    }
}