using System.Windows.Forms;

namespace Unown_Finder_G4;

public static class WinFormsUtil
{
    public static DialogResult Error(string message)
    {
        System.Media.SystemSounds.Exclamation.Play();
        return MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
