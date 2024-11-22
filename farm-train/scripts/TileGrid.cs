using System;
using System.Linq;
using FarmTrain.classes;
using Godot;
using Godot.NativeInterop;

public partial class TileGrid : TileMapLayer
{
    //continuous thingy
    public struct TileInfo
    {
        public Vector2I Position { get; set; }
        public int SoilType { get; set; }

        public Sprite2D PlantSprite { get; set; }

        public int PlantIndex { get; set; }
        public float Moisture { get; set; }
        public int GrowthStage { get; set; }

        public Plant GetPlantType()
        {
            return PlantIndex == -1 ? null : Global._plants[PlantIndex];
        }

        public bool IsMature()
        {
            return PlantIndex != -1 && GrowthStage == GetPlantType().GrowthStages;
        }

        #region Daily Conditions
        private void Grow()
        {
            if (IsMature())
                return;
            //TODO: Make growth reduce moisture?
            GrowthStage++;
            PlantSprite.Texture = GD.Load<Texture2D>(GetPlantType().TexturePaths[GrowthStage - 1]);
        }

        public (bool, TileInfo) TurnDay(int day)
        {
            if (GetPlantType().GrowthCheck(CalcSun(day), CalcMoisture(day)))
            {
                Grow();
            }

            return (IsMature(), this);
        }

        //Idea get sun and moisture by using pos x, y, day, plant type, soil type, and run seed as seed, so it is "random" but deterministic, instead of randomly doing it and loading for each tile.
        private string DaySeedString(int day) //standardize method for seeding
        {
            return $"{Global.Seed}{day}{SoilType}{Position.X}{Position.Y}";
        }

        private float RandomLevel(int day)
        {
            double result = new Random(GD.Hash(DaySeedString(day))).NextDouble();
            return (float)result;
        }

        private float CalcMoisture(int day)
        {
            //TODO: Soil types determine possible moisture vals
            Moisture += RandomLevel(day) * SoilType * .3f;
            return Moisture;
        }

        private float CalcSun(int day)
        {
            return RandomLevel(day);
        }
        #endregion

        //Performs clone or crossbreed event based on neighbors
        public bool Propagate(int day, TileInfo[] neighbors)
        {
            GD.Print("Propagate attempt!");
            if (RandomLevel(day) < .5f) //only do a propagation attempt 50% of the time
                return false;
            TileInfo[] validNeighbors = neighbors.Where(tile => tile.IsMature()).ToArray(); //out of the non-null neighbors only mature are valid
            switch (validNeighbors.Length)
            {
                //no valid = end attempt
                case 0:
                    return false;
                //only 1 valid causes a cloning event
                case 1:
                    SowPlant(validNeighbors[0].GetPlantType());
                    break;
                //>= 2 causes a random pair of parents and offspring to be picked
                case > 1:
                    validNeighbors = validNeighbors.OrderBy(e => Global.Rng.Randf()).ToArray();
                    SowPlant(
                        Plant.ChooseOffspring(
                            validNeighbors[0].GetPlantType(),
                            validNeighbors[1].GetPlantType()
                        )
                    );
                    break;
            }
            return true;
        }

        public void SowPlant(Plant plantType)
        {
            PlantIndex = plantType.GetTypeName();
            GrowthStage = 1;
            PlantSprite.Texture = GD.Load<Texture2D>(plantType.TexturePaths[GrowthStage - 1]);
            PlantSprite.Position =
                new Vector2(Position.X * Global.TileWidth, Position.Y * Global.TileHeight)
                + Global.SpriteOffset;
        }

        public TileInfo Harvest()
        {
            // Logic to remove plant here
            PlantIndex = -1;
            GrowthStage = -1;
            PlantSprite.Texture = null;
            //TODO: Tie into tool/inven system, return items? event listener?
            return this;
        }
    }

    private TileInfo[] _tileInfo = new TileInfo[Global.TileMapSize.X * Global.TileMapSize.Y];

    private int _day;
    private BetterTerrain _bt;

    private int CoordToIndex(int x, int y)
    {
        return x + y * Global.TileMapSize.X;
    }

    [Signal]
    public delegate void DayPassedEventHandler(int prevDay);

    [Signal]
    public delegate void UnlockEventHandler(int id);

    //Creates a plantTile, adds to tree and inits
    private TileInfo AddTile(int x, int y)
    {
        int idx = CoordToIndex(x, y);
        var temp = new Sprite2D();
        AddChild(temp);

        var result = new TileInfo
        {
            Position = new Vector2I(x, y),
            PlantIndex = -1,
            SoilType = _bt.GetCell(new Vector2I(x, y)),
            PlantSprite = temp,
        };

        _tileInfo[idx] = result;

        return result;
    }

