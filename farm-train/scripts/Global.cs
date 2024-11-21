using System;
using Godot;

public partial class Global : Node
{
    //TODO: Spread these vars to the classes that actually need them
    public static uint Seed;
    public static RandomNumberGenerator Rng = new RandomNumberGenerator();
    public const int PlantTypes = 3;
    public const int TileWidth = 16;
    public const int TileHeight = 16;
    public static Vector2 SpriteOffset = new(8f, 8f);
    public const int PlayerSpeed = 100;
    public const float InteractionRadius = 4 * TileWidth;
    public static Vector2I TileMapSize = new(40, 23);

    public override void _Ready()
    {
        //Seed = GD.Randi();
        Seed = 1;
        Rng.Seed = Seed;
    }

    public static Vector2 GetTileAtPos(Vector2 globalPos)
    {
        double x = Math.Floor((globalPos.X) / TileWidth) * TileWidth;
        double y = Math.Floor((globalPos.Y) / TileHeight) * TileHeight;
        return new Vector2((float)x, (float)y);
    }
}
