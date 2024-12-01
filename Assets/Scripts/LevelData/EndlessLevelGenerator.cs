namespace LevelData
{
    internal abstract class EndlessLevelGenerator
    {
        private const int MaxHeight = 18; //sets the amount of layers of bricks that should be generated.

        internal static string Generate()
        {
            var str = "";
            for (var i = 0; i < MaxHeight; i++)
            {
                for (var j = 0; j < 14; j++) // the limit of 14 is set here as this is the total amount of bricks that are able to fit along the x-axis of the screen.
                {
                    str += Globals.Random.Next(0, 2); //simple random generation, 0 means empty space, 1 means space will be occupied by brick.
                }

                str += ",";
            }

            return str;
        }
    }
}
