using System;
using System.Linq;
using System.Net;
using FarmTrain.classes;
using FarmTrain.scripts;
using Godot;
using Godot.NativeInterop;

public partial class TileGrid : TileMapLayer
{
    //continuous thingy
    public TileDataManager PlantDataManager = TileDataManager.Instance;

    public Plant GetPlantType(TileInfo tileInfo)
    {
        return tileInfo.PlantId == -1 || tileInfo.SoilType == 0
            ? null
            : Global.Plants[tileInfo.PlantId];
    }

    public bool IsMature(TileInfo tileInfo)
    {
        return tileInfo.PlantId > 0 && tileInfo.GrowthStage == GetPlantType(tileInfo).GrowthStages;
    }

    #region Daily Conditions
    public int TurnDay(int day, TileInfo tileInfo)
    {
        if (tileInfo.PlantId == -1)
            return 0;
        if (GetPlantType(tileInfo) == Plant.CROSSBREED)
        {
            return Propagate(tileInfo);
        }
        Grow(day, tileInfo);
        return 0;
    }

    private void Grow(int day, TileInfo tileInfo)
    {
        if (IsMature(tileInfo))
            return;
        if (!GetPlantType(tileInfo).GrowthCheck(CalcSun(tileInfo), CalcMoisture(tileInfo)))
            return;

        GD.Print("Grow");
        GD.Print(tileInfo.GrowthStage);

        //TODO: Make growth reduce moisture?
        PlantDataManager.SetPropertyValueAtCoord(
            TileDataManager.Properties.GrowthStage,
            tileInfo.GetCoordinates(),
            tileInfo.GrowthStage + 1
        );
        TileSprites[CoordToIndex(tileInfo.GetCoordinates())].Texture = GD.Load<Texture2D>(
            GetPlantType(tileInfo).TexturePaths[tileInfo.GrowthStage]
        );
    }

    private string DaySeedString(TileInfo tileInfo) //standardize method for seeding
    {
        return $"{Global.Seed}{_day}{tileInfo.SoilType}{tileInfo.CoordX}{tileInfo.CoordY}";
    }

    //Returns int between 0 and 100
    private int RandomLevel(TileInfo tileInfo)
    {
        double result = new Random(GD.Hash(DaySeedString(tileInfo))).NextDouble();
        return (int)(result * 100);
    }

    private int CalcMoisture(TileInfo tileInfo)
    {
        //TODO: Soil types determine possible moisture vals
        int result = tileInfo.MoistureLevel + RandomLevel(tileInfo) * tileInfo.SoilType * 3;
        PlantDataManager.SetPropertyValueAtCoord(
            TileDataManager.Properties.MoistureLevel,
            tileInfo.GetCoordinates(),
            result
        );
        return result;
    }

    private int CalcSun(TileInfo tileInfo)
    {
        return RandomLevel(tileInfo);
    }
    #endregion


    //Performs clone or crossbreed event based on neighbors
    private int Propagate(TileInfo tileInfo)
    {
        GD.Print("Propagate attempt!");
        if (RandomLevel(tileInfo) < .5f) //only do a propagation attempt 50% of the time
            return -1;
        TileInfo[] neighbors =
        {
            PlantDataManager.GetTileInfoAtCoord(tileInfo.GetCoordinates() + Vector2I.Left),
            PlantDataManager.GetTileInfoAtCoord(tileInfo.GetCoordinates() + Vector2I.Right),
            PlantDataManager.GetTileInfoAtCoord(tileInfo.GetCoordinates() + Vector2I.Up),
            PlantDataManager.GetTileInfoAtCoord(tileInfo.GetCoordinates() + Vector2I.Down),
        };
        neighbors = neighbors.Where(IsMature).ToArray(); //out of the non-null neighbors only mature are valid
        switch (neighbors.Length)
        {
            //no valid = end attempt
            case 0:
                return -1;
            //only 1 valid causes a cloning event
            case 1:
                SowPlant(tileInfo, GetPlantType(neighbors[0]));
                break;
            //>= 2 causes a random pair of parents and offspring to be picked
            case > 1:
                neighbors = neighbors.OrderBy((e) => Global.Rng.Randf()).ToArray();
                SowPlant(
                    tileInfo,
                    Plant.ChooseOffspring(GetPlantType(neighbors[0]), GetPlantType(neighbors[1]))
                );
                break;
        }
        return PlantDataManager.GetPropertyValueAtCoord(
            TileDataManager.Properties.PlantId,
            tileInfo.GetCoordinates()
        );
    }

