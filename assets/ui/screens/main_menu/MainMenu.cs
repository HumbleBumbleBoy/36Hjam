using Godot;

namespace Hjam.assets.ui.screens.main_menu;

public partial class MainMenu : Node2D
{

    private void OnPlayButtonPressed()
    {
        // TODO
    }

    private void OnSettingsButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://assets/ui/screens/settings/settings_menu.tscn");
    }

    private void OnHelpButtonPressed()
    {
        // TODO
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}