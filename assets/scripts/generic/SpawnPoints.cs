using System;
using Godot;

namespace Hjam.assets.scripts.generic;

public partial class SpawnPoints : Node
{
    
    private readonly Random _random = new();
    
    public Node2D GetRandomSpawnPoint()
    {
        var spawnPoints = GetChildren();
        if (spawnPoints.Count == 0)
            return null!;
        
        var randomIndex = _random.Next(0, spawnPoints.Count);
        return spawnPoints[randomIndex] as Node2D ?? null!;
    }
    
}