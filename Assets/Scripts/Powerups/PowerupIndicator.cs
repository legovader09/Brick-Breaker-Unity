using UnityEngine;
using UnityEngine.UI;

namespace Powerups
{
    public class PowerupIndicator : MonoBehaviour
    {
        internal PowerupComponent.PowerupCodes PowerCode;


        /// <summary>
        /// Upon creation, finds and sets the sprite to the relevant powerup code.
        /// </summary>
        /// <param name="p">The powerup code to set the texture to.</param>
        internal void Create(PowerupComponent.PowerupCodes p)
        {
            PowerCode = p;
            gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>($"PowerupIndicators/{(int)p}-Indicator");
        }
    }
}
