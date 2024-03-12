namespace Unown_Finder_G4;

public record struct PokeRNG(uint Seed)
{
    public uint Next() => Seed = (Seed * 0x41c64e6d) + 0x6073;
    public ushort Next16() => (ushort)(Next() >> 16);

    /// <summary>
    /// Advances the RNG seed to the next state value a specified amount of times.
    /// </summary>
    /// <param name="frames">Amount of times to advance.</param>
    /// <returns>Seed advanced the specified amount of times.</returns>
    public void Advance(int frames)
    {
        for (int i = 0; i < frames; i++)
            Next();
    }

    private void Prev2() => Seed = (Seed * 0xDC6C95D9) + 0x4D3CB126;

    public int CalculateSlot(bool isSynchronize, int syncNature)
    {
        // Method K -- seek a PID with the nature we want.
        // Specialized for Unown only; we don't need to consider cute charm or other leads / random levels.
        var esv = Next16() % 100;
        var slot = GetRegular((byte)esv);
        int nature;
        if (isSynchronize && Next16() % 2 == 0)
            nature = syncNature;
        else
            nature = Next16() % 25;

        while (true)
        {
            var rand1 = Next16();
            var rand2 = Next16();
            var pid = (rand2 << 16) | rand1;
            if (pid % 25 != nature)
                continue;

            // Unwind 2 steps so future calls to Next() will return the same PID.
            Prev2();
            return slot;
        }
    }

    private static byte GetRegular(uint roll) => roll switch
    {
        < 20 => 0, // 00,19 (20%)
        < 40 => 1, // 20,39 (20%)
        < 50 => 2, // 40,49 (10%)
        < 60 => 3, // 50,59 (10%)
        < 70 => 4, // 60,69 (10%)
        < 80 => 5, // 70,79 (10%)
        < 85 => 6, // 80,84 ( 5%)
        < 90 => 7, // 85,89 ( 5%)
        < 94 => 8, // 90,93 ( 4%)
        < 98 => 9, // 94,97 ( 4%)
        < 99 => 10,// 98,98 ( 1%)
        _    => 11,//    99 ( 1%)
    };
}
