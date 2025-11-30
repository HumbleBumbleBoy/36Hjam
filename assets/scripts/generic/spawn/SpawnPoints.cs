using System;
using System.Linq;
using Godot;

namespace Hjam.assets.scripts.generic;

public partial class SpawnPoints : Node
{
    
    private readonly Random _random = new();
    
    public Node2D? GetRandomSpawnPoint()
    {
        var spawnPoints = GetChildren().Where(child => child is Node2D { Visible: true }).ToList();
        if (spawnPoints.Count == 0)
            return null;
        
        var randomIndex = _random.Next(0, spawnPoints.Count);
        return spawnPoints[randomIndex] as Node2D ?? null!;
    }
    
    public Vector2 GetRandomSpawnPosition()
    {
        var spawnPoint = GetRandomSpawnPoint();
        return spawnPoint?.GlobalPosition ?? Vector2.Zero;
    }
    
}