using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WaterSort.Core;

public static class Program
{
    private static int _failures;

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            _failures++;
            Console.WriteLine("FAIL: " + message);
        }
    }

    private static List<List<int>> Tubes(params int[][] tubes) => tubes.Select(t => t.ToList()).ToList();

    public static int Main()
    {
        SolverRules();
        HandcraftedLevels();
        GeneratedLevels(1000);
        ReverseMoveFallback();
        Localization();

        Console.WriteLine(_failures == 0 ? "ALL TESTS PASSED" : $"{_failures} FAILURE(S)");
        return _failures == 0 ? 0 : 1;
    }

    private static void SolverRules()
    {
        // Pour onto matching color with enough space
        var t = Tubes(new[] { 1, 2, 2 }, new[] { 1, 2 }, new int[0]);
        Check(LevelSolver.CanPour(t, 0, 1, 4), "matching color with space should be pourable");

        // Not enough space for the whole run
        t = Tubes(new[] { 1, 2, 2 }, new[] { 1, 1, 2 }, new int[0]);
        Check(!LevelSolver.CanPour(t, 0, 1, 4), "run larger than space must be rejected");

        // Different colors
        t = Tubes(new[] { 1, 2 }, new[] { 2, 1 });
        Check(!LevelSolver.CanPour(t, 0, 1, 4), "different top colors must be rejected");

        // Completed tube cannot be poured from
        t = Tubes(new[] { 3, 3, 3, 3 }, new int[0]);
        Check(!LevelSolver.CanPour(t, 0, 1, 4), "complete tube must not be a source");

        // Pour moves the whole run
        t = Tubes(new[] { 1, 2, 2 }, new int[0]);
        LevelSolver.Pour(t, 0, 1);
        Check(t[0].SequenceEqual(new[] { 1 }) && t[1].SequenceEqual(new[] { 2, 2 }), "pour must move the whole top run");

        // Already solved
        t = Tubes(new[] { 1, 1, 1, 1 }, new int[0]);
        Check(LevelSolver.Solve(t, 4)?.Count == 0, "solved state should need 0 moves");

        // Unsolvable: two full mixed tubes and no space
        t = Tubes(new[] { 1, 2, 1, 2 }, new[] { 2, 1, 2, 1 });
        Check(LevelSolver.Solve(t, 4) == null, "no free space should be unsolvable");

        // Solution replays to solved state
        t = Tubes(new[] { 1, 2, 1, 2 }, new[] { 2, 1, 2, 1 }, new int[0], new int[0]);
        var solution = LevelSolver.Solve(t, 4);
        Check(solution != null, "level 0 layout should be solvable");
        if (solution != null)
            Check(Replay(t, solution, 4), "level 0 solution should replay to a solved state");
    }

    private static bool Replay(List<List<int>> start, List<LevelSolver.Move> moves, int capacity)
    {
        var state = LevelSolver.Clone(start);
        foreach (var move in moves)
        {
            if (!LevelSolver.CanPour(state, move.From, move.To, capacity))
                return false;
            LevelSolver.Pour(state, move.From, move.To);
        }
        return LevelSolver.IsSolved(state, capacity);
    }

    private static void HandcraftedLevels()
    {
        for (int i = 0; i < 3; i++)
        {
            var level = LevelDataGenerator.GenerateLevel(i);
            Check(level != null && level.IsValid(), $"Level_{i}.json should load and be valid");
            if (level == null) continue;

            Check(level.TubeConfigurations.Count == level.TubeCount, $"Level_{i}.json tube count");
            var solution = LevelSolver.Solve(level.ToSolverState(), level.TubeCapacity);
            Check(solution != null && solution.Count > 0, $"Level_{i}.json should be solvable and not pre-solved");
            Console.WriteLine($"Level_{i}.json: {level.TubeCount} tubes, solved in {solution?.Count} moves");
        }

        // Round trip through ToJson / FromJson
        var original = LevelDataGenerator.GenerateLevel(1);
        var copy = LevelData.FromJson(original.ToJson());
        Check(copy.IsValid() && copy.ToSolverState().SelectMany(x => x).SequenceEqual(original.ToSolverState().SelectMany(x => x)),
            "LevelData JSON round trip should preserve tubes");
    }

    private static void GeneratedLevels(int count)
    {
        var watch = Stopwatch.StartNew();
        long slowest = 0;
        int slowestLevel = -1;
        int minMoves = int.MaxValue, maxMoves = 0;

        for (int i = 3; i < count; i++)
        {
            var levelWatch = Stopwatch.StartNew();
            var level = LevelDataGenerator.GenerateProceduralLevel(i);
            levelWatch.Stop();

            if (levelWatch.ElapsedMilliseconds > slowest)
            {
                slowest = levelWatch.ElapsedMilliseconds;
                slowestLevel = i;
            }

            Check(level.IsValid(), $"generated level {i} should be valid");
            Check(level.TubeCount == level.TubeConfigurations.Count, $"generated level {i} tube count");

            var state = level.ToSolverState();
            Check(!LevelSolver.IsSolved(state, level.TubeCapacity), $"generated level {i} must not start solved");

            var solution = LevelSolver.Solve(state, level.TubeCapacity);
            Check(solution != null, $"generated level {i} must be solvable");
            if (solution != null)
            {
                Check(Replay(state, solution, level.TubeCapacity), $"generated level {i} solution must replay");
                minMoves = Math.Min(minMoves, solution.Count);
                maxMoves = Math.Max(maxMoves, solution.Count);
            }

            // Deterministic
            var again = LevelDataGenerator.GenerateProceduralLevel(i);
            Check(again.ToSolverState().SelectMany(x => x).SequenceEqual(state.SelectMany(x => x)) &&
                  again.TubeCount == level.TubeCount, $"generated level {i} must be deterministic");
        }

        Console.WriteLine($"Generated levels 3..{count - 1}: OK in {watch.ElapsedMilliseconds} ms " +
                          $"(slowest: level {slowestLevel}, {slowest} ms; solutions {minMoves}-{maxMoves} moves)");
    }

    private static void ReverseMoveFallback()
    {
        var random = new Random(42);
        for (int colors = 2; colors <= 12; colors++)
        {
            for (int run = 0; run < 20; run++)
            {
                var tubes = LevelDataGenerator.ShuffleByReverseMoves(LevelDataGenerator.GetAvailableColors(colors), 4, 2, random);
                var solution = LevelSolver.Solve(tubes, 4, 2000000);
                Check(solution != null, $"reverse-move level ({colors} colors, run {run}) must be solvable");
            }
        }
        Console.WriteLine("Reverse-move fallback: OK");
    }

    private static void Localization()
    {
        var placeholder = new Regex(@"\{\d+\}");

        foreach (var entry in LocalizationTable.Entries)
        {
            Check(entry.Value.Length == 2 && entry.Value.All(v => !string.IsNullOrWhiteSpace(v)),
                $"'{entry.Key}' needs Turkish and English text");
            if (entry.Value.Length != 2) continue;

            var tr = placeholder.Matches(entry.Value[0]).Select(m => m.Value).OrderBy(v => v);
            var en = placeholder.Matches(entry.Value[1]).Select(m => m.Value).OrderBy(v => v);
            Check(tr.SequenceEqual(en), $"'{entry.Key}' has different placeholders in Turkish and English");

            foreach (var text in entry.Value)
            {
                try { string.Format(text, 1, 2, 3); }
                catch (FormatException) { Check(false, $"'{entry.Key}' is not a valid format string: {text}"); }
            }
        }

        // Every key referenced from code must exist
        string root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../../Assets"));
        var keyPattern = new Regex(@"(?:Loc\.Get\(|LocalizedText\(|Key\s*=\s*|Button\([^,]+,\s*""[^""]+"",\s*|Toggle\([^,]+,\s*""[^""]+"",\s*|Text\([^,]+,\s*""[^""]+"",\s*)""([a-z_]+\.[a-z_.]+)""");
        int used = 0;
        foreach (var file in Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            foreach (Match match in keyPattern.Matches(File.ReadAllText(file)))
            {
                used++;
                string key = match.Groups[1].Value;
                Check(LocalizationTable.Entries.ContainsKey(key), $"{Path.GetFileName(file)} uses missing localization key '{key}'");
            }
        }
        Check(used > 20, $"expected to find localization keys in code, found {used}");

        Console.WriteLine($"Localization: {LocalizationTable.Entries.Count} keys, {used} uses checked");
    }
}
