using System;
using System.Xml;
using Godot;

public partial class StateManager : Node
{
    private const string SavePath = "user://Saves";
    private const string AutoSavePath = "/autoSave.tres";
    private static SaveFile _autoSave;
    private static SaveFile _currentSave;
    private const string SlotPath = "/Save"; //Save + slotnum + .tres

    public static TileGrid CurTileGrid = null;

    public override void _Ready()
    {
        DirAccess dir = DirAccess.Open(SavePath);
        if (dir == null)
        {
            dir = DirAccess.Open("user://");
            dir.MakeDir("Saves");
        }
        _autoSave = FileAccess.FileExists(SavePath + AutoSavePath)
            ? ResourceLoader.Load<SaveFile>(SavePath + AutoSavePath, "")
            : new SaveFile();
        ResourceSaver.Save(_autoSave, SavePath + AutoSavePath);

        _currentSave = _autoSave;
        CallDeferred("LoadState", _currentSave.CurrentState());
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("undo"))
        {
            LoadState(_currentSave.Undo());
            ResourceSaver.Save(_currentSave, SavePath + AutoSavePath);
        }
        else if (@event.IsActionPressed("redo"))
        {
            LoadState(_currentSave.Redo());
            ResourceSaver.Save(_currentSave, SavePath + AutoSavePath);
        }
    }

    public static void NewGame()
    {
        _currentSave = new SaveFile();
        LoadState(_currentSave.CurrentState());
    }

    public static void AutoSave(byte[] state, int day)
    {
        //Saves
        _currentSave.AddState(state, day);
        ResourceSaver.Save(_currentSave, SavePath + AutoSavePath);
        _autoSave.OverwriteWith(_currentSave);
    }

    public static void SaveToFile(int slot)
    {
        if (slot == 0)
        {
            ResourceSaver.Save(_currentSave.Duplicate(), SavePath + AutoSavePath);
            return;
        }
        ResourceSaver.Save(_currentSave.Duplicate(), SavePath + SlotPath + slot + ".tres");
    }

    public static void SaveFile(int fileNum) { }

    private static void LoadState(GameState loadState)
    {
        if (loadState == null)
            return;
        CurTileGrid.Reload(loadState.TileInfo, loadState.Day);
    }

    public static void LoadFromFile(int slot)
    {
        SaveFile tempSave;
        if (slot == 0)
        {
            tempSave = ResourceLoader.Load<SaveFile>(SavePath + AutoSavePath, "");
        }
        else
        {
            tempSave = FileAccess.FileExists(SavePath + SlotPath + slot + ".tres")
                ? ResourceLoader.Load<SaveFile>(SavePath + SlotPath + slot + ".tres", "")
                : new SaveFile();
        }
        _currentSave.OverwriteWith(tempSave);
        LoadState(tempSave.CurrentState());
    }
}
