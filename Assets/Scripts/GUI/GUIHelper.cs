using System;
using System.Collections;
using System.Collections.Generic;
using EventListeners;
using JetBrains.Annotations;
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
        [UsedImplicitly] public int amountOfIndicators;
        private readonly List<PowerupCodes> _powerList = new();
        private SoundHelper _soundHelper;

        private void Start()
        {
            _soundHelper = GetComponent<SoundHelper>();
        }

        /// <summary>
        /// Adds an active powerup's indicator from the sidebar.
        /// </summary>
        /// <param name="p">The relevant <see cref="PowerupComponent.PowerupCodes"/> to add.</param>
        internal void AddPowerupToSidebar(PowerupCodes p)
        {
            if (_powerList.Contains(p)) return;
            var ind = Instantiate(indicatorPrefab, parent.transform, false);
            ind.transform.localPosition = new(initPosition.x,
                initPosition.y + (ind.GetComponent<RectTransform>().rect.height * _powerList.Count), initPosition.z);
            ind.GetComponent<PowerupIndicator>().Create(p);
            _powerList.Add(p);
        }

        /// <summary>
        /// Cause an activated powerup to "blink" repeatedly.
        /// </summary>
        /// <param name="p">The relevant <see cref="PowerupComponent.PowerupCodes"/> to flash.</param>
        /// <remarks>Make sure to call this function with <see cref="MonoBehaviour.StartCoroutine(IEnumerator)"></see>.</remarks>
        internal IEnumerator ShowPowerupExpiring(PowerupCodes p)
        {
            if (!_powerList.Contains(p)) yield break;
            var g = GameObject.FindGameObjectsWithTag("Indicator");
            foreach (var o in g)
                if (o.GetComponent<PowerupIndicator>().PowerCode == p)
                {
                    var image = o.GetComponent<Image>();
                    // 6 half seconds of flashing. = 3 seconds.
                    for (var i = 0; i < 6; i++)
                    {
                        image.color = new(1f, 1f, 1f, 0.5f); //50% opacity
                        _soundHelper.PlaySound("Sound/SFX/zipclick");
                        yield return new WaitForSeconds(0.25f);
                        
                        image.color = new(1f, 1f, 1f, 1f); //100% opacity
                        _soundHelper.PlaySound("Sound/SFX/zipclick");
                        yield return new WaitForSeconds(0.25f);
                    }
                    break; // gameobject has been found, and only one of a type exists in the list of indicators.
                }
        }

        /// <summary>
        /// Removes an active powerup's indicator from the sidebar.
        /// </summary>
        /// <param name="p">The relevant <see cref="PowerupComponent.PowerupCodes"/> to remove.</param>
        internal void RemovePowerupFromSidebar(PowerupCodes p)
        {
            if (_powerList.Contains(p))
            {
                GameObject[] g = GameObject.FindGameObjectsWithTag("Indicator");
                foreach (GameObject o in g)
                    if (o.GetComponent<PowerupIndicator>().PowerCode == p)
                    {
                        Destroy(o);
                        _powerList.Remove(p);
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
                if (_powerList.Contains((PowerupCodes)i))
                {
                    GameObject[] g = GameObject.FindGameObjectsWithTag("Indicator");
                    foreach (GameObject o in g)
                        if (o.GetComponent<PowerupIndicator>().PowerCode == (PowerupCodes)i)
                        {
                            Destroy(o);
                            _powerList.Remove((PowerupCodes)i);
                        }
                }
            }
        }

        void Update()
        {
            amountOfIndicators = _powerList.Count;
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
                o.transform.localPosition = new(
                    initPosition.x,
                    initPosition.y + (o.GetComponent<RectTransform>().rect.height * i), //move y scale down by 1 unit of block.
                    initPosition.z);
                i++;
            }
        }
    }
}
