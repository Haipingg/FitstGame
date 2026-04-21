using Godot;
using System;

public partial class InputLearn : Node2D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        // if(Input.IsKeyPressed(Key.Space))
        // {
        //     GD.Print("speace");
        // }
        //这里的Jump必须和游戏引擎项目输入映射的名字相等。名字可以自定义
        // if (Input.IsActionJustPressed("Jump"))
        // {
        //     GD.Print("Jump 按下");
        // }else if (Input.IsActionPressed("Jump"))
        // {
        //     GD.Print("Jump 按中");
        // }else if (Input.IsActionJustReleased("Jump"))
        // {
        //     GD.Print("Jump 抬起");
        // }

        //返回0 1 ；0代表按键未按下，1代表按键按下。可用于持续移动的操作上
        // float c = Input.GetActionStrength("Jump");
        // GD.Print(c);

        //水平轴方向。输出-1代表按下左键，1代表按下右键。0代表不按
        // float h = Input.GetAxis("Left","Right");
        // GD.Print(h);

        //返回一个坐标向量
        Vector2 v =Input.GetVector("Left","Right","Up","Down");
        GD.Print(v);
    }

    
    // public override void _Input(InputEvent @event)
    // {
	// 	if(@event is InputEventKey)
    //     {
    //         var key = @event as InputEventKey;
	// 		if(key.Keycode == Key.Space)
    //         {
	// 			//检测按键抬起事件
    //             // if (key.IsReleased())
    //             // {
    //             //     GD.Print("up");
    //             // }else if (key.IsPressed())	//按下按中都会触发
    //             // {
    //             //     GD.Print("two");
    //             // }else if (key.IsEcho())	//按下中
    //             // {
    //             //     GD.Print("sssss");
    //             // }

	// 			if (key.IsPressed() && !key.IsEcho())	//排除调按下中的事件，只有按下事件触发
    //             {
    //                 GD.Print("down");
    //             }
    //             // GD.Print("space");
    //         }
    //     }

	// 	if(@event is InputEventMouse)
    //     {
    //         var key = @event as InputEventMouse;
    //         if (key.IsPressed())
    //         {
	// 			GD.Print(key.Position);
	// 			GD.Print(key.ButtonMask);
    //             GD.Print("鼠标按下");
    //         }
    //     }
    //     base._Input(@event);
    // }

}
