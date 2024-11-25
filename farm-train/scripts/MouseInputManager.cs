// MouseInputManager.cs
using System;
using System.Collections.Generic;
using FarmTrain.classes;
using FarmTrain.scripts;
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

    public void ConnectToGrid()
    {
        TileClick += StateManager.CurTileGrid.TileClick;
        StateManager.CurTileGrid.Unlock += Unlock;
    }

    public override void _Ready()
    {
        StateManager.CurTileGrid = GetNode<TileGrid>("TileGrid");
        ConnectToGrid();
        _make_TileCursor();
        _make_Player();
        _initializeTools();
    }

    public void Unlock(int id)
    {
        GD.Print("Unlock: " + id);
        switch (id)
        {
            case 2:
                _tools[4].IsDisabled = false;
                GetNode<Win>("Win").playerWin();
                break;
        }
    }

    private void _initializeTools()
    {
        _tools = new List<Tool>
        {
            new Tool("Open_Hand", false, null),
            new Tool("Hoe", false, "res://assets/trowel.png"),
            new Tool("Cross_Breed_Tool", false, "res://assets/plants/idkwhattocallthis.png"),
            new Tool("Seed_One", false, "res://assets/plants/plant1-0.png"),
            new Tool("Seed_two", true, "res://assets/plants/plant2-0.png"),
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
            Player.FlipH = (GetGlobalMousePosition().X < Player.Position.X);
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Move the tile selector graphic
        if (@event is InputEventMouseMotion eventMouseMotion)
        {
            TileCursor.Position =
                Utils.GetTileAtPos(eventMouseMotion.Position) + Global.SpriteOffset;
            TileCursor.Visible =
                TileCursor.Position.DistanceTo(Player.Position) < Global.InteractionRadius;
        }
        // Move the player to the selected area
        else if (@event.IsActionPressed("action2"))
        {
            _movePlayer(GetGlobalMousePosition());
        }
        // Move the player by WASD or Arrow
        else if (@event.IsActionPressed("up"))
        {
            Vector2 newPosition = Player.Position + (Vector2.Up * Global.TileHeight);
            ;
            _movePlayer(newPosition);
        }
        else if (@event.IsActionPressed("left"))
        {
            Vector2 newPosition = Player.Position + (Vector2.Left * Global.TileWidth);
            ;
            _movePlayer(newPosition);
        }
        else if (@event.IsActionPressed("down"))
        {
            Vector2 newPosition = Player.Position + (Vector2.Down * Global.TileWidth);
            ;
            _movePlayer(newPosition);
        }
        else if (@event.IsActionPressed("right"))
        {
            Vector2 newPosition = Player.Position + (Vector2.Right * Global.TileWidth);
            ;
            _movePlayer(newPosition);
        }
        // Change the selected tool
        else if (@event.IsActionPressed("toolup"))
        {
            do
            {
                _selectedToolIndex = (_selectedToolIndex + 1) % _tools.Count;
            } while (_tools[_selectedToolIndex].IsDisabled);
            GetNode<Sprite2D>("%ToolTexture").Texture = _tools[_selectedToolIndex]._texture;
        }
        else if (@event.IsActionPressed("tooldown"))
        {
            do
            {
                _selectedToolIndex = (_selectedToolIndex - 1 + _tools.Count) % _tools.Count;
            } while (_tools[_selectedToolIndex].IsDisabled);
            GetNode<Sprite2D>("%ToolTexture").Texture = _tools[_selectedToolIndex]._texture;
        }
    }

    private void _movePlayer(Vector2 newPosition)
    {
        _playerPositionTween?.Kill();
        _playerPositionTween = Player.CreateTween();
        _playerPositionTween.TweenProperty(
            Player,
            "position",
            (Utils.GetTileAtPos(newPosition) + Global.SpriteOffset),
            (Utils.GetTileAtPos(newPosition) + Global.SpriteOffset).DistanceTo(Player.Position)
                / Global.PlayerSpeed
        );
        Player.FlipH = (GetGlobalMousePosition().X < Player.Position.X);
    }
}
