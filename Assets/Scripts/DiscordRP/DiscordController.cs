using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DiscordController : MonoBehaviour
{
#if UNITY_STANDALONE
    Discord.Activity activity = new Discord.Activity
    {
        State = "Main Menu",
        Details = "In the main menu.",
        Timestamps =
        {
            Start = 0,
            End = 0,
        },
        Instance = true
    };

    DateTime epoch;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            if (Globals.discord == null)
                Globals.discord = new Discord.Discord(819670954091216936, (ulong)Discord.CreateFlags.NoRequireDiscord);

            activity.Timestamps = new Discord.ActivityTimestamps();
            epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            activity.Timestamps.Start = (long)(DateTime.UtcNow - epoch).TotalSeconds;
        }
        catch { }
    }

    // Update is called once per frame
    void Update()
    {
        try
        {
            Globals.discord.RunCallbacks();

            if (SceneManager.GetActiveScene().name == "GameView")
            {
                if (Globals.Lives == 0)
                {
                    activity.Details = $"Game Over | {GameObject.Find("LvlTxt").GetComponent<Text>().text} | Lives: {Globals.Lives}";
                    activity.State = $"Score: {Globals.Score} | Bricks Left: {Globals.BricksRemaining}";
                }
                else
                {
                    activity.Details = $"In-Game | {GameObject.Find("LvlTxt").GetComponent<Text>().text} | Lives: {Globals.Lives}";
                    activity.State = $"Score: {Globals.Score} | Bricks Left: {Globals.BricksRemaining}";
                }
            }
            else
            {
                activity.Details = "In-Game";
                activity.State = "Main Menu";
            }

            var activityManager = Globals.discord.GetActivityManager();
            activityManager.UpdateActivity(activity, (result) =>
            {
                if (result != Discord.Result.Ok)
                    Debug.Log("Discord Failed.");
            });
        }
        catch { }
    }

    internal void ResetTimeElapsed()
    {
        try
        {
            epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            activity.Timestamps.Start = (long)(DateTime.UtcNow - epoch).TotalSeconds; //discord timestamps are calculated based on epoch times, so we must calculate the seconds that have elapsed since the very first epoch calender date.
        }
        catch { }
    }
#endif
}