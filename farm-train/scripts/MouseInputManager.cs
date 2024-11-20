// MouseInputManager.cs
using System;
using System.Collections.Generic;
using FarmTrain.classes;
using Godot;

public partial class MouseInputManager : Node2D
{
    public Sprite2D TileCursor;
    public AnimatedSprite2D Player;
    private Tween _playerPositionTween;

    private List<Tool> _tools;
    private int _selectedToolIndex;

    [Signal]
    public delegate void TileClickEventHandler(Vector2 pos, Tool tool);

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
        _initializeTools();
        GetNode<Label>("%ToolLabel").Text = "Current Tool: " + _tools[_selectedToolIndex].ToolName;
    }

    private void _initializeTools()
    {
        _tools = new List<Tool>
        {
            new Tool("Open_Hand", false),
            new Tool("Hoe", false),
            new Tool("Seed_One", false),
            new Tool("Seed_two", true),
            new Tool("Cross_Breed_Tool", true),
        };
        _selectedToolIndex = 0;
    }

    public override void _Process(double delta)
    {
        if (Input.IsActionPressed("action1"))
        {
            if (!TileCursor.Visible)
                return;
            EmitSignal(SignalName.TileClick, GetGlobalMousePosition(), _tools[_selectedToolIndex]);
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Move the tile selector graphic
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            TileCursor.Position =
                Global.GetTileAtPos(eventMouseMotion.Position) + Global.SpriteOffset;
            TileCursor.Visible =
                TileCursor.Position.DistanceTo(Player.Position) < Global.InteractionRadius;
        }
        // Move the player to the selected area
        else if (@event.IsActionPressed("action2"))
        {
            _playerPositionTween?.Kill();
            _playerPositionTween = Player.CreateTween();
            _playerPositionTween.TweenProperty(
                Player,
                "position",
                (Global.GetTileAtPos(GetGlobalMousePosition()) + Global.SpriteOffset),
                (Global.GetTileAtPos(GetGlobalMousePosition()) + Global.SpriteOffset).DistanceTo(
                    Player.Position
                ) / Global.PlayerSpeed
            );
        }
        // Change the selected tool
        else if (@event is InputEventMouseButton eventMouseButton && eventMouseButton.Pressed)
        {
            if (eventMouseButton.ButtonIndex == MouseButton.WheelUp)
            {
                do
                {
                    _selectedToolIndex = (_selectedToolIndex + 1) % _tools.Count;
                } while (_tools[_selectedToolIndex].IsDisabled);
            }
            else if (eventMouseButton.ButtonIndex == MouseButton.WheelDown)
            {
                do
                {
                    _selectedToolIndex = (_selectedToolIndex - 1 + _tools.Count) % _tools.Count;
                } while (_tools[_selectedToolIndex].IsDisabled);
            }
            GetNode<Label>("%ToolLabel").Text =
                "Current Tool: " + _tools[_selectedToolIndex].ToolName;
        }
    }
}
