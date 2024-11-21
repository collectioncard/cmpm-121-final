using System;
using Godot;

public partial class Win : Label
{
    private float _timer;

    public void playerWin()
    {
        Visible = true;
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        _timer += (float)delta;
        if (_timer >= .1)
        {
            CreateTween()
                .TweenProperty(this, "modulate", Color.FromHsv(Global.Rng.Randf(), 1, 1), 0.1f);
            _timer = 0;
        }
    }
}
