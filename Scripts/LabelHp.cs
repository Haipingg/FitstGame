using Godot;
using System;

public partial class LabelHp : Label
{
	// Called when the node enters the scene tree for the first time.
	private GameData data;
	public override void _Ready()
	{
		data = GetNode<GameData>("../../GameData");
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// GD.Print(player.HP);
		this.Text = "HP:"+data.HP;
	}
}
