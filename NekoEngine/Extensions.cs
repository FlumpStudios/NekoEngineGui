namespace NekoEngine
{
    internal static class Extensions
    {
        internal static bool RgbEquels(this Color col1, Color col2) => 
            col1.R == col2.R && col1.G == col2.G && col1.B == col2.B;

        internal static byte GetIndexFromColour(this Color colour, byte baseUndex)
        {
            // TODO: Actually do something
            return baseUndex;
        }
    }
}
