using System.Collections.Generic;
using System.Text;

namespace WaterSort.Core
{
    /// <summary>
    /// Pure C# water sort solver (no UnityEngine dependency so it can be unit tested).
    /// Uses the same rules as MoveValidator:
    /// - the whole top run of one color is poured at once
    /// - target must be empty, or have the same top color and room for the whole run
    /// - a completed tube (full, single color) cannot be poured from
    /// Tubes are lists of color ids, bottom to top. 0 is never a valid color.
    /// </summary>
    public static class LevelSolver
    {
        public struct Move
        {
            public int From;
            public int To;

            public Move(int from, int to)
            {
                From = from;
                To = to;
            }

            public override string ToString()
            {
                return $"{From}->{To}";
            }
        }

        private class Node
        {
            public List<List<int>> State;
            public Node Parent;
            public Move Move;
        }

        public static int TopRun(List<int> tube)
        {
            if (tube.Count == 0) return 0;

            int top = tube[tube.Count - 1];
            int run = 0;
            for (int i = tube.Count - 1; i >= 0 && tube[i] == top; i--)
                run++;
            return run;
        }

        /// <summary>
        /// Empty, or full with a single color
        /// </summary>
        public static bool IsComplete(List<int> tube, int capacity)
        {
            if (tube.Count == 0) return true;
            return tube.Count == capacity && TopRun(tube) == capacity;
        }

        public static bool IsSolved(List<List<int>> tubes, int capacity)
        {
            foreach (var tube in tubes)
            {
                if (!IsComplete(tube, capacity))
                    return false;
            }
            return true;
        }

        public static bool CanPour(List<List<int>> tubes, int from, int to, int capacity)
        {
            if (from == to) return false;

            var source = tubes[from];
            var target = tubes[to];

            if (source.Count == 0) return false;
            if (IsComplete(source, capacity)) return false;
            if (target.Count >= capacity) return false;
            if (target.Count == 0) return true;

            if (source[source.Count - 1] != target[target.Count - 1]) return false;

            return TopRun(source) <= capacity - target.Count;
        }

        /// <summary>
        /// Pours the whole top run. Caller must check CanPour first.
        /// </summary>
        public static void Pour(List<List<int>> tubes, int from, int to)
        {
            var source = tubes[from];
            var target = tubes[to];
            int run = TopRun(source);
            int color = source[source.Count - 1];

            source.RemoveRange(source.Count - run, run);
            for (int i = 0; i < run; i++)
                target.Add(color);
        }

        public static List<List<int>> Clone(List<List<int>> tubes)
        {
            var copy = new List<List<int>>(tubes.Count);
            foreach (var tube in tubes)
                copy.Add(new List<int>(tube));
            return copy;
        }

        /// <summary>
        /// Finds a solution with depth-first search over unique states.
        /// Returns the list of moves (empty if already solved) or null if no
        /// solution was found within maxStates explored states.
        /// </summary>
        public static List<Move> Solve(List<List<int>> start, int capacity, int maxStates = 200000)
        {
            if (IsSolved(start, capacity))
                return new List<Move>();

            var visited = new HashSet<string> { StateKey(start) };
            var stack = new Stack<Node>();
            stack.Push(new Node { State = Clone(start) });

            while (stack.Count > 0)
            {
                Node node = stack.Pop();

                List<Move> moves = GetUsefulMoves(node.State, capacity);

                // Push in reverse so the best-ranked move is explored first
                for (int i = moves.Count - 1; i >= 0; i--)
                {
                    var next = Clone(node.State);
                    Pour(next, moves[i].From, moves[i].To);

                    if (!visited.Add(StateKey(next)))
                        continue;

                    var child = new Node { State = next, Parent = node, Move = moves[i] };

                    if (IsSolved(next, capacity))
                        return BuildPath(child);

                    if (visited.Count >= maxStates)
                        return null;

                    stack.Push(child);
                }
            }

            return null;
        }

        /// <summary>
        /// Next move towards a solution from the given state, or null if none exists
        /// </summary>
        public static Move? GetHint(List<List<int>> state, int capacity, int maxStates = 200000)
        {
            var solution = Solve(state, capacity, maxStates);
            if (solution == null || solution.Count == 0)
                return null;
            return solution[0];
        }

        /// <summary>
        /// Legal moves, minus pointless ones, best first:
        /// pours onto a matching color before pours into an empty tube.
        /// </summary>
        private static List<Move> GetUsefulMoves(List<List<int>> tubes, int capacity)
        {
            var onColor = new List<Move>();
            var intoEmpty = new List<Move>();

            for (int from = 0; from < tubes.Count; from++)
            {
                var source = tubes[from];
                bool sourceIsSingleColor = source.Count > 0 && TopRun(source) == source.Count;
                bool triedEmpty = false;

                for (int to = 0; to < tubes.Count; to++)
                {
                    if (!CanPour(tubes, from, to, capacity))
                        continue;

                    if (tubes[to].Count == 0)
                    {
                        // Moving a single-color tube into an empty one changes nothing,
                        // and all empty tubes are equivalent so try only one of them
                        if (sourceIsSingleColor || triedEmpty)
                            continue;

                        triedEmpty = true;
                        intoEmpty.Add(new Move(from, to));
                    }
                    else
                    {
                        onColor.Add(new Move(from, to));
                    }
                }
            }

            onColor.AddRange(intoEmpty);
            return onColor;
        }

        private static List<Move> BuildPath(Node node)
        {
            var path = new List<Move>();
            while (node.Parent != null)
            {
                path.Add(node.Move);
                node = node.Parent;
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Order-independent key: tube order does not matter for solvability
        /// </summary>
        private static string StateKey(List<List<int>> tubes)
        {
            var parts = new List<string>(tubes.Count);
            var sb = new StringBuilder();
            foreach (var tube in tubes)
            {
                sb.Clear();
                foreach (int color in tube)
                    sb.Append((char)('a' + color));
                parts.Add(sb.ToString());
            }
            parts.Sort(System.StringComparer.Ordinal);
            return string.Join("|", parts);
        }
    }
}
