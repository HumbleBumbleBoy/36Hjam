using Godot;

namespace Hjam.assets.ui.screens.settings;

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