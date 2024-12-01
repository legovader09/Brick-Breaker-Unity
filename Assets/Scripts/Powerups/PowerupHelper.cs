using System.Collections;
using GUI;
using LevelData;
using UnityEngine;

namespace Powerups
{
    public class PowerupHelper : MonoBehaviour
    {
        GUIHelper _gui;
        IEnumerator _multiplyFlash;
        IEnumerator _safetyNetFlash;

        // Start is called before the first frame update
        internal void SetMultiplier(float multiplyVal)
        {
            IEnumerator mp = SetMultiplierEnumerator(multiplyVal);
            StopCoroutine(mp);
            StartCoroutine(mp);
        }

        /// <summary>
        /// Sets the score multiplier.
        /// </summary>
        /// <param name="multiplyVal">The float value to set the score multiplier by (usually 2 for double, and 0.5 for half)</param>
        /// <returns></returns>
        IEnumerator SetMultiplierEnumerator(float multiplyVal)
        {
            if (_multiplyFlash != null)
                StopCoroutine(_multiplyFlash);

            _gui = GameObject.Find("EventSystem").GetComponent<GUIHelper>();
            Globals.ScoreMultiplier = multiplyVal;

            yield return new WaitForSeconds(7f);

            _multiplyFlash = _gui.ShowPowerupExpiring(Mathf.Approximately(multiplyVal, 2f) 
                ? PowerupComponent.PowerupCodes.DoublePoints 
                : PowerupComponent.PowerupCodes.HalfPoints);

            StartCoroutine(_multiplyFlash);

            yield return new WaitForSeconds(3f);

            Globals.ScoreMultiplier = 1f;
            _gui.RemovePowerupFromSidebar(PowerupComponent.PowerupCodes.DoublePoints);
            _gui.RemovePowerupFromSidebar(PowerupComponent.PowerupCodes.HalfPoints);
            Destroy(gameObject);
        }

        internal void SetSafetyNet()
        {
            IEnumerator sn = SetSafetyNetEnumerator();
            StopCoroutine(sn);
            StartCoroutine(sn);
        }

        /// <summary>
        /// Set the safety net at the bottom of the screen, lasts 10 in game seconds.
        /// </summary>
        /// <returns></returns>
        IEnumerator SetSafetyNetEnumerator()
        {
            if (_safetyNetFlash != null)
                StopCoroutine(_safetyNetFlash);

            GameObject.Find("EventSystem").GetComponent<GameTracker>().safetyNet.SetActive(true);
            yield return new WaitForSeconds(7f);

            _safetyNetFlash = GameObject.Find("EventSystem"). // show powerup expiring after 7 seconds, to give a 3 second notice.
                GetComponent<GUIHelper>().
                ShowPowerupExpiring(PowerupComponent.PowerupCodes.SafetyNet);

            StartCoroutine(_safetyNetFlash);

            yield return new WaitForSeconds(3f);
            GameObject.Find("EventSystem").GetComponent<GameTracker>().safetyNet.SetActive(false);
            GameObject.Find("EventSystem").GetComponent<GUIHelper>().
                RemovePowerupFromSidebar(PowerupComponent.PowerupCodes.SafetyNet);

            Destroy(gameObject);
        }
    }
}
