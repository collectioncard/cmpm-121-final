using System;
using Godot;

public partial class Global : Node
{
    //TODO: Spread these vars to the classes that actually need them
    public static uint Seed;
    public static readonly Plant[] _plants = new Plant[PlantTypes];
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

        /* TODO: Rework Later
         Define plants ?elsewhere? then read and generate plant classes*/
        _plants[0] = Plant.CROSSBREED;
        _plants[1] = new Plant();
        string[] plant1Textures =
        {
            "res://assets/plants/plant1-1.png",
            "res://assets/plants/plant1-2.png",
            "res://assets/plants/plant1-3.png",
        };
        _plants[1].Init(1, plant1Textures, 1, 3);
        _plants[2] = new Plant();
        string[] plant2Textures =
        {
            "res://assets/plants/plant2-1.png",
            "res://assets/plants/plant2-2.png",
            "res://assets/plants/plant2-3.png",
        };
        _plants[2].Init(2, plant2Textures, 1, 3);
        _plants[1].AddOffspring(_plants[2]);
    }

    public static Vector2 GetTileAtPos(Vector2 globalPos)
    {
        double x = Math.Floor((globalPos.X) / TileWidth) * TileWidth;
        double y = Math.Floor((globalPos.Y) / TileHeight) * TileHeight;
        return new Vector2((float)x, (float)y);
    }
}
