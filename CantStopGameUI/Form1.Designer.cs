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
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(8, 5);
            label1.Margin = new Padding(2, 0, 2, 0);
            label1.Name = "label1";
            label1.Size = new Size(120, 15);
            label1.TabIndex = 0;
            label1.Text = "Players (one per line):";
            // 
            // txtPlayers
            // 
            txtPlayers.Location = new Point(8, 22);
            txtPlayers.Margin = new Padding(2);
            txtPlayers.Multiline = true;
            txtPlayers.Name = "txtPlayers";
            txtPlayers.ScrollBars = ScrollBars.Vertical;
            txtPlayers.Size = new Size(106, 50);
            txtPlayers.TabIndex = 1;
            // 
            // btnStartGame
            // 
            btnStartGame.Location = new Point(118, 24);
            btnStartGame.Margin = new Padding(2);
            btnStartGame.Name = "btnStartGame";
            btnStartGame.Size = new Size(78, 20);
            btnStartGame.TabIndex = 2;
            btnStartGame.Text = "Start Game";
            btnStartGame.UseVisualStyleBackColor = true;
            btnStartGame.Click += btnStartGame_Click;
            // 
            // lblCurrentPlayer
            // 
            lblCurrentPlayer.AutoSize = true;
            lblCurrentPlayer.Location = new Point(216, 5);
            lblCurrentPlayer.Margin = new Padding(2, 0, 2, 0);
            lblCurrentPlayer.Name = "lblCurrentPlayer";
            lblCurrentPlayer.Size = new Size(88, 15);
            lblCurrentPlayer.TabIndex = 3;
            lblCurrentPlayer.Text = "Current Player: ";
            // 
            // lblDice
            // 
            lblDice.AutoSize = true;
            lblDice.Location = new Point(216, 24);
            lblDice.Margin = new Padding(2, 0, 2, 0);
            lblDice.Name = "lblDice";
            lblDice.Size = new Size(36, 15);
            lblDice.TabIndex = 4;
            lblDice.Text = "Dice: ";
            // 
            // lstPairs
            // 
            lstPairs.FormattingEnabled = true;
            lstPairs.ItemHeight = 15;
            lstPairs.Location = new Point(216, 41);
            lstPairs.Margin = new Padding(2);
            lstPairs.Name = "lstPairs";
            lstPairs.Size = new Size(127, 79);
            lstPairs.TabIndex = 5;
            // 
            // btnRoll
            // 
            btnRoll.Location = new Point(346, 24);
            btnRoll.Margin = new Padding(2);
            btnRoll.Name = "btnRoll";
            btnRoll.Size = new Size(78, 20);
            btnRoll.TabIndex = 6;
            btnRoll.Text = "Roll";
            btnRoll.UseVisualStyleBackColor = true;
            btnRoll.Click += btnRoll_Click;
            // 
            // btnApplyPair
            // 
            btnApplyPair.Location = new Point(216, 122);
            btnApplyPair.Margin = new Padding(2);
            btnApplyPair.Name = "btnApplyPair";
            btnApplyPair.Size = new Size(78, 20);
            btnApplyPair.TabIndex = 7;
            btnApplyPair.Text = "Use Pair";
            btnApplyPair.UseVisualStyleBackColor = true;
            btnApplyPair.Click += btnApplyPair_Click;
            // 
            // btnStop
            // 
            btnStop.Location = new Point(346, 48);
            btnStop.Margin = new Padding(2);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(78, 20);
            btnStop.TabIndex = 8;
            btnStop.Text = "Stop";
            btnStop.UseVisualStyleBackColor = true;
            btnStop.Click += btnStop_Click;
            // 
            // tblBoard
            // 
            tblBoard.AutoSize = true;
            tblBoard.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            tblBoard.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            tblBoard.ColumnCount = 2;
            tblBoard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblBoard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tblBoard.Location = new Point(8, 190);
            tblBoard.Margin = new Padding(2);
            tblBoard.Name = "tblBoard";
            tblBoard.RowCount = 2;
            tblBoard.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblBoard.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            tblBoard.Size = new Size(3, 3);
            tblBoard.TabIndex = 9;
            // 
            // pnlBoardVisual
            // 
            pnlBoardVisual.BackColor = Color.White;
            pnlBoardVisual.Location = new Point(8, 322);
            pnlBoardVisual.Margin = new Padding(2);
            pnlBoardVisual.Name = "pnlBoardVisual";
            pnlBoardVisual.Size = new Size(490, 285);
            pnlBoardVisual.TabIndex = 10;
            pnlBoardVisual.Paint += pnlBoardVisual_Paint;
            // 
            // lstAutomatedPlayers
            // 
            lstAutomatedPlayers.FormattingEnabled = true;
            lstAutomatedPlayers.ItemHeight = 15;
            lstAutomatedPlayers.Items.AddRange(new object[] { "Fear Of Heights!", "Team2", "Average Intelligence", "Slice & Dice", "Six Shooter", "Courage the Cowardly Dog" });
            lstAutomatedPlayers.Location = new Point(8, 96);
            lstAutomatedPlayers.Name = "lstAutomatedPlayers";
            lstAutomatedPlayers.SelectionMode = SelectionMode.MultiSimple;
            lstAutomatedPlayers.Size = new Size(151, 94);
            lstAutomatedPlayers.TabIndex = 11;
            // 
            // lblAutomatedPlayers
            // 
            lblAutomatedPlayers.AutoSize = true;
            lblAutomatedPlayers.Location = new Point(8, 78);
            lblAutomatedPlayers.Name = "lblAutomatedPlayers";
            lblAutomatedPlayers.Size = new Size(132, 15);
            lblAutomatedPlayers.TabIndex = 12;
            lblAutomatedPlayers.Text = "Add Automated Players";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1329, 614);
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
            Margin = new Padding(2);
            Name = "Form1";
            Text = "Form1";
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
    }
}
