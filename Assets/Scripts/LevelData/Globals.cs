using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Static class file that stores all the important variables.
/// </summary>
public static class Globals
{
    public static System.Random RNG = new System.Random();
    public static bool CustomLevel { get; set; } = false;
    public static int AmountOfLevels { get; set; } = 7;
    public static string CustomLevelData { get; set; }
    public static int ChanceToDropPowerup { get; set; } = 40;
    public static float BallSpeed { get; set; } = 200f;
    public static bool GamePaused { get; set; }
    public static int Score { get; set; } = 0;
    public static int Lives { get; set; } = 3;
    public static bool EndlessMode { get; set; } = false;
    public static string EndlessLevelData { get; set; }
    public static float ScoreMultiplier { get; set; } = 1f;
    public static bool StartWithEndless { get; set; } = false;
    public static bool AIMode { get; set; } = false;
    public static Discord.Discord discord { get; set; } = null;
    public static int BricksRemaining { get; set; } = 0;
}
