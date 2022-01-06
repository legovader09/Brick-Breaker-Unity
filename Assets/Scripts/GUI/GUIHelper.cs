using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static PowerupComponent;

public class GUIHelper : MonoBehaviour
{
    public GameObject indicatorPrefab;
    public GameObject parent;
    public Vector3 initPosition;
    public int amountOfIndicators;
    internal List<PowerupComponent.PowerupCodes> powerList = new List<PowerupComponent.PowerupCodes>();

    /// <summary>
    /// Adds an active powerup's indicator from the sidebar.
    /// </summary>
    /// <param name="p">The relevant <see cref="PowerupComponent.PowerupCodes"/> to add.</param>
    internal void AddPowerupToSidebar(PowerupComponent.PowerupCodes p)
    {
        if (!powerList.Contains(p))
        {
            GameObject ind = Instantiate(indicatorPrefab, parent.transform, false);
            ind.transform.localPosition = new Vector3(initPosition.x,
                initPosition.y + (ind.GetComponent<RectTransform>().rect.height * powerList.Count), initPosition.z);
            ind.GetComponent<PowerupIndicator>().Create(p);
            powerList.Add(p);
        }
    }

    /// <summary>
    /// Cause an activated powerup to "blink" repeatedly.
    /// </summary>
    /// <param name="p">The relevant <see cref="PowerupComponent.PowerupCodes"/> to flash.</param>
    /// <remarks>Make sure to call this function with <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"></see>.</remarks>
    internal IEnumerator ShowPowerupExpiring(PowerupComponent.PowerupCodes p)
    {
        if (powerList.Contains(p))
        {
            GameObject[] g = GameObject.FindGameObjectsWithTag("Indicator");
            foreach (GameObject o in g)
                if (o.GetComponent<PowerupIndicator>().powerCode == p)
                {
                    for (int i = 0; i < 6; i++)
                    {
                        try { o.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.5f); } catch { } //50% opacity
                        gameObject.GetComponent<SoundHelper>().PlaySound("Sound/SFX/zipclick");
                        yield return new WaitForSeconds(0.25f);
                        try { o.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f); } catch { } //100% opacity
                        gameObject.GetComponent<SoundHelper>().PlaySound("Sound/SFX/zipclick");
                        yield return new WaitForSeconds(0.25f);
                    }
                    break; // gameobject has been found, and only one of a type exists in the list of indicators.
                }
        }
    }

    /// <summary>
    /// Removes an active powerup's indicator from the sidebar.
    /// </summary>
    /// <param name="p">The relevant <see cref="PowerupComponent.PowerupCodes"/> to remove.</param>
    internal void RemovePowerupFromSidebar(PowerupComponent.PowerupCodes p)
    {
        if (powerList.Contains(p))
        {
            GameObject[] g = GameObject.FindGameObjectsWithTag("Indicator");
            foreach (GameObject o in g)
                if (o.GetComponent<PowerupIndicator>().powerCode == p)
                {
                    Destroy(o);
                    powerList.Remove(p);
                }
        }
    }


    /// <summary>
    /// Removes all active powerups indicator from the sidebar.
    /// </summary>
    internal void RemoveAllPowerups()
    {
        for (int i = 1; i <= 16; i++)
        {
            if (powerList.Contains((PowerupCodes)i))
            {
                GameObject[] g = GameObject.FindGameObjectsWithTag("Indicator");
                foreach (GameObject o in g)
                    if (o.GetComponent<PowerupIndicator>().powerCode == (PowerupCodes)i)
                    {
                        Destroy(o);
                        powerList.Remove((PowerupCodes)i);
                    }
            }
        }
    }

    void Update()
    {
        amountOfIndicators = powerList.Count;
        gameObject.GetComponentInParent<GUIHelper>().MoveIndicatorsDown();
    }

    /// <summary>
    /// Moves all active indicators to the bottom of the screen to allow showing new powerups at the top.
    /// </summary>
    private void MoveIndicatorsDown()
    {
        GameObject[] g = GameObject.FindGameObjectsWithTag("Indicator");
        int i = 0;
        foreach (GameObject o in g)
        {
            o.transform.localPosition = new Vector3(
                initPosition.x,
                initPosition.y + (o.GetComponent<RectTransform>().rect.height * i), //move y scale down by 1 unit of block.
                initPosition.z);
            i++;
        }
    }
}
