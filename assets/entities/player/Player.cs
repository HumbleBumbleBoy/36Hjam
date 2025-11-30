using Godot;
using Hjam.assets.entities.player.state;
using Hjam.assets.scripts.lib.state;

namespace Hjam.assets.entities.player;

public partial class Player : Node2D
{
	
	public StateMachine<Player> StateMachine { get; private set; } = null!;

	public override void _Ready()
	{
		StateMachine = new StateMachine<Player>(this);
		AddChild(StateMachine);
		
		StateMachine.ChangeState(new PlayerIdleState());
	}

}