using UnityEngine;
using UnityEngine.UI;

namespace Powerups
{
    public class PowerupIndicator : MonoBehaviour
    {
        internal PowerupCodes PowerCode;

        /// <summary>
        /// Upon creation, finds and sets the sprite to the relevant powerup code.
        /// </summary>
        /// <param name="powerup">The powerup code to set the texture to.</param>
        internal void Create(PowerupCodes powerup)
        {
            PowerCode = powerup;
            gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>($"PowerupIndicators/{(int)powerup}-Indicator");
        }
    }
}
