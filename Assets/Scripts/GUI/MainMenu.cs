using Constants;
using EventListeners;
using LevelData;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GUI
{
    public class MainMenu : MonoBehaviour
    {
        public InputField inputCustomLvl;
        public Text inputText;
        public Toggle endlessToggle;
        public Button btnQuit;

        public GameObject pnlLeaderboard;

        // Start is called before the first frame update
        private void Start()
        {
            GetComponent<SoundHelper>().PlaySound($"Sound/BGM/Menu_{Globals.Random.Next(1, 3)}", true); //play random BGM

            // add quit function
#if UNITY_STANDALONE
        btnQuit.onClick.AddListener(Quit);
#elif UNITY_WEBGL
            btnQuit.gameObject.SetActive(false);
#endif
        }

        // check for ESC keypress every frame. For quitting game.
        private void Update()
        {
            if (Input.GetKey("escape"))
                Quit();
        }

        public void SetVolumeSlider(Slider g)
        {
            switch (g.gameObject.name)
            {
                case "SFXSlider":
                    PlayerPrefs.SetFloat(ConfigConstants.SFXVolumeSetting, g.value);
                    break;
                case "BGMSlider":
                    PlayerPrefs.SetFloat(ConfigConstants.BGMVolumeSetting, g.value);
                    gameObject.GetComponent<AudioSource>().volume = g.value; //immediately changes background music volume.
                    break;
            }
        }

        public void HideUIMenu(GameObject menuPanel) => menuPanel.SetActive(false);
        public void ShowUIMenu(GameObject menuPanel)
        {
            menuPanel.SetActive(true);

            if (menuPanel.name != "pnlSettings") return;
            GameObject.Find("SFXSlider").GetComponent<Slider>().value = PlayerPrefs.GetFloat(ConfigConstants.SFXVolumeSetting); //loads sfx and bgm volumes from PlayerPrefs.
            GameObject.Find("BGMSlider").GetComponent<Slider>().value = PlayerPrefs.GetFloat(ConfigConstants.BGMVolumeSetting);
        }

        public void LoadCredit(Text text) => text.text = Resources.Load<TextAsset>("Credits").text; //Loads Credits.txt from file to display.

        public void ShowLeaderboardGUI()
        {
            pnlLeaderboard.SetActive(true);
            if (!pnlLeaderboard.GetComponent<HighscoreController>().HasLoaded) //only loads it once per instance of the main menu to save data usage.
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
            if (!inputText.text.Contains("1")) return;
            Globals.GamePaused = false;
            Globals.Lives = 3;
            Globals.Score = 0;
            Globals.CustomLevelData = inputText.text;
            Globals.CustomLevel = true;
            SceneManager.LoadScene("GameView");
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

        private void Quit()
        {
#if UNITY_STANDALONE
        try //this stops discord from displaying the "Playing Game" status, it is however in a try statement in case of failure (due to discord not being available)
        {
            Globals.Discord.RunCallbacks();
            var activityManager = Globals.Discord.GetActivityManager();
            activityManager.ClearActivity((result) =>
            {
                Debug.Log(result == Discord.Result.Ok ? "Discord Success." : "Discord Failed.");
            });
        }
        catch { /* ignored */ }
        Application.Quit();
#endif
        }
    }
}
