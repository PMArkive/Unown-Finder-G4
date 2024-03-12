using System;
using System.Text;

namespace Unown_Finder_G4;

public sealed class SearchSettings
{
    public RoomUnlock Unlocks { get; init; }

    public uint InitialSeed { get; init; }
    public uint StartingFrame { get; init; }
    public uint MaxFrames { get; init; }
    public ushort TID { get; init; }
    public ushort SID { get; init; }
    public char Form { get; init; }
    public int Nature { get; init; }
    public IVRange[] IVRanges { get; init; } = new IVRange[6];

    public bool FilterNature { get; init; }
    public bool FilterForm { get; init; }
    public bool FilterShiny { get; init; }

    public ShinyState ShinyState { get; init; }
    public bool Synchronize { get; init; }
    public int SynchronizeNature { get; init; }

    public bool IsFilterMatch(uint pid, ReadOnlySpan<uint> ivs, char main, char qe, uint shinyXor, StringBuilder radio)
    {
        if (FilterForm)
        {
            if (Form != main && Form != qe)
            {
                if (Form >= 26)
                    return false; // not a radio form

                var seek = (char)('A' + Form);
                foreach (var chunk in radio.GetChunks())
                {
                    if (chunk.Span.Contains(seek))
                        return true;
                }
                return false;
            }
        }

        if (FilterShiny)
        {
            if (ShinyState == ShinyState.Shiny && shinyXor >= 8)
                return false;
            if (ShinyState == ShinyState.Star && shinyXor is not (< 8 and not 0))
                return false;
            if (ShinyState == ShinyState.Square && shinyXor != 0)
                return false;
            if (ShinyState == ShinyState.NotShiny && shinyXor < 8)
                return false;
        }

        if (FilterNature)
        {
            if (Nature != pid % 25)
                return false;
        }

        var compare = IVRanges;
        for (int i = 0; i < 6; i++)
        {
            if (ivs[i] < compare[i].Min)
                return false;
            if (ivs[i] > compare[i].Max)
                return false;
        }
        return true;
    }

    public uint GetShinyXor(ushort low, ushort high)
    {
        uint result = TID;
        result ^= SID;
        result ^= low;
        result ^= high;
        return result;
    }
}
