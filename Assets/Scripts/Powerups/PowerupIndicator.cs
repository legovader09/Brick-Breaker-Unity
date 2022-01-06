using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PowerupIndicator : MonoBehaviour
{
    internal PowerupComponent.PowerupCodes powerCode;


    /// <summary>
    /// Upon creation, finds and sets the sprite to the relevant powerup code.
    /// </summary>
    /// <param name="p">The powerup code to set the texture to.</param>
    internal void Create(PowerupComponent.PowerupCodes p)
    {
        powerCode = p;
        gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>($"PowerupIndicators/{(int)p}-Indicator");
    }
}
