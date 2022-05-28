using PlayerIOClient;
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
    public static Client consistentClient { get; private set; } = null;
    private static Connection conn { get; set; } = null;


    public static bool ConnectToPlayerIO(string name = "guest")
    {
        if (consistentClient != null)
        {
            conn.Disconnect();
            conn = null;
            consistentClient = null;
        }

        PlayerIO.Authenticate(
            "brick-breaker-jeffkfgevk6eg5yp5m3jsq",
            "public",
            new Dictionary<string, string>
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
                consistentClient = client;

                client.Multiplayer.CreateJoinRoom(
                    null,                    //Room id. If set to null a random roomid is used
                    "Brick",                   //The room type started on the server
                    true,                               //Should the room be visible in the lobby?
                    null,
                    null,
                    delegate (Connection connection) {
                        Debug.Log("Joined Room.");
                        conn = connection;
                    },
                    delegate (PlayerIOError error) {
                        Debug.Log("Error Joining Room: " + error.ToString());
                    }
                );
            },
            delegate (PlayerIOError error)
            {
                Debug.Log("Error connecting: " + error.ToString());
            }
        );
        return true;
    }
}
