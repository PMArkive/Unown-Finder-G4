using System;
using System.Collections.Generic;

namespace Unown_Finder_G4;

public static class Util
{
    public static readonly string[] NatureNames =
    [
        "Hardy", "Lonely", "Brave", "Adamant", "Naughty",
        "Bold", "Docile", "Relaxed", "Impish", "Lax",
        "Timid", "Hasty", "Serious", "Jolly", "Naive",
        "Modest", "Mild", "Quiet", "Bashful", "Rash",
        "Calm", "Gentle", "Sassy", "Careful", "Quirky",
    ];

    public static readonly IReadOnlyList<string> HiddenPowerTypes =
    [
        "Fighting", "Flying", "Poison",
        "Ground", "Rock", "Bug",
        "Ghost", "Steel", "Fire",
        "Water", "Grass", "Electric",
        "Psychic", "Ice", "Dragon",
        "Dark",
    ];

    /// <summary>
    /// Unpacks the random IV result into a speed-last order.
    /// </summary>
    public static void GetIVs(uint iv1, uint iv2, Span<uint> result)
    {
        uint hp = iv1 & 0x1f;
        uint atk = (iv1 >> 5) & 0x1f;
        uint defense = (iv1 >> 10) & 0x1f;
        uint spa = (iv2 >> 5) & 0x1f;
        uint spd = (iv2 >> 10) & 0x1f;
        uint spe = iv2 & 0x1f;
        result[0] = hp;
        result[1] = atk;
        result[2] = defense;
        result[3] = spa;
        result[4] = spd;
        result[5] = spe;
    }

    /// <summary>
    /// Checks for the Hidden Power type; speed is the last index.
    /// </summary>
    public static string GetHPowerType(ReadOnlySpan<uint> ivs)
    {
        uint a = ivs[0] & 1;
        uint b = ivs[1] & 1;
        uint c = ivs[2] & 1;
        uint d = ivs[5] & 1;
        uint e = ivs[3] & 1;
        uint f = ivs[4] & 1;

        return HiddenPowerTypes[(int)(((a + (2 * b) + (4 * c) + (8 * d) + (16 * e) + (32 * f)) * 15) / 63)];
    }

    /// <summary>
    /// Gets the Hidden Power damage; speed is the last index.
    /// </summary>
    public static uint GetHPowerDamage(ReadOnlySpan<uint> ivs)
    {
        uint u = (ivs[0] >> 1) & 1;
        uint v = (ivs[1] >> 1) & 1;
        uint w = (ivs[2] >> 1) & 1;
        uint x = (ivs[5] >> 1) & 1;
        uint y = (ivs[3] >> 1) & 1;
        uint z = (ivs[4] >> 1) & 1;

        return (((u + (2 * v) + (4 * w) + (8 * x) + (16 * y) + (32 * z)) * 40) / 63) + 30;
    }

    public static string GetChatotPitch(uint seed)
    {
        var rand100 = (byte)(((seed & 0x1fff) * 100) >> 13);
        return GetPitchFromRand100(rand100);
    }

    private static string GetPitchFromRand100(byte result) => result switch
    {
        < 20 => $"L {result}",
        < 40 => $"ML {result}",
        < 60 => $"M {result}",
        < 80 => $"MH {result}",
        _ => $"H {result}",
    };

    public static string GetCellCall(uint seed) => (byte)(seed % 3) switch
    {
        0 => "E",
        1 => "K",
        _ => "P",
    };
}
