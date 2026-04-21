using Godot;

public partial class Hurtbox : Area2D
{
    public override void _Ready()
    {
        AreaEntered += OnAreaEntered;
    }

    private void OnAreaEntered(Area2D area)
    {
        if (area.Name == "Area2DAttack")
        {
            if (area.HasMethod("GetDamage"))
            {
                int damage = (int)area.Call("GetDamage");
                TakeDamage(damage);
            }
        }
    }

    private void TakeDamage(int damage)
    {
        if (GetParent() is Player player)
        {
            player.TakeDamage(damage);
        }
    }
}