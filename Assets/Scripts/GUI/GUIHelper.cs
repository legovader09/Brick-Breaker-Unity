using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enums;
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
        private readonly List<PowerupListItem> _powerList = new();
        private SoundHelper _soundHelper;

        private void Start()
        {
            _soundHelper = GetComponent<SoundHelper>();
        }

        private void LateUpdate()
        {
            MoveIndicatorsDown();
        }

        /// <summary>
        /// Adds an active powerup's indicator to the sidebar.
        /// </summary>
        /// <param name="powerup">The relevant <see cref="PowerupCodes"/> to add.</param>
        internal void AddPowerupToSidebar(PowerupCodes powerup)
        {
            if (CheckIfPowerupExists(powerup)) return;
            var indicator = Instantiate(indicatorPrefab, parent.transform, false);
            indicator.transform.localPosition = new(initPosition.x,
                initPosition.y + indicator.GetComponent<RectTransform>().rect.height * _powerList.Count, initPosition.z);
            indicator.GetComponent<PowerupIndicator>().Create(powerup);
            _powerList.Add(new() { PowerupCode = powerup, PowerupPrefab = indicator });
        }

        /// <summary>
        /// Causes an activated powerup to "blink" repeatedly.
        /// </summary>
        /// <param name="powerup">The relevant <see cref="PowerupCodes"/> to flash.</param>
        /// <remarks>Make sure to call this function with <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"></see>.</remarks>
        internal IEnumerator ShowPowerupExpiring(PowerupCodes powerup)
        {
            if (!CheckIfPowerupExists(powerup)) yield break;

            var indicator = _powerList.First(item => item.PowerupCode == powerup).PowerupPrefab;
            var image = indicator.GetComponent<Image>();

            const int blinkCount = 12;
            const float halfBlinkDuration = 0.25f;

            for (var i = 0; i < blinkCount; i++)
            {
                SetImageOpacity(image, i % 2 == 0 ? 0.5f : 1f);
                _soundHelper.PlaySound("Sound/SFX/zipclick");
                yield return new WaitForSeconds(halfBlinkDuration);
            }
        }

        /// <summary>
        /// Sets the alpha opacity of the given image.
        /// </summary>
        /// <param name="image">The image whose opacity needs to be set.</param>
        /// <param name="opacity">The desired opacity level.</param>
        private void SetImageOpacity(Image image, float opacity)
        {
            if (!image) return;
            image.color = new(1f, 1f, 1f, opacity);
        }

        /// <summary>
        /// Removes an active powerup's indicator from the sidebar.
        /// </summary>
        /// <param name="powerup">The relevant <see cref="PowerupCodes"/> to remove.</param>
        internal void RemovePowerupFromSidebar(PowerupCodes powerup)
        {
            if (!CheckIfPowerupExists(powerup)) return;
            var matchPowerup = _powerList.First(item => item.PowerupCode == powerup);
            Destroy(matchPowerup.PowerupPrefab);
            _powerList.Remove(matchPowerup);
        }
        
        internal bool CheckIfPowerupExists(PowerupCodes powerup) => _powerList.Any(item => item.PowerupCode == powerup);

        /// <summary>
        /// Removes all active powerup indicators from the sidebar.
        /// </summary>
        internal void RemoveAllPowerups()
        {
            foreach (var indicator in _powerList.ToList())
            {
                Destroy(indicator.PowerupPrefab);
                _powerList.Remove(indicator);
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
