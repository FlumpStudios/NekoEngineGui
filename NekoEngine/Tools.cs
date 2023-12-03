namespace NekoEngine
{
    internal static class Tools
    {
        internal static ushort SFG_TD(ushort floorH, ushort ceilH, ushort floorT, ushort ceilT)
        {
            return ((ushort)((floorH & 0x001f) | ((floorT & 0x0007) << 5) | ((ceilH & 0x001f) << 8) | ((ceilT & 0x0007) << 13)));
        }
    }
}
