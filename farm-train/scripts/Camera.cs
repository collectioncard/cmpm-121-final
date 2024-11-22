using System;
using Godot;

public partial class Camera : Camera2D
{
    private Vector2 _screenSize = Vector2.Zero;
    private Vector2 _posVelocity = Vector2.Zero;
    private const int Speed = 5;
    private bool _canPan = true;
    private bool _manualPan = false;

    public override void _Ready()
    {
        _screenSize = GetViewport().GetVisibleRect().Size;
        Input.MouseMode = Input.MouseModeEnum.Confined;
    }

    public override void _Process(double delta)
    {
        //Camera's origin in the screen/viewport. Fixes issues with camera over moving past limits.
        Position = -(GetCanvasTransform().Origin) + _posVelocity;
    }

    public override void _Input(InputEvent @event)
    {
        _manualPan = Input.IsActionPressed("pan");
        if (!_manualPan && _canPan && @event is InputEventMouseMotion)
        {
            Vector2 tempPosVel = new Vector2(0, 0);
            if (GetLocalMousePosition().X <= 0)
            {
                tempPosVel.X = -Speed;
            }
            else if (GetLocalMousePosition().X >= _screenSize.X - 1)
            {
                tempPosVel.X = Speed;
            }

            if (GetLocalMousePosition().Y <= 0)
            {
                tempPosVel.Y = -5;
            }
            else if (GetLocalMousePosition().Y >= _screenSize.Y - 1)
            {
                tempPosVel.Y = Speed;
            }
            _posVelocity = tempPosVel;
        }
        else if (_manualPan && @event is InputEventMouseMotion eventMouseMotion)
        {
            _posVelocity = Vector2.Zero;
            Position -= eventMouseMotion.Relative;
        }
        else if (@event.IsActionPressed("menu"))
        {
            _canPan = Input.MouseMode == Input.MouseModeEnum.Confined;
        }
    }
}
