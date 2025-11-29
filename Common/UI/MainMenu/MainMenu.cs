using Godot;
using System;

public partial class MainMenu : Node2D
{

    private void OnPlayButtonPressed()
    {
        GetTree().ChangeSceneToFile("res://Locations/GameScene/game_scene.tscn");
    }

    private void OnSettingsButtonPressed()
    {
        // 
    }

    private void OnHelpButtonPressed()
    {
        //
    }

    private void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
