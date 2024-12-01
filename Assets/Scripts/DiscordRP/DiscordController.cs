using System;
using LevelData;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace DiscordRP
{
    public class DiscordController : MonoBehaviour
    {
#if UNITY_STANDALONE
        private Discord.Activity _activity = new()
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

        private readonly DateTime _epoch = new (1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private void Start()
        {
            try
            {
                Globals.Discord ??= new(819670954091216936, (ulong)Discord.CreateFlags.NoRequireDiscord);
                _activity.Timestamps = new();
                _activity.Timestamps.Start = (long)(DateTime.UtcNow - _epoch).TotalSeconds;
            }
            catch { /* ignored */ }
        }

        private void Update()
        {
            try
            {
                Globals.Discord.RunCallbacks();

                if (SceneManager.GetActiveScene().name == "GameView")
                {
                    _activity.Details = Globals.Lives == 0 
                        ? $"Game Over | {GameObject.Find("LvlTxt").GetComponent<Text>().text} | Lives: {Globals.Lives}" 
                        : $"In-Game | {GameObject.Find("LvlTxt").GetComponent<Text>().text} | Lives: {Globals.Lives}";
                    _activity.State = $"Score: {Globals.Score} | Bricks Left: {Globals.BricksRemaining}";
                }
                else
                {
                    _activity.Details = "In-Game";
                    _activity.State = "Main Menu";
                }

                var activityManager = Globals.Discord.GetActivityManager();
                activityManager.UpdateActivity(_activity, (result) => { if (result != Discord.Result.Ok) Debug.Log("Discord Failed."); });
            }
            catch { /* ignored */ }
        }

        internal void ResetTimeElapsed()
        {
            try
            {
                // discord timestamps are calculated based on epoch time
                _activity.Timestamps.Start = (long)(DateTime.UtcNow - _epoch).TotalSeconds;
            }
            catch { /* ignored */ }
        }
#endif
    }
}