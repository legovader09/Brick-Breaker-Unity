using System;
using System.Collections;
using LevelData;
using PlayerIOClient;
using UnityEngine;

namespace GUI
{
    public class HighscoreController : MonoBehaviour
    {
        internal bool HasLoaded;
        public GameObject canvas;
        public GameObject leadboardEntryPrefab;
        public GameSessionData sessionData;

        /// <summary>
        /// Posts score to database.
        /// </summary>
        /// <param name="score">The score they achieved.</param>
        /// <param name="level">The highest level they achieved</param>
        /// <param name="completeAction">Action to run once highscore has been posted</param>
        /// <returns></returns>
        internal IEnumerator PostHighscore(int score, int level, Action<bool> completeAction)
        {
            var postSuccess = false;
            var client = sessionData.Connection?.Client;
            if (client == null)
            {
                completeAction(false);
                yield break;
            }
            try
            {
                client.Leaderboards.Set("highscores", "score", score, null);
                client.Leaderboards.Set("highscores", "levels", level, null);
                postSuccess = true;
            }
            catch (Exception ex)
            {
                Debug.Log($"An error occured while posting highscore: {ex.Message}");
            }
            completeAction(postSuccess);
            yield return true;
        }

        /// <summary>
        /// Get the scores from the MySQL DB to display in a GUIText.
        /// </summary>
        /// <returns></returns>
        internal IEnumerator GetHighscores()
        {
            // CLear leaderboard
            for (var i = canvas.transform.childCount - 1; i >= 0; i--)
            {
                var child = canvas.transform.GetChild(i).gameObject;
                Destroy(child);
            }
            
            sessionData.Connection.Client.Leaderboards.GetTop("highscores", "score", 0, 20, null,
            delegate (LeaderboardEntry[] e)
            {
                foreach (var entry in e)
                {
                    var entryPrefab = Instantiate(leadboardEntryPrefab, canvas.transform);
                    entryPrefab.GetComponent<LeaderboardEntryComponent>().SetData(entry.Rank, entry.ConnectUserId, entry.Score);
                }
            });

            yield return null;
        }
    }
}