    public void SowPlant(TileInfo tileInfo, Plant plantType)
    {
        int plantIndex = plantType.GetTypeName();
        PlantDataManager.SetPropertyValueAtCoord(
            TileDataManager.Properties.PlantId,
            tileInfo.GetCoordinates(),
            plantIndex
        );
        PlantDataManager.SetPropertyValueAtCoord(
            TileDataManager.Properties.GrowthStage,
            tileInfo.GetCoordinates(),
            1
        );
        if (TileSprites[CoordToIndex(tileInfo.GetCoordinates())] == null)
        {
            TileSprites[CoordToIndex(tileInfo.GetCoordinates())] = new Sprite2D();
            AddChild(TileSprites[CoordToIndex(tileInfo.GetCoordinates())]);
        }
        TileSprites[CoordToIndex(tileInfo.GetCoordinates())].Texture = GD.Load<Texture2D>(
            plantType.TexturePaths[0]
        );
        TileSprites[CoordToIndex(tileInfo.GetCoordinates())].Position =
            tileInfo.GetCoordinates() * Global.TileHeight + Global.SpriteOffset;
    }

    public void Harvest(TileInfo tileInfo)
    {
        // Logic to remove plant here
        PlantDataManager.SetPropertyValueAtCoord(
            TileDataManager.Properties.PlantId,
            tileInfo.GetCoordinates(),
            -1
        );
        PlantDataManager.SetPropertyValueAtCoord(
            TileDataManager.Properties.GrowthStage,
            tileInfo.GetCoordinates(),
            -1
        );
        TileSprites[CoordToIndex(tileInfo.GetCoordinates())].Texture = null;
        //TODO: Tie into tool/inven system, return items? event listener?
    }

    private static Sprite2D[] TileSprites = new Sprite2D[
        Global.TileMapSize.X * Global.TileMapSize.Y
    ];

    private static int _day;
    private BetterTerrain _bt;

    private static int CoordToIndex(Vector2I coord)
    {
        return coord.X + coord.Y * Global.TileMapSize.X;
    }

    private static int CoordToIndex(int x, int y)
    {
        return x + y * Global.TileMapSize.X;
    }

    private static Vector2I indextoCoord(int idx)
    {
        return new Vector2I(idx % Global.TileMapSize.X, idx / Global.TileMapSize.X);
    }

    private void DayPassed()
    {
        for (int idx = 0; idx < Global.TileMapSize.X * Global.TileMapSize.Y; idx++)
        {
            if (
                PlantDataManager.GetPropertyValueAtCoord(
                    TileDataManager.Properties.SoilType,
                    Utils.IndexToCoords(idx)
                ) == 0
            )
                continue;
            if (
                PlantDataManager.GetPropertyValueAtCoord(
                    TileDataManager.Properties.PlantId,
                    Utils.IndexToCoords(idx)
                ) == -1
            )
                continue;
            int queueUnlock = TurnDay(_day, PlantDataManager.GetTileInfoAtCoord(indextoCoord(idx)));
            EmitSignal(SignalName.Unlock, queueUnlock);
        }
        _day++;
        StateManager.AutoSave(PlantDataManager.ExportBytes(), _day);
    }

    [Signal]
    public delegate void UnlockEventHandler(int id);

    //Either returns the existing plantTile or returns a new one at x,y
    private TileInfo GetPlantTile(int x, int y)
    {
        return PlantDataManager.GetTileInfoAtCoord(new Vector2I(x, y));
    }

    //Returns plantTile it exists else returns null
    private TileInfo? QueryTileInfo(int x, int y)
    {
        if (x < 0 || x >= Global.TileMapSize.X || y < 0 || y >= Global.TileMapSize.Y)
            return null;
        TileInfo tileInfo = PlantDataManager.GetTileInfoAtCoord(new Vector2I(x, y));
        if (tileInfo.SoilType == 0)
            return null;
        return tileInfo;
    }

    private byte[] _initialTiles;

    public override void _Ready()
    {
        _initialTiles = GetTileMapDataAsArray();
        _bt = new BetterTerrain(this);
    }

