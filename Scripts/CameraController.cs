using Godot;
using System;

/// <summary>
/// 摄像机控制器类
/// 功能：管理游戏摄像机的跟随、震动、地图边界限制、缩放等功能
/// 继承自Camera2D，支持平滑跟随、震屏特效和缩放
/// </summary>
public partial class CameraController : Camera2D
{
	/// <summary>
	/// 摄像机跟随的目标节点
	/// </summary>
	[Export]
	public Node2D target;

	/// <summary>
	/// 摄像机跟随速度，数值越大跟随越快
	/// </summary>
	[Export]
	public float followSpeed = 5.0f;

	/// <summary>
	/// 缓冲半径，玩家在此范围内移动时摄像机不会跟随
	/// 用于实现玩家可以在屏幕内自由移动的效果
	/// </summary>
	[Export]
	public float bufferRadius = 100.0f;

	/// <summary>
	/// 地图大小，用于限制摄像机不能超出地图边界
	/// </summary>
	[Export]
	public Vector2 mapSize = new Vector2(2000, 1000);

	/// <summary>
	/// 震动持续时间，单位为秒
	/// </summary>
	[Export]
	public float shakeDuration = 0.5f;

	/// <summary>
	/// 震动强度，数值越大震动幅度越大
	/// </summary>
	[Export]
	public float shakeIntensity = 10.0f;

	/// <summary>
	/// 目标缩放值
	/// </summary>
	[Export]
	public Vector2 zoomValue = new Vector2(1.0f, 1.0f);

	/// <summary>
	/// 缩放速度
	/// </summary>
	[Export]
	public float zoomSpeed = 2.0f;

	/// <summary>
	/// 摄像机状态枚举
	/// Following: 跟随状态
	/// Shaking: 震动状态
	/// Idle: 空闲状态
	/// </summary>
	private enum CameraState { Following, Shaking, Idle }

	/// <summary>
	/// 当前摄像机状态
	/// </summary>
	private CameraState currentState = CameraState.Following;

	/// <summary>
	/// 震动偏移量，用于计算震动时的位置偏移
	/// </summary>
	private Vector2 shakeOffset = Vector2.Zero;

	/// <summary>
	/// 震动已持续的时间
	/// </summary>
	private float shakeTime = 0.0f;

	/// <summary>
	/// 随机数生成器，用于震动效果
	/// </summary>
	private Random random = new Random();

	/// <summary>
	/// 目标位置，摄像机要移动到的目标点
	/// </summary>
	private Vector2 targetPosition;

	/// <summary>
	/// 是否正在执行回中操作
	/// </summary>
	private bool isReturningToCenter = false;

	/// <summary>
	/// 回中目标点
	/// </summary>
	private Vector2 returnTarget;

	/// <summary>
	/// 摄像机准备就绪时的回调
	/// 初始化目标位置和摄像机初始位置
	/// </summary>
	public override void _Ready()
	{
		if (target != null)
		{
			targetPosition = target.GlobalPosition;
			GlobalPosition = targetPosition;
		}
		Zoom = zoomValue;
	}

	/// <summary>
	/// 每帧执行的逻辑
	/// 根据当前状态更新摄像机
	/// </summary>
	public override void _Process(double delta)
	{
		float deltaFloat = (float)delta;

		// 更新缩放
		UpdateZoom(deltaFloat);

		// 根据当前状态执行相应的更新逻辑
		switch (currentState)
		{
			case CameraState.Following:
				UpdateFollowing(deltaFloat);
				break;
			case CameraState.Shaking:
				UpdateShaking(deltaFloat);
				break;
			case CameraState.Idle:
				break;
		}

		// 确保摄像机位置不会超出地图边界（已注释，使用Camera2D的limit属性）
		// ClampToMap();

		// 绘制调试边界（已注释）
		// QueueRedraw();
	}

	/// <summary>
	/// 更新缩放
	/// </summary>
	private void UpdateZoom(float delta)
	{
		if (Zoom != zoomValue)
		{
			Zoom = Zoom.Lerp(zoomValue, zoomSpeed * delta);
		}
	}

	/// <summary>
	/// 绘制地图边界调试线
	/// </summary>
	public override void _Draw()
	{
		// 地图边界绘制已注释，使用Camera2D的limit属性
		// Color borderColor = new Color(1, 0, 0, 0.3f);
		// DrawRect(new Rect2(Vector2.Zero, mapSize), borderColor, false, 2);
	}

