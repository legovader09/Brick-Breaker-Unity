namespace Assets.Scripts
{
    class EndlessLevelGenerator
    {
        int maxHeight = 18; //sets the amount of layers of bricks that should be generated.
        internal string Generate()
        {
            string str = "";
            for (int i = 0; i < maxHeight; i++)
            {
                for (int j = 0; j < 14; j++) // the limit of 14 is set here as this is the total amount of bricks that are able to fit along the x-axis of the screen.
                {
                    str += Globals.RNG.Next(0, 2); //simple random generation, 0 means empty space, 1 means space will be occupied by brick.
                }
                if (i != maxHeight) str += ","; //only add comma (new line for map interpreter) when not the last line.
            }

            return str;
        }
    }
}
