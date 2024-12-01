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
            BrickColours temp;
            switch (colour) //due to the nature of the brick ID numbers, I could not simply iterate the colour codes, meaning a long switch case was the next best thing here.
            {
                case BrickColours.Purple:
                    temp = BrickColours.DarkBlue;
                    break;
                case BrickColours.DarkBlue:
                    temp = BrickColours.LightBlue;
                    break;
                case BrickColours.LightBlue:
                    temp = BrickColours.DarkGreen;
                    break;
                case BrickColours.DarkGreen:
                    temp = BrickColours.LightGreen;
                    break;
                case BrickColours.LightGreen:
                    temp = BrickColours.Yellow;
                    break;
                case BrickColours.Yellow:
                    temp = BrickColours.Orange;
                    break;
                case BrickColours.Orange:
                    temp = BrickColours.Red;
                    break;
                case BrickColours.Red:
                    temp = BrickColours.Brown;
                    break;
                case BrickColours.Brown:
                    temp = BrickColours.Grey;
                    break;
                default:
                    temp = (BrickColours)(new System.Random().Next(5) * 2 + 1); //get a random brick colour, the random is doubled and the + 1 is to make sure the default texture is picked, not the damaged one.
                    break;
            }
            return temp;
        }
    }
}
