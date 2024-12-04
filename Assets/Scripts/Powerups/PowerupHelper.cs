using System;
using System.Collections;
using System.Collections.Generic;
using Enums;
using GUI;
using UnityEngine;

namespace Powerups
{
    public class PowerupHelper : MonoBehaviour
    {
        private const float WarningThreshold = 3f;
        private readonly Dictionary<PowerupCodes, Coroutine> _activePowerupCoroutines = new();
        private readonly Dictionary<PowerupCodes, float> _remainingDurations = new();
        private readonly Dictionary<PowerupCodes, Coroutine> _warningCoroutines = new();
        private PowerupCodes _powerupId;
        private GUIHelper _guiHelper;

        private void Awake()
        {
            _guiHelper = GameObject.Find("EventSystem").GetComponent<GUIHelper>();
        }

        /// <summary>
        /// Activates a power-up with a specified duration and custom start and end actions.
        /// </summary>
        /// <param name="id">The ID of the power-up.</param>
        /// <param name="duration">The duration for the power-up effect.</param>
        /// <param name="onStartAction">Action to execute at the start of the power-up.</param>
        /// <param name="onEndAction">Action to execute when the power-up ends.</param>
        /// <param name="onExpiringAction">Action to execute when the warning threshold has been reached.</param>
        public void ActivatePowerup(PowerupCodes id, float duration, Action onStartAction, Action onEndAction, Action onExpiringAction = null)
        {
            _guiHelper.AddPowerupToSidebar(id);
            if (_remainingDurations.TryAdd(id, 0)) onStartAction?.Invoke();
            _remainingDurations[id] += duration;

            if (_warningCoroutines.TryGetValue(id, out var existingWarningCoroutine))
            {
                StopCoroutine(existingWarningCoroutine);
                _warningCoroutines.Remove(id);
            }

            if (_activePowerupCoroutines.ContainsKey(id)) return;
            var coroutine = StartCoroutine(PowerupRoutine(id, onEndAction, onExpiringAction));
            _activePowerupCoroutines[id] = coroutine;
        }
        
        /// <summary>
        /// Removes a powerup from the tracker and immediately stops any ongoing coroutines for that powerup.
        /// </summary>
        /// <param name="id">The ID of the power-up.</param>
        public void RemovePowerup(PowerupCodes id)
        {
            if (_activePowerupCoroutines.TryGetValue(id, out var coroutine))
            {
                StopCoroutine(coroutine);
                _activePowerupCoroutines.Remove(id);
                _guiHelper.RemovePowerupFromSidebar(id);
            }

            if (_remainingDurations.ContainsKey(id)) _remainingDurations.Remove(id);
        }
        
        private IEnumerator PowerupRoutine(PowerupCodes id, Action onEndAction, Action onExpiringAction)
        {
            while (_remainingDurations[id] > 0)
            {
                if (_remainingDurations[id] <= WarningThreshold)
                {
                    if (!_warningCoroutines.ContainsKey(id))
                    {
                        onExpiringAction?.Invoke();
                        var warningCoroutine = StartCoroutine(_guiHelper.ShowPowerupExpiring(id));
                        _warningCoroutines[id] = warningCoroutine;
                    }
                }

                _remainingDurations[id] -= Time.deltaTime;
                yield return null;
            }

            onEndAction?.Invoke();
            _guiHelper.RemovePowerupFromSidebar(id);

            _activePowerupCoroutines.Remove(id);
            _remainingDurations.Remove(id);
            if (_warningCoroutines.ContainsKey(id)) _warningCoroutines.Remove(id);
        }
    }
}
