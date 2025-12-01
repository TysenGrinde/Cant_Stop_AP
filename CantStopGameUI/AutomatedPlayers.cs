using System;
using System.Collections.Generic;
using System.Linq;

namespace CantStopGameUI
{
    // ========== Shared types ==========

    public interface IAutomatedPlayer
    {
        string DisplayName { get; }

        // Decide which pair to use and whether to stop after applying it.
        AutomatedMove ChooseMove(CantStopGame game, DiceRollResult roll);
    }

    public class AutomatedMove
    {
        public int PairIndex { get; set; }
        public bool StopAfter { get; set; }
    }

    /// <summary>
    /// Base class with helpers and simple per–turn state such as roll count.
    /// All specific AIs inherit from this and override ChooseMove.
    /// </summary>
    public abstract class BaseAutomatedPlayer : IAutomatedPlayer
    {
        public abstract string DisplayName { get; }

        protected readonly Random Rng = new Random();

        // Number of rolls this player has made in the current turn.
        public int RollCountThisTurn { get; private set; }

        public virtual void OnTurnStart(CantStopGame game)
        {
            RollCountThisTurn = 0;
        }

        public virtual void OnRoll(CantStopGame game, DiceRollResult roll)
        {
            RollCountThisTurn++;
        }

        public abstract AutomatedMove ChooseMove(CantStopGame game, DiceRollResult roll);

        // ---------- Helper methods ----------

        protected static int SumToColumnIndex(int sum) => sum - 2;

        protected static bool IsValidSum(int sum) => sum >= 2 && sum <= 12;

        protected static int DistanceToCenter(int sum) => Math.Abs(sum - 7);

        protected static int CurrentPlayerIndex(CantStopGame game) => game.CurrentPlayer;

        /// <summary>
        /// Permanent + this-turn progress for current player on given column index.
        /// </summary>
        protected static int CurrentTotalOnColumn(CantStopGame game, int colIndex)
        {
            int p = game.CurrentPlayer;
            int basePos = game.PlayerPositions[p, colIndex];
            int temp = game.TurnProgress[colIndex];
            return basePos + temp;
        }

        /// <summary>Spaces remaining to finish for current player on this column.</summary>
        protected static int DistanceToFinish(CantStopGame game, int colIndex)
        {
            int total = CurrentTotalOnColumn(game, colIndex);
            return CantStopGame.ColumnHeights[colIndex] - total;
        }

        protected static bool ColumnHasCampForCurrent(CantStopGame game, int colIndex)
        {
            int p = game.CurrentPlayer;
            return game.PlayerPositions[p, colIndex] > 0;
        }

        protected static bool ColumnHasClimberForCurrent(CantStopGame game, int colIndex)
        {
            return game.TurnProgress[colIndex] > 0;
        }

        protected static int ActiveClimbersCount(CantStopGame game)
        {
            return game.ActiveColumns.Count;
        }

        protected static bool AnyOpponentWithinMovesOfFinishing(CantStopGame game, int colIndex, int movesThreshold)
        {
            int height = CantStopGame.ColumnHeights[colIndex];
            for (int p = 0; p < game.PlayerNames.Count; p++)
            {
                if (p == game.CurrentPlayer) continue;
                int pos = game.PlayerPositions[p, colIndex];
                if (height - pos <= movesThreshold)
                    return true;
            }
            return false;
        }

        protected enum RiskCategory
        {
            Safe,      // 6,7,8
            Regular,   // 4,5,9,10
            Risky      // 2,3,11,12
        }

        protected static RiskCategory GetRiskCategory(int sum)
        {
            switch (sum)
            {
                case 6:
                case 7:
                case 8:
                    return RiskCategory.Safe;
                case 2:
                case 3:
                case 11:
                case 12:
                    return RiskCategory.Risky;
                default:
                    return RiskCategory.Regular;
            }
        }

        protected enum SliceClass
        {
            Extreme, // 2,3,11,12
            Middle,  // 4,5,9,10
            Stable   // 6,7,8
        }

        protected static SliceClass GetSliceClass(int sum)
        {
            switch (sum)
            {
                case 2:
                case 3:
                case 11:
                case 12:
                    return SliceClass.Extreme;
                case 4:
                case 5:
                case 9:
                case 10:
                    return SliceClass.Middle;
                default:
                    return SliceClass.Stable;
            }
        }

