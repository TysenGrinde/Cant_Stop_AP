using System;
using System.Collections.Generic;
using System.Linq;

namespace CantStopGameUI
{
    /// <summary>
    /// Fast, headless simulation of AI-vs-AI games.
    /// No UI, no delays. Used only for stats / batch runs.
    /// </summary>
    public static class FastSimulator
    {
        // ---------- Per-game result ----------

        public class SingleGameResult
        {
            public int WinnerIndex { get; set; }
            public string WinnerName { get; set; } = string.Empty;

            public int Turns { get; set; }

            // Per-player stats for this one game
            public int[] TurnsTakenPerPlayer { get; set; } = Array.Empty<int>();
            public int[] RollsPerPlayer { get; set; } = Array.Empty<int>();
            public int[] BustsPerPlayer { get; set; } = Array.Empty<int>();

            // Final column owners (index = column 0..10 => sums 2..12)
            public int[] FinalColumnOwner { get; set; } = Array.Empty<int>();
        }

        // ---------- Per-card aggregated stats ----------

        public class CardStats
        {
            public string Name { get; set; } = string.Empty;

            public int GamesPlayed { get; set; }
            public int GamesWon { get; set; }

            public int TotalTurns { get; set; }
            public int TotalRolls { get; set; }
            public int TotalBusts { get; set; }

            // How many times this card finished column (2..12)
            // ColumnsClaimed[0] -> sum 2, ColumnsClaimed[10] -> sum 12
            public int[] ColumnsClaimed { get; set; } = new int[CantStopGame.NumColumns];

            public double WinRate =>
                GamesPlayed > 0 ? (double)GamesWon / GamesPlayed : 0.0;

            public double AvgRollsPerTurn =>
                TotalTurns > 0 ? (double)TotalRolls / TotalTurns : 0.0;

            // Busts per roll (not per turn)
            public double BustRatePerRoll =>
                TotalRolls > 0 ? (double)TotalBusts / TotalRolls : 0.0;
        }

        // ---------- Aggregate over many games ----------

        public class AggregateResult
        {
            public int TotalGames { get; set; }
            public int TotalTurns { get; set; }
            public CardStats[] PerCard { get; set; } = Array.Empty<CardStats>();

            public double AvgGameLength =>
                TotalGames > 0 ? (double)TotalTurns / TotalGames : 0.0;
        }

        // ---------- Run a single AI-vs-AI game ----------

        public static SingleGameResult RunSingleGame(IReadOnlyList<string> playerCardNames)
        {
            if (playerCardNames == null) throw new ArgumentNullException(nameof(playerCardNames));
            if (playerCardNames.Count < 2 || playerCardNames.Count > 4)
                throw new ArgumentException("Number of players must be between 2 and 4.", nameof(playerCardNames));

            int n = playerCardNames.Count;

            // Fresh controllers each game
            var controllers = playerCardNames
                .Select(name => (IAutomatedPlayer)AutomatedPlayerFactory.Create(name))
                .ToList();

            var names = controllers.Select(c => c.DisplayName).ToList();
            var game = new CantStopGame(names);

            int[] turnsTaken = new int[n];
            int[] rollsPerPlayer = new int[n];
            int[] bustsPerPlayer = new int[n];

            int turns = 0;

            while (!game.GameWon)
            {
                int current = game.CurrentPlayer;
                var controller = controllers[current];

                if (controller is BaseAutomatedPlayer baseAi)
                    baseAi.OnTurnStart(game);

                RunSingleTurn(game, controller, current, rollsPerPlayer, bustsPerPlayer);

                turns++;
                turnsTaken[current]++;
            }

            return new SingleGameResult
            {
                WinnerIndex = game.WinnerIndex,
                WinnerName = game.PlayerNames[game.WinnerIndex],
                Turns = turns,
                TurnsTakenPerPlayer = turnsTaken,
                RollsPerPlayer = rollsPerPlayer,
                BustsPerPlayer = bustsPerPlayer,
                FinalColumnOwner = (int[])game.ColumnOwner.Clone()
            };
        }

        // Play exactly one full turn for a given player (no UI, just stats)
        private static void RunSingleTurn(
            CantStopGame game,
            IAutomatedPlayer controller,
            int currentPlayerIndex,
            int[] rollsPerPlayer,
            int[] bustsPerPlayer)
        {
            while (true)
            {
                var roll = game.Roll();
                rollsPerPlayer[currentPlayerIndex]++;

                if (controller is BaseAutomatedPlayer baseAi)
                    baseAi.OnRoll(game, roll);

                // Bust on the roll itself
                if (roll.IsBust)
                {
                    bustsPerPlayer[currentPlayerIndex]++;
                    game.HandleBustAndAdvance();
                    return;
                }

                var move = controller.ChooseMove(game, roll);

                // Invalid / null move => treat as "stop now".
                if (move == null || move.PairIndex < 0 || move.PairIndex >= roll.ValidPairs.Count)
                {
                    var endResInvalid = game.StopAndCommit();
                    if (endResInvalid.GameWon)
                        return;
                    return;
                }

                var applyRes = game.ApplyPairChoice(move.PairIndex);

                // Bust from chosen pair
                if (!applyRes.AppliedAny && applyRes.Bust)
                {
                    bustsPerPlayer[currentPlayerIndex]++;
                    game.HandleBustAndAdvance();
                    return;
                }

                // Player chooses to stop after this move
                if (move.StopAfter)
                {
                    var endRes = game.StopAndCommit();
                    if (endRes.GameWon)
                        return;
                    return;
                }

                // Otherwise: keep rolling in this turn; loop continues.
            }
        }

        // ---------- Run many games and aggregate stats ----------

        public static AggregateResult RunManyGames(IReadOnlyList<string> playerCardNames, int gameCount)
        {
            if (gameCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(gameCount), "Game count must be positive.");
            if (playerCardNames == null)
                throw new ArgumentNullException(nameof(playerCardNames));
            if (playerCardNames.Count < 2 || playerCardNames.Count > 4)
                throw new ArgumentException("Number of players must be between 2 and 4.", nameof(playerCardNames));

            int n = playerCardNames.Count;

            // Initialize per-card stats (one per player slot in this lineup)
            var stats = new CardStats[n];
            for (int i = 0; i < n; i++)
            {
                stats[i] = new CardStats
                {
                    Name = playerCardNames[i],
                    ColumnsClaimed = new int[CantStopGame.NumColumns]
                };
            }

            int totalTurns = 0;

            for (int g = 0; g < gameCount; g++)
            {
                var result = RunSingleGame(playerCardNames);
                totalTurns += result.Turns;

                // Per-card stats from this game
                for (int i = 0; i < n; i++)
                {
                    stats[i].GamesPlayed++;
                    stats[i].TotalTurns += result.TurnsTakenPerPlayer[i];
                    stats[i].TotalRolls += result.RollsPerPlayer[i];
                    stats[i].TotalBusts += result.BustsPerPlayer[i];
                }

                // Winner
                stats[result.WinnerIndex].GamesWon++;

                // Column wins per card: look at final ColumnOwner
                for (int col = 0; col < CantStopGame.NumColumns; col++)
                {
                    int owner = result.FinalColumnOwner[col];
                    if (owner >= 0 && owner < n)
                    {
                        stats[owner].ColumnsClaimed[col]++;
                    }
                }
            }

            return new AggregateResult
            {
                TotalGames = gameCount,
                TotalTurns = totalTurns,
                PerCard = stats
            };
        }
    }
}
