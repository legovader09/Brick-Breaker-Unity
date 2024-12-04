using Constants;
using Enums;
using UnityEngine;

namespace EventListeners
{
    public class SoundHelper : MonoBehaviour
    {
        public AudioTypes audioTypes = AudioTypes.SFX;
        private AudioSource _audioSource;
        private AudioListener _audioListener;

        private void Awake()
        {
            _audioSource = GetComponent<AudioSource>();
            _audioListener = Camera.main?.GetComponent<AudioListener>();
            if (!PlayerPrefs.HasKey(ConfigConstants.SFXVolumeSetting)) PlayerPrefs.SetFloat(ConfigConstants.SFXVolumeSetting, 0.3f);
            if (!PlayerPrefs.HasKey(ConfigConstants.BGMVolumeSetting)) PlayerPrefs.SetFloat(ConfigConstants.BGMVolumeSetting, 0.3f);

            _audioSource.volume = PlayerPrefs.GetFloat(audioTypes == AudioTypes.SFX ? ConfigConstants.SFXVolumeSetting : ConfigConstants.BGMVolumeSetting);
        }

        internal void PlaySound(string sound, bool loop = false)
        {
            _audioSource.volume = PlayerPrefs.GetFloat(audioTypes == AudioTypes.SFX ? ConfigConstants.SFXVolumeSetting : ConfigConstants.BGMVolumeSetting);

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
        
        internal void TogglePauseSound() => _audioListener.enabled = !_audioListener.enabled;
    }
}
