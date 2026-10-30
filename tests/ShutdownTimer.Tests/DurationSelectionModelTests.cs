using ShutdownTimer.App.Application;
using Xunit;

namespace ShutdownTimer.Tests;

public sealed class DurationSelectionModelTests
{
    [Fact]
    public void StartsAtThirtyMinutesWithBothStepsAvailable()
    {
        var model = new DurationSelectionModel();

        Assert.Equal(30, model.SelectedMinutes);
        Assert.True(model.CanIncrement);
        Assert.True(model.CanDecrement);
    }

    [Theory]
    [InlineData(15)]
    [InlineData(30)]
    [InlineData(45)]
    public void SelectingPresetSetsExactValue(int minutes)
    {
        var model = new DurationSelectionModel();
        model.Increment();

        model.SelectPreset(minutes);

        Assert.Equal(minutes, model.SelectedMinutes);
    }

    [Theory]
    [InlineData(6)]
    [InlineData(17)]
    [InlineData(37)]
    [InlineData(179)]
    public void SetMinutesAcceptsAnyWholeMinuteWithinBounds(int minutes)
    {
        var model = new DurationSelectionModel();

        model.SetMinutes(minutes);

        Assert.Equal(minutes, model.SelectedMinutes);
    }

    [Fact]
    public void MultipleIncrementAndDecrementOperationsUseFiveMinuteStepsFromCustomValue()
    {
        var model = new DurationSelectionModel();
        model.SetMinutes(37);

        model.Increment();
        model.Decrement();

        Assert.Equal(37, model.SelectedMinutes);
    }

    [Fact]
    public void DecrementStopsAtMinimumWithoutWrapping()
    {
        var model = new DurationSelectionModel();
        model.SetMinutes(6);

        model.Decrement();

        Assert.Equal(DurationSelectionModel.MinimumMinutes, model.SelectedMinutes);
        Assert.False(model.CanDecrement);

        model.Decrement();

        Assert.Equal(DurationSelectionModel.MinimumMinutes, model.SelectedMinutes);
    }

    [Fact]
    public void IncrementStopsAtMaximumWithoutWrapping()
    {
        var model = new DurationSelectionModel();
        model.SetMinutes(179);

        model.Increment();

        Assert.Equal(DurationSelectionModel.MaximumMinutes, model.SelectedMinutes);
        Assert.False(model.CanIncrement);

        model.Increment();

        Assert.Equal(DurationSelectionModel.MaximumMinutes, model.SelectedMinutes);
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(0)]
    [InlineData(4)]
    [InlineData(181)]
    public void SetMinutesRejectsValuesOutsideBounds(int minutes)
    {
        var model = new DurationSelectionModel();

        Assert.Throws<ArgumentOutOfRangeException>(() => model.SetMinutes(minutes));

        Assert.Equal(DurationSelectionModel.DefaultMinutes, model.SelectedMinutes);
    }

    [Theory]
    [InlineData(-5)]
    [InlineData(0)]
    [InlineData(10)]
    [InlineData(60)]
    [InlineData(181)]
    public void UnsupportedPresetIsRejectedWithoutChangingSelection(int minutes)
    {
        var model = new DurationSelectionModel();

        Assert.Throws<ArgumentOutOfRangeException>(() => model.SelectPreset(minutes));

        Assert.Equal(DurationSelectionModel.DefaultMinutes, model.SelectedMinutes);
    }

    [Fact]
    public void PresetsAreTheSupportedValues()
    {
        Assert.Equal(new[] { 15, 30, 45 }, DurationSelectionModel.Presets);
    }
}
