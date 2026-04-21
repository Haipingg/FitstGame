using Godot;
using System;
using System.Threading.Tasks;

public partial class Frog : CharacterBody2D
{
	public const float Speed = 100.0f;
	public const float JumpVelocity = -400.0f;


	private Player player;

	private bool IsChasing = false;
	private bool IsDie = false;
	private GameData gameData;
	private AnimatedSprite2D animSpriteForg;
    public override void _Ready()
    {
        animSpriteForg = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		gameData = GetNode<GameData>("/root/GameData");
		// 添加AreaEntered信号处理
		Area2D area2DAttack = GetNode<Area2D>("Area2DAttack");
		Area2D area2DDeath = GetNode<Area2D>("Area2DDeath");
		area2DAttack.AreaEntered += OnAreaEnteredAttack;
		area2DDeath.AreaEntered += OnAreaEnteredDeath;
    }
	public override void _PhysicsProcess(double delta)
	{
		if(IsDie) return;
		Vector2 velocity = Velocity;

		// Add the gravity.
		if (!IsOnFloor())
		{
			velocity += GetGravity() * (float)delta;
		}


		//主角与怪物的位置差
	if (IsChasing && player != null)
	{
	Vector2 pos = player.GlobalPosition - this.GlobalPosition;
	if(pos.X > 0)
	{
		animSpriteForg.FlipH = true;
		velocity.X = Speed;

	}else if(pos.X < 0)
	{
		animSpriteForg.FlipH = false;
		velocity.X = -Speed;

	}
	animSpriteForg.Play("Jump");
	}
	else
	{
		animSpriteForg.Play("Idle");
		velocity.X = 0;
	}


		Velocity = velocity;
		MoveAndSlide();
	}
	private void OnPlayerBodyEnter(Node2D body)
	{
		if(body.Name == "Player")
		{
			player = body as Player;
			IsChasing = true;
		}
		
	}
		private void OnPlayerBodyExit(Node2D body)
	{
		if(body.Name == "Player")
		{
			IsChasing = false;
		}
	}

	private void OnPlayerAttack(Node2D body)
	{
		if(body.Name == "Hurtbox")
		{
			player = body as Player;
			if (gameData != null)
			{
				gameData.HP -= 3;
				GD.Print(gameData.HP);
			}
		}
	}

	public async void OnDeathEnter(Node2D body)
	{
		if(body.Name == "Hitbox"|| body.Name == "LeftHitbox")
		{
			animSpriteForg.Play("Death");
			IsDie = true;
			//怪物死亡，销毁当前节点
			await ToSignal(animSpriteForg,AnimatedSprite2D.SignalName.AnimationFinished);

			this.QueueFree();
		}
	}

	private void OnAreaEnteredAttack(Area2D area)
	{
		GD.Print("Area2DAttack碰撞到: " + area.Name);
		if(area.Name == "Hurtbox")
		{
			OnPlayerAttack(area);
		}
	}

	private void OnAreaEnteredDeath(Area2D area)
	{
		if(area != null && area.Monitoring)
		{
			GD.Print("Hitbox监控状态: " + area.Monitoring);
			OnDeathEnter(area);
		}
	}
}
