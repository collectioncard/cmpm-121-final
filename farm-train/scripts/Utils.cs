using System;
using Godot;

namespace FarmTrain.scripts;

public static class Utils
{
    public static int CoordsToIndex(Vector2I tilePos)
    {
        if (
            tilePos.X < 0
            || tilePos.Y < 0
            || tilePos.X >= Global.TileMapSize.X
            || tilePos.Y >= Global.TileMapSize.Y
        )
            throw new ArgumentOutOfRangeException(
                nameof(tilePos),
                "Tile position is out of bounds."
            );
        return tilePos.X + tilePos.Y * Global.TileMapSize.X;
    }

    public static Vector2I IndexToCoords(int idx)
    {
        if (idx < 0 || idx >= Global.TileMapSize.X * Global.TileMapSize.Y)
            throw new ArgumentOutOfRangeException(nameof(idx), "Index is out of bounds.");
        return new Vector2I(idx % Global.TileMapSize.X, idx / Global.TileMapSize.X);
    }

    public static Vector2 GetTileAtPos(Vector2 globalPos)
    {
        double x = Math.Floor((globalPos.X) / Global.TileWidth) * Global.TileWidth;
        double y = Math.Floor((globalPos.Y) / Global.TileHeight) * Global.TileHeight;
        return new Vector2((float)x, (float)y);
    }
}
