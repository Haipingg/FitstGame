using Godot;
using System;

public partial class NodeLearn : Node2D
{
	//导出
	[Export]
	public Node inputNode;
		//导出
	[Export]
	public Node newParent;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		//节点访问GetNode //相对路径
		// Node node = GetNode("TextureRect3/TextureRect2");
		// node.QueueFree();

		//节点访问GetNode //绝对路径/root/....		默认获取到的节点是Node类型，<>使用泛型将当前节点的类型命名，方便使用各个节点的方法调用
		TextureRect node2 = GetNode<TextureRect>("/root/Scene02/TextureRect");
		node2.FlipV = true;
		//获取父节点
		Node node = GetParent();
		GD.Print(node.Name);
		//可将某个节点作为唯一值直接访问。特征：前面加了%
		TextureRect TextureRect3 = GetNode<TextureRect>("%TextureRect3");
		GD.Print(TextureRect3.Name);

		GD.Print(inputNode.Name);

		//CallDeferred方法是延迟执行的
		// GetParent().CallDeferred(Node.MethodName.RemoveChild,this);
		// newParent.CallDeferred(Node.MethodName.AddChild,this);
		//可以使用以下方法直接更改父节点
		// this.CallDeferred(Node.MethodName.Reparent,newParent);


	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		// Node2D node2d = new Node2D();
		// node2d.Name = "NewNode2D";
		// this.AddChild(node2d);

		// GetParent().RemoveChild(this);
		// newParent.AddChild(this);

		// this.Reparent(newParent);
	}
}
