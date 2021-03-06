using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using PlayerIOClient;
using System.Collections.Generic;

public class HighscoreController : MonoBehaviour
{
    private readonly string secretKey = "bricker";
    private readonly string addScoreURL = "http://localhost:8080/addscore.php?";
    private readonly string highscoreURL = "http://localhost:8080/display.php";
    internal bool hasLoaded;
    internal bool? postSuccess = null;
    public Text textField;

    /// <summary>
    /// Posts score to database.
    /// </summary>
    /// <param name="name">Username of player</param>
    /// <param name="score">The score they achieved.</param>
    /// <param name="level">The highest level they achieved</param>
    /// <returns></returns>
    internal IEnumerator PostHighscore(string name, int score, int level)
    {
        Globals.ConnectToPlayerIO(name);
        yield return new WaitUntil(() => Globals.consistentClient != null);

        Globals.consistentClient.Leaderboards.Set("highscores", "score", score, null);
        Globals.consistentClient.Leaderboards.Set("highscores", "levels", level, null);

        postSuccess = true;
        yield return true;
    }

    /// <summary>
    /// Get the scores from the MySQL DB to display in a GUIText.
    /// </summary>
    /// <returns></returns>
    internal IEnumerator GetHighscores()
    {
        if (Globals.ConnectToPlayerIO())
        {
            yield return new WaitUntil(() => Globals.consistentClient != null);

            Globals.consistentClient.Leaderboards.GetTop("highscores", "score", 0, 20, null,
            delegate (LeaderboardEntry[] e)
            {
                foreach (LeaderboardEntry entry in e)
                {
                    textField.text += string.Format("  {0}    {1}           {2}\n", entry.Rank, entry.ConnectUserId, entry.Score);
                }
            }); 
        }

        yield return null;
    }

    /// <summary>
    /// Posts score to database.
    /// </summary>
    /// <param name="name">Username of player</param>
    /// <param name="score">The score they achieved.</param>
    /// <param name="level">The highest level they achieved</param>
    /// <returns></returns>
    [Obsolete("This method uses PHP and MySQL database connection which are no longer maintained. Use PostHighscore() instead.", true)]
    internal IEnumerator PostScores(string name, int score, int level)
    {
        postSuccess = true;
        //This connects to a server side php script that will add the player's stats to a MySQL database
        string hash = Md5Sum(name + score + level + secretKey);
 
        string post_url = addScoreURL + "name=" + WWW.EscapeURL(name) + "&score=" + score + "&level=" + level + "&hash=" + hash;

        // Post the URL to the site and create a download object to get the result.
        WWW hs_post = new WWW(post_url);
        yield return hs_post; // Wait until the download is done
 
        if (hs_post.error != null)
        {
            postSuccess = false;
            Debug.Log("There was an error posting the high score: " + hs_post.error);
        }
    }

    /// <summary>
    /// Get the scores from the MySQL DB to display in a GUIText.
    /// </summary>
    /// <returns></returns>
    [Obsolete("Use GetHighscores() instead.", true)]
    internal IEnumerator GetScores()
    {
        textField.text = "Loading Scores";
        WWW hs_get = new WWW(highscoreURL);
        yield return hs_get;
 
        if (hs_get.error != null)
        {
            Debug.Log("There was an error getting the high score: " + hs_get.error);
        }
        else
        {
            textField.text = hs_get.text; // this is a GUIText that will display the scores in game.
            hasLoaded = true;
        }
    }

    /// <summary>
    /// Encrypts php request to ensure data arrives at endpoint securely.
    /// The md5 encryption here is identical to the method used in php to ensure I yield the same result.
    /// </summary>
    /// <param name="strToEncrypt">String to encrypt</param>
    /// <returns></returns>
    [Obsolete("", true)]
    private string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }
}