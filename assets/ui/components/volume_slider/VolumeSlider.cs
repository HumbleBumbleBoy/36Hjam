using Godot;
using System;
using System.Collections.Generic;

namespace HJam.assets.ui.components.volume_slider;

[Tool]
public partial class VolumeSlider : HBoxContainer
{
    private static readonly Dictionary<int, double> VolumePercentageByBusIndex = new();

    private string _busName = "<Bus Name>";
    private RichTextLabel _volumeLabel;
    private HSlider _volumeSlider;
    private RichTextLabel _percentageLabel;

    [Export] public string BusName
    {
        get => _busName;
        set { _busName = value; UpdateVolumeLabel(); }
    }

    [Export] public int BusIndex { get; set; }

    public override void _Ready()
    {
        _volumeLabel = GetNode<RichTextLabel>("VolumeLabel");
        UpdateVolumeLabel();

        _volumeSlider = GetNode<HSlider>("VolumeSlider");
        _volumeSlider.ValueChanged += _volumeSlider_ValueChanged;

        _percentageLabel = GetNode<RichTextLabel>("PercentageLabel");
        UpdatePercentageLabel();

        _volumeSlider.Value = VolumePercentageByBusIndex.GetValueOrDefault(BusIndex, 50.0);
    }

    private void _volumeSlider_ValueChanged(double value)
    {
        UpdatePercentageLabel();
        VolumePercentageByBusIndex[BusIndex] = value;

        if (value == 0)
        {
            AudioServer.SetBusMute(BusIndex, true); return;
        }

        if (AudioServer.IsBusMute(BusIndex))
        {
            AudioServer.SetBusMute(BusIndex, false);
        }

        var decibels = ConvertPercentageToDb(value);
        AudioServer.SetBusVolumeDb(BusIndex, decibels);
    }

    private void UpdateVolumeLabel()
    {
        if (_volumeLabel == null) { return; }

        _volumeLabel.Text = _busName;
    }

    private void UpdatePercentageLabel()
    {
        var volume = _volumeSlider.Value;
        var volumePercentage = $"{Mathf.Floor(volume)}%";
        _percentageLabel.Text = volumePercentage;
    }

    private static float ConvertPercentageToDb(double percentage)
    {
        const float scale = 20.0f;
        const float divisor = 50.0f;
        return scale * (float)Math.Log10(percentage / divisor);
    }
}
