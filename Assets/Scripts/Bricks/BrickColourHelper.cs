using Enums;
using UnityEngine;

namespace Bricks
{
    public static class BrickColourHelper
    {
        public static Sprite GetBrickColour(BrickColours colour)
        {
            var tempCol = (int)colour;
            var colourCode = $"{(tempCol < 10 ? "0" : "")}"; // add 0 on the front if the number is less than 10
            colourCode += tempCol;
            return Resources.Load<Sprite>($"Bricks/{colourCode}-Breakout-Tiles");
        }

        internal static BrickColours GetNextColour(BrickColours colour)
        {
            var temp = colour switch
            {
                BrickColours.Purple => BrickColours.DarkBlue,
                BrickColours.DarkBlue => BrickColours.LightBlue,
                BrickColours.LightBlue => BrickColours.DarkGreen,
                BrickColours.DarkGreen => BrickColours.LightGreen,
                BrickColours.LightGreen => BrickColours.Yellow,
                BrickColours.Yellow => BrickColours.Orange,
                BrickColours.Orange => BrickColours.Red,
                BrickColours.Red => BrickColours.Brown,
                BrickColours.Brown => BrickColours.Grey,
                _ => (BrickColours)(new System.Random().Next(5) * 2 + 1)
            };
            return temp;
        }
    }
}
