using System;
using System.Linq;
using FarmTrain.classes;
using Godot;
using Godot.NativeInterop;

public partial class TileGrid : TileMapLayer
{
    private int _day;
    private Plant[] _plants = new Plant[Global.PlantTypes]; //TODO: Look into Godot arrays/collections
    private BetterTerrain _bt;
    private readonly PlantTile[,] _plantGrid = new PlantTile[
        Global.TileMapSize.X,
        Global.TileMapSize.Y
    ];

    [Signal]
    public delegate void DayPassedEventHandler(int prevDay);

    [Signal]
    public delegate void UnlockEventHandler(int id);

    //Creates a plantTile, adds to tree and inits
    private PlantTile AddPlantTile(int x, int y)
    {
        _plantGrid[x, y] = new PlantTile();
        AddChild(_plantGrid[x, y]);
        _plantGrid[x, y].Init(x, y, _bt.GetCell(new Vector2I(x, y)));
        return _plantGrid[x, y];
    }

    //Either returns the existing plantTile or returns a new one at x,y
    private PlantTile GetPlantTile(int x, int y)
    {
        return _plantGrid[x, y] ?? AddPlantTile(x, y);
    }

    //Returns plantTile it exists else returns null
    private PlantTile QueryPlantTile(int x, int y)
    {
        if (x < 0 || x >= Global.TileMapSize.X || y < 0 || y >= Global.TileMapSize.Y)
            return null;
        return _plantGrid[x, y];
    }

    //Returns array of existing plantTiles or null
    private PlantTile[] QueryNeighborTiles(int x, int y)
    {
        PlantTile[] result =
        {
            QueryPlantTile(x - 1, y),
            QueryPlantTile(x, y - 1),
            QueryPlantTile(x + 1, y),
            QueryPlantTile(x, y + 1),
        };
        return result.Where(tile => tile != null).ToArray();
    }

    public override void _Ready()
    {
        _bt = new BetterTerrain(this);

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

    public void TileClick(Vector2 pos, Tool tool)
    {
        var tilePos = LocalToMap(pos);

        switch (tool.ToolName)
        {
            case "Open_Hand":
                if (
                    QueryPlantTile(tilePos.X, tilePos.Y) != null
                    && QueryPlantTile(tilePos.X, tilePos.Y).CanBeHarvested()
                )
                {
                    DayPassed -= _plantGrid[tilePos.X, tilePos.Y].TurnDay;
                    _plantGrid[tilePos.X, tilePos.Y].Harvest();
                }
                break;
            case "Hoe":
                if (_bt.GetCell(tilePos) != 5)
                {
                    AddPlantTile(tilePos.X, tilePos.Y);
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
                    _plantGrid[tilePos.X, tilePos.Y].SowPlant(Plant.CROSSBREED);
                    DayPassedEventHandler crossBreed = null; //TODO: Look back at this. Is there a better way? Main issue is if we want to disconnect on demand.
                    crossBreed = (day) =>
                    {
                        if (
                            _plantGrid[tilePos.X, tilePos.Y]
                                .Propagate(day, QueryNeighborTiles(tilePos.X, tilePos.Y))
                        )
                        {
                            EmitSignal(
                                SignalName.Unlock,
                                _plantGrid[tilePos.X, tilePos.Y].GetPlantType().GetTypeName()
                            );
                            DayPassed += _plantGrid[tilePos.X, tilePos.Y].TurnDay;
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
        if (
            QueryPlantTile(tilePos.X, tilePos.Y) == null
            || QueryPlantTile(tilePos.X, tilePos.Y).GetPlantType() != null
        )
            return;
        _plantGrid[tilePos.X, tilePos.Y].SowPlant(_plants[plantIndex]);
        DayPassed += _plantGrid[tilePos.X, tilePos.Y].TurnDay;
    }

    private partial class PlantTile : Sprite2D
    {
        private int _soilType;
        private Plant _plantType;
        private Vector2I _tilePos;
        private int _growthStage = -1;
        private float _moistureLevel;

        public void Init(int x, int y, int soilType)
        {
            _soilType = soilType;
            _tilePos = new Vector2I(x, y);
            _moistureLevel = 0;
            _growthStage = -1;
        }

        public int GetGrowthStage()
        {
            return _growthStage;
        }

        public Plant GetPlantType()
        {
            return _plantType;
        }

        public bool IsMature()
        {
            return _plantType != null && _growthStage == _plantType.GrowthStages;
        }

        #region Daily Conditions
        private void Grow()
        {
            if (_growthStage >= _plantType.GrowthStages)
                return;
            //TODO: Make growth reduce moisture?
            _growthStage++;
            Texture = GD.Load<Texture2D>(_plantType.TexturePaths[_growthStage - 1]);
        }

        public void TurnDay(int day)
        {
            if (_plantType.GrowthCheck(CalcSun(day), CalcMoisture(day)))
            {
                Grow();
            }
        }

        //Idea get sun and moisture by using pos x, y, day, plant type, soil type, and run seed as seed, so it is "random" but deterministic, instead of randomly doing it and loading for each tile.
        private string DaySeedString(int day) //standardize method for seeding
        {
            return $"{Global.Seed}{day}{_soilType}{_tilePos.X}{_tilePos.Y}";
        }

        private float RandomLevel(int day)
        {
            double result = new Random(GD.Hash(DaySeedString(day))).NextDouble();
            return (float)result;
        }

        private float CalcMoisture(int day)
        {
            //TODO: Soil types determine possible moisture vals
            _moistureLevel += RandomLevel(day) * _soilType * .3f;
            return _moistureLevel;
        }

        private float CalcSun(int day)
        {
            return RandomLevel(day);
        }
        #endregion

        //Performs clone or crossbreed event based on neighbors
        public bool Propagate(int day, PlantTile[] neighbors)
        {
            if (RandomLevel(day) < .5f) //only do a propagation attempt 50% of the time
                return false;
            PlantTile[] validNeighbors = neighbors.Where(tile => tile.IsMature()).ToArray(); //out of the non-null neighbors only mature are valid
            switch (validNeighbors.Length)
            {
                //no valid = end attempt
                case 0:
                    return false;
                //only 1 valid causes a cloning event
                case 1:
                    SowPlant(validNeighbors[0]._plantType);
                    break;
                //>= 2 causes a random pair of parents and offspring to be picked
                case > 1:
                    validNeighbors = validNeighbors.OrderBy(e => Global.Rng.Randf()).ToArray();
                    SowPlant(
                        Plant.ChooseOffspring(
                            validNeighbors[0]._plantType,
                            validNeighbors[1]._plantType
                        )
                    );
                    break;
            }
            return true;
        }

        public void SowPlant(Plant plantType)
        {
            _plantType = plantType;
            _growthStage = 1;
            Texture = GD.Load<Texture2D>(plantType.TexturePaths[_growthStage - 1]);
            Position =
                new Vector2(_tilePos.X * Global.TileWidth, _tilePos.Y * Global.TileHeight)
                + Global.SpriteOffset;
        }

        public void Harvest()
        {
            // Logic to remove plant here
            _plantType = null;
            _growthStage = -1;
            Texture = null;
            //TODO: Tie into tool/inven system, return items? event listener?
        }

        public bool CanBeHarvested()
        {
            // Logic to determine if a plant can be harvested here
            return _plantType != null && _growthStage == _plantType.GrowthStages;
        }
    }
}
