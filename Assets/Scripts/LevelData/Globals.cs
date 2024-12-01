using PlayerIOClient;
using UnityEngine;

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
        public static Client ConsistentClient { get; private set; }
        private static Connection Conn { get; set; }


        public static bool ConnectToPlayerIO(string name = "guest")
        {
            if (ConsistentClient != null)
            {
                Conn.Disconnect();
                Conn = null;
                ConsistentClient = null;
            }

            PlayerIO.UseSecureApiRequests = true;
            PlayerIO.Authenticate(
                "brick-breaker-jeffkfgevk6eg5yp5m3jsq",
                "public",
                new()
                {
                    { "userId", name },
                },
                null,
                delegate (Client client)
                {
                    Debug.Log("Successfully connected to Player.IO");

                    Debug.Log("Create ServerEndpoint");
                    // Comment out the line below to use the live servers instead of your development server
                    //client.Multiplayer.DevelopmentServer = new ServerEndpoint("localhost", 8184);
                    client.Multiplayer.UseSecureConnections = true;
                    ConsistentClient = client;

                    client.Multiplayer.CreateJoinRoom(
                        null,
                        "Brick",
                        true,
                        null,
                        null,
                        delegate (Connection connection) {
                            Debug.Log("Joined Room.");
                            Conn = connection;
                        },
                        delegate (PlayerIOError error) {
                            Debug.Log("Error Joining Room: " + error);
                        }
                    );
                },
                delegate (PlayerIOError error)
                {
                    Debug.Log("Error connecting: " + error);
                }
            );
            return true;
        }
    }
}