    public void TileClick(Vector2 pos, Tool tool)
    {
        var tilePos = LocalToMap(pos);

        switch (tool.ToolName)
        {
            case "Open_Hand":
                if (
                    QueryTileInfo(tilePos.X, tilePos.Y).HasValue
                    && IsMature(GetPlantTile(tilePos.X, tilePos.Y)) == true
                )
                {
                    GD.Print("Harvest!");
                    TileInfo curTile = GetPlantTile(tilePos.X, tilePos.Y);
                    Harvest(curTile);
                    StateManager.AutoSave(PlantDataManager.ExportBytes(), _day);
                }
                break;
            case "Hoe":
                if (_bt.GetCell(tilePos) != 5)
                {
                    TileInfo newTile = new TileInfo(
                        _bt.GetCell(tilePos),
                        -1,
                        0,
                        0,
                        tilePos.X,
                        tilePos.Y
                    );
                    PlantDataManager.SetTileInfoAtCoord(newTile);
                    _bt.SetCell(tilePos, 5);
                    _bt.UpdateTerrainCell(tilePos);
                    StateManager.AutoSave(PlantDataManager.ExportBytes(), _day);
                }
                break;
            case "Seed_One":
                // Only place if the tile is null plant
                PlantSeed(tilePos, 1);
                break;
            case "Seed_two":
                PlantSeed(tilePos, 2);
                break;
            case "Cross_Breed_Tool":
                if (
                    _bt.GetCell(tilePos) == 5
                    && GetPlantType(GetPlantTile(tilePos.X, tilePos.Y)) == null
                )
                {
                    GD.Print("PLacing crossbreed");
                    SowPlant(PlantDataManager.GetTileInfoAtCoord(tilePos), Plant.CROSSBREED);
                    StateManager.AutoSave(PlantDataManager.ExportBytes(), _day);
                }
                break;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("nextDay"))
        {
            DayPassed();
            GetParent().GetNode<Label>("%DayLabel").Text = "Day: " + _day;
        }
    }

    private void PlantSeed(Vector2I tilePos, int plantIndex)
    {
        if (_bt.GetCell(tilePos) == 5 && GetPlantType(GetPlantTile(tilePos.X, tilePos.Y)) == null)
        {
            TileInfo curTile = GetPlantTile(tilePos.X, tilePos.Y);
            SowPlant(curTile, Global.Plants[plantIndex]);
            StateManager.AutoSave(PlantDataManager.ExportBytes(), _day);
        }
    }

    public void Reload(byte[] newState, int day)
    {
        //Reset tilemap
        SetTileMapDataFromArray(_initialTiles);
        foreach (Sprite2D sprite in TileSprites)
        {
            if (sprite != null)
            {
                sprite.Texture = null;
            }
        }
        //Copy conditions over
        _day = day;
        GetParent().GetNode<Label>("%DayLabel").Text = "Day: " + _day;
        if (newState == null)
        {
            PlantDataManager.Clear();
        }
        else
        {
            PlantDataManager.OverwriteData(newState);
        }
        for (int idx = 0; idx < Global.TileMapSize.X * Global.TileMapSize.Y; idx++)
        {
            //No tile
            if (
                PlantDataManager.GetPropertyValueAtCoord(
                    TileDataManager.Properties.SoilType,
                    indextoCoord(idx)
                ) == 0
            )
                continue;
            _bt.SetCell(indextoCoord(idx), 5);
            _bt.UpdateTerrainCell(indextoCoord(idx));
            //empty plant
            if (
                PlantDataManager.GetPropertyValueAtCoord(
                    TileDataManager.Properties.PlantId,
                    indextoCoord(idx)
                ) == -1
            )
                continue;
            //Get current tile, update sprites to match
            Vector2I tempPos = indextoCoord(idx);
            TileInfo curTile = GetPlantTile(tempPos.X, tempPos.Y);
            if (TileSprites[CoordToIndex(curTile.GetCoordinates())] == null)
            {
                TileSprites[CoordToIndex(curTile.GetCoordinates())] = new Sprite2D();
                AddChild(TileSprites[CoordToIndex(curTile.GetCoordinates())]);
            }
            TileSprites[CoordToIndex(curTile.GetCoordinates())].Texture = GD.Load<Texture2D>(
                GetPlantType(curTile).TexturePaths[curTile.GrowthStage - 1]
            );
            TileSprites[CoordToIndex(curTile.GetCoordinates())].Position =
                new Vector2(curTile.CoordX * Global.TileWidth, curTile.CoordY * Global.TileHeight)
                + Global.SpriteOffset;
        }
    }
}
