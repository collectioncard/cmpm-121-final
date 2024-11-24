using System;
using Godot;

public partial class StateManager : Node
{
    private const string SavePath = "res://Saves/save1.tres";
    private static SaveFile _save;

    public static TileGrid CurTileGrid = null;

    public override void _Ready()
    {
        _save = FileAccess.FileExists(SavePath)
            ? ResourceLoader.Load<SaveFile>(SavePath, "")
            : new SaveFile();
        ResourceSaver.Save(_save, SavePath);
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("undo"))
        {
            LoadState(_save.Undo());
            ResourceSaver.Save(_save, SavePath);
        }
        else if (@event.IsActionPressed("redo"))
        {
            LoadState(_save.Redo());
            ResourceSaver.Save(_save, SavePath);
        }
    }

    public static void SaveState(byte[] state, int day)
    {
        //Saves
        _save.AddState(state, day);
        ResourceSaver.Save(_save, SavePath);
    }

    private static void LoadState(GameState loadState)
    {
        if (loadState == null)
            return;
        CurTileGrid.Reload(loadState.TileInfo, loadState.Day);
    }
}
