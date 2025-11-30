using System.Collections.Generic;
using Godot;
using Hjam.assets.entities.bullet;
using Hjam.assets.entities.player.state;
using Hjam.assets.scripts.lib.state;

namespace Hjam.assets.entities.player;

public partial class Player : CharacterBody2D
{
	
	public struct WeaponData
	{
		public string Name;
		public int Damage;
		public int MovementSpeed;
		public float AttackSpeed;
	}
	
	[Export] public int Speed = 600;
	
	public static readonly List<WeaponData> Weapons =
	[
		new()
		{
			Name = "Pistol",
			Damage = 25,
			MovementSpeed = 600,
			AttackSpeed = 0.25f
		}
	];
	
	private WeaponData _currentWeapon = Weapons[0];
	private float _weaponCooldown = 0f;
	
	public StateMachine<Player> StateMachine { get; private set; } = null!;

	public override void _Ready()
	{
		_currentWeapon = Weapons[(int) GD.Randi() % Weapons.Count];
		
		StateMachine = new StateMachine<Player>(this);
		AddChild(StateMachine);
		
		StateMachine.ChangeState(new PlayerIdleState());
	}

	public void UpdateWeapon()
	{
		var mousePosition = GetGlobalMousePosition();
		var direction = (mousePosition - GlobalPosition).Normalized();
		
		var weapons = GetNode<Node2D>("weapons");
		var weaponContainer = weapons.GetNode<Node2D>(_currentWeapon.Name);
		weaponContainer.Show();
		weaponContainer.Rotation = direction.Angle();
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

		UpdateWeapon();

		if (Input.IsActionJustPressed("shoot"))
		{
			if (_weaponCooldown >= _currentWeapon.AttackSpeed)
			{
				var bulletScene = GD.Load<PackedScene>("res://assets/entities/bullet/Bullet.tscn");
				var bullet = bulletScene.Instantiate<Bullet>();
				
				var weapons = GetNode<Node2D>("weapons");
				var weaponContainer = weapons.GetNode<Node2D>(_currentWeapon.Name);
				var muzzle = weaponContainer.GetNode<Node2D>("Muzzle");
				
				bullet.GlobalPosition = muzzle.GlobalPosition;
				bullet.Rotation = (GetGlobalMousePosition() - muzzle.GlobalPosition).Angle();
				bullet.Damage = _currentWeapon.Damage;
				bullet.Speed = _currentWeapon.MovementSpeed;
				
				GetParent().AddChild(bullet);
				
				_weaponCooldown = 0;
			}
		}
		
		_weaponCooldown += (float) delta;
		
		StateMachine.Update(delta);
	}
	
	public override void _PhysicsProcess(double delta)
	{
		StateMachine.FixedUpdate(delta);
	}
}