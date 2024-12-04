using System.Collections.Generic;
using System.Linq;
using Enums;

namespace Powerups
{
    public static class PowerupSpawnWeights
    {
        public static readonly Dictionary<PowerupCodes, double> PowerupWeights = new()
        {
            { PowerupCodes.Pts50, 0.2 },
            { PowerupCodes.Pts100, 0.04 },
            { PowerupCodes.Pts250, 0.03 },
            { PowerupCodes.Pts500, 0.025 },
            { PowerupCodes.SlowBall, 0.05 },
            { PowerupCodes.FastBall, 0.075 },
            { PowerupCodes.TripleBall, 0.10 },
            { PowerupCodes.LifeUp, 0.02 },
            { PowerupCodes.LaserBeam, 0.10 },
            { PowerupCodes.GrowPaddle, 0.10 },
            { PowerupCodes.ShrinkPaddle, 0.10 },
            { PowerupCodes.SafetyNet, 0.05 },
            { PowerupCodes.HalfPoints, 0.10 },
            { PowerupCodes.DoublePoints, 0.10 },
            { PowerupCodes.RedFireBall, 0.05 },
        };
        
        public static PowerupCodes GetRandomWeightedPowerup()
        {
            var random = new System.Random();
            var totalWeight = PowerupWeights.Values.Sum();
            var randomValue = random.NextDouble() * totalWeight;

            var cumulativeWeight = 0.0;
            foreach (var pair in PowerupWeights)
            {
                cumulativeWeight += pair.Value;
                if (randomValue < cumulativeWeight)
                {
                    return pair.Key;
                }
            }

            // Fallback in case nothing matches (should not happen with correctly set weights)
            return PowerupWeights.Keys.Last();
        }
    }
}