	/// <summary>
	/// 更新跟随逻辑
	/// 实现带缓冲效果的平滑跟随
	/// 当玩家移动超出缓冲半径时，摄像机才会跟随
	/// 当人物离开地图边界时，相机固定，人物回到相机中心
	/// </summary>
	private void UpdateFollowing(float delta)
	{
		if (target == null)
		{
			return;
		}

		Vector2 targetGlobal = target.GlobalPosition;
		Vector2 cameraGlobal = GlobalPosition;

		// 获取视口大小（考虑缩放）
		Vector2 viewportSize = GetViewportRect().Size / Zoom;
		Vector2 halfViewport = viewportSize * 0.5f;

		// 计算摄像机可以在地图中移动的边界范围（已注释，使用Camera2D的limit属性）
		// float minX = halfViewport.X;
		// float maxX = mapSize.X - halfViewport.X;
		// float minY = halfViewport.Y;
		// float maxY = mapSize.Y - halfViewport.Y;

		// 计算目标与摄像机之间的距离
		Vector2 distance = targetGlobal - cameraGlobal;
		float distanceLength = distance.Length();

		// 检查是否需要回中（已注释，使用Camera2D的limit属性）
		// bool cameraAtLeftEdge = cameraGlobal.X <= minX;
		// bool cameraAtRightEdge = cameraGlobal.X >= maxX;
		// bool cameraAtTopEdge = cameraGlobal.Y <= minY;
		// bool cameraAtBottomEdge = cameraGlobal.Y >= maxY;
		// bool cameraAtEdge = cameraAtLeftEdge || cameraAtRightEdge || cameraAtTopEdge || cameraAtBottomEdge;

		// 如果相机在边界上，启用回中模式（已注释）
		// if (cameraAtEdge && !isReturningToCenter)
		// {
		// 	isReturningToCenter = true;
		// 	returnTarget = new Vector2(
		// 		Mathf.Clamp(targetGlobal.X, minX, maxX),
		// 		Mathf.Clamp(targetGlobal.Y, minY, maxY)
		// 	);
		// 	GD.Print($"相机到达边界，开始回中 - 目标位置: {returnTarget}");
		// }

		// 如果在回中模式（已注释）
		// if (isReturningToCenter)
		// {
		// 	Vector2 returnDistance = targetGlobal - returnTarget;
		// 	float returnDistanceLength = returnDistance.Length();
		// 	if (returnDistanceLength > 10.0f)
		// 	{
		// 		Vector2 moveDirection = returnDistance.Normalized();
		// 		float moveAmount = returnDistanceLength * followSpeed * delta * 0.3f;
		// 		Vector2 newPlayerPos = targetGlobal - moveDirection * moveAmount;
		// 		newPlayerPos = new Vector2(
		// 			Mathf.Clamp(newPlayerPos.X, 0, mapSize.X),
		// 			Mathf.Clamp(newPlayerPos.Y, 0, mapSize.Y)
		// 		);
		// 		target.GlobalPosition = newPlayerPos;
		// 	}
		// 	else
		// 	{
		// 		isReturningToCenter = false;
		// 		targetPosition = cameraGlobal;
		// 		GD.Print($"人物已回到中心，退出回中模式");
		// 	}
		// 	return;
		// }

		// 正常跟随逻辑：只有当距离超过缓冲半径时才移动目标位置
		if (distanceLength > bufferRadius)
		{
			Vector2 moveDirection = distance.Normalized();
			float moveDistance = distanceLength - bufferRadius;
			targetPosition += moveDirection * moveDistance * followSpeed * delta;
		}

		// 使用线性插值平滑移动摄像机到目标位置
		GlobalPosition = GlobalPosition.Lerp(targetPosition, followSpeed * delta);
	}

	/// <summary>
	/// 更新震动逻辑
	/// </summary>
	private void UpdateShaking(float delta)
	{
		shakeTime += delta;

		if (shakeTime >= shakeDuration)
		{
			currentState = CameraState.Following;
			shakeOffset = Vector2.Zero;
			shakeTime = 0.0f;
			return;
		}

		float intensity = shakeIntensity * (1.0f - shakeTime / shakeDuration);
		shakeOffset = new Vector2(
			(float)(random.NextDouble() * 2 - 1) * intensity,
			(float)(random.NextDouble() * 2 - 1) * intensity
		);

		GlobalPosition = targetPosition + shakeOffset;
	}

	/// <summary>
	/// 限制摄像机在地图边界内（已注释，使用Camera2D的limit属性）
	/// </summary>
	private void ClampToMap()
	{
		// // 获取视口大小（考虑缩放）
		// Vector2 viewportSize = GetViewportRect().Size / Zoom;
		// Vector2 halfViewport = viewportSize * 0.5f;

		// float minX = halfViewport.X;
		// float maxX = mapSize.X - halfViewport.X;
		// float minY = halfViewport.Y;
		// float maxY = mapSize.Y - halfViewport.Y;

		// // 限制摄像机位置
		// GlobalPosition = new Vector2(
		// 	Mathf.Clamp(GlobalPosition.X, minX, maxX),
		// 	Mathf.Clamp(GlobalPosition.Y, minY, maxY)
		// );

		// // 限制目标位置
		// targetPosition = new Vector2(
		// 	Mathf.Clamp(targetPosition.X, minX, maxX),
		// 	Mathf.Clamp(targetPosition.Y, minY, maxY)
		// );
	}

	/// <summary>
	/// 触发震屏效果
	/// </summary>
	public void ShakeCamera()
	{
		currentState = CameraState.Shaking;
		shakeTime = 0.0f;
	}

	/// <summary>
	/// 设置目标缩放值
	/// </summary>
	public void SetZoom(Vector2 newZoom)
	{
		zoomValue = newZoom;
	}

	/// <summary>
	/// 设置目标
	/// </summary>
	public void SetTarget(Node2D newTarget)
	{
		target = newTarget;
		if (target != null)
		{
			targetPosition = target.GlobalPosition;
		}
	}

	/// <summary>
	/// 设置地图大小
	/// </summary>
	public void SetMapSize(Vector2 newMapSize)
	{
		mapSize = newMapSize;
	}

	/// <summary>
	/// 获取是否在回中模式
	/// </summary>
	public bool IsReturningToCenter()
	{
		return isReturningToCenter;
	}
}