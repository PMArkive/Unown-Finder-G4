using System;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Unown_Finder_G4;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        Bold = new Font(dataGridView1.Font, FontStyle.Bold);
    }

    private void GenerateButton_Click(object sender, EventArgs e)
    {
        if (!TryGetSettings(out var settings))
            return;

        // Unlocked forms
        var unlocks = settings.Unlocks;
        Span<byte> tmp1 = stackalloc byte[26];
        int unlockedCount = RuinsOfAlph.AddUnlockedForms(tmp1, unlocks.UnlockedAJ, unlocks.UnlockedRV, unlocks.UnlockedKQ, unlocks.UnlockedWZ);
        var unlocked = tmp1[..unlockedCount];

        // Remaining to be seen forms
        ReadOnlySpan<bool> seen =
        [
            ACheck.Checked, BCheck.Checked, CCheck.Checked, DCheck.Checked, ECheck.Checked, FCheck.Checked, GCheck.Checked, HCheck.Checked, ICheck.Checked, JCheck.Checked,
            KCheck.Checked, LCheck.Checked, MCheck.Checked, NCheck.Checked, OCheck.Checked, PCheck.Checked, QCheck.Checked,
            RCheck.Checked, SCheck.Checked, TCheck.Checked, UCheck.Checked, VCheck.Checked,
            WCheck.Checked, XCheck.Checked, YCheck.Checked, ZCheck.Checked,
        ];
        Span<byte> temp = stackalloc byte[26];
        int remainSeenCount = RuinsOfAlph.GetRemainSeen(unlocked, seen, temp);
        ReadOnlySpan<byte> remainSeen = temp[..remainSeenCount];

        Span<uint> ivs = stackalloc uint[6];
        PopulateResults(settings, ivs, unlocked, remainSeen);
    }

    private void PopulateResults(SearchSettings settings, Span<uint> ivs, ReadOnlySpan<byte> unlocked, ReadOnlySpan<byte> remainSeen)
    {
        var radioLetters = new StringBuilder(remainSeen.Length);
        var rng = new PokeRNG(settings.InitialSeed);
        if (settings.StartingFrame > 1)
            rng.Advance((int)settings.StartingFrame - 1);

        dataGridView1.Rows.Clear();
        for (uint frame = settings.StartingFrame; frame < settings.MaxFrames; frame++)
        {
            var rand = rng; // copy
            var singleRand = rng.Next();

            var slot = rand.CalculateSlot(settings.Synchronize, settings.SynchronizeNature);
            ushort low = rand.Next16();
            ushort high = rand.Next16();
            uint pid = (uint)(high << 16) | low;
            ushort iv1 = rand.Next16();
            ushort iv2 = rand.Next16();
            Util.GetIVs(iv1, iv2, ivs);

            rand.Next();
            ushort letterRand = rand.Next16();
            var formAZ = RuinsOfAlph.GetFormAZ(letterRand, unlocked);
            var formQE = RuinsOfAlph.GetFormEQ(letterRand);
            bool isRadioProc = letterRand % 100 < 50;
            letterRand = rand.Next16();

            radioLetters.Clear();
            if (isRadioProc)
                RuinsOfAlph.AppendRadioForRemainSeen(remainSeen, letterRand, radioLetters);
            else
                radioLetters.Append(RuinsOfAlph.GetFormAZ(letterRand, unlocked));

            uint shinyXor = settings.GetShinyXor(low, high);
            if (!settings.IsFilterMatch(pid, ivs, formAZ, formQE, shinyXor, radioLetters))
                continue;

            bool isShiny = shinyXor < 8;
            bool isSquareShiny = shinyXor == 0;
            var nature = pid % 25;
            int resultRow = dataGridView1.Rows.Count;
            dataGridView1.Rows.Add(
                frame,
                Util.GetCellCall(singleRand),
                Util.GetChatotPitch(singleRand),
                pid.ToString("X8"),
                isSquareShiny ? "Square" : isShiny ? "Star" : "-",
                Util.NatureNames[nature],
                ivs[0],
                ivs[1],
                ivs[2],
                ivs[3],
                ivs[4],
                ivs[5],
                formAZ,
                isRadioProc ? "🎵🎵" : "-",
                radioLetters.ToString(),
                formQE,
                Util.GetHPowerType(ivs),
                Util.GetHPowerDamage(ivs));

            // Fancy colors!
            var cells = dataGridView1.Rows[resultRow].Cells;

            const int cellShiny = 4;
            const int cellNature = 5;
            const int cellIVStart = 6;

            if (isShiny)
                cells[cellShiny].Style.BackColor = Color.Yellow;

            // Bold if matches synch nature.
            if (settings.Synchronize && nature == settings.SynchronizeNature)
                Set(cells[cellNature], Color.ForestGreen); 

            if      (ivs[0] == 00) Set(cells[cellIVStart], Color.Red);
            else if (ivs[0] == 31) Set(cells[cellIVStart], Color.Blue);
            if      (ivs[1] == 00) Set(cells[cellIVStart + 1], Color.Red);
            else if (ivs[1] == 31) Set(cells[cellIVStart + 1], Color.Blue);
            if      (ivs[2] == 00) Set(cells[cellIVStart + 2], Color.Red);
            else if (ivs[2] == 31) Set(cells[cellIVStart + 2], Color.Blue);
            if      (ivs[3] == 00) Set(cells[cellIVStart + 3], Color.Red);
            else if (ivs[3] == 31) Set(cells[cellIVStart + 3], Color.Blue);
            if      (ivs[4] == 00) Set(cells[cellIVStart + 4], Color.Red);
            else if (ivs[4] == 31) Set(cells[cellIVStart + 4], Color.Blue);
            if      (ivs[5] == 00) Set(cells[cellIVStart + 5], Color.Red);
            else if (ivs[5] == 31) Set(cells[cellIVStart + 5], Color.Blue);
        }
    }

    private readonly Font Bold;

    private void Set(DataGridViewCell cell, Color color)
    {
        var style = cell.Style;
        style.Font = Bold;
        style.ForeColor = color;
    }

    private bool TryGetSettings([NotNullWhen(true)] out SearchSettings? result)
    {
        result = null;
        if (!uint.TryParse(InitialSeedTB.Text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint initial))
        { WinFormsUtil.Error("Error: Seed has not been entered properly."); return false; }
        if (!AtoJCheck.Checked && !KtoQCheck.Checked && !RtoVCheck.Checked && !WtoZCheck.Checked)
        { WinFormsUtil.Error("Error: You have not unlocked any letters."); return false; }

        uint startingFrame, maxFrames;
        try
        {
            startingFrame = (uint)StartingFrameNUD.Value;
            maxFrames = (uint)AmountofFramesNUD.Value;
        }
        catch
        { WinFormsUtil.Error("Error: Starting Frame and/or Frame Amount have not been entered properly."); return false; }

        ReadOnlySpan<uint> minIVs = [(uint)HPMinNUD.Value, (uint)AtkMinNUD.Value, (uint)DefMinNUD.Value, (uint)SpAMinNUD.Value, (uint)SpDMinNUD.Value, (uint)SpeMinNUD.Value];
        ReadOnlySpan<uint> maxIVs = [(uint)HPMaxNUD.Value, (uint)AtkMaxNUD.Value, (uint)DefMaxNUD.Value, (uint)SpAMaxNUD.Value, (uint)SpDMaxNUD.Value, (uint)SpeMaxNUD.Value];

        char selectedLetter = default;
        if (FormCB.SelectedIndex > 0)
        {
            if (FormCB.Text.Length == 0 || (selectedLetter = FormCB.Text[0]) is not ((>= 'A' and <= 'Z') or '!' or '?'))
            { WinFormsUtil.Error("Error: Chosen Form has not been entered properly."); return false; }
        }

        int nature = Array.IndexOf(Util.NatureNames, NatureCB.Text);
        if (nature < 0 && NatureCB.SelectedIndex != 0)
        { WinFormsUtil.Error("Error: Chosen Nature has not been entered properly."); return false; }

        for (int j = 0; j < 6; j++)
        {
            if (maxIVs[j] >= minIVs[j] && minIVs[j] <= maxIVs[j])
                continue; // valid
            WinFormsUtil.Error("Error: IV Range has not been entered properly.");
            return false;
        }

        result = new SearchSettings
        {
            Unlocks = new RoomUnlock
            {
                UnlockedAJ = AtoJCheck.Checked,
                UnlockedRV = RtoVCheck.Checked,
                UnlockedKQ = KtoQCheck.Checked,
                UnlockedWZ = WtoZCheck.Checked,
            },
            InitialSeed = initial,
            StartingFrame = startingFrame,
            MaxFrames = maxFrames,
            TID = (ushort)TIDNUD.Value,
            SID = (ushort)SIDNUD.Value,
            IVRanges =
            [
                new((uint)HPMinNUD.Value, (uint)HPMaxNUD.Value),
                new((uint)AtkMinNUD.Value, (uint)AtkMaxNUD.Value),
                new((uint)DefMinNUD.Value, (uint)DefMaxNUD.Value),
                new((uint)SpAMinNUD.Value, (uint)SpAMaxNUD.Value),
                new((uint)SpDMinNUD.Value, (uint)SpDMaxNUD.Value),
                new((uint)SpeMinNUD.Value, (uint)SpeMaxNUD.Value),
            ],

            FilterNature = NatureCB.SelectedIndex != 0,
            Nature = nature,

            FilterForm = FormCB.SelectedIndex != 0,
            Form = selectedLetter,

            FilterShiny = ShinyCB.SelectedIndex != 0,
            ShinyState = ShinyCB.SelectedIndex switch
            {
                1 => ShinyState.Star,
                2 => ShinyState.Square,
                3 => ShinyState.Shiny,
                _ => ShinyState.NotShiny,
            },

            Synchronize = comboBox1.SelectedIndex != 0,
            SynchronizeNature = comboBox1.SelectedIndex - 1,
        };
        return true;
    }
}
