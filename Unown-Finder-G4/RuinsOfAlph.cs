using System;
using System.Text;

namespace Unown_Finder_G4;

public static class RuinsOfAlph
{
    public static char GetFormAZ(ushort rand, ReadOnlySpan<byte> available) => GetForm(available[rand % available.Length]);
    private static char GetForm(int value) => (char)('A' + value); // 0-25
    public static char GetFormEQ(ushort letterRand) => (letterRand % 2) == 0 ? '!' : '?'; // 26/27

    public static int AddUnlockedForms(Span<byte> result, bool aj, bool rv, bool kq, bool wz)
    {
        int count = 0;
        if (aj) count = AddForms(result, count, 0);
        if (rv) count = AddForms(result, count, 1);
        if (kq) count = AddForms(result, count, 2);
        if (wz) count = AddForms(result, count, 3);
        return count;
    }

    private static int AddForms(Span<byte> forms, int count, int depth)
    {
        var (start, end) = GetForms(depth);
        for (var i = start; i <= end; i++)
            forms[count++] = i;
        return count;
    }

    private static (byte Start, byte End) GetForms(int index) => index switch
    {
        0 => (00, 09), // A-J = 10
        1 => (17, 21), // R-V = 5
        2 => (10, 16), // K-Q = 7
        3 => (22, 25), // W-Z = 4
        _ => throw new ArgumentOutOfRangeException(nameof(index)),
    };

    public static int GetRemainSeen(ReadOnlySpan<byte> unlocked, ReadOnlySpan<bool> seen, Span<byte> temp)
    {
        int count = 0;
        foreach (var form in unlocked)
        {
            if (!seen[form])
                temp[count++] = form;
        }
        return count;
    }

    public static void AppendRadioForRemainSeen(ReadOnlySpan<byte> remainSeen, ushort letterRand, StringBuilder result)
    {
        Span<byte> copy = stackalloc byte[remainSeen.Length];
        remainSeen.CopyTo(copy);

        while (copy.Length != 0)
        {
            int index = letterRand % copy.Length;
            result.Append(GetForm(copy[index]));

            // Remove the index from the remainSeen list, and shift everything after it down by one.
            copy[(index + 1)..].CopyTo(copy[index..]);
            copy = copy[..^1];
        }
    }
}
