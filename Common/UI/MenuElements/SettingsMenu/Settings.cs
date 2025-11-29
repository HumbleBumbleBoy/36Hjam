using Godot;
using System;

public partial class Settings : Control
{
    private void OnBackButtonPressed()
    {
        //
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
