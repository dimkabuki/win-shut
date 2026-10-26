using System.Drawing;
using System.Windows.Forms;

namespace ShutdownTimer.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var form = new Form
        {
            Text = "win-shut - bootstrap only",
            StartPosition = FormStartPosition.CenterScreen,
            ClientSize = new Size(480, 160)
        };

        form.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = "Application scaffold only. Timer UI is not implemented yet.",
            TextAlign = ContentAlignment.MiddleCenter
        });

        System.Windows.Forms.Application.Run(form);
    }
}
