using System.Drawing;
using System.Windows.Forms;
using ShutdownTimer.App.Application;

namespace ShutdownTimer.App.UI;

public sealed class CountdownOverlayForm : Form
{
    private const int WmNcHitTest = 0x84;
    private const int HtClient = 1;
    private const int HtCaption = 2;

    private static readonly Color WarningDimColor = Color.FromArgb(190, 92, 92);
    private static readonly Color WarningBrightColor = Color.FromArgb(255, 138, 138);

    private readonly Font _captionFont = new("Segoe UI", 7.5F, FontStyle.Bold);
    private readonly Font _countdownFont = new("Segoe UI", 18F, FontStyle.Bold);
    private readonly Font _hideFont = new("Segoe UI", 11F, FontStyle.Regular);
    private readonly System.Windows.Forms.Timer _pulseTimer;

    private string _countdownText = "--:--";
    private Color _countdownColor = Color.WhiteSmoke;
    private bool _pulseBright;
    private bool _resourcesDisposed;

    public CountdownOverlayForm()
    {
        Text = "win-shut - approximate countdown";
        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.Manual;
        ShowInTaskbar = false;
        ShowIcon = false;
        TopMost = true;
        BackColor = Color.FromArgb(31, 33, 37);
        ForeColor = Color.WhiteSmoke;
        ClientSize = new Size(276, 92);
        DoubleBuffered = true;

        var workingArea = Screen.PrimaryScreen?.WorkingArea ?? SystemInformation.WorkingArea;
        Location = new Point(workingArea.Right - Width - 24, workingArea.Bottom - Height - 32);

        _pulseTimer = new System.Windows.Forms.Timer
        {
            Interval = 500
        };
        _pulseTimer.Tick += OnPulseTimerTick;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_resourcesDisposed)
        {
            _resourcesDisposed = true;
            _pulseTimer.Stop();
            _pulseTimer.Tick -= OnPulseTimerTick;
            try
            {
                base.Dispose(disposing);
            }
            finally
            {
                _pulseTimer.Dispose();
                _captionFont.Dispose();
                _countdownFont.Dispose();
                _hideFont.Dispose();
            }

            return;
        }

        base.Dispose(disposing);
    }

    public void UpdateCountdownStatus(TimeSpan remaining, bool zeroReached)
    {
        _countdownText = CountdownFormatter.Format(remaining);

        var warningActive = !zeroReached && CountdownVisualState.IsFinalSecondsWarningActive(remaining);
        if (warningActive)
        {
            if (!_pulseTimer.Enabled)
            {
                _pulseBright = false;
                _countdownColor = WarningDimColor;
                _pulseTimer.Start();
            }
        }
        else
        {
            _pulseTimer.Stop();
            _pulseBright = false;
            _countdownColor = ForeColor;
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        TextRenderer.DrawText(
            e.Graphics,
            "APPROXIMATE COUNTDOWN",
            _captionFont,
            new Rectangle(16, 8, ClientSize.Width - 58, 18),
            Color.LightGray,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

        TextRenderer.DrawText(
            e.Graphics,
            "×",
            _hideFont,
            HideButtonBounds,
            Color.Silver,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

        TextRenderer.DrawText(
            e.Graphics,
            _countdownText,
            _countdownFont,
            new Rectangle(14, 27, ClientSize.Width - 24, 55),
            _countdownColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && HideButtonBounds.Contains(e.Location))
        {
            Hide();
            return;
        }

        base.OnMouseDown(e);
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WmNcHitTest)
        {
            base.WndProc(ref m);
            if (m.Result == (IntPtr)HtClient)
            {
                var coordinates = m.LParam.ToInt64();
                var screenPoint = new Point(
                    unchecked((short)coordinates),
                    unchecked((short)(coordinates >> 16)));
                var clientPoint = PointToClient(screenPoint);

                if (!HideButtonBounds.Contains(clientPoint))
                {
                    m.Result = (IntPtr)HtCaption;
                }
            }

            return;
        }

        base.WndProc(ref m);
    }

    private Rectangle HideButtonBounds => new(ClientSize.Width - 34, 4, 28, 24);

    private void OnPulseTimerTick(object? sender, EventArgs e)
    {
        _pulseBright = !_pulseBright;
        _countdownColor = _pulseBright ? WarningBrightColor : WarningDimColor;
        Invalidate();
    }
}
