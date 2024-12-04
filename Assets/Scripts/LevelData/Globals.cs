using Networking;

namespace LevelData
{
    /// <summary>
    /// Static class file that stores all the important variables.
    /// </summary>
    public static class Globals
    {
        public static readonly System.Random Random = new();
        public static bool CustomLevel { get; set; }
        public static int AmountOfLevels { get; set; } = 7;
        public static string CustomLevelData { get; set; }
        public static int ChanceToDropPowerup { get; set; } = 40;
        public static float BallSpeed { get; set; } = 200f;
        public static bool GamePaused { get; set; }
        public static int Score { get; set; }
        public static int Lives { get; set; } = 3;
        public static bool EndlessMode { get; set; }
        public static string EndlessLevelData { get; set; }
        public static float ScoreMultiplier { get; set; } = 1f;
        public static bool StartWithEndless { get; set; }
        public static bool AIMode { get; set; }
        public static Discord.Discord Discord { get; set; }
        public static int BricksRemaining { get; set; }
        public static NetworkManager Client { get; set; } = new();
    }
}
