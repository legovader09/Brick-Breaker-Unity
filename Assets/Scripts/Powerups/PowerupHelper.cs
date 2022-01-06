using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerupHelper : MonoBehaviour
{
    GUIHelper gui;
    IEnumerator multiplyFlash;
    IEnumerator safetyNetFlash;

    // Start is called before the first frame update
    internal void SetMultiplier(float MultiplyVal)
    {
        IEnumerator mp = setMultiplier(MultiplyVal);
        StopCoroutine(mp);
        StartCoroutine(mp);
    }

    /// <summary>
    /// Sets the score multiplier.
    /// </summary>
    /// <param name="MultiplyVal">The float value to set the score multiplier by (usually 2 for double, and 0.5 for half)</param>
    /// <returns></returns>
    IEnumerator setMultiplier(float MultiplyVal)
    {
        if (multiplyFlash != null)
            StopCoroutine(multiplyFlash);

        gui = GameObject.Find("EventSystem").GetComponent<GUIHelper>();
        Globals.ScoreMultiplier = MultiplyVal;

        yield return new WaitForSeconds(7f);

        if (MultiplyVal == 2f) ///after 7 seconds show the powerup indicator flashing to show expiring.
            multiplyFlash = gui.ShowPowerupExpiring(PowerupComponent.PowerupCodes.DoublePoints);
        else
            multiplyFlash = gui.ShowPowerupExpiring(PowerupComponent.PowerupCodes.HalfPoints);

        StartCoroutine(multiplyFlash);

        yield return new WaitForSeconds(3f);

        Globals.ScoreMultiplier = 1f;
        gui.RemovePowerupFromSidebar(PowerupComponent.PowerupCodes.DoublePoints);
        gui.RemovePowerupFromSidebar(PowerupComponent.PowerupCodes.HalfPoints);
        Destroy(gameObject);
    }

    internal void SetSafetyNet()
    {
        IEnumerator sn = setSafetyNet();
        StopCoroutine(sn);
        StartCoroutine(sn);
    }

    /// <summary>
    /// Set the safety net at the bottom of the screen, lasts 10 in game seconds.
    /// </summary>
    /// <returns></returns>
    IEnumerator setSafetyNet()
    {
        if (safetyNetFlash != null)
            StopCoroutine(safetyNetFlash);

        GameObject.Find("EventSystem").GetComponent<GameTracker>().safetyNet.SetActive(true);
        yield return new WaitForSeconds(7f);

        safetyNetFlash = GameObject.Find("EventSystem"). // show powerup expiring after 7 seconds, to give a 3 second notice.
                            GetComponent<GUIHelper>().
                                ShowPowerupExpiring(PowerupComponent.PowerupCodes.SafetyNet);

        StartCoroutine(safetyNetFlash);

        yield return new WaitForSeconds(3f);
        GameObject.Find("EventSystem").GetComponent<GameTracker>().safetyNet.SetActive(false);
        GameObject.Find("EventSystem").GetComponent<GUIHelper>().
                            RemovePowerupFromSidebar(PowerupComponent.PowerupCodes.SafetyNet);

        Destroy(gameObject);
    }
}
