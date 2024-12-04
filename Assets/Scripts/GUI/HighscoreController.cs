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
            var client = sessionData.Connection?.Client;
            if (client == null)
            {
                completeAction(false);
                yield break;
            }
            
            bool? postSuccess = null;
            long? highestScore = null;
            long? highestLevel = null;

            client.Leaderboards.GetNeighbourhood("highscores", "score", 0, 1, null,
                entry => highestScore = entry.Length > 0 ? Math.Max(entry[0].Score, score) : score,
                _ => highestScore = score);
            client.Leaderboards.GetNeighbourhood("highscores", "levels", 0, 1, null,
                entry => highestLevel = entry.Length > 0 ? Math.Max(entry[0].Score, level) : level,
                _ => highestLevel = level);
                
            yield return new WaitUntil(() => highestScore != null && highestLevel != null);
            client.Leaderboards.Set("highscores", "levels", (long)highestLevel!, 
                _ => postSuccess = true, _ => postSuccess = false);
            client.Leaderboards.Set("highscores", "score", (long)highestScore!,
                _ => postSuccess = true, _ => postSuccess = false);

            yield return new WaitUntil(() => postSuccess != null);
            completeAction((bool)postSuccess!);
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