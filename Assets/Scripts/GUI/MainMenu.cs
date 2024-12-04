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
        public Dialog dialogPrefab;
        public GameSessionData sessionData;
        public GameObject pnlLeaderboard;
        
        // Start is called before the first frame update
        private void Start()
        {
            if (!PlayerPrefs.HasKey("username") || PlayerPrefs.GetString("username").Trim().Length == 0)
            {
                StartCoroutine(Dialog.ShowInputDialog(dialogPrefab, val =>
                {
                    PlayerPrefs.SetString("username", val);
                    PlayerPrefs.Save();
                },
                "Pick a Username", "This will be shown on the leaderboard and online matches.", okOnly: true));
            }
            
            sessionData.Initialize(success =>
            {
                GetComponent<SoundHelper>().PlaySound($"Sound/BGM/Menu_{sessionData.Random.Next(1, 3)}", true); //play random BGM
                sessionData.ResetData(); 
                if (success) return;
                sessionData.ErrorMessage = "Unable to connect to game servers.";
                SceneManager.LoadScene("MainMenu");
            });

            if (sessionData.ErrorMessage != string.Empty)
            {
                StartCoroutine(Dialog.ShowMessageDialog(dialogPrefab, _ => sessionData.ErrorMessage = string.Empty,
                    "Error", sessionData.ErrorMessage));
            }

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
            // TODO: Reusable settings component + Add username reset in settings
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
            inputCustomLvl.text = sessionData.CustomLevelData;
        }

        public void LeaveCustomLevelGUI()
        {
            inputCustomLvl.text = "";
            inputCustomLvl.gameObject.SetActive(false);
        }

        public void LaunchCustomLevelGUI()
        {
            if (!inputText.text.Contains("1")) return;
            sessionData.ResetData();
            sessionData.CustomLevelData = inputText.text;
            sessionData.customLevel = true;
            SceneManager.LoadScene("GameView");
        }

        public void StartGame()
        {
            sessionData.ResetData();
            sessionData.endlessMode = endlessToggle.isOn;
            sessionData.startWithEndless = endlessToggle.isOn;
            SceneManager.LoadScene("GameView");
        }

        private void Quit()
        {
#if UNITY_STANDALONE
        try //this stops discord from displaying the "Playing Game" status, it is however in a try statement in case of failure (due to discord not being available)
        {
            sessionData.Discord.RunCallbacks();
            var activityManager = sessionData.Discord.GetActivityManager();
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
