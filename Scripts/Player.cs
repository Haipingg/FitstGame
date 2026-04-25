using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float Speed = 150.0f;
	public const float JumpVelocity = -450.0f;
	public const float JumpGravityMultiplier = 1.2f; // 跳跃时的重力倍数
	public const float FallGravityMultiplier = 1.5f; // 坠落时的重力倍数

	private AnimatedSprite2D animSprite;
	private Hitbox hitbox;
	private Hitbox leftHitbox;

	public int MaxAirJumps = 1;
	private int AirJumpUsed;
	private bool wasOnFloor = false;

	private enum AttackState { Idle, Attack1, Attack2, Attack3, Recovery }
	private AttackState attackState = AttackState.Idle;

	private float attackBufferTime = 0.2f;
	private float attackBufferCounter = 0f;
	private bool attackBuffered = false;

	private float recoveryTime = 0.15f;
	private float recoveryCounter = 0f;

	private int currentAttackPhase = 1;

	private bool canCancel = false;
	private float cancelWindowStart = 0.1f;
	private float cancelWindowEnd = 0.3f;

    public override void _Ready()
    {
        animSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		hitbox = GetNode<Hitbox>("Hitbox");
		leftHitbox = GetNode<Hitbox>("LeftHitbox");
		hitbox.Disable();
		leftHitbox.Disable();
        animSprite.AnimationFinished += OnAnimationFinished;
    }

	public override void _PhysicsProcess(double delta)
	{
		float deltaFloat = (float)delta;

		UpdateAttackBuffer(deltaFloat);
		UpdateRecovery(deltaFloat);

		if (attackState != AttackState.Idle && canCancel && attackBuffered)
		{
			TryComboAttack();
		}

		Vector2 velocity = Velocity;

		// 检测是否刚刚离开地面
		if (IsOnFloor())
		{
			AirJumpUsed = 0;
			wasOnFloor = true;
		}
		else if (wasOnFloor)
		{
			// 刚刚离开地面，重置空中跳跃次数
			AirJumpUsed = 0;
			wasOnFloor = false;
		}

		if (!IsOnFloor())
		{
			// 跳跃分为三个部分，使用不同的重力倍数
			if (velocity.Y < 0) // 向上跃的部分
			{
				velocity += GetGravity() * JumpGravityMultiplier * deltaFloat;
			}
			else // 到达最高点开始坠落和向下坠落的部分
			{
				velocity += GetGravity() * FallGravityMultiplier * deltaFloat;
			}
		}

		if (attackState != AttackState.Idle && attackState != AttackState.Recovery)
		{
			Velocity = velocity;
			MoveAndSlide();
			return;
		}

		if (Input.IsActionJustPressed("ui_accept"))
		{
			if (IsOnFloor())
			{
				AirJumpUsed = 0;
				velocity.Y = JumpVelocity;
			}
			else if (AirJumpUsed < MaxAirJumps)
			{
				velocity.Y = JumpVelocity;
				AirJumpUsed++;
			}
		}

		Vector2 direction = Input.GetVector("Left", "Right", "Up", "Down");
		if (direction != Vector2.Zero)
		{
			velocity.X = direction.X * Speed;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
		}

		Velocity = velocity;
		MoveAndSlide();
		AnimationControl();
	}

	private void UpdateAttackBuffer(float delta)
	{
		if (attackBuffered)
		{
			attackBufferCounter -= delta;
			if (attackBufferCounter <= 0)
			{
				attackBuffered = false;
			}
		}
	}

	private void UpdateRecovery(float delta)
	{
		if (attackState == AttackState.Recovery)
		{
			recoveryCounter -= delta;
			if (recoveryCounter <= 0)
			{
				attackState = AttackState.Idle;
				hitbox.Disable();
				leftHitbox.Disable();
				GD.Print("攻击结束，恢复idle");
			}
		}
	}

	private void AnimationControl()
	{
		if (attackState != AttackState.Idle && attackState != AttackState.Recovery)
		{
			return;
		}

		// 跳跃动画分为三个部分
		if (!IsOnFloor())
		{
			if (Velocity.Y < -10) // 向上跃的部分
			{
				animSprite.Play("JumpUp");
			}
			else if (Velocity.Y > 10) // 向下坠落的部分
			{
				animSprite.Play("JumpDown");
			}
			else // 到达最高点开始坠落的部分
			{
				animSprite.Play("JumpPeak");
			}
		}
		else if (Velocity.Length() > 10)
		{
			if (Velocity.X < 0)
			{
				animSprite.Play("Run");
				animSprite.FlipH = true;
			}
			else if (Velocity.X > 0)
			{
				animSprite.Play("Run");
				animSprite.FlipH = false;
			}
		}
		else
		{
			animSprite.Play("Idle");
		}
	}

    public override void _Input(InputEvent @event)
    {
		if (Input.IsActionJustPressed("Fight"))
        {
			BufferedAttack();
        }
        base._Input(@event);
    }

	private void BufferedAttack()
	{
		if (attackState == AttackState.Idle)
		{
			StartAttack(1);
		}
		else if (attackState != AttackState.Recovery)
		{
			attackBuffered = true;
			attackBufferCounter = attackBufferTime;
		}
	}

	private void TryComboAttack()
	{
		if (canCancel && attackBuffered)
		{
			attackBuffered = false;
			int nextPhase = currentAttackPhase + 1;
			if (nextPhase <= 3)
			{
				StartAttack(nextPhase);
			}
		}
	}

	private void StartAttack(int phase)
	{
		currentAttackPhase = phase;
		canCancel = false;

		string animName = phase switch
		{
			1 => "Attack1",
			2 => "Attack2",
			3 => "Attack3",
			_ => "Attack1"
		};

		animSprite.Play(animName);
		attackState = (AttackState)phase;

		if (animSprite.FlipH)
		{
			leftHitbox.Enable();
		}
		else
		{
			hitbox.Enable();
		}

		GD.Print($"第{phase}段攻击开始");

		CallDeferred(nameof(EnableCancelWindow), 0.1f);
	}

	private void EnableCancelWindow(float delay)
	{
		canCancel = true;
	}

	private void OnAnimationFinished()
	{
		string currentAnim = animSprite.Animation;

		if (currentAnim == "Attack1" || currentAnim == "Attack2" || currentAnim == "Attack3")
		{
			if (attackBuffered && currentAttackPhase < 3)
			{
				attackBuffered = false;
				TryComboAttack();
			}
			else
			{
				StartRecovery();
			}
		}
	}

	private void StartRecovery()
	{
		attackState = AttackState.Recovery;
		recoveryCounter = recoveryTime;
		canCancel = false;
		hitbox.Disable();
		leftHitbox.Disable();
		GD.Print("进入后摇阶段");
	}

	public void TakeDamage(int damage)
	{
		GD.Print($"Player受到{damage}点伤害");
		// 触发震屏效果
		Camera2D camera = GetNode<Camera2D>("Camera2D");
		if (camera is CameraController cameraController)
		{
			cameraController.ShakeCamera();
		}
	}
}