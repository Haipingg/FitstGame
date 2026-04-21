using Godot;
using System;

public partial class Sprite2d : Sprite2D
{
	[Export]
	public int animationRate = 10;
	[Export]
	private double timer = 0;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		timer += delta;
		if (timer > 1.0 / animationRate)
		{
			timer = 0;
			int frame2 = this.Frame +1;
			frame2%=9;
			this.Frame = frame2;

		}
	}
}
