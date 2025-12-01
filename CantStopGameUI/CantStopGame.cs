using System;
using System.Collections.Generic;

namespace CantStopGameUI
{
    public class CantStopGame
    {
        public const int NumColumns = 11; // 2–12
        public static readonly int[] ColumnHeights = { 3, 5, 7, 9, 11, 13, 11, 9, 7, 5, 3 };

        private readonly Random _rng = new Random();

        public List<string> PlayerNames { get; }
        public int[,] PlayerPositions { get; }       // permanent progress
        public int[] ColumnOwner { get; }            // -1 = unowned, otherwise player index
        public int[] PlayerCompleted { get; }        // how many columns each player has won

        public int CurrentPlayer { get; private set; }
        public bool GameWon { get; private set; }
        public int WinnerIndex { get; private set; } = -1;

        // Turn state
        public int[] TurnProgress { get; private set; } = new int[NumColumns];
        public List<int> ActiveColumns { get; private set; } = new List<int>(); // indexes 0..10
        public bool TurnInProgress { get; private set; }
        public bool TurnBusted { get; private set; }

        public int[] LastDice { get; private set; } = new int[4];
        public List<(int sum1, int sum2)> LastValidPairs { get; private set; } =
            new List<(int sum1, int sum2)>();

        public CantStopGame(IEnumerable<string> players)
        {
            PlayerNames = new List<string>(players);
            if (PlayerNames.Count < 2 || PlayerNames.Count > 4)
                throw new ArgumentException("Number of players must be 2–4.");

            int n = PlayerNames.Count;
            PlayerPositions = new int[n, NumColumns];
            ColumnOwner = new int[NumColumns];
            PlayerCompleted = new int[n];

            for (int i = 0; i < NumColumns; i++)
                ColumnOwner[i] = -1; // unowned
        }

        public void StartNewTurn()
        {
            Array.Clear(TurnProgress, 0, TurnProgress.Length);
            ActiveColumns.Clear();
            TurnInProgress = true;
            TurnBusted = false;
            LastValidPairs.Clear();
        }

        public DiceRollResult Roll()
        {
            if (!TurnInProgress)
            {
                StartNewTurn();
            }

            // Roll 4 dice
            for (int i = 0; i < 4; i++)
                LastDice[i] = _rng.Next(1, 7);

            int d1 = LastDice[0];
            int d2 = LastDice[1];
            int d3 = LastDice[2];
            int d4 = LastDice[3];

            var allPairs = new List<(int, int)>
            {
                (d1 + d2, d3 + d4),
                (d1 + d3, d2 + d4),
                (d1 + d4, d2 + d3)
            };

            bool IsPlayable(int col)
            {
                if (col < 2 || col > 12)
                    return false;
                int idx = col - 2;
                if (ColumnOwner[idx] != -1 && ColumnOwner[idx] != CurrentPlayer)
                    return false;
                if (PlayerPositions[CurrentPlayer, idx] >= ColumnHeights[idx])
                    return false;
                return true;
            }

            bool IsActive(int col)
            {
                int idx = col - 2;
                return ActiveColumns.Contains(idx);
            }

            LastValidPairs.Clear();

            foreach (var p in allPairs)
            {
                int a = p.Item1;
                int b = p.Item2;

                bool aPlayable = IsPlayable(a);
                bool bPlayable = IsPlayable(b);

                bool aActive = IsActive(a);
                bool bActive = IsActive(b);

                int activeCount = ActiveColumns.Count;

                // Same rule as your C++ code
                if ((aPlayable && (aActive || activeCount < 3)) ||
                    (bPlayable && (bActive || activeCount < 3)))
                {
                    LastValidPairs.Add(p);
                }
            }

            if (LastValidPairs.Count == 0)
            {
                TurnBusted = true;
            }

            return new DiceRollResult
            {
                Dice = (int[])LastDice.Clone(),
                ValidPairs = new List<(int, int)>(LastValidPairs),
                IsBust = TurnBusted
            };
        }

        public PairApplyResult ApplyPairChoice(int pairIndex)
        {
            if (TurnBusted)
            {
                return new PairApplyResult { AppliedAny = false, Bust = true };
            }

            if (pairIndex < 0 || pairIndex >= LastValidPairs.Count)
                throw new ArgumentOutOfRangeException(nameof(pairIndex));

            var pair = LastValidPairs[pairIndex];
            int sum1 = pair.sum1;
            int sum2 = pair.sum2;

            int col1 = sum1 - 2;
            int col2 = sum2 - 2;

            bool TryMove(int col)
            {
                if (col < 0 || col >= NumColumns)
                    return false;
                if (ColumnOwner[col] != -1 && ColumnOwner[col] != CurrentPlayer)
                    return false;
                if (PlayerPositions[CurrentPlayer, col] >= ColumnHeights[col])
                    return false;

                bool alreadyActive = ActiveColumns.Contains(col);
                if (!alreadyActive && ActiveColumns.Count >= 3)
                    return false;

                TurnProgress[col]++;
                if (!alreadyActive)
                    ActiveColumns.Add(col);
                return true;
            }

            bool move1 = TryMove(col1);
            bool move2 = TryMove(col2);
            bool appliedAny = move1 || move2;

            if (!appliedAny)
            {
                TurnBusted = true;
                return new PairApplyResult { AppliedAny = false, Bust = true };
            }

            return new PairApplyResult { AppliedAny = true, Bust = false };
        }

        public TurnEndResult StopAndCommit()
        {
            if (!TurnInProgress)
                throw new InvalidOperationException("No turn in progress.");

            bool completedAny = false;

            for (int i = 0; i < NumColumns; i++)
            {
                if (TurnProgress[i] > 0)
                {
                    PlayerPositions[CurrentPlayer, i] += TurnProgress[i];

                    if (PlayerPositions[CurrentPlayer, i] >= ColumnHeights[i])
                    {
                        PlayerPositions[CurrentPlayer, i] = ColumnHeights[i];

                        if (ColumnOwner[i] == -1)
                        {
                            ColumnOwner[i] = CurrentPlayer;
                            PlayerCompleted[CurrentPlayer]++;
                            completedAny = true;

                            if (PlayerCompleted[CurrentPlayer] >= 3)
                            {
                                GameWon = true;
                                WinnerIndex = CurrentPlayer;
                            }
                        }
                    }
                }
            }

            // clear turn state
            TurnInProgress = false;
            TurnBusted = false;
            Array.Clear(TurnProgress, 0, TurnProgress.Length);
            ActiveColumns.Clear();
            LastValidPairs.Clear();

            var result = new TurnEndResult
            {
                CompletedAnyColumns = completedAny,
                GameWon = GameWon,
                WinnerIndex = WinnerIndex,
                PlayerCompletedColumns = PlayerCompleted[CurrentPlayer]
            };

            if (!GameWon)
            {
                CurrentPlayer = (CurrentPlayer + 1) % PlayerNames.Count;
            }

            return result;
        }

        public void HandleBustAndAdvance()
        {
            if (!TurnBusted)
                return;

            TurnInProgress = false;
            TurnBusted = false;
            Array.Clear(TurnProgress, 0, TurnProgress.Length);
            ActiveColumns.Clear();
            LastValidPairs.Clear();

            CurrentPlayer = (CurrentPlayer + 1) % PlayerNames.Count;
        }
    }

    public class DiceRollResult
    {
        public int[] Dice { get; set; } = Array.Empty<int>();
        public List<(int sum1, int sum2)> ValidPairs { get; set; } = new();
        public bool IsBust { get; set; }
    }

    public class PairApplyResult
    {
        public bool AppliedAny { get; set; }
        public bool Bust { get; set; }
    }

    public class TurnEndResult
    {
        public bool CompletedAnyColumns { get; set; }
        public bool GameWon { get; set; }
        public int WinnerIndex { get; set; }
        public int PlayerCompletedColumns { get; set; }
    }
}
