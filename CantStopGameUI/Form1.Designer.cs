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
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 9);
            label1.Name = "label1";
            label1.Size = new Size(179, 25);
            label1.TabIndex = 0;
            label1.Text = "Players (one per line):";
            // 
            // txtPlayers
            // 
            txtPlayers.Location = new Point(12, 37);
            txtPlayers.Multiline = true;
            txtPlayers.Name = "txtPlayers";
            txtPlayers.ScrollBars = ScrollBars.Vertical;
            txtPlayers.Size = new Size(150, 80);
            txtPlayers.TabIndex = 1;
            // 
            // btnStartGame
            // 
            btnStartGame.Location = new Point(28, 123);
            btnStartGame.Name = "btnStartGame";
            btnStartGame.Size = new Size(112, 34);
            btnStartGame.TabIndex = 2;
            btnStartGame.Text = "Start Game";
            btnStartGame.UseVisualStyleBackColor = true;
            btnStartGame.Click += btnStartGame_Click;
            // 
            // lblCurrentPlayer
            // 
            lblCurrentPlayer.AutoSize = true;
            lblCurrentPlayer.Location = new Point(308, 9);
            lblCurrentPlayer.Name = "lblCurrentPlayer";
            lblCurrentPlayer.Size = new Size(131, 25);
            lblCurrentPlayer.TabIndex = 3;
            lblCurrentPlayer.Text = "Current Player: ";
            // 
            // lblDice
            // 
            lblDice.AutoSize = true;
            lblDice.Location = new Point(308, 40);
            lblDice.Name = "lblDice";
            lblDice.Size = new Size(55, 25);
            lblDice.TabIndex = 4;
            lblDice.Text = "Dice: ";
            // 
            // lstPairs
            // 
            lstPairs.FormattingEnabled = true;
            lstPairs.ItemHeight = 25;
            lstPairs.Location = new Point(308, 68);
            lstPairs.Name = "lstPairs";
            lstPairs.Size = new Size(180, 129);
            lstPairs.TabIndex = 5;
            // 
            // btnRoll
            // 
            btnRoll.Location = new Point(494, 40);
            btnRoll.Name = "btnRoll";
            btnRoll.Size = new Size(112, 34);
            btnRoll.TabIndex = 6;
            btnRoll.Text = "Roll";
            btnRoll.UseVisualStyleBackColor = true;
            btnRoll.Click += btnRoll_Click;
            // 
            // btnApplyPair
            // 
            btnApplyPair.Location = new Point(308, 203);
            btnApplyPair.Name = "btnApplyPair";
            btnApplyPair.Size = new Size(112, 34);
            btnApplyPair.TabIndex = 7;
            btnApplyPair.Text = "Use Pair";
            btnApplyPair.UseVisualStyleBackColor = true;
            btnApplyPair.Click += btnApplyPair_Click;
            // 
            // btnStop
            // 
            btnStop.Location = new Point(494, 80);
            btnStop.Name = "btnStop";
            btnStop.Size = new Size(112, 34);
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
            tblBoard.Location = new Point(12, 250);
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
            pnlBoardVisual.Location = new Point(12, 675);
            pnlBoardVisual.Name = "pnlBoardVisual";
            pnlBoardVisual.Size = new Size(476, 337);
            pnlBoardVisual.TabIndex = 10;
            pnlBoardVisual.Paint += pnlBoardVisual_Paint;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1898, 1024);
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
    }
}
