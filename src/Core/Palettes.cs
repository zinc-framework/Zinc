namespace Zinc;

//https://lospec.com/palette-list
public static class Palettes
{
    public static List<uint> ActivePalette;
    public static void SetActivePalette(List<uint> palette)
    {
        ActivePalette = palette;
    }
    public static uint GetRandomColor()
    {
        return ActivePalette[Quick.Random.Next(ActivePalette.Count)];
    }
    //RGBA
    //could source gen this instead to sg_color from included lospec hex files?
    public static readonly List<uint> ENDESGA = new()
    {
        0xbe4a2fFF,
        0xd77643FF,
        0xead4aaFF,
        0xe4a672FF,
        0xb86f50FF,
        0x733e39FF,
        0x3e2731FF,
        0xa22633FF,
        0xe43b44FF,
        0xf77622FF,
        0xfeae34FF,
        0xfee761FF,
        0x63c74dFF,
        0x3e8948FF,
        0x265c42FF,
        0x193c3eFF,
        0x124e89FF,
        0x0099dbFF,
        0x2ce8f5FF,
        0xffffffFF,
        0xc0cbdcFF,
        0x8b9bb4FF,
        0x5a6988FF,
        0x3a4466FF,
        0x262b44FF,
        0x181425FF,
        0xff0044FF,
        0x68386cFF,
        0xb55088FF,
        0xf6757aFF,
        0xe8b796FF,
        0xc28569FF
    };

    public static readonly List<uint> ONE_BIT_MONITOR_GLOW = new()
    {
        0x222323FF,
        0xf0f6f0FF
    };

}