    //Either returns the existing plantTile or returns a new one at x,y
    private TileInfo GetPlantTile(int x, int y)
    {
        return _tileInfo[CoordToIndex(x, y)];
    }

    //Returns plantTile it exists else returns null
    private TileInfo? QueryTileInfo(int x, int y)
    {
        if (x < 0 || x >= Global.TileMapSize.X || y < 0 || y >= Global.TileMapSize.Y)
            return null;
        return _tileInfo[CoordToIndex(x, y)];
    }

    //Returns array of existing infoTiles
    private TileInfo[] QueryNeighborTiles(int x, int y)
    {
        TileInfo?[] result =
        {
            QueryTileInfo(x - 1, y),
            QueryTileInfo(x, y - 1),
            QueryTileInfo(x + 1, y),
            QueryTileInfo(x, y + 1),
        };

        return result.Where(tile => tile.HasValue).Select(tile => tile.Value).ToArray();
    }

    public override void _Ready()
    {
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
                    && QueryTileInfo(tilePos.X, tilePos.Y)?.IsMature() == true
                )
                {
                    GD.Print("Harvest!");
                    TileInfo curTile = GetPlantTile(tilePos.X, tilePos.Y);
                    //DayPassed -= curTile.TurnDay;
                    var newTile = curTile.Harvest();
                    _tileInfo[CoordToIndex(tilePos.X, tilePos.Y)] = newTile;
                }
                break;
            case "Hoe":
                if (_bt.GetCell(tilePos) != 5)
                {
                    AddTile(tilePos.X, tilePos.Y);
                    _bt.SetCell(tilePos, 5);
                    _bt.UpdateTerrainCell(tilePos);
                }
                break;
            case "Seed_One":
                // Only place if the tile is null plant
                PlantSeed(tilePos, 1);
                break;
            case "Seed_Twp":
                PlantSeed(tilePos, 2);
                break;
            case "Cross_Breed_Tool":
                if (
                    _bt.GetCell(tilePos) == 5
                    && GetPlantTile(tilePos.X, tilePos.Y).GetPlantType() == null
                )
                {
                    GD.Print("PLacing crossbreed");
                    TileInfo curTile = GetPlantTile(tilePos.X, tilePos.Y);
                    curTile.SowPlant(Plant.CROSSBREED);
                    DayPassedEventHandler crossBreed = null; //TODO: Look back at this. Is there a better way? Main issue is if we want to disconnect on demand.
                    crossBreed = (day) =>
                    {
                        if (curTile.Propagate(day, QueryNeighborTiles(tilePos.X, tilePos.Y)))
                        {
                            EmitSignal(SignalName.Unlock, curTile.GetPlantType().GetTypeName());
                            DayPassedEventHandler dayPassed = null; //TODO: Look back at this. Is there a better way? Main issue is if we want to disconnect on demand.
                            dayPassed = (newDay) =>
                            {
                                var (isMature, newTileInfo) = curTile.TurnDay(newDay);
                                _tileInfo[CoordToIndex(tilePos.X, tilePos.Y)] = newTileInfo;
                                if (isMature)
                                    DayPassed -= dayPassed;
                            };
                            DayPassed += dayPassed;
                            DayPassed -= crossBreed;
                        }
                    };
                    DayPassed += crossBreed;
                    _tileInfo[CoordToIndex(tilePos.X, tilePos.Y)] = curTile;
                }
                break;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("nextDay"))
        {
            _day++;
            GetNode<Label>("%DayLabel").Text = "Day: " + _day;
            EmitSignal(SignalName.DayPassed, _day);
        }
    }

    private void PlantSeed(Vector2I tilePos, int plantIndex)
    {
        if (_bt.GetCell(tilePos) == 5 && GetPlantTile(tilePos.X, tilePos.Y).GetPlantType() == null)
        {
            TileInfo curTile = GetPlantTile(tilePos.X, tilePos.Y);
            curTile.SowPlant(Global._plants[plantIndex]);
            DayPassedEventHandler dayPassed = null; //TODO: Look back at this. Is there a better way? Main issue is if we want to disconnect on demand.
            dayPassed = (day) =>
            {
                var (isMature, newTileInfo) = curTile.TurnDay(day);
                _tileInfo[CoordToIndex(tilePos.X, tilePos.Y)] = newTileInfo;
                if (isMature)
                    DayPassed -= dayPassed;
            };
            DayPassed += dayPassed;
            _tileInfo[CoordToIndex(tilePos.X, tilePos.Y)] = curTile;
        }
    }
}
