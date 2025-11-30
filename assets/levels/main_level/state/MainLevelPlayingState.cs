using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using Hjam.assets.entities.enemy;
using Hjam.assets.entities.player;
using Hjam.assets.scripts.generic;
using Hjam.assets.scripts.lib.state;

namespace Hjam.assets.levels.main_level.state;

public class MainLevelPlayingState(int stage) : State<Node2D>
{
    
    private readonly Random _random = new();
    
    private readonly List<PackedScene> _enemyScenes =
    [
        GD.Load<PackedScene>("res://assets/entities/pistol_guy/PistolGuy.tscn"),
        GD.Load<PackedScene>("res://assets/entities/knife_guy/KnifeGuy.tscn")
    ];
    
    private float _timeSinceLastSpawn = 0f;
    
    
    public override Task OnEnter(Node2D context, State<Node2D>? previousState)
    {
        return Task.CompletedTask;
    }

    public override Task OnUpdate(Node2D context, double deltaTime)
    {
        _timeSinceLastSpawn += (float)deltaTime;
        var secondsBetweenSpawns = SecondsBetweenEnemySpawns(min: 0.5f, max: 3f);
        if (!(_timeSinceLastSpawn >= secondsBetweenSpawns)) return Task.CompletedTask;
        _timeSinceLastSpawn = 0f;
        
        var enemiesToSpawn = HowManyEnemiesToSpawn();
        for (var i = 0; i < enemiesToSpawn; i++)
        {
            SpawnEnemy(context);
        }

        return Task.CompletedTask;
    }

    private float SecondsBetweenEnemySpawns(float min, float max)
    {
        var baseTime = Math.Max(max - (stage - 1) * 0.5f, min);
        return (float)(_random.NextDouble() * (max - baseTime) + baseTime);
    }
 
    private int HowManyEnemiesToSpawn()
    {
        return 5 + (stage - 1) * 3;
    }
    
    private int MaxAliveEnemies()
    {
        return 10 + (stage - 1) * 5;
    }
    
    private Enemy CreateEnemy(Node2D context)
    {
        var enemyScene = _enemyScenes[_random.Next(0, _enemyScenes.Count)];
        var enemy = enemyScene.Instantiate<Enemy>();
        enemy.AddToGroup("enemy");
        context.AddChild(enemy);
        return enemy;
    }
    
    private void SpawnEnemy(Node2D context)
    {
        if (context.GetTree().GetFirstNodeInGroup("player") is not Player player)
        {
            GD.Print("No player found, cannot spawn enemy.");
            return;
        }
        
        var spawnPoints = context.GetTree().GetFirstNodeInGroup("spawn_points") as SpawnPoints;

        var allAlive = new List<Vector2> { player.GlobalPosition };
        foreach (var alive in context.GetTree().GetNodesInGroup("enemy"))
        {
            if (alive is Enemy e)
            {
                allAlive.Add(e.GlobalPosition);
            }
        }

        if (allAlive.Count - 2 > MaxAliveEnemies())
        {
            return;
        }
        
        var position = spawnPoints?.GetFarRandomSpawnPoint(allAlive, minDistance: 200f);
        if (position == null)
        {
            GD.Print("No valid spawn point found for enemy.");
            return;
        }
        
        var enemy = CreateEnemy(context);
        enemy.GlobalPosition = position.GlobalPosition;
    }
    
}