using Godot;
using System;

public partial class Vector2Learn : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Vector2 v1 = new Vector2(100,100);	//可以表示坐标，也可以表示向量
		Vector2 v2 = new Vector2(1,0);	//长度为1的向量称为单位向量

		//常用向量
		// Vector2.Up;
		// Vector2.Down;
		// Vector2.Left;
		// Vector2.Right;
		// Vector2.Zero;	//(0,0)
		// Vector2.One;	//(1,1)

		Vector2 a = new Vector2(3,4);
		Vector2 b = new Vector2(3,5);
		//一般是一个坐标加上一个向量，代表这个物体移动了多少
		Vector2 c =	a + b;	//(6,9)
		Vector2 e = a*2;	//这个是乘法，一般用于向量，带表物体的移动速度*2

		a.Length();	//获取向量长度，这里是5
		a.LengthSquared();		//这里获取到的是没有开平方的值，是25

		a.Normalized();	//单位化向量，使向量的长度变为1




	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
