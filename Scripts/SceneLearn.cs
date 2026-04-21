using Godot;
using System;

public partial class SceneLearn : Node2D
{

	[Export]
	public PackedScene ohterScene;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Jump"))
		{
			//场景跳转
			SceneTree st  = GetTree();
			// st.ChangeSceneToFile("res://Scene/Scene03.tscn");
			// st.ChangeSceneToPacked(ohterScene);

			//在场景中加载其他场景
			//获取需要加载的场景实例
			Node node = ohterScene.Instantiate();
			st.CurrentScene.AddChild(node);



		}
	}
}
