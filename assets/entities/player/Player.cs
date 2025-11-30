using Godot;
using Hjam.assets.entities.player.state;
using Hjam.assets.scripts.lib.state;

namespace Hjam.assets.entities.player;

public partial class Player : CharacterBody2D
{
	
	[Export] public int Speed = 300;
	
	public StateMachine<Player> StateMachine { get; private set; } = null!;

	public override void _Ready()
	{
		StateMachine = new StateMachine<Player>(this);
		AddChild(StateMachine);
		
		StateMachine.ChangeState(new PlayerIdleState());
	}

	public override void _Process(double delta)
	{
		var movementVector = Input.GetVector(
			"move_left",
			"move_right",
			"move_up",
			"move_down"
		);
		if (movementVector != Vector2.Zero)
		{
			StateMachine.EmitSignal("move", movementVector, Speed);
		}
		else
		{
			StateMachine.EmitSignal("stop_moving");
		}

		StateMachine.Update(delta);
	}
	
	public override void _PhysicsProcess(double delta)
	{
		StateMachine.FixedUpdate(delta);
	}
}