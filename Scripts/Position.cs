using Godot;
using System;

public partial class Position : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		GD.Print(this.Position);	//获取局部坐标
		GD.Print(this.GlobalPosition);	//获取全局坐标 如果父节点的坐标移动了，这个坐标的位置等于父节点的坐标加自己移动的坐标
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