        /// <summary>
        /// Score a pair for "closest to 7" behavior. Higher is better.
        /// </summary>
        protected static double ScorePairByCenter(int sum1, int sum2)
        {
            int d1 = DistanceToCenter(sum1);
            int d2 = DistanceToCenter(sum2);
            // negative distance, tie-break by lower sums
            return -(d1 + d2) * 10 - Math.Min(sum1, sum2);
        }

        /// <summary>
        /// Choose index of pair with maximum score according to provided scorer.
        /// </summary>
        protected static int ArgMaxPair(IList<(int sum1, int sum2)> pairs, Func<(int sum1, int sum2), double> scorer)

        {
            int bestIndex = 0;
            double bestScore = double.NegativeInfinity;
            for (int i = 0; i < pairs.Count; i++)
            {
                double s = scorer(pairs[i]);
                if (s > bestScore)
                {
                    bestScore = s;
                    bestIndex = i;
                }
            }
            return bestIndex;
        }

        /// <summary>
        /// Return count of active columns of each risk type.
        /// </summary>
        protected static (int safe, int regular, int risky) CountActiveByRisk(CantStopGame game)
        {
            int safe = 0, regular = 0, risky = 0;
            foreach (int col in game.ActiveColumns)
            {
                int sum = col + 2;
                switch (GetRiskCategory(sum))
                {
                    case RiskCategory.Safe: safe++; break;
                    case RiskCategory.Regular: regular++; break;
                    case RiskCategory.Risky: risky++; break;
                }
            }
            return (safe, regular, risky);
        }

        /// <summary>
        /// Count how many times each sum (2..12) appears among all pairings of the four dice.
        /// (For Team2's "if a number appears twice" rule.)
        /// </summary>
        protected static int[] CountSumsFromDice(int[] dice)
        {
            int d1 = dice[0], d2 = dice[1], d3 = dice[2], d4 = dice[3];
            var allPairs = new List<(int, int)>
            {
                (d1 + d2, d3 + d4),
                (d1 + d3, d2 + d4),
                (d1 + d4, d2 + d3)
            };

            int[] counts = new int[13];
            foreach (var p in allPairs)
            {
                if (IsValidSum(p.Item1)) counts[p.Item1]++;
                if (IsValidSum(p.Item2)) counts[p.Item2]++;
            }
            return counts;
        }

        /// <summary>
        /// Minimum spaces remaining among all opponents on a given column.
        /// </summary>
        protected static int MinOpponentDistanceToFinish(CantStopGame game, int colIndex)
        {
            int height = CantStopGame.ColumnHeights[colIndex];
            int best = int.MaxValue;
            for (int p = 0; p < game.PlayerNames.Count; p++)
            {
                if (p == game.CurrentPlayer) continue;
                int pos = game.PlayerPositions[p, colIndex];
                int dist = height - pos;
                if (dist < best) best = dist;
            }
            return best;
        }

        /// <summary>
        /// How many completed tracks the leader has over us.
        /// </summary>
        protected static int CompletedBehindLeader(CantStopGame game)
        {
            int myCompleted = game.PlayerCompleted[game.CurrentPlayer];
            int best = myCompleted;
            for (int p = 0; p < game.PlayerNames.Count; p++)
                if (game.PlayerCompleted[p] > best)
                    best = game.PlayerCompleted[p];
            return best - myCompleted;
        }
    }

    // =====================================================================
    //  Average Intelligence
    // =====================================================================

    // Card:
    // - Start turn: roll and pick tracks closest to 7 until 3 paths are chosen
    //   (tie -> lower track)
    // - While rolling:
    //   * If there is only 1 path to choose AND you've rolled > 3 times:
    //       choose path and END TURN
    //   * else choose tracks closest to 7, tie -> lower, keep rolling
    public class AverageIntelligencePlayer : BaseAutomatedPlayer
    {
        public override string DisplayName => "Average Intelligence";

