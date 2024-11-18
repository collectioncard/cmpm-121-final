using Godot;
using System;

public partial class MouseInputManager : Node2D
{
    public Sprite2D TileCursor;
    public AnimatedSprite2D Player;
    private Tween _playerPositionTween;

    [Signal]
    public delegate void TileClickEventHandler(Vector2 pos);

    private void _make_TileCursor()
    {
        TileCursor = new Sprite2D();
        AddChild(TileCursor);
        TileCursor.Texture = GD.Load<Texture2D>("res://assets/tileCursor.png");
    }

    private void _make_Player()
    {
        Player = new AnimatedSprite2D();
        AddChild(Player);
        Player.Position = new Vector2(56, 56);
        Player.SpriteFrames = GD.Load<SpriteFrames>("res://assets/playerAnims.tres");
        Player.Play();  
    }
    
    public override void _Ready()
    {
        TileClick += GetNode<TileGrid>("TileGrid").TileClick;
        _make_TileCursor();
        _make_Player();
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("action1"))
        {
            if (!TileCursor.Visible) return;
            EmitSignal(SignalName.TileClick, GetGlobalMousePosition());
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            TileCursor.Position = Global.GetTileAtPos(eventMouseMotion.Position) + Global.SpriteOffset;
            TileCursor.Visible = TileCursor.Position.DistanceTo(Player.Position) < Global.InteractionRadius;
            
        }
        else if (@event.IsActionPressed("action2"))
        {
            _playerPositionTween?.Kill();
            _playerPositionTween = Player.CreateTween();
            _playerPositionTween.TweenProperty(Player, "position", GetGlobalMousePosition(), GetGlobalMousePosition().DistanceTo(Player.Position)/Global.PlayerSpeed);
        }
    }
}
