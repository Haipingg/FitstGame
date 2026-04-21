using Godot;
using System;

public partial class MyScript : TextureRect
{
	public override void _EnterTree()
	{
		// base._EnterTree();
		GD.Print("enterTree");
	}

	// Called when the node enters the scene tree for the first time.生命周期：场景加载中间后执行
	public override void _Ready()
	{
		GD.Print("12345");
		//销毁当前节点
		// this.QueueFree();
		//退出游戏
		// GetTree().Quit();
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.生命周期：场景加载成功组件后每帧都会执行
	public override void _Process(double delta)
	{
		// GD.Print("C");
	}
	public override void _PhysicsProcess(double delta)
	{
		base._PhysicsProcess(delta);
	}
	public override void _ExitTree()
	{
		GD.Print("exit");
		// base._ExitTree();
	}


}
