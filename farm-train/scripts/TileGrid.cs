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
        public Vector2I Position { get; init; }
        public int SoilType { get; init; } //!!

        public int PlantIndex { get; set; } //!!
        public float Moisture { get; set; } //!!
        public int GrowthStage { get; set; } //!!

        public TileInfo()
        {
            Position = default;
            PlantIndex = -1;
            SoilType = 0;
            Moisture = 0;
            GrowthStage = -1;
        }

        public Plant GetPlantType()
        {
            return PlantIndex == -1 ? null : Global._plants[PlantIndex];
        }

        public bool IsMature()
        {
            return PlantIndex != -1 && GrowthStage == GetPlantType().GrowthStages;
        }

        #region Daily Conditions
        public int TurnDay(int day)
        {
            if (PlantIndex == -1)
                return 0;
            if (GetPlantType() == Plant.CROSSBREED)
            {
                return Propagate(day);
            }
            Grow(day);
            return 0;
        }

        private void Grow(int day)
        {
            if (IsMature())
                return;
            if (!GetPlantType().GrowthCheck(CalcSun(day), CalcMoisture(day)))
                return;

            GD.Print("Grow");
            GD.Print(GrowthStage);

            //TODO: Make growth reduce moisture?
            GrowthStage++;
            ContiguousTileInfo.CurState.UpdateProperty(
                this,
                ContiguousTileInfo.Properties.GrowthStage
            );
            TileSprites[CoordToIndex(Position)].Texture = GD.Load<Texture2D>(
                GetPlantType().TexturePaths[GrowthStage - 1]
            );
        }

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
            ContiguousTileInfo.CurState.UpdateProperty(
                this,
                ContiguousTileInfo.Properties.Moisture
            );
            return Moisture;
        }

        private float CalcSun(int day)
        {
            return RandomLevel(day);
        }
        #endregion
        //Performs clone or crossbreed event based on neighbors
        private int Propagate(int day)
        {
            GD.Print("Propagate attempt!");
            if (RandomLevel(day) < .5f) //only do a propagation attempt 50% of the time
                return -1;
            TileInfo[] neighbors =
            {
                ContiguousTileInfo.CurState.GetTileInfo(Position + Vector2I.Left),
                ContiguousTileInfo.CurState.GetTileInfo(Position + Vector2I.Right),
                ContiguousTileInfo.CurState.GetTileInfo(Position + Vector2I.Up),
                ContiguousTileInfo.CurState.GetTileInfo(Position + Vector2I.Down),
            };
            neighbors = neighbors.Where(tile => tile.IsMature()).ToArray(); //out of the non-null neighbors only mature are valid
            switch (neighbors.Length)
            {
                //no valid = end attempt
                case 0:
                    return -1;
                //only 1 valid causes a cloning event
                case 1:
                    SowPlant(neighbors[0].GetPlantType());
                    break;
                //>= 2 causes a random pair of parents and offspring to be picked
                case > 1:
                    neighbors = neighbors.OrderBy((e) => Global.Rng.Randf()).ToArray();
                    SowPlant(
                        Plant.ChooseOffspring(
                            neighbors[0].GetPlantType(),
                            neighbors[1].GetPlantType()
                        )
                    );
                    break;
            }
            return GetPlantType().GetTypeName();
        }

        public void SowPlant(Plant plantType)
        {
            PlantIndex = plantType.GetTypeName();
            GrowthStage = 1;
            TileSprites[CoordToIndex(Position)].Texture = GD.Load<Texture2D>(
                plantType.TexturePaths[GrowthStage - 1]
            );
            TileSprites[CoordToIndex(Position)].Position =
                new Vector2(Position.X * Global.TileWidth, Position.Y * Global.TileHeight)
                + Global.SpriteOffset;
            ContiguousTileInfo.CurState.SaveTileInfo(this);
        }

        public void Harvest()
        {
            // Logic to remove plant here
            PlantIndex = -1;
            GrowthStage = -1;
            TileSprites[CoordToIndex(Position)].Texture = null;
            ContiguousTileInfo.CurState.SaveTileInfo(this);
            //TODO: Tie into tool/inven system, return items? event listener?
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
        public enum Properties
        {
            SoilType,
            PlantIndex,
            Moisture,
            GrowthStage,
        }

        private static ContiguousTileInfo _instance;
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
        public const int SizeOfTileInfo = 16;
        private byte[] _state = new byte[
            (Global.TileMapSize.X * Global.TileMapSize.Y) * SizeOfTileInfo
        ];

        public void UpdateProperty(TileInfo tileInfo, Properties property)
        {
            switch (property)
            {
                case Properties.SoilType:
                    int sIdx = CoordToIndex(tileInfo.Position);
                    byte[] sVal = BitConverter.GetBytes(tileInfo.SoilType);
                    _state[sIdx * SizeOfTileInfo + 0] = sVal[0];
                    _state[sIdx * SizeOfTileInfo + 1] = sVal[1];
                    _state[sIdx * SizeOfTileInfo + 2] = sVal[2];
                    _state[sIdx * SizeOfTileInfo + 3] = sVal[3];
                    break;
                case Properties.PlantIndex:
                    int pIdx = CoordToIndex(tileInfo.Position);
                    byte[] pVal = BitConverter.GetBytes(tileInfo.PlantIndex);
                    _state[pIdx * SizeOfTileInfo + 4] = pVal[0];
                    _state[pIdx * SizeOfTileInfo + 5] = pVal[1];
                    _state[pIdx * SizeOfTileInfo + 6] = pVal[2];
                    _state[pIdx * SizeOfTileInfo + 7] = pVal[3];
                    break;
                case Properties.Moisture:
                    int mIdx = CoordToIndex(tileInfo.Position);
                    byte[] mVal = BitConverter.GetBytes(tileInfo.Moisture);
                    _state[mIdx * SizeOfTileInfo + 8] = mVal[0];
                    _state[mIdx * SizeOfTileInfo + 9] = mVal[1];
                    _state[mIdx * SizeOfTileInfo + 10] = mVal[2];
                    _state[mIdx * SizeOfTileInfo + 11] = mVal[3];
                    break;
                case Properties.GrowthStage:
                    int gIdx = CoordToIndex(tileInfo.Position);
                    byte[] gVal = BitConverter.GetBytes(tileInfo.GrowthStage);
                    _state[gIdx * SizeOfTileInfo + 12] = gVal[0];
                    _state[gIdx * SizeOfTileInfo + 13] = gVal[1];
                    _state[gIdx * SizeOfTileInfo + 14] = gVal[2];
                    _state[gIdx * SizeOfTileInfo + 15] = gVal[3];
                    break;
            }
        }

        public byte[] GetProperty(int idx, Properties property) //Returning int or float is scary. Returns bytes.
        {
            byte[] sVal = new byte[4];
            switch (property)
            {
                case Properties.SoilType:
                    sVal[0] = _state[idx * SizeOfTileInfo + 0];
                    sVal[1] = _state[idx * SizeOfTileInfo + 1];
                    sVal[2] = _state[idx * SizeOfTileInfo + 2];
                    sVal[3] = _state[idx * SizeOfTileInfo + 3];
                    break;
                case Properties.PlantIndex:
                    sVal[0] = _state[idx * SizeOfTileInfo + 4];
                    sVal[1] = _state[idx * SizeOfTileInfo + 5];
                    sVal[2] = _state[idx * SizeOfTileInfo + 6];
                    sVal[3] = _state[idx * SizeOfTileInfo + 7];
                    break;
                case Properties.Moisture:
                    sVal[0] = _state[idx * SizeOfTileInfo + 8];
                    sVal[1] = _state[idx * SizeOfTileInfo + 9];
                    sVal[2] = _state[idx * SizeOfTileInfo + 10];
                    sVal[3] = _state[idx * SizeOfTileInfo + 11];
                    break;
                case Properties.GrowthStage:
                    sVal[0] = _state[idx * SizeOfTileInfo + 12];
                    sVal[1] = _state[idx * SizeOfTileInfo + 13];
                    sVal[2] = _state[idx * SizeOfTileInfo + 14];
                    sVal[3] = _state[idx * SizeOfTileInfo + 15];
                    break;
            }
            return sVal;
        }

        public void SaveTileInfo(TileInfo tileInfo)
        {
            UpdateProperty(tileInfo, Properties.SoilType);
            UpdateProperty(tileInfo, Properties.PlantIndex);
            UpdateProperty(tileInfo, Properties.Moisture);
            UpdateProperty(tileInfo, Properties.GrowthStage);
        }

        public TileInfo GetTileInfo(Vector2I position)
        {
            return new TileInfo
            {
                Position = position,
                SoilType = BitConverter.ToInt32(_state, CoordToIndex(position) * SizeOfTileInfo),
                PlantIndex = BitConverter.ToInt32(
                    _state,
                    CoordToIndex(position) * SizeOfTileInfo + 4
                ),
                Moisture = BitConverter.ToSingle(
                    _state,
                    CoordToIndex(position) * SizeOfTileInfo + 8
                ),
                GrowthStage = BitConverter.ToInt32(
                    _state,
                    CoordToIndex(position) * SizeOfTileInfo + 12
                ),
            };
        }
    }

    private static Sprite2D[] TileSprites = new Sprite2D[
        Global.TileMapSize.X * Global.TileMapSize.Y
    ];

    private int _day;
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
                BitConverter.ToInt32(
                    ContiguousTileInfo.CurState.GetProperty(
                        idx,
                        ContiguousTileInfo.Properties.SoilType
                    )
                ) == 0
            )
                continue;
            if (
                BitConverter.ToInt32(
                    ContiguousTileInfo.CurState.GetProperty(
                        idx,
                        ContiguousTileInfo.Properties.PlantIndex
                    )
                ) == -1
            )
                continue;
            int queueUnlock = ContiguousTileInfo
                .CurState.GetTileInfo(indextoCoord(idx))
                .TurnDay(_day);
            EmitSignal(SignalName.Unlock, queueUnlock);
        }
        _day++;
    }

    [Signal]
    public delegate void UnlockEventHandler(int id);

    //Creates a plantTile, adds to tree and inits
    private TileInfo AddTile(int x, int y)
    {
        int idx = CoordToIndex(x, y);
        TileSprites[idx] = new Sprite2D();
        AddChild(TileSprites[idx]);

        var result = new TileInfo
        {
            Position = new Vector2I(x, y),
            PlantIndex = -1,
            SoilType = _bt.GetCell(new Vector2I(x, y)),
        };

        ContiguousTileInfo.CurState.SaveTileInfo(result);

        return result;
    }

    //Either returns the existing plantTile or returns a new one at x,y
    private TileInfo GetPlantTile(int x, int y)
    {
        return ContiguousTileInfo.CurState.GetTileInfo(new Vector2I(x, y));
    }

    //Returns plantTile it exists else returns null
    private TileInfo? QueryTileInfo(int x, int y)
    {
        if (x < 0 || x >= Global.TileMapSize.X || y < 0 || y >= Global.TileMapSize.Y)
            return null;
        return ContiguousTileInfo.CurState.GetTileInfo(new Vector2I(x, y));
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
                    curTile.Harvest();
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
                    ContiguousTileInfo.CurState.GetTileInfo(tilePos).SowPlant(Plant.CROSSBREED);
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
            DayPassed();
        }
    }

    private void PlantSeed(Vector2I tilePos, int plantIndex)
    {
        if (_bt.GetCell(tilePos) == 5 && GetPlantTile(tilePos.X, tilePos.Y).GetPlantType() == null)
        {
            TileInfo curTile = GetPlantTile(tilePos.X, tilePos.Y);
            curTile.SowPlant(Global._plants[plantIndex]);
        }
    }
}
