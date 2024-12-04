using System.Collections;
using Enums;
using GUI;
using LevelData;
using UnityEngine;

namespace Powerups
{
    public class PowerupHelper : MonoBehaviour
    {
        private GUIHelper _gui;
        private IEnumerator _multiplyFlash;
        private IEnumerator _safetyNetFlash;

        // Start is called before the first frame update
        internal void SetMultiplier(float multiplyVal)
        {
            var mp = SetMultiplierEnumerator(multiplyVal);
            StopCoroutine(mp);
            StartCoroutine(mp);
        }

        /// <summary>
        /// Sets the score multiplier.
        /// </summary>
        /// <param name="multiplyVal">The float value to set the score multiplier by (usually 2 for double, and 0.5 for half)</param>
        /// <returns></returns>
        private IEnumerator SetMultiplierEnumerator(float multiplyVal)
        {
            if (_multiplyFlash != null) StopCoroutine(_multiplyFlash);

            _gui = GameObject.Find("EventSystem").GetComponent<GUIHelper>();
            Globals.ScoreMultiplier = multiplyVal;

            yield return new WaitForSeconds(7f);

            _multiplyFlash = _gui.ShowPowerupExpiring(Mathf.Approximately(multiplyVal, 2f) 
                ? PowerupCodes.DoublePoints 
                : PowerupCodes.HalfPoints);

            StartCoroutine(_multiplyFlash);

            yield return new WaitForSeconds(3f);

            Globals.ScoreMultiplier = 1f;
            _gui.RemovePowerupFromSidebar(PowerupCodes.DoublePoints);
            _gui.RemovePowerupFromSidebar(PowerupCodes.HalfPoints);
            Destroy(gameObject);
        }

        internal void SetSafetyNet()
        {
            var sn = SetSafetyNetEnumerator();
            StopCoroutine(sn);
            StartCoroutine(sn);
        }

        /// <summary>
        /// Set the safety net at the bottom of the screen, lasts 10 in game seconds.
        /// </summary>
        /// <returns></returns>
        private IEnumerator SetSafetyNetEnumerator()
        {
            if (_safetyNetFlash != null) StopCoroutine(_safetyNetFlash);

            GameObject.Find("EventSystem").GetComponent<GameTracker>().safetyNet.SetActive(true);
            yield return new WaitForSeconds(7f);

            _safetyNetFlash = GameObject.Find("EventSystem"). // show powerup expiring after 7 seconds, to give a 3 second notice.
                GetComponent<GUIHelper>().
                ShowPowerupExpiring(PowerupCodes.SafetyNet);

            StartCoroutine(_safetyNetFlash);

            yield return new WaitForSeconds(3f);
            GameObject.Find("EventSystem").GetComponent<GameTracker>().safetyNet.SetActive(false);
            GameObject.Find("EventSystem").GetComponent<GUIHelper>().
                RemovePowerupFromSidebar(PowerupCodes.SafetyNet);

            Destroy(gameObject);
        }
    }
}