        public override AutomatedMove ChooseMove(CantStopGame game, DiceRollResult roll)
        {
            var pairs = roll.ValidPairs;
            if (pairs.Count == 0)
            {
                // Bust handled by caller, but be safe.
                return new AutomatedMove { PairIndex = 0, StopAfter = true };
            }

            // Count how many distinct playable sums exist this roll.
            var playableSums = new HashSet<int>();
            foreach (var p in pairs)
            {
                if (IsValidSum(p.sum1)) playableSums.Add(p.sum1);
                if (IsValidSum(p.sum2)) playableSums.Add(p.sum2);
            }

            bool onlyOnePath = playableSums.Count == 1;

            // "If there is only one path to choose and you've rolled > 3 times:
            //   choose path and END TURN"
            if (onlyOnePath && RollCountThisTurn > 3)
            {
                int index = ArgMaxPair(pairs, _ => 0.0); // any, they all go to same track
                return new AutomatedMove
                {
                    PairIndex = index,
                    StopAfter = true
                };
            }

            // Otherwise: "choose tracks closest to 7. If tie, choose lower path."
            int bestIndex = ArgMaxPair(pairs, p =>
            {
                double baseScore = ScorePairByCenter(p.sum1, p.sum2);

                // Small bias to stay on already-started climbers if possible.
                double climberBonus = 0;
                int col1 = SumToColumnIndex(p.sum1);
                int col2 = SumToColumnIndex(p.sum2);
                if (col1 >= 0 && col1 < CantStopGame.NumColumns && ColumnHasClimberForCurrent(game, col1))
                    climberBonus += 2.0;
                if (col2 >= 0 && col2 < CantStopGame.NumColumns && ColumnHasClimberForCurrent(game, col2))
                    climberBonus += 2.0;

                return baseScore + climberBonus;
            });

            return new AutomatedMove
            {
                PairIndex = bestIndex,
                StopAfter = false // "CONTINUE ROLLING DICE"
            };
        }
    }

    // =====================================================================
    //  Courage the Cowardly Dog
    // =====================================================================

    // Card highlights:
    // - FOMO: if last place by >1 track, roll until bust or finish 1 track (overrides other rules)
    // - Prefer tracks closest to center; tiebreak (6&8 etc.) choose lower value.
    // - When track already started, prioritize it.
    // - If opponent is 3 spots or less from finishing a track, avoid that track (unless forced).
    // - Roll for 4 turns, or until all climbers are in outer tracks (2,3,11,12).
    // - COWARDLY: auto stop when 1 roll away from completing a track.
    public class CourageTheCowardlyDogPlayer : BaseAutomatedPlayer
    {
        public override string DisplayName => "Courage the Cowardly Dog";

        public override AutomatedMove ChooseMove(CantStopGame game, DiceRollResult roll)
        {
            var pairs = roll.ValidPairs;
            if (pairs.Count == 0)
                return new AutomatedMove { PairIndex = 0, StopAfter = true };

            int myCompleted = game.PlayerCompleted[game.CurrentPlayer];
            int maxCompleted = game.PlayerCompleted.Max();
            bool lastByMoreThanOne = (maxCompleted - myCompleted) > 1;

            // Are all current climbers in outer tracks?
            bool allClimbersOuter = game.ActiveColumns.Count > 0 &&
                                    game.ActiveColumns.All(c =>
                                    {
                                        int s = c + 2;
                                        return s == 2 || s == 3 || s == 11 || s == 12;
                                    });

            // Pair scoring for movement preferences.
            int bestIndex = ArgMaxPair(pairs, p =>
            {
                double score = 0;
                int[] sums = { p.sum1, p.sum2 };

                foreach (int sum in sums)
                {
                    if (!IsValidSum(sum)) continue;
                    int col = SumToColumnIndex(sum);

                    // Prefer tracks closest to center.
                    score -= DistanceToCenter(sum);

                    // Prioritize started tracks (camp or climber).
                    if (ColumnHasCampForCurrent(game, col) || ColumnHasClimberForCurrent(game, col))
                        score += 4;

                    // Avoid if any opponent <=3 from top on that column.
                    int oppDist = MinOpponentDistanceToFinish(game, col);
                    if (oppDist <= 3)
                        score -= 6;

                    // Slight bonus for being close to finishing ourselves.
                    int myDist = DistanceToFinish(game, col);
                    score += (10 - myDist) * 0.3;
                }

                // Tiebreak: lower sum slightly better (6 over 8, etc.)
                score -= Math.Min(p.sum1, p.sum2) * 0.01;

                return score;
            });

            // Can we claim (finish) a new track right now if we stop?
            bool canClaimTrackNow = false;
            for (int col = 0; col < CantStopGame.NumColumns; col++)
            {
                // Only care about tracks we don't already own
                if (game.ColumnOwner[col] == game.CurrentPlayer)
                    continue;

                if (ColumnHasCampForCurrent(game, col) || ColumnHasClimberForCurrent(game, col))
                {
                    // DistanceToFinish <= 0 means our climber is already at or above the top
                    if (DistanceToFinish(game, col) <= 0)
                    {
                        canClaimTrackNow = true;
                        break;
                    }
                }
            }

            // Stopping logic.
            bool stop = false;

            // Base cowardly / pacing logic when NOT in FOMO mode.
            if (!lastByMoreThanOne)
            {
                // Cowardly: stop when 1 roll away from completing a track,
                // BUT only on active columns (with climbers), not just any camp.
                for (int col = 0; col < CantStopGame.NumColumns; col++)
                {
                    if (ColumnHasClimberForCurrent(game, col) &&
                        DistanceToFinish(game, col) <= 1)
                    {
                        stop = true;
                        break;
                    }
                }


                // Roll for 4 turns, or until all climbers in outer tracks.
                if (!stop && RollCountThisTurn >= 4 && !allClimbersOuter)
                    stop = true;

                // If we can actually claim a track right now, we definitely stop.
                if (canClaimTrackNow)
                    stop = true;
            }
            else
            {
                // FOMO: last by >1 track
                // Roll until bust OR until we can finish a (new) track this turn.
                stop = canClaimTrackNow;
            }


            return new AutomatedMove
            {
                PairIndex = bestIndex,
                StopAfter = stop
            };
        }
    }

    // =====================================================================
    //  Fear Of Heights!
    // =====================================================================

    // Card:
    // Choosing movement (priority):
    //   If unused tokens:
    //     1) place in 2/12 if available
    //     2) else tracks closest to center
    //   After first roll:
    //     1) move in 2/12 if populated
    //     2) move in other tracks, starting with closest to center
    //   Tie: track with fewer opponent tokens, then lower number.
    //
    // When to stop – after all tokens placed (3 climbers) and one condition:
    //   2,12 -> 1 move
    //   3,4,5,9,10,11 -> 2 moves
    //   6,7,8 -> 3 moves
    //   End of a track
    //   +1 move per threshold for each completed track behind leader
    //
    // If any opponent <=2 moves from winning final track, go to bust/track completion.
    public class FearOfHeightsPlayer : BaseAutomatedPlayer
    {
        public override string DisplayName => "Fear Of Heights!";

