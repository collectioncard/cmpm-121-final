using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class SaveFile : Resource
{
    [Export]
    private Godot.Collections.Array<GameState> _undoStack =
        new Godot.Collections.Array<GameState>();

    [Export]
    private Godot.Collections.Array<GameState> _redoStack =
        new Godot.Collections.Array<GameState>();

    public void AddState(byte[] data, int day)
    {
        //Push new to state
        _undoStack.Add(new GameState { TileInfo = (byte[])data.Clone(), Day = day });
        _redoStack.Clear();
    }

    public GameState CurrentState()
    {
        if (_undoStack.Count == 0)
        {
            return new GameState();
        }
        return _undoStack.Last();
    }

    public GameState Undo()
    {
        if (_undoStack.Count == 0)
            return null;
        GameState temp = _undoStack.Last();
        _redoStack.Add(temp);
        _undoStack.RemoveAt(_undoStack.Count - 1);
        return _undoStack.Count == 0 ? new GameState() : _undoStack.Last();
    }

    public GameState Redo()
    {
        if (_redoStack.Count == 0)
            return null;
        GameState temp = _redoStack.Last();
        _undoStack.Add(temp);
        _redoStack.RemoveAt(_redoStack.Count - 1);
        return temp;
    }

    public void OverwriteWith(SaveFile writeFrom)
    {
        _undoStack = writeFrom._undoStack.Duplicate();
        _redoStack = writeFrom._redoStack.Duplicate();
    }
}
