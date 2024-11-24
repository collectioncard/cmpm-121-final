using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

public partial class GameState : Resource
{
    [Export]
    public byte[] TileInfo;

    [Export]
    public int Day = 0;
}