        public override AutomatedMove ChooseMove(CantStopGame game, DiceRollResult roll)
        {
            var pairs = roll.ValidPairs;
            if (pairs.Count == 0)
                return new AutomatedMove { PairIndex = 0, StopAfter = true };

            bool firstRoll = RollCountThisTurn == 1;
            int activeCount = ActiveClimbersCount(game);
            bool haveUnusedTokens = activeCount < 3;

            int bestIndex = ArgMaxPair(pairs, p =>
            {
                double score = 0;
                int[] sums = { p.sum1, p.sum2 };

                foreach (int sum in sums)
                {
                    if (!IsValidSum(sum)) continue;
                    int col = SumToColumnIndex(sum);

                    // If unused tokens: try to open 2/12 first, then closest to center.
                    if (haveUnusedTokens && !ColumnHasClimberForCurrent(game, col))
                    {
                        if (sum == 2 || sum == 12)
                            score += 20;
                        else
                            score += 10 - DistanceToCenter(sum);
                    }

                    // After first roll: move on 2/12 if populated, else other populated tracks
                    // starting closest to center.
                    if (!firstRoll && ColumnHasClimberForCurrent(game, col))
                    {
                        if (sum == 2 || sum == 12)
                            score += 15;
                        else
                            score += 8 - DistanceToCenter(sum);
                    }

                    // Tie-breaking:
                    //  - prefer shorter distance to finish
                    //  - fewer opponent tokens
                    //  - lower column number
                    int myDist = DistanceToFinish(game, col);
                    score += (10 - myDist) * 0.1;

                    int oppTokens = 0;
                    for (int pIndex = 0; pIndex < game.PlayerNames.Count; pIndex++)
                    {
                        if (pIndex == game.CurrentPlayer) continue;
                        if (game.PlayerPositions[pIndex, col] > 0)
                            oppTokens++;
                    }
                    score -= oppTokens * 0.2;
                    score -= (col + 2) * 0.01;
                }

                return score;
            });

            // ---- Stopping logic ----

            bool stop = false;

            if (ActiveClimbersCount(game) == 3)
            {
                // For each active column, determine its "move threshold".
                int thresholdMoves = int.MaxValue;
                foreach (int col in game.ActiveColumns)
                {
                    int sum = col + 2;
                    int required;
                    if (sum == 2 || sum == 12)
                        required = 1;
                    else if (sum == 3 || sum == 4 || sum == 5 ||
                             sum == 9 || sum == 10 || sum == 11)
                        required = 2;
                    else
                        required = 3; // 6,7,8

                    // use minimum of the three thresholds
                    thresholdMoves = Math.Min(thresholdMoves, required);
                }

                // Add +1 move per completed track behind the leader.
                int behind = CompletedBehindLeader(game);
                if (thresholdMoves < int.MaxValue)
                    thresholdMoves += behind;

                // "End of a track" always triggers stop.
                bool atEnd = false;
                foreach (int col in game.ActiveColumns)
                {
                    if (DistanceToFinish(game, col) <= 0)
                    {
                        atEnd = true;
                        break;
                    }
                }

                if (atEnd || RollCountThisTurn >= thresholdMoves)
                    stop = true;
            }

            // If any opponent is <=2 moves from winning their final track, go until bust / track completion.
            bool opponentNearWin = false;
            for (int col = 0; col < CantStopGame.NumColumns; col++)
            {
                if (AnyOpponentWithinMovesOfFinishing(game, col, 2) &&
                    game.ColumnOwner[col] == -1) // track still available to claim
                {
                    opponentNearWin = true;
                    break;
                }
            }
            if (opponentNearWin)
                stop = false;

            return new AutomatedMove
            {
                PairIndex = bestIndex,
                StopAfter = stop
            };
        }
    }

    // =====================================================================
    //  Six Shooter
    // =====================================================================

    // Card:
    // HAVE CLIMBERS:
    //   - If you can place a climber on a track with a camp, do so.
    //   - Otherwise use point system:
    //       7,8,6 -> 3 points
    //       2,12  -> 2 points
    //       5,9   -> 1 point
    //       Doubles bonus + tiebreaker
    //   - Place climber where you get most points.
    //
    // NO CLIMBERS:
    //   - Choose to advance the track closest to the end.
    //
    // WHEN TO STOP:
    //   - When you have reached halfway up a track OR completed a track.
    //
    // Ties: 7,8,6,2,12,5,9,4,10,3,11.
    public class SixShooterPlayer : BaseAutomatedPlayer
    {
        public override string DisplayName => "Six Shooter";

