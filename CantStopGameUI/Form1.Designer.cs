namespace CantStopGameUI
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            label1 = new Label();
            txtPlayers = new TextBox();
            btnStartGame = new Button();
            lblCurrentPlayer = new Label();
            lblDice = new Label();
            lstPairs = new ListBox();
            btnRoll = new Button();
            btnApplyPair = new Button();
            btnStop = new Button();
            tblBoard = new TableLayoutPanel();
            pnlBoardVisual = new Panel();
            lstAutomatedPlayers = new ListBox();
            lblAutomatedPlayers = new Label();
            numSimGames = new NumericUpDown();
            btnFastSim = new Button();
            txtSimResults = new TextBox();
            ((System.ComponentModel.ISupportInitialize)numSimGames).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(10, 10);
            label1.Name = "label1";
            label1.Size = new Size(120, 15);
            label1.TabIndex = 0;
            label1.Text = "Players (one per line):";
            // 
            // txtPlayers
            // 
            txtPlayers.Location = new Point(10, 30);
            txtPlayers.Multiline = true;
            txtPlayers.Name = "txtPlayers";
            txtPlayers.ScrollBars = ScrollBars.Vertical;
            txtPlayers.Size = new Size(150, 80);
            txtPlayers.TabIndex = 1;
            // 
            // btnStartGame
            // 
            btnStartGame.Location = new Point(10, 115);
            btnStartGame.Name = "btnStartGame";
            btnStartGame.Size = new Size(150, 23);
            btnStartGame.TabIndex = 2;
            btnStartGame.Text = "Start Game";
            btnStartGame.UseVisualStyleBackColor = true;
            btnStartGame.Click += btnStartGame_Click;
            // 
            // lblAutomatedPlayers
            // 
            lblAutomatedPlayers.AutoSize = true;
            lblAutomatedPlayers.Location = new Point(180, 10);
            lblAutomatedPlayers.Name = "lblAutomatedPlayers";
            lblAutomatedPlayers.Size = new Size(132, 15);
            lblAutomatedPlayers.TabIndex = 12;
            lblAutomatedPlayers.Text = "Add Automated Players";
            // 
            // lstAutomatedPlayers
            // 
            lstAutomatedPlayers.FormattingEnabled = true;
            lstAutomatedPlayers.ItemHeight = 15;
            lstAutomatedPlayers.Items.AddRange(new object[]
            {
        "Fear Of Heights!",
        "Team2",
        "Average Intelligence",
        "Slice & Dice",
        "Six Shooter",
        "Courage the Cowardly Dog"
            });
            lstAutomatedPlayers.Location = new Point(180, 30);
            lstAutomatedPlayers.Name = "lstAutomatedPlayers";
            lstAutomatedPlayers.SelectionMode = SelectionMode.MultiSimple;
            lstAutomatedPlayers.Size = new Size(160, 109);
            lstAutomatedPlayers.TabIndex = 11;
            lstAutomatedPlayers.SelectedIndexChanged += lstAutomatedPlayers_SelectedIndexChanged;
            // 
            // lblCurrentPlayer
            // 
            lblCurrentPlayer.AutoSize = true;
            lblCurrentPlayer.Location = new Point(360, 10);
            lblCurrentPlayer.Name = "lblCurrentPlayer";
            lblCurrentPlayer.Size = new Size(88, 15);
            lblCurrentPlayer.TabIndex = 3;
            lblCurrentPlayer.Text = "Current Player:";
            // 
            // lblDice
            // 
            lblDice.AutoSize = true;
            lblDice.Location = new Point(360, 30);
            lblDice.Name = "lblDice";
            lblDice.Size = new Size(36, 15);
            lblDice.TabIndex = 4;
            lblDice.Text = "Dice: ";
            // 
            // lstPairs
            // 
            lstPairs.FormattingEnabled = true;
            lstPairs.ItemHeight = 15;
            lstPairs.Location = new Point(360, 50);
            lstPairs.Name = "lstPairs";
            lstPairs.Size = new Size(180, 94);
            lstPairs.TabIndex = 5;
            // 
            // btnRoll
            // 
            btnRoll.Location = new Point(560, 28);
            btnRoll.Name = "btnRoll";
            btnRoll.Size = new Size(80, 23);
            btnRoll.TabIndex = 6;
            btnRoll.Text = "Roll";
            btnRoll.UseVisualStyleBackColor = true;
            btnRoll.Click += btnRoll_Click;
            // 
            // btnStop
            // 
            btnStop.Location = new Point(560, 57);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(80, 23);
            btnStop.TabIndex = 8;
            btnStop.Text = "Stop";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // btnApplyPair
            // 
            btnApplyPair.Location = new Point(560, 86);
            btnApplyPair.Name = "btnApplyPair";
            btnApplyPair.Size = new Size(80, 23);
            btnApplyPair.TabIndex = 7;
            btnApplyPair.Text = "Use Pair";
            btnApplyPair.UseVisualStyleBackColor = true;
            btnApplyPair.Click += btnApplyPair_Click;
            // 
            // tblBoard
            // 
            tblBoard.ColumnCount = 1;
            tblBoard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            tblBoard.Location = new Point(10, 145);
            tblBoard.Name = "tblBoard";
            tblBoard.RowCount = 1;
            tblBoard.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            tblBoard.Size = new Size(600, 120);
            tblBoard.TabIndex = 9;
            tblBoard.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            // 
            // pnlBoardVisual
            // 
            pnlBoardVisual.BackColor = Color.White;
            pnlBoardVisual.Location = new Point(8, 190);
            pnlBoardVisual.Margin = new Padding(2);
            pnlBoardVisual.Name = "pnlBoardVisual";
            pnlBoardVisual.Size = new Size(1100, 800);
            pnlBoardVisual.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
            pnlBoardVisual.TabIndex = 10;
            pnlBoardVisual.Paint += pnlBoardVisual_Paint;
            // 
            // txtSimResults
            // 
            txtSimResults.Location = new Point(1128, 190);
            txtSimResults.Multiline = true;
            txtSimResults.Name = "txtSimResults";
            txtSimResults.ReadOnly = true;
            txtSimResults.ScrollBars = ScrollBars.Vertical;
            txtSimResults.Size = new Size(700, 800);
            txtSimResults.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtSimResults.TabIndex = 15;
            //
            // numSimGames
            // 
            numSimGames.Location = new Point(1320, 150);
            numSimGames.Maximum = new decimal(new int[] { 100000, 0, 0, 0 });
            numSimGames.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numSimGames.Name = "numSimGames";
            numSimGames.Size = new Size(120, 23);
            numSimGames.TabIndex = 13;
            numSimGames.Value = new decimal(new int[] { 100, 0, 0, 0 });
            numSimGames.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            //
            // btnFastSim
            // 
            btnFastSim.Location = new Point(1128, 150);
            btnFastSim.Name = "btnFastSim";
            btnFastSim.Size = new Size(120, 23);
            btnFastSim.TabIndex = 14;
            btnFastSim.Text = "Run Fast Sim";
            btnFastSim.UseVisualStyleBackColor = true;
            btnFastSim.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFastSim.Click += btnFastSim_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1920, 1080);
            StartPosition = FormStartPosition.CenterScreen;
            Controls.Add(txtSimResults);
            Controls.Add(btnFastSim);
            Controls.Add(numSimGames);
            Controls.Add(lblAutomatedPlayers);
            Controls.Add(lstAutomatedPlayers);
            Controls.Add(pnlBoardVisual);
            Controls.Add(tblBoard);
            Controls.Add(btnStop);
            Controls.Add(btnApplyPair);
            Controls.Add(btnRoll);
            Controls.Add(lstPairs);
            Controls.Add(lblDice);
            Controls.Add(lblCurrentPlayer);
            Controls.Add(btnStartGame);
            Controls.Add(txtPlayers);
            Controls.Add(label1);
            Name = "Form1";
            Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)numSimGames).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }


        #endregion

        private Label label1;
        private TextBox txtPlayers;
        private Button btnStartGame;
        private Label lblCurrentPlayer;
        private Label lblDice;
        private ListBox lstPairs;
        private Button btnRoll;
        private Button btnApplyPair;
        private Button btnStop;
        private TableLayoutPanel tblBoard;
        private Panel pnlBoardVisual;
        private ListBox lstAutomatedPlayers;
        private Label lblAutomatedPlayers;
        private NumericUpDown numSimGames;
        private Button btnFastSim;
        private TextBox txtSimResults;
    }
}
