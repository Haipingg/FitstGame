using Godot;
using System;

public partial class Player : CharacterBody2D
{
	public const float Speed = 150.0f;
	public const float JumpVelocity = -450.0f;
	public const float JumpGravityMultiplier = 1.2f;
	public const float FallGravityMultiplier = 1.5f;
	public const float WallSlideSpeed = 100.0f;
	public const float WallJumpVelocityX = 200.0f;
	public const float WallJumpVelocityY = -350.0f;
	public const float DashSpeed = 500.0f;
	public const float DashDuration = 0.3f;
	public const float DashCooldown = 0.5f;
	public const int MaxAirDashes = 1;

	[Export]
	public uint DashCollisionMask = 1;

	private AnimatedSprite2D animSprite;
	private Hurtbox hurtbox;
	private Hitbox hitbox;
	private Hitbox leftHitbox;
	private RayCast2D floorRay;
	private RayCast2D wallLeft;
	private RayCast2D wallRight;

	public int MaxAirJumps = 1;
	private int AirJumpUsed;
	private bool wasOnFloor = false;
	private bool isWallSliding = false;
	private bool wallJumpUsed = false;

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

	private bool isDashing = false;
	private float dashTimer = 0f;
	private int airDashUsed = 0;
	private float dashCooldownTimer = 0f;
	private bool isDashOnCooldown = false;
	private uint originalCollisionMask = 0;

    public override void _Ready()
    {
        animSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
		hurtbox = GetNode<Hurtbox>("Hurtbox");
		hitbox = GetNode<Hitbox>("Hitbox");
		leftHitbox = GetNode<Hitbox>("LeftHitbox");
		floorRay = GetNode<RayCast2D>("FloorRay");
		wallLeft = GetNode<RayCast2D>("WallLeft");
		wallRight = GetNode<RayCast2D>("WallRight");
		hitbox.Disable();
		leftHitbox.Disable();
        animSprite.AnimationFinished += OnAnimationFinished;
    }

	public override void _PhysicsProcess(double delta)
	{
		float deltaFloat = (float)delta;

		UpdateAttackBuffer(deltaFloat);
		UpdateRecovery(deltaFloat);
		UpdateDash(deltaFloat);

		if (attackState != AttackState.Idle && canCancel && attackBuffered)
		{
			TryComboAttack();
		}

		if (isDashing)
		{
			HandleDashMovement();
			return;
		}

		Vector2 velocity = Velocity;

		if (IsOnFloor())
		{
			AirJumpUsed = 0;
			airDashUsed = 0;
			wasOnFloor = true;
			isWallSliding = false;
			wallJumpUsed = false;
		}
		else if (wasOnFloor)
		{
			AirJumpUsed = 0;
			wasOnFloor = false;
		}

		bool isTouchingLeftWall = wallLeft.IsColliding();
		bool isTouchingRightWall = wallRight.IsColliding();

		if (!IsOnFloor())
		{
			if ((isTouchingLeftWall && velocity.X <= 0) || (isTouchingRightWall && velocity.X >= 0))
			{
				isWallSliding = true;
				if (velocity.Y > WallSlideSpeed)
				{
					velocity.Y = WallSlideSpeed;
				}
			}
			else
			{
				isWallSliding = false;
			}

			if (!isWallSliding)
			{
				if (velocity.Y < 0)
				{
					velocity += GetGravity() * JumpGravityMultiplier * deltaFloat;
				}
				else
				{
					velocity += GetGravity() * FallGravityMultiplier * deltaFloat;
				}
			}
			else
			{
				velocity += GetGravity() * deltaFloat;
			}
		}

		if (attackState != AttackState.Idle && attackState != AttackState.Recovery)
		{
			Velocity = velocity;
			MoveAndSlide();
			return;
		}

		if (Input.IsActionJustPressed("Dash") && CanDash())
		{
			StartDash();
			return;
		}

		if (Input.IsActionJustPressed("ui_accept"))
		{
			if (IsOnFloor())
			{
				AirJumpUsed = 0;
				velocity.Y = JumpVelocity;
				wallJumpUsed = false;
			}
			else if (isWallSliding && !wallJumpUsed)
			{
				velocity.Y = WallJumpVelocityY;
				if (wallLeft.IsColliding())
				{
					velocity.X = WallJumpVelocityX;
				}
				else
				{
					velocity.X = -WallJumpVelocityX;
				}
				wallJumpUsed = true;
				isWallSliding = false;
				AirJumpUsed = 0;
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
			if (Velocity.X < 0)
			{
				animSprite.FlipH = true;
			}
			else if (Velocity.X > 0)
			{
				animSprite.FlipH = false;
			}

			if (Velocity.Y < -10)
			{
				animSprite.Play("JumpUp");
			}
			else if (Velocity.Y > 10)
			{
				animSprite.Play("JumpDown");
			}
			else
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
		if (isDashing)
			return;
		GD.Print($"Player受到{damage}点伤害");
		Camera2D camera = GetNode<Camera2D>("Camera2D");
		if (camera is CameraController cameraController)
		{
			cameraController.ShakeCamera();
		}
	}

	private void StartDash()
	{
		isDashing = true;
		dashTimer = DashDuration;
		hurtbox.Monitoring = false;
		hurtbox.Visible = false;
		animSprite.Play("Dash");
		if (!IsOnFloor())
		{
			airDashUsed++;
		}
		isDashOnCooldown = true;
		dashCooldownTimer = DashCooldown;
		originalCollisionMask = CollisionMask;
		CollisionMask = DashCollisionMask;
	}

	private void UpdateDash(float delta)
	{
		if (isDashing)
		{
			dashTimer -= delta;
			if (dashTimer <= 0)
			{
				EndDash();
			}
		}
		if (isDashOnCooldown)
		{
			dashCooldownTimer -= delta;
			if (dashCooldownTimer <= 0)
			{
				isDashOnCooldown = false;
				dashCooldownTimer = 0;
			}
		}
	}

	private void HandleDashMovement()
	{
		float dashDirection = animSprite.FlipH ? -1 : 1;
		Velocity = new Vector2(dashDirection * DashSpeed, 0);
		MoveAndSlide();
	}

	private void EndDash()
	{
		isDashing = false;
		dashTimer = 0;
		hurtbox.Monitoring = true;
		hurtbox.Visible = true;
		Velocity = Vector2.Zero;
		CollisionMask = originalCollisionMask;
	}

	private bool CanDash()
	{
		if (isDashing)
			return false;
		if (isDashOnCooldown)
			return false;
		if (!IsOnFloor() && airDashUsed >= MaxAirDashes)
			return false;
		return true;
	}
}