        public override AutomatedMove ChooseMove(CantStopGame game, DiceRollResult roll)
        {
            var pairs = roll.ValidPairs;
            if (pairs.Count == 0)
                return new AutomatedMove { PairIndex = 0, StopAfter = true };

            bool haveClimbers = ActiveClimbersCount(game) > 0;

            int[] tieOrder = { 7, 8, 6, 2, 12, 5, 9, 4, 10, 3, 11 };

            int bestIndex = ArgMaxPair(pairs, p =>
            {
                double score = 0;
                int[] sums = { p.sum1, p.sum2 };

                if (haveClimbers)
                {
                    // Track with our camp gets priority.
                    foreach (int sum in sums)
                    {
                        if (!IsValidSum(sum)) continue;
                        int col = SumToColumnIndex(sum);
                        if (ColumnHasCampForCurrent(game, col))
                            score += 6;
                    }

                    // Base point system.
                    foreach (int sum in sums)
                    {
                        if (!IsValidSum(sum)) continue;
                        switch (sum)
                        {
                            case 7:
                            case 8:
                            case 6:
                                score += 3; break;
                            case 2:
                            case 12:
                                score += 2; break;
                            case 5:
                            case 9:
                                score += 1; break;
                        }
                    }

                    // Doubles bonus (approximated as equal sums in pair).
                    if (p.sum1 == p.sum2)
                        score += 1.5;
                }
                else
                {
                    // No climbers: track closest to end.
                    foreach (int sum in sums)
                    {
                        if (!IsValidSum(sum)) continue;
                        int col = SumToColumnIndex(sum);
                        int dist = DistanceToFinish(game, col);
                        score += (20 - dist);
                    }
                }

                // Tiebreak: fixed order.
                int bestTie = 100;
                foreach (int sum in sums)
                {
                    for (int i = 0; i < tieOrder.Length; i++)
                    {
                        if (tieOrder[i] == sum)
                        {
                            if (i < bestTie) bestTie = i;
                            break;
                        }
                    }
                }
                score += (tieOrder.Length - bestTie) * 0.01;

                return score;
            });

            // Stop when any of our tracks is >= halfway OR completed.
            bool stop = false;
            for (int col = 0; col < CantStopGame.NumColumns; col++)
            {
                int current = CurrentTotalOnColumn(game, col);
                int height = CantStopGame.ColumnHeights[col];
                if (current >= height && height > 0)
                {
                    stop = true; // completed a track
                    break;
                }
                if (current >= height / 2 && height > 0)
                    stop = true;
            }

            return new AutomatedMove
            {
                PairIndex = bestIndex,
                StopAfter = stop
            };
        }
    }

    // =====================================================================
    //  Slice & Dice
    // =====================================================================

    // Card:
    // Dice classifications:
    //   E = 2,3,11,12
    //   M = 4,5,9,10
    //   S = 6,7,8
    //
    // Stop if:
    //   - a track is completed
    //   - >=2 S -> roll 12 times (Aggro 16)
    //   -  =1 S -> roll 7  times (Aggro 10)
    //   - no S with middle -> roll 5 (Aggro 8)
    //   - only E -> roll 2 (Aggro 3)
    //
    // Opponents:
    //   - if opponents' tracks completed > ours, use Aggro number instead of base.
    //   - any extreme route with opponent chips at least 3 from top – avoid that track.
    //
    // Priorities for picking tracks:
    //   1) prioritize Stable, then Extreme, then Middle
    //   2) choose tracks with your camps in that classification
    //   3) if no camp, choose track furthest from center in that classification
    //   4) if both have camp, choose more advanced camp
    //   5) if equal camps, choose track with no enemy camps
    //   6) else, choose higher number
    public class SliceAndDicePlayer : BaseAutomatedPlayer
    {
        public override string DisplayName => "Slice & Dice";

