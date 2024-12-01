using LevelData;
using UnityEngine;

namespace EventListeners
{
    public enum AudioType { BGM, SFX }
    public class SoundHelper : MonoBehaviour
    {
        private const string SFXVolumeSetting = "sfxvol";
        private const string BGMVolumeSetting = "bgmvol";
        public AudioType audioType = AudioType.SFX;
        private AudioSource _audioSource;
        private AudioListener _audioListener;

        // Start is called before the first frame update
        private void Start()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioListener = Camera.main?.GetComponent<AudioListener>();
            if (!PlayerPrefs.HasKey(SFXVolumeSetting)) PlayerPrefs.SetFloat(SFXVolumeSetting, 0.3f);
            if (!PlayerPrefs.HasKey(BGMVolumeSetting)) PlayerPrefs.SetFloat(BGMVolumeSetting, 0.3f);

            _audioSource.volume = PlayerPrefs.GetFloat(audioType == AudioType.SFX ? SFXVolumeSetting : BGMVolumeSetting);
        }

        internal void PlaySound(string sound, bool loop = false)
        {
            _audioSource.volume = PlayerPrefs.GetFloat(audioType == AudioType.SFX ? SFXVolumeSetting : BGMVolumeSetting);

            if (loop)
            {
                _audioSource.loop = true;
                _audioSource.clip = (AudioClip)Resources.Load(sound);
                _audioSource.Play();
            }
            else
            {
                _audioSource.PlayOneShot((AudioClip)Resources.Load(sound));
            }
            Debug.Log("Playing sound: " + sound);

            _audioListener.enabled = true;
        }

        internal void StopSound() => _audioSource.Stop();
        
        //Pause essentially just mutes the sound until unpaused again. This only works for BGM.
        internal void PauseSound() => _audioListener.enabled = !Globals.GamePaused;
    }
}
