using System.Drawing;
using System.Windows.Forms;
using ShutdownTimer.App.Application;

namespace ShutdownTimer.App.UI;

public sealed class TrayController : ApplicationContext
{
    private readonly SetupForm _setupForm;
    private readonly CountdownOverlayForm _overlayForm;
    private readonly CountdownController _countdownController;
    private readonly ContextMenuStrip _trayMenu;
    private readonly NotifyIcon _notifyIcon;
    private readonly System.Windows.Forms.Timer _countdownTimer;
    private readonly ToolStripMenuItem _showCountdownItem;
    private bool _isExiting;
    private bool _cleanupStarted;

    public TrayController(
        SetupForm setupForm,
        CountdownOverlayForm overlayForm,
        CountdownController countdownController)
    {
        _setupForm = setupForm ?? throw new ArgumentNullException(nameof(setupForm));
        _overlayForm = overlayForm ?? throw new ArgumentNullException(nameof(overlayForm));
        _countdownController = countdownController ?? throw new ArgumentNullException(nameof(countdownController));

        _showCountdownItem = new ToolStripMenuItem("Show countdown", null, (_, _) => ShowCountdown())
        {
            Enabled = false
        };

        _trayMenu = new ContextMenuStrip();
        _trayMenu.Items.Add(new ToolStripMenuItem("Show setup", null, (_, _) => ShowSetup()));
        _trayMenu.Items.Add(_showCountdownItem);
        _trayMenu.Items.Add(new ToolStripSeparator());
        _trayMenu.Items.Add(new ToolStripMenuItem("Exit application", null, (_, _) => ExitApplication()));

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "win-shut - approximate countdown, not Windows status",
            ContextMenuStrip = _trayMenu,
            Visible = true
        };
        _notifyIcon.DoubleClick += OnTrayIconDoubleClick;

        _countdownTimer = new System.Windows.Forms.Timer
        {
            Interval = 250
        };
        _countdownTimer.Tick += OnCountdownTimerTick;

        _setupForm.ShutdownScheduled += OnShutdownScheduled;
        _setupForm.FormClosing += OnFormClosing;
        _overlayForm.FormClosing += OnFormClosing;
        _setupForm.Show();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Cleanup(closeForms: true);
        }

        base.Dispose(disposing);
    }

    private void OnShutdownScheduled(object? sender, EventArgs e)
    {
        if (_isExiting)
        {
            return;
        }

        _showCountdownItem.Enabled = true;
        _setupForm.Hide();
        UpdateCountdownPresentation();
        _overlayForm.Show();
        _overlayForm.Activate();
        _countdownTimer.Start();
    }

    private void OnCountdownTimerTick(object? sender, EventArgs e)
    {
        UpdateCountdownPresentation();

        if (_countdownController.State == CountdownControllerState.ZeroReached)
        {
            _countdownTimer.Stop();
        }
    }

    private void UpdateCountdownPresentation()
    {
        _countdownController.Refresh();
        _setupForm.UpdateCountdownStatus();
        _overlayForm.UpdateCountdownStatus(
            _countdownController.Remaining,
            _countdownController.State == CountdownControllerState.ZeroReached);
    }

    private void ShowSetup()
    {
        if (_isExiting)
        {
            return;
        }

        if (_setupForm.WindowState == FormWindowState.Minimized)
        {
            _setupForm.WindowState = FormWindowState.Normal;
        }

        _setupForm.Show();
        _setupForm.BringToFront();
        _setupForm.Activate();
    }

    private void ShowCountdown()
    {
        if (_isExiting || _countdownController.State == CountdownControllerState.Idle)
        {
            return;
        }

        UpdateCountdownPresentation();
        _overlayForm.Show();
        _overlayForm.BringToFront();
        _overlayForm.Activate();
    }

    private void OnTrayIconDoubleClick(object? sender, EventArgs e)
    {
        if (_countdownController.State == CountdownControllerState.Idle)
        {
            ShowSetup();
            return;
        }

        ShowCountdown();
    }

    private void OnFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_isExiting)
        {
            return;
        }

        if (e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            if (sender is Form form)
            {
                form.Hide();
            }

            return;
        }

        // Do not cancel or delay system/session shutdown. Release UI resources
        // and let Windows close the remaining forms normally.
        Cleanup(closeForms: false);
        ExitThread();
    }

    private void ExitApplication()
    {
        if (_isExiting)
        {
            return;
        }

        Cleanup(closeForms: true);
        ExitThread();
    }

    private void Cleanup(bool closeForms)
    {
        if (_cleanupStarted)
        {
            return;
        }

        _cleanupStarted = true;
        _isExiting = true;

        _setupForm.ShutdownScheduled -= OnShutdownScheduled;
        _setupForm.FormClosing -= OnFormClosing;
        _overlayForm.FormClosing -= OnFormClosing;
        _notifyIcon.DoubleClick -= OnTrayIconDoubleClick;
        _countdownTimer.Tick -= OnCountdownTimerTick;

        _countdownTimer.Stop();
        _countdownTimer.Dispose();

        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip = null;
        _notifyIcon.Dispose();
        _trayMenu.Dispose();

        if (closeForms)
        {
            CloseAndDispose(_setupForm);
            CloseAndDispose(_overlayForm);
        }
    }

    private static void CloseAndDispose(Form form)
    {
        if (form.IsDisposed)
        {
            return;
        }

        try
        {
            form.Close();
        }
        finally
        {
            if (!form.IsDisposed)
            {
                form.Dispose();
            }
        }
    }
}
