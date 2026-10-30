using System.Drawing;
using System.Windows.Forms;
using ShutdownTimer.App.Application;

namespace ShutdownTimer.App.UI;

public sealed class SetupForm : Form
{
    private readonly DurationSelectionModel _durationModel;
    private readonly CountdownController _countdownController;
    private readonly NumericUpDown _durationInput;
    private readonly Label _statusLabel;
    private readonly Button _decrementButton;
    private readonly Button _incrementButton;
    private readonly Button _startButton;
    private readonly Button[] _presetButtons;
    private readonly List<Font> _ownedFonts = [];
    private bool _startInProgress;
    private bool _resourcesDisposed;

    public event EventHandler? ShutdownScheduled;

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
        Font = CreateOwnedFont("Segoe UI", 9F, FontStyle.Regular);
        MinimumSize = Size;
        AccessibleName = "win-shut shutdown timer setup";

        var heading = new Label
        {
            Name = "headingLabel",
            Location = new Point(24, 18),
            Size = new Size(472, 34),
            Font = CreateOwnedFont("Segoe UI", 17F, FontStyle.Bold),
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
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };

        _presetButtons = DurationSelectionModel.Presets
            .Select(CreatePresetButton)
            .ToArray();

        foreach (var button in _presetButtons)
        {
            presetPanel.Controls.Add(button);
        }

        var durationLabel = new Label
        {
            Name = "durationLabel",
            Location = new Point(82, 145),
            Size = new Size(160, 28),
            Text = "Duration (minutes):",
            TextAlign = ContentAlignment.MiddleRight
        };

        _durationInput = new NumericUpDown
        {
            Name = "durationInput",
            Location = new Point(254, 142),
            Size = new Size(130, 32),
            Minimum = DurationSelectionModel.MinimumMinutes,
            Maximum = DurationSelectionModel.MaximumMinutes,
            Increment = 1,
            DecimalPlaces = 0,
            ThousandsSeparator = false,
            TextAlign = HorizontalAlignment.Center,
            Font = CreateOwnedFont("Segoe UI", 12F, FontStyle.Regular),
            Value = _durationModel.SelectedMinutes,
            AccessibleName = "Duration in whole minutes"
        };
        _durationInput.ValueChanged += (_, _) =>
        {
            _durationModel.SetMinutes(decimal.ToInt32(_durationInput.Value));
            UpdateDurationControls();
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
        _startButton.Font = CreateOwnedFont("Segoe UI", 10F, FontStyle.Bold);
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
            durationLabel,
            _durationInput,
            _decrementButton,
            _incrementButton,
            forcedShutdownWarning,
            countdownWarning,
            _startButton,
            _statusLabel
        ]);

        UpdateDurationControls();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_resourcesDisposed)
        {
            _resourcesDisposed = true;
            try
            {
                base.Dispose(disposing);
            }
            finally
            {
                foreach (var font in _ownedFonts)
                {
                    font.Dispose();
                }

                _ownedFonts.Clear();
            }

            return;
        }

        base.Dispose(disposing);
    }

    private Font CreateOwnedFont(string familyName, float size, FontStyle style)
    {
        var font = new Font(familyName, size, style);
        _ownedFonts.Add(font);
        return font;
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
        var selectedMinutes = (decimal)_durationModel.SelectedMinutes;
        if (_durationInput.Value != selectedMinutes)
        {
            _durationInput.Value = selectedMinutes;
        }

        foreach (var button in _presetButtons)
        {
            var preset = (int)button.Tag!;
            button.Enabled = CanEditDuration;
            button.UseVisualStyleBackColor = false;
            button.BackColor = preset == _durationModel.SelectedMinutes
                ? Color.LightBlue
                : SystemColors.Control;
        }

        _durationInput.Enabled = CanEditDuration;
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
        UpdateCountdownStatus();
        ShutdownScheduled?.Invoke(this, EventArgs.Empty);
    }

    internal void UpdateCountdownStatus()
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
