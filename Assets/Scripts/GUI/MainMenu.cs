using PlayerIOClient;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public InputField inputCustomLvl;
    public Text inputText;
    public Toggle endlessToggle;
    public Button btnQuit;

    public GameObject pnlLeaderboard;

    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<SoundHelper>().PlaySound($"Sound/BGM/Menu_{Globals.RNG.Next(1, 3)}", true); //play random BGM

        // add quit function
#if UNITY_STANDALONE
        btnQuit.onClick.AddListener(Quit);
#elif UNITY_WEBGL
        btnQuit.gameObject.SetActive(false);
#endif
    }

    // check for ESC keypress every frame. For quitting game.
    void Update()
    {
        if (Input.GetKey("escape"))
            Quit();
    }

    public void SetVolumeSlider(Slider g)
    {
        if (g.gameObject.name == "SFXSlider") //sets sfx volume to slider value
            PlayerPrefs.SetFloat("sfxvol", g.value);

        else if (g.gameObject.name == "BGMSlider") //sets bgm volume to slider value
        {
            PlayerPrefs.SetFloat("bgmvol", g.value);
            gameObject.GetComponent<AudioSource>().volume = g.value; //immediately changes background music volume.
        }
    }

    public void HideUIMenu(GameObject menuPanel) => menuPanel.SetActive(false);
    public void ShowUIMenu(GameObject menuPanel)
    {
        menuPanel.SetActive(true);

        if (menuPanel.name == "pnlSettings")
        {
            GameObject.Find("SFXSlider").GetComponent<Slider>().value = PlayerPrefs.GetFloat("sfxvol"); //loads sfx and bgm volumes from PlayerPrefs.
            GameObject.Find("BGMSlider").GetComponent<Slider>().value = PlayerPrefs.GetFloat("bgmvol");
        }
    }

    public void LoadCredit(Text text) => text.text = Resources.Load<TextAsset>("Credits").text; //Loads Credits.txt from file to display.

    public void ShowLeaderboardGUI()
    {
        pnlLeaderboard.SetActive(true);
        if (!pnlLeaderboard.GetComponent<HighscoreController>().hasLoaded) //only loads it once per instance of the main menu to save data usage.
            StartCoroutine(pnlLeaderboard.GetComponent<HighscoreController>().GetHighscores());
    }

    public void CustomLevelGUI()
    {
        inputCustomLvl.gameObject.SetActive(true);
        inputCustomLvl.text = Globals.CustomLevelData;
    }

    public void LeaveCustomLevelGUI()
    {
        inputCustomLvl.text = "";
        inputCustomLvl.gameObject.SetActive(false);
    }

    public void LaunchCustomLevelGUI()
    {
        if (inputText.text.Contains("1"))
        {
            Globals.GamePaused = false;
            Globals.Lives = 3;
            Globals.Score = 0;
            Globals.CustomLevelData = inputText.text;
            Globals.CustomLevel = true;
            SceneManager.LoadScene("GameView");
        }
    }

    public void StartGame()
    {
        Globals.GamePaused = false;
        Globals.Lives = 3;
        Globals.Score = 0;
        Globals.CustomLevel = false;
        Globals.EndlessMode = endlessToggle.isOn;
        Globals.StartWithEndless = endlessToggle.isOn;
        SceneManager.LoadScene("GameView");
    }

    void Quit()
    {

#if UNITY_STANDALONE
        try //this stops discord from displaying the "Playing Game" status, it is however in a try statement in case of failure (due to discord not being available)
        {
            Globals.discord.RunCallbacks();
            var activityManager = Globals.discord.GetActivityManager();
            activityManager.ClearActivity((result) =>
            {
                if (result == Discord.Result.Ok)
                    Debug.Log("Discord Success.");
                else
                    Debug.Log("Discord Failed.");
            });
        }
        catch { }

        Application.Quit();
#endif
    }
}
