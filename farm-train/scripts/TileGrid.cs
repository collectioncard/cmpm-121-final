using System;
using System.Linq;
using FarmTrain.classes;
using Godot;
using Godot.NativeInterop;

public partial class TileGrid : TileMapLayer
{
    //continuous thingy
    protected struct TileInfo
    {
        public Vector2I Position { get; set; }
        public int SoilType { get; set; } //!!

        public Sprite2D PlantSprite { get; set; }

        public int PlantIndex { get; set; } //!!
        public float Moisture { get; set; } //!!
        public int GrowthStage { get; set; } //!!

        public DayPassedEventHandler TurnDay;

        public Plant GetPlantType()
        {
            return PlantIndex == -1 ? null : Global._plants[PlantIndex];
        }

        public bool IsMature()
        {
            return PlantIndex != -1 && GrowthStage == GetPlantType().GrowthStages;
        }

        #region Daily Conditions

        public void Grow()
        {
            if (IsMature())
                return;
            GD.Print("Grow");
            GD.Print(GrowthStage);
            //TODO: Make growth reduce moisture?
            GrowthStage++;
            PlantSprite.Texture = GD.Load<Texture2D>(GetPlantType().TexturePaths[GrowthStage - 1]);
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

        public float CalcMoisture(int day)
        {
            //TODO: Soil types determine possible moisture vals
            Moisture += RandomLevel(day) * SoilType * .3f;
            return Moisture;
        }

        public float CalcSun(int day)
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

    protected class ContiguousTileInfo
    {
        /*
        public int SoilType { get; set; } //!! -- 4 (idx + 0)
        public int PlantIndex { get; set; } //!! -- 4 (idx + 4)
        public float Moisture { get; set; } //!! -- 4 (idx + 8)
        public int GrowthStage { get; set; } //!! -- 4 (idx + 12)
        */
        private static ContiguousTileInfo _instance = null;
        public static ContiguousTileInfo CurState
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ContiguousTileInfo();
                }
                return _instance;
            }
        }
        private const int SizeOfTileInfo = 16;
        private byte[] _state = new byte[
            (Global.TileMapSize.X * Global.TileMapSize.Y) * SizeOfTileInfo
        ];

        public void UpdateProperty(TileInfo tileInfo, string property)
        {
            switch (property)
            {
                case "soiltype":
                    int sIdx = CoordToIndex(tileInfo.Position);
                    byte[] sVal = BitConverter.GetBytes(tileInfo.SoilType);
                    _state[sIdx + 0] = sVal[0];
                    _state[sIdx + 1] = sVal[1];
                    _state[sIdx + 2] = sVal[2];
                    _state[sIdx + 3] = sVal[3];
                    break;
                case "plantindex":
                    int pIdx = CoordToIndex(tileInfo.Position);
                    byte[] pVal = BitConverter.GetBytes(tileInfo.PlantIndex);
                    _state[pIdx + 4] = pVal[0];
                    _state[pIdx + 5] = pVal[1];
                    _state[pIdx + 6] = pVal[2];
                    _state[pIdx + 7] = pVal[3];
                    break;
                case "moisture":
                    int mIdx = CoordToIndex(tileInfo.Position);
                    byte[] mVal = BitConverter.GetBytes(tileInfo.Moisture);
                    _state[mIdx + 8] = mVal[0];
                    _state[mIdx + 9] = mVal[1];
                    _state[mIdx + 10] = mVal[2];
                    _state[mIdx + 11] = mVal[3];
                    break;
                case "growthstage":
                    int gIdx = CoordToIndex(tileInfo.Position);
                    byte[] gVal = BitConverter.GetBytes(tileInfo.GrowthStage);
                    _state[gIdx + 12] = gVal[0];
                    _state[gIdx + 13] = gVal[1];
                    _state[gIdx + 14] = gVal[2];
                    _state[gIdx + 15] = gVal[3];
                    break;
            }
        }

        public void SaveTileInfo(TileInfo tileInfo)
        {
            UpdateProperty(tileInfo, "soiltype");
            UpdateProperty(tileInfo, "plantindex");
            UpdateProperty(tileInfo, "moisture");
            UpdateProperty(tileInfo, "growthstage");
        }

        public TileInfo GetTileInfo(Vector2I position)
        {
            return new TileInfo
            {
                Position = position,
                SoilType = BitConverter.ToInt32(_state, CoordToIndex(position)),
                PlantIndex = BitConverter.ToInt32(_state, CoordToIndex(position) + 4),
                Moisture = BitConverter.ToSingle(_state, CoordToIndex(position) + 8),
                GrowthStage = BitConverter.ToInt32(_state, CoordToIndex(position) + 12),
            };
        }
    }

    private TileInfo[] _tileInfo = new TileInfo[Global.TileMapSize.X * Global.TileMapSize.Y];

    private int _day;
    private BetterTerrain _bt;

    protected static int CoordToIndex(Vector2I coord)
    {
        return coord.X + coord.Y * Global.TileMapSize.X;
    }

    protected static int CoordToIndex(int x, int y)
    {
        return x + y * Global.TileMapSize.X;
    }

    [Signal]
    public delegate void DayPassedEventHandler(int prevDay);

    private DayPassedEventHandler CreateDayPassHandler(Vector2I pos)
    {
        return (day) =>
        {
            if (
                _tileInfo[CoordToIndex(pos)]
                    .GetPlantType()
                    .GrowthCheck(
                        _tileInfo[CoordToIndex(pos)].CalcSun(day),
                        _tileInfo[CoordToIndex(pos)].CalcMoisture(day)
                    )
            )
            {
                _tileInfo[CoordToIndex(pos)].Grow();
            }
        };
    }

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
    private ref TileInfo GetPlantTile(int x, int y)
    {
        return ref _tileInfo[CoordToIndex(x, y)];
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
        ContiguousTileInfo.CurState.SaveTileInfo(new TileInfo());
        _bt = new BetterTerrain(this);
        GD.Print("hre");
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
                    ref TileInfo curTile = ref GetPlantTile(tilePos.X, tilePos.Y);
                    curTile.Harvest();
                    DayPassed -= curTile.TurnDay;
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
            case "Seed_two":
                PlantSeed(tilePos, 2);
                break;
            case "Cross_Breed_Tool":
                if (
                    _bt.GetCell(tilePos) == 5
                    && GetPlantTile(tilePos.X, tilePos.Y).GetPlantType() == null
                )
                {
                    GD.Print("PLacing crossbreed");
                    _tileInfo[CoordToIndex(tilePos.X, tilePos.Y)].SowPlant(Plant.CROSSBREED);
                    DayPassedEventHandler crossBreed = null; //TODO: Look back at this. Is there a better way? Main issue is if we want to disconnect on demand.
                    crossBreed = (day) =>
                    {
                        GD.Print("Crossbreed event");
                        if (
                            GetPlantTile(tilePos.X, tilePos.Y)
                                .Propagate(day, QueryNeighborTiles(tilePos.X, tilePos.Y))
                        )
                        {
                            EmitSignal(
                                SignalName.Unlock,
                                GetPlantTile(tilePos.X, tilePos.Y).GetPlantType().GetTypeName()
                            );
                            _tileInfo[CoordToIndex(tilePos.X, tilePos.Y)].TurnDay =
                                CreateDayPassHandler(tilePos);
                            DayPassed += _tileInfo[CoordToIndex(tilePos.X, tilePos.Y)].TurnDay;
                            DayPassed -= crossBreed;
                        }
                    };
                    DayPassed += crossBreed;
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
            ref TileInfo curTile = ref GetPlantTile(tilePos.X, tilePos.Y);
            curTile.SowPlant(Global._plants[plantIndex]);

            curTile.TurnDay = CreateDayPassHandler(curTile.Position);
            DayPassed += curTile.TurnDay;
        }
    }
}
