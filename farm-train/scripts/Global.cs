using Godot;
using System;

public partial class Global : Node
{
    public const int TileWidth = 16;
    public const int TileHeight = 16;
    public static Vector2 SpriteOffset = new (8f, 8f);
    public const int PlayerSpeed = 100;
    public const float InteractionRadius = 4 * TileWidth;
    public static Vector2I TileMapSize = new (40, 23);

    public static Vector2 GetTiledCoordsAtPos(Vector2 globalPos)
    {
        double x = Math.Floor((globalPos.X) / TileWidth) * TileWidth;
        double y = Math.Floor((globalPos.Y) / TileHeight) * TileHeight;
        return new Vector2((float)x, (float)y);
    }
}
