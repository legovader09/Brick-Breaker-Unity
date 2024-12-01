using LevelData;
using UnityEngine;

namespace EventListeners
{
    public class SoundHelper : MonoBehaviour
    {
        public enum AudioType
        { 
            BGM,
            Sfx
        }

        public AudioType audioType = AudioType.Sfx;

        // Start is called before the first frame update
        void Start()
        {
            if (!PlayerPrefs.HasKey("sfxvol")) //if sfx settings don't exist in playerprefs
                PlayerPrefs.SetFloat("sfxvol", 0.3f);

            if (!PlayerPrefs.HasKey("bgmvol")) //if bgm settings don't exist in playerprefs
                PlayerPrefs.SetFloat("bgmvol", 0.3f);

            if (audioType == AudioType.Sfx)
                gameObject.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("sfxvol");
            else
                gameObject.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("bgmvol");
        }

        internal void PlaySound(string sound, bool loop = false)
        {
            if (audioType == AudioType.Sfx) //set corresponding volume levels before playing the sound.
                gameObject.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("sfxvol");
            else
                gameObject.GetComponent<AudioSource>().volume = PlayerPrefs.GetFloat("bgmvol");

            if (loop)
            {
                gameObject.GetComponent<AudioSource>().loop = loop;
                gameObject.GetComponent<AudioSource>().clip = (AudioClip)Resources.Load(sound);
                gameObject.GetComponent<AudioSource>().Play();
            }
            else
            {
                gameObject.GetComponent<AudioSource>().PlayOneShot((AudioClip)Resources.Load(sound));
            }
            Debug.Log("Playing sound: " + sound);

            Camera.main.GetComponent<AudioListener>().enabled = true;
        }

        internal void StopSound() => gameObject.GetComponent<AudioSource>().Stop();

        internal void PauseSound() => Camera.main.GetComponent<AudioListener>().enabled = !Globals.GamePaused; //Pause essentially just mutes the sound until unpaused again. This only works for BGM.
    }
}
