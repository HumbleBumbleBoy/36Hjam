using Godot;

namespace Hjam.assets.ui.screens.main_menu;

public partial class MainMenu : Node2D
{

    private void OnPlayButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://assets/levels/main_level/main_level.tscn");
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