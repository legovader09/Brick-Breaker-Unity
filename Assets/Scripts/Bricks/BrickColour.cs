using UnityEngine;

namespace Bricks
{
    public enum BrickColours //Colour enum, the numbers correspend to the texture IDs in the resources folder.
    {
        Purple = 5,
        DarkBlue = 1,
        LightBlue = 11,
        DarkGreen = 15,
        LightGreen = 3,
        Yellow = 13,
        Orange = 9,
        Red = 7,
        Brown = 19,
        Grey = 17
    }

    public static class BrickColour
    {
        public static Sprite GetBrickColour(BrickColours colour)
        {
            var tempCol = (int)colour;
            var colourCode = $"{(tempCol < 10 ? "0" : "")}"; //simple check to see if ID is below 10, if so add 0 at the front of a string.
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
