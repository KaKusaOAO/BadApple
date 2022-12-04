﻿using System.Text;
using BigGustave;
using KaLib.Utils;

namespace BadApple;

public static class Program
{
    private const string Namespace = "badapple";
    private const string FuncDir = "functions";

    public static void Main(string[] args)
    {
        Logger.Level = LogLevel.Verbose;
        Logger.Logged += Logger.LogToEmulatedTerminalAsync;
        Logger.RunThreaded();

        try
        {
            Directory.Delete("datapack", true);
        }
        catch (Exception)
        {
            // No
        }

        const int frames = 4381;
        Logger.Info($"Generating routes... ({frames} entries, binary search)");
        GenerateRoute(0, frames);

        Logger.Info($"Generating {frames} frames...");
        for (var i = 0; i <= frames; i++)
        {
            GenerateFrameFunction(i);
        }

        Logger.Info("Done!");
    }

    private static void GenerateFrameFunction(int index)
    {
        var png = Png.Open(Path.Combine("frames", $"frames_{index.ToString().PadLeft(5, '0')}.png"));
        var y0 = png.Height + 10;
        var x0 = 0;
        var sb = new StringBuilder();

        for (var x = 0; x < png.Width; x++)
        {
            for (var y = 0; y < png.Height; y++)
            {
                var pixel = png.GetPixel(x, y);
                var grade = (int)Math.Round(pixel.R / 255.0 * 21);
                
                // Palette based on this thread:
                // https://www.reddit.com/r/Minecraft/comments/fy2u2c
                var block = grade switch
                {
                    0 => "black_concrete",
                    1 => "obsidian",
                    2 => "black_wool",
                    3 => "black_concrete_powder",
                    4 => "coal_block",
                    5 => "gray_concrete",
                    6 => "gray_wool",
                    7 => "gray_concrete_powder",
                    8 => "cobblestone",
                    9 => "gravel",
                    10 => "andesite",
                    11 => "stone",
                    12 => "light_gray_concrete",
                    13 => "light_gray_wool",
                    14 => "light_gray_concrete_powder",
                    15 => "diorite",
                    16 => "white_concrete",
                    17 => "bone_block",
                    18 => "smooth_quartz",
                    19 => "white_wool",
                    20 => "white_concrete_powder",
                    21 => "snow_block",
                    _ => throw new IndexOutOfRangeException("Should never reach here")
                };

                sb.AppendLine($"setblock {x0 + x} {y0 - y} 0 {block}");
            }
        }

        Save(sb, FramePath, $"{index}");
    }

    private static void GenerateRoute(int minInclusive, int maxInclusive)
    {
        if (minInclusive > maxInclusive)
        {
            throw new ArgumentException($"{nameof(maxInclusive)} is not larger than {nameof(minInclusive)}");
        }

        var count = maxInclusive - minInclusive + 1;
        var half = count / 2;
        var a1 = minInclusive;
        var a2 = minInclusive + half - 1;
        var b1 = a2 + 1;
        var b2 = maxInclusive;

        var sb = new StringBuilder();
        var name = $"{minInclusive}_{maxInclusive}";

        sb.AppendLine(
            a1 == a2
                ? $"execute if entity @e[tag=BadApple.Main,scores={{BadApple.Time={a1}}}] run function {Namespace}:frames/{a1}"
                : $"execute if entity @e[tag=BadApple.Main,scores={{BadApple.Time={a1}..{a2}}}] run function {Namespace}:routes/{a1}_{a2}");
        sb.AppendLine(
            b1 == b2
                ? $"execute if entity @e[tag=BadApple.Main,scores={{BadApple.Time={b1}}}] run function {Namespace}:frames/{b1}"
                : $"execute if entity @e[tag=BadApple.Main,scores={{BadApple.Time={b1}..{b2}}}] run function {Namespace}:routes/{b1}_{b2}");
        Save(sb, RoutePath, name);

        if (a1 != a2) GenerateRoute(a1, a2);
        if (b1 != b2) GenerateRoute(b1, b2);
    }

    private static void Save(StringBuilder sb, string path, string name)
    {
        var fullPath = Path.GetFullPath(path);
        var fullRoot = Path.GetFullPath(FuncPath);
        var p = fullPath[(fullRoot.Length + 1)..];

        Directory.CreateDirectory(path);
        sb.Insert(0, "\n");
        sb.Insert(0, $"# Auto-generated by Kaka // {DateTime.Now}\n");
        sb.Insert(0, $"#> {Namespace}:{p}/{name}\n");

        File.WriteAllText(Path.Combine(path, $"{name}.mcfunction"), sb.ToString());
    }

    private static string FuncPath => Path.Combine("datapack", Namespace, FuncDir);
    private static string RoutePath => Path.Combine(FuncPath, "routes");
    private static string FramePath => Path.Combine(FuncPath, "frames");
}