        public override AutomatedMove ChooseMove(CantStopGame game, DiceRollResult roll)
        {
            var pairs = roll.ValidPairs;
            if (pairs.Count == 0)
                return new AutomatedMove { PairIndex = 0, StopAfter = true };

            // Pair selection based on priorities.
            int bestIndex = ArgMaxPair(pairs, p =>
            {
                double score = 0;
                int[] sums = { p.sum1, p.sum2 };

                foreach (int sum in sums)
                {
                    if (!IsValidSum(sum)) continue;
                    int col = SumToColumnIndex(sum);

                    // Classification priority
                    switch (GetSliceClass(sum))
                    {
                        case SliceClass.Stable: score += 30; break;
                        case SliceClass.Extreme: score += 20; break;
                        case SliceClass.Middle: score += 10; break;
                    }

                    bool hasCamp = ColumnHasCampForCurrent(game, col);
                    bool hasClimber = ColumnHasClimberForCurrent(game, col);

                    if (hasCamp || hasClimber)
                        score += 8;

                    // If no camp, prefer furthest from center *within* that class.
                    if (!hasCamp)
                        score += DistanceToCenter(sum) * 0.5;

                    // Prefer more advanced camp.
                    int dist = DistanceToFinish(game, col);
                    score += (15 - dist) * 0.4;

                    // Enemy camps penalty.
                    int enemyCamps = 0;
                    for (int pIndex = 0; pIndex < game.PlayerNames.Count; pIndex++)
                    {
                        if (pIndex == game.CurrentPlayer) continue;
                        if (game.PlayerPositions[pIndex, col] > 0)
                            enemyCamps++;
                    }
                    score -= enemyCamps * 0.5;

                    // Else choose higher number.
                    score += sum * 0.05;

                    // Opponents clause: avoid extreme routes where opponent is within 3 of the top.
                    if (GetSliceClass(sum) == SliceClass.Extreme &&
                        MinOpponentDistanceToFinish(game, col) <= 3)
                    {
                        score -= 5;
                    }
                }

                return score;
            });

            // Stopping rules: compute classification mix for active columns.
            bool anyStable = false, anyMiddle = false;
            int stableCount = 0;
            foreach (int col in game.ActiveColumns)
            {
                int sum = col + 2;
                switch (GetSliceClass(sum))
                {
                    case SliceClass.Stable: anyStable = true; stableCount++; break;
                    case SliceClass.Middle: anyMiddle = true; break;
                }
            }

            int baseMaxRolls;
            if (stableCount >= 2)
                baseMaxRolls = 12;
            else if (stableCount == 1)
                baseMaxRolls = 7;
            else if (!anyStable && anyMiddle)
                baseMaxRolls = 5;
            else // only extremes
                baseMaxRolls = 2;

            // Aggro: if behind leader, use Aggro thresholds.
            bool behind = CompletedBehindLeader(game) > 0;
            if (behind)
            {
                if (stableCount >= 2)
                    baseMaxRolls = 16;
                else if (stableCount == 1)
                    baseMaxRolls = 10;
                else if (!anyStable && anyMiddle)
                    baseMaxRolls = 8;
                else
                    baseMaxRolls = 3;
            }

            bool stop = false;

            // Stop if a track is completed.
            for (int col = 0; col < CantStopGame.NumColumns; col++)
            {
                if ((ColumnHasCampForCurrent(game, col) || ColumnHasClimberForCurrent(game, col)) &&
                    DistanceToFinish(game, col) <= 0)
                {
                    stop = true;
                    break;
                }
            }

            // Also stop if we've reached roll cap for our classification mix.
            if (!stop && RollCountThisTurn >= baseMaxRolls)
                stop = true;

            return new AutomatedMove
            {
                PairIndex = bestIndex,
                StopAfter = stop
            };
        }
    }

    // =====================================================================
    //  Team2
    // =====================================================================

    // Card:
    // GENERAL
    //   - Don't stop until 3 climbers in play.
    //   - Complete a track = end turn.
    //
    // WHEN TO STOP (after 3 climbers):
    //   - 2 or more RISKY = stop.
    //   - 1 RISKY + 2 SAFE        = 3 turns
    //   - 1 RISKY + 2 REGULAR     = 3 turns
    //   - 1 SAFE  + 2 REGULAR     = 6 turns
    //   - 1 SAFE  + 1 REG + 1 RISK = 4 turns
    //   - 2 SAFE + 1 REGULAR      = 9 turns
    //   - 3 SAFE  = complete 1 track (we handle via "complete track" rule)
    //   - 3 REGULAR = 4 turns
    //
    // SPLITTING DICE WHEN NO CLIMBERS:
    //   - If a number appears twice, advance that number's track.
    //   - When choosing between tracks, pick those with fewest camps-to-completion.
    //   - If tie, choose highest-risk number.
    //   - If tie again, smaller number.
    //   - If a previous roll got progress on at least 1 risk, prioritize track closest to 7.
    public class Team2Player : BaseAutomatedPlayer
    {
        public override string DisplayName => "Team2";

