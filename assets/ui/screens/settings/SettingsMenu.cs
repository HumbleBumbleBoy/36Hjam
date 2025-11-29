using Godot;

namespace Hjam.assets.ui.screens.settings;

public partial class SettingsMenu : Control
{
    private void OnBackButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://assets/ui/screens/main_menu/main_menu.tscn");
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}