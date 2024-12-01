using System.Collections;
using System.Collections.Generic;
using EventListeners;
using Powerups;
using UnityEngine;
using UnityEngine.UI;
using static Powerups.PowerupComponent;

namespace GUI
{
    public class GUIHelper : MonoBehaviour
    {
        public GameObject indicatorPrefab;
        public GameObject parent;
        public Vector3 initPosition;
        private readonly List<PowerupCodes> _powerList = new();
        private SoundHelper _soundHelper;

        private void Start()
        {
            _soundHelper = GetComponent<SoundHelper>();
        }

        /// <summary>
        /// Adds an active powerup's indicator from the sidebar.
        /// </summary>
        /// <param name="powerup">The relevant <see cref="PowerupCodes"/> to add.</param>
        internal void AddPowerupToSidebar(PowerupCodes powerup)
        {
            if (_powerList.Contains(powerup)) return;
            var ind = Instantiate(indicatorPrefab, parent.transform, false);
            ind.transform.localPosition = new(initPosition.x,
                initPosition.y + ind.GetComponent<RectTransform>().rect.height * _powerList.Count, initPosition.z);
            ind.GetComponent<PowerupIndicator>().Create(powerup);
            _powerList.Add(powerup);
        }

        /// <summary>
        /// Causes an activated powerup to "blink" repeatedly.
        /// </summary>
        /// <param name="p">The relevant <see cref="PowerupCodes"/> to flash.</param>
        /// <remarks>Make sure to call this function with <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"></see>.</remarks>
        internal IEnumerator ShowPowerupExpiring(PowerupCodes p)
        {
            if (!_powerList.Contains(p))
                yield break;

            var indicators = GameObject.FindGameObjectsWithTag("Indicator");
    
            foreach (var indicator in indicators)
            {
                var powerupIndicator = indicator.GetComponent<PowerupIndicator>();

                if (powerupIndicator == null || powerupIndicator.PowerCode != p) continue;
                var image = indicator.GetComponent<Image>();

                // Blink the indicator 6 times, i.e., for 3 seconds in total
                const int blinkCount = 12;
                const float halfBlinkDuration = 0.25f;

                for (var i = 0; i < blinkCount; i++)
                {
                    // alternate between 0.5 and 1 opacity
                    SetImageOpacity(image, i % 2 == 0 ? 0.5f : 1f);
                    _soundHelper.PlaySound("Sound/SFX/zipclick");
                    yield return new WaitForSeconds(halfBlinkDuration);
                }
                break; // Exit loop after processing the found indicator
            }
        }

        /// <summary>
        /// Sets the alpha opacity of the given image.
        /// </summary>
        /// <param name="image">The image whose opacity needs to be set.</param>
        /// <param name="opacity">The desired opacity level.</param>
        private void SetImageOpacity(Image image, float opacity)
        {
            if (image == null) return;
            image.color = new(1f, 1f, 1f, opacity);
        }

        /// <summary>
        /// Removes an active powerup's indicator from the sidebar.
        /// </summary>
        /// <param name="powerup">The relevant <see cref="PowerupCodes"/> to remove.</param>
        internal void RemovePowerupFromSidebar(PowerupCodes powerup)
        {
            if (!_powerList.Contains(powerup)) return;
    
            var indicators = GameObject.FindGameObjectsWithTag("Indicator");
            var indicatorRemoved = false;
    
            foreach (var indicator in indicators)
            {
                var powerupIndicator = indicator.GetComponent<PowerupIndicator>();

                if (powerupIndicator == null || powerupIndicator.PowerCode != powerup) continue;
                Destroy(indicator);
                _powerList.Remove(powerup);
                indicatorRemoved = true;
                break;
            }

            if (indicatorRemoved) MoveIndicatorsDown();
        }

        /// <summary>
        /// Removes all active powerup indicators from the sidebar.
        /// </summary>
        internal void RemoveAllPowerups()
        {
            var powerupIndicators = GameObject.FindGameObjectsWithTag("Indicator");
    
            foreach (var indicator in powerupIndicators)
            {
                var powerupIndicatorComponent = indicator.GetComponent<PowerupIndicator>();

                if (!powerupIndicatorComponent || !_powerList.Contains(powerupIndicatorComponent.PowerCode)) continue;
                _powerList.Remove(powerupIndicatorComponent.PowerCode);
                Destroy(indicator);
            }
        }

        /// <summary>
        /// Moves all active indicators to the bottom of the screen to allow showing new powerups at the top.
        /// </summary>
        private void MoveIndicatorsDown()
        {
            var indicators = GameObject.FindGameObjectsWithTag("Indicator");
            
            var yOffset = initPosition.y;
            var blockHeight = indicators.Length > 0 ? indicators[0].GetComponent<RectTransform>().rect.height : 0;

            foreach (var indicator in indicators)
            {
                indicator.transform.localPosition = new(
                    initPosition.x,
                    yOffset,
                    initPosition.z);
                yOffset += blockHeight; // Move y position down by the block height for the next indicator
            }
        }
    }
}