        public override AutomatedMove ChooseMove(CantStopGame game, DiceRollResult roll)
        {
            var pairs = roll.ValidPairs;
            if (pairs.Count == 0)
                return new AutomatedMove { PairIndex = 0, StopAfter = true };

            int activeCount = ActiveClimbersCount(game);

            int bestIndex;

            if (activeCount == 0)
            {
                // "Splitting dice when no climbers".
                int[] sumCounts = CountSumsFromDice(roll.Dice);

                bestIndex = ArgMaxPair(pairs, p =>
                {
                    double score = 0;
                    int[] sums = { p.sum1, p.sum2 };

                    foreach (int sum in sums)
                    {
                        if (!IsValidSum(sum)) continue;
                        int col = SumToColumnIndex(sum);

                        // If a number appears twice, give big bonus.
                        score += sumCounts[sum] * 4;

                        // Fewest camps to completion = small distance.
                        int dist = DistanceToFinish(game, col);
                        score += (20 - dist);

                        // Highest risk number: Risky > Regular > Safe.
                        switch (GetRiskCategory(sum))
                        {
                            case RiskCategory.Risky: score += 3; break;
                            case RiskCategory.Regular: score += 2; break;
                            case RiskCategory.Safe: score += 1; break;
                        }

                        // If we already pushed a risk track this turn, prefer sums close to 7.
                        bool anyRiskProgress = game.ActiveColumns.Any(c =>
                        {
                            int s = c + 2;
                            return GetRiskCategory(s) == RiskCategory.Risky &&
                                   game.TurnProgress[c] > 0;
                        });
                        if (anyRiskProgress)
                            score -= DistanceToCenter(sum);
                    }

                    // Final tiebreak: smaller number.
                    score -= Math.Min(p.sum1, p.sum2) * 0.01;
                    return score;
                });
            }
            else
            {
                // With climbers: advance track(s) closest to completion.
                bestIndex = ArgMaxPair(pairs, p =>
                {
                    double score = 0;
                    int[] sums = { p.sum1, p.sum2 };

                    foreach (int sum in sums)
                    {
                        if (!IsValidSum(sum)) continue;
                        int col = SumToColumnIndex(sum);
                        int dist = DistanceToFinish(game, col);
                        score += (25 - dist);
                    }
                    return score;
                });
            }

            // ----- WHEN TO STOP -----

            bool stop = false;

            // Don't stop until 3 climbers in play.
            if (activeCount < 3)
            {
                stop = false;
            }
            else
            {
                var (safe, regular, risky) = CountActiveByRisk(game);

                // Complete a track = end turn (approx: if any active track within 1 move).
                bool completesTrackSoon = false;
                foreach (int col in game.ActiveColumns)
                {
                    if (DistanceToFinish(game, col) <= 1)
                    {
                        completesTrackSoon = true;
                        break;
                    }
                }
                if (completesTrackSoon)
                    stop = true;

                // 2 or more risky -> stop.
                if (risky >= 2)
                    stop = true;

                // Turn-count thresholds based on (safe,regular,risky).
                int neededRolls = int.MaxValue;

                if (risky == 1 && safe == 2 && regular == 0)
                    neededRolls = 3;
                else if (risky == 1 && regular == 2 && safe == 0)
                    neededRolls = 3;
                else if (safe == 1 && regular == 2 && risky == 0)
                    neededRolls = 6;
                else if (safe == 1 && regular == 1 && risky == 1)
                    neededRolls = 4;
                else if (safe == 2 && regular == 1 && risky == 0)
                    neededRolls = 9;
                else if (regular == 3 && safe == 0 && risky == 0)
                    neededRolls = 4;
                // 3 safe -> "complete 1 track" which we approximated via completesTrackSoon above.

                if (neededRolls != int.MaxValue && RollCountThisTurn >= neededRolls)
                    stop = true;
            }

            return new AutomatedMove
            {
                PairIndex = bestIndex,
                StopAfter = stop
            };
        }
    }

    // =====================================================================
    //  Factory
    // =====================================================================

    public static class AutomatedPlayerFactory
    {
        public static IAutomatedPlayer Create(string name)
        {
            return name switch
            {
                "Fear Of Heights!" => new FearOfHeightsPlayer(),
                "Team2" => new Team2Player(),
                "Average Intelligence" => new AverageIntelligencePlayer(),
                "Slice & Dice" => new SliceAndDicePlayer(),
                "Six Shooter" => new SixShooterPlayer(),
                "Courage the Cowardly Dog" => new CourageTheCowardlyDogPlayer(),
                _ => new AverageIntelligencePlayer(),
            };
        }
    }
}
