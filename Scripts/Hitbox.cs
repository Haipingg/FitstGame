using Godot;

public partial class Hitbox : Area2D
{
    [Export]
    public int Damage { get; set; } = 10;

    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area.Name == "Area2DDeath")
        {
            if (area.HasMethod("OnDeathEnter"))
            {
                area.Call("OnDeathEnter", this);
            }
        }
    }

    public void Enable()
    {
        Monitoring = true;
        Visible = true;
    }

    public void Disable()
    {
        Monitoring = false;
        Visible = false;
    }
}