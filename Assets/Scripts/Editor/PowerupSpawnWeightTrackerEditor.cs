using System.Linq;
using System.Collections.Generic;
using Enums;
using Powerups;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class PowerupSpawnWeightTrackerEditor : EditorWindow
    {
        private Dictionary<PowerupCodes, int> _selectionCounts = new();
        private string _iterationInput = "100";  // Default value for iterations

        [MenuItem("Brick Breaker/Powerup Tracker")]
        public static void ShowWindow()
        {
            GetWindow<PowerupSpawnWeightTrackerEditor>("Powerup Spawn Weight Tracker");
        }

        private void OnGUI()
        {
            GUILayout.Label("Iteration Settings", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Iterations:", GUILayout.Width(100));
            _iterationInput = GUILayout.TextField(_iterationInput, GUILayout.Width(50));
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Run Iteration"))
            {
                if (int.TryParse(_iterationInput, out var iterations))
                {
                    TrackPowerupSelections(iterations);
                }
                else
                {
                    Debug.LogWarning("Please enter a valid number for iterations.");
                }
            }

            GUILayout.Space(20);
            GUILayout.Label("Powerup Selection Results", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (_selectionCounts.Count <= 0) return;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Powerup", GUILayout.Width(100));
            GUILayout.Label("Count", GUILayout.Width(60));
            GUILayout.Label("Spawn Weight", GUILayout.Width(120));
            GUILayout.EndHorizontal();

            GUILayout.Label("----------------------------------------------");

            foreach (var pair in _selectionCounts)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(pair.Key.ToString(), GUILayout.Width(100));
                GUILayout.Label(pair.Value.ToString(), GUILayout.Width(60));
                GUILayout.Label((PowerupSpawnWeights.PowerupWeights[pair.Key] * 100).ToString("F1"), GUILayout.Width(150));
                GUILayout.EndHorizontal();
            }

            GUILayout.Label("----------------------------------------------");
        }

        private void TrackPowerupSelections(int iterations)
        {
            _selectionCounts = GetSimulationResults(iterations).OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
        }

        private static Dictionary<PowerupCodes, int> GetSimulationResults(int iterations)
        {
            var selectionCounts = new Dictionary<PowerupCodes, int>();

            foreach (var key in PowerupSpawnWeights.PowerupWeights.Keys)
            {
                selectionCounts[key] = 0;
            }

            for (var i = 0; i < iterations; i++)
            {
                var selectedPowerup = PowerupSpawnWeights.GetRandomWeightedPowerup();
                selectionCounts[selectedPowerup]++;
            }

            return selectionCounts;
        }
    }
}