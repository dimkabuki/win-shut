using System.Drawing;
using System.Windows.Forms;
using ShutdownTimer.App.Application;

namespace ShutdownTimer.App.UI;

public sealed class SetupForm : Form
{
    private readonly DurationSelectionModel _durationModel;
    private readonly CountdownController _countdownController;
    private readonly Label _selectedDurationLabel;
    private readonly Label _statusLabel;
    private readonly Button _decrementButton;
    private readonly Button _incrementButton;
    private readonly Button _startButton;
    private readonly Button[] _presetButtons;
    private readonly System.Windows.Forms.Timer _countdownTimer;
    private bool _startInProgress;

    public SetupForm(DurationSelectionModel durationModel, CountdownController countdownController)
    {
        _durationModel = durationModel ?? throw new ArgumentNullException(nameof(durationModel));
        _countdownController = countdownController ?? throw new ArgumentNullException(nameof(countdownController));

        Text = "win-shut";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = true;
        ShowInTaskbar = true;
        ClientSize = new Size(520, 516);
        Font = new Font("Segoe UI", 9F, FontStyle.Regular);
        MinimumSize = Size;
        AccessibleName = "win-shut shutdown timer setup";

        var heading = new Label
        {
            Name = "headingLabel",
            Location = new Point(24, 18),
            Size = new Size(472, 34),
            Font = new Font(Font.FontFamily, 17F, FontStyle.Bold),
            Text = "Schedule shutdown",
            TextAlign = ContentAlignment.MiddleLeft
        };

        var subtitle = new Label
        {
            Name = "subtitleLabel",
            Location = new Point(24, 55),
            Size = new Size(472, 26),
            Text = "Choose how long this PC should remain on.",
            TextAlign = ContentAlignment.MiddleLeft
        };

        var presetPanel = new FlowLayoutPanel
        {
            Name = "presetPanel",
            Location = new Point(24, 91),
            Size = new Size(472, 42),
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = System.Windows.Forms.Padding.Empty,
            Padding = System.Windows.Forms.Padding.Empty
        };

        _presetButtons = DurationSelectionModel.Presets
            .Select(CreatePresetButton)
            .ToArray();

        foreach (var button in _presetButtons)
        {
            presetPanel.Controls.Add(button);
        }

        _selectedDurationLabel = new Label
        {
            Name = "selectedDurationLabel",
            Location = new Point(24, 139),
            Size = new Size(472, 38),
            Font = new Font(Font.FontFamily, 21F, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleCenter
        };

        _decrementButton = CreateButton("-5 min", "decrementButton", new Size(126, 38));
        _decrementButton.Location = new Point(82, 182);
        _decrementButton.Click += (_, _) =>
        {
            _durationModel.Decrement();
            UpdateDurationControls();
        };

        _incrementButton = CreateButton("+5 min", "incrementButton", new Size(126, 38));
        _incrementButton.Location = new Point(312, 182);
        _incrementButton.Click += (_, _) =>
        {
            _durationModel.Increment();
            UpdateDurationControls();
        };

        var forcedShutdownWarning = CreateWarningLabel(
            "forcedShutdownWarning",
            new Point(24, 235),
            new Size(472, 52),
            "Warning: Windows will force-close applications without save prompts. Unsaved work can be lost.");

        var countdownWarning = CreateWarningLabel(
            "countdownWarning",
            new Point(24, 294),
            new Size(472, 52),
            "The countdown is an estimate only. The app cannot detect shutdown cancellation outside this app.");

        _startButton = CreateButton("Start shutdown timer", "startButton", new Size(250, 44));
        _startButton.Location = new Point(135, 361);
        _startButton.Font = new Font(Font.FontFamily, 10F, FontStyle.Bold);
        _startButton.Click += OnStartClicked;

        _statusLabel = new Label
        {
            Name = "statusLabel",
            Location = new Point(24, 416),
            Size = new Size(472, 78),
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(9),
            TextAlign = ContentAlignment.MiddleLeft,
            AutoEllipsis = true,
            AccessibleName = "Shutdown scheduling status",
            Text = "Select a duration, then press Start."
        };

        Controls.AddRange(
        [
            heading,
            subtitle,
            presetPanel,
            _selectedDurationLabel,
            _decrementButton,
            _incrementButton,
            forcedShutdownWarning,
            countdownWarning,
            _startButton,
            _statusLabel
        ]);

        _countdownTimer = new System.Windows.Forms.Timer
        {
            Interval = 1000
        };
        _countdownTimer.Tick += OnCountdownTimerTick;
        FormClosed += (_, _) => _countdownTimer.Dispose();

        UpdateDurationControls();
    }

    private Button CreatePresetButton(int minutes)
    {
        var button = CreateButton($"{minutes} min", $"preset{minutes}Button", new Size(140, 38));
        button.Margin = new Padding(0, 0, 12, 0);
        button.Tag = minutes;
        button.Click += (_, _) =>
        {
            _durationModel.SelectPreset(minutes);
            UpdateDurationControls();
        };

        return button;
    }

    private static Button CreateButton(string text, string name, Size size)
    {
        return new Button
        {
            Name = name,
            Text = text,
            Size = size,
            FlatStyle = FlatStyle.Flat,
            UseVisualStyleBackColor = false,
            AccessibleName = text
        };
    }

    private static Label CreateWarningLabel(string name, Point location, Size size, string text)
    {
        return new Label
        {
            Name = name,
            Location = location,
            Size = size,
            Padding = new Padding(9, 5, 9, 5),
            BackColor = SystemColors.Info,
            ForeColor = SystemColors.InfoText,
            BorderStyle = BorderStyle.FixedSingle,
            TextAlign = ContentAlignment.MiddleLeft,
            Text = text,
            AccessibleName = text
        };
    }

    private void UpdateDurationControls()
    {
        _selectedDurationLabel.Text = $"{_durationModel.SelectedMinutes} minutes";

        foreach (var button in _presetButtons)
        {
            var preset = (int)button.Tag!;
            button.Enabled = CanEditDuration;
            button.UseVisualStyleBackColor = false;
            button.BackColor = preset == _durationModel.SelectedMinutes
                ? Color.LightBlue
                : SystemColors.Control;
        }

        _decrementButton.Enabled = CanEditDuration && _durationModel.CanDecrement;
        _incrementButton.Enabled = CanEditDuration && _durationModel.CanIncrement;
        _startButton.Enabled = !_startInProgress && _countdownController.State == CountdownControllerState.Idle;
    }

    private bool CanEditDuration =>
        !_startInProgress && _countdownController.State == CountdownControllerState.Idle;

    private async void OnStartClicked(object? sender, EventArgs e)
    {
        if (_startInProgress || _countdownController.State != CountdownControllerState.Idle)
        {
            return;
        }

        var selectedMinutes = _durationModel.SelectedMinutes;
        _startInProgress = true;
        SetStatus("Requesting the Windows shutdown command...", SystemColors.ControlText);
        UpdateDurationControls();

        var scheduled = await _countdownController.StartAsync(selectedMinutes);
        _startInProgress = false;

        if (!scheduled)
        {
            var details = _countdownController.ErrorMessage;
            var message = "Windows could not schedule the shutdown. No countdown was started. "
                + "Check whether Windows allows shutdown, then try again.";

            if (!string.IsNullOrWhiteSpace(details))
            {
                message += Environment.NewLine + details;
            }

            SetStatus(message, Color.DarkRed);
            UpdateDurationControls();
            return;
        }

        UpdateDurationControls();
        _countdownTimer.Start();
        UpdateCountdownStatus();
    }

    private void OnCountdownTimerTick(object? sender, EventArgs e)
    {
        _countdownController.Refresh();
        UpdateCountdownStatus();

        if (_countdownController.State == CountdownControllerState.ZeroReached)
        {
            _countdownTimer.Stop();
        }
    }

    private void UpdateCountdownStatus()
    {
        var remaining = CountdownFormatter.Format(_countdownController.Remaining);

        if (_countdownController.State == CountdownControllerState.ZeroReached)
        {
            SetStatus(
                "Local estimate: 00:00. Windows shutdown is not confirmed.",
                SystemColors.ControlText);
            return;
        }

        SetStatus(
            $"Shutdown request accepted. Approximate time remaining: {remaining}.",
            Color.DarkGreen);
    }

    private void SetStatus(string message, Color color)
    {
        _statusLabel.Text = message;
        _statusLabel.ForeColor = color;
    }
}
