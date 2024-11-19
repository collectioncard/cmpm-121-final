using Godot;
using System;

public partial class TileGrid : TileMapLayer
{
	private int _day = 0;
	private Plant[] _plants;
	private BetterTerrain _bt;
	private PlantTile[,] _plantGrid = new PlantTile[Global.TileMapSize.X, Global.TileMapSize.Y];
	
	[Signal]
	public delegate void DayPassedEventHandler(int prevDay);
	
	private void InitPlantGrid()
	{
		for (int x = 0; x < Global.TileMapSize.X; x++)
		{
			for (int y = 0; y < Global.TileMapSize.Y; y++)
			{
				_plantGrid[x, y] = new PlantTile();
				AddChild(_plantGrid[x, y]);
				_plantGrid[x, y].Init(x, y, _bt.GetCell(new Vector2I(x, y)));
			}
		}
	}
	
	public override void _Ready()
	{
		_bt = new BetterTerrain(this);
		InitPlantGrid();

		/* Rework Later
		 Define plants ?elsewhere? then read and generate plant classes*/
		Plant p1 = new Plant();
		string[] plant1Textures = 
		{
			"res://assets/plants/plant1-1.png", "res://assets/plants/plant1-2.png", "res://assets/plants/plant1-3.png"
		};
		p1.Init("Plant1", plant1Textures, null, 1, 3);
		Array.Resize(ref _plants, 1);
		_plants[0] = p1;
	}

	public void TileClick(Vector2 pos)
	{
		Vector2I tilePos = LocalToMap(pos);
		if (_bt.GetCell(tilePos) != 5)
		{
			_bt.SetCell(tilePos, 5);
			_bt.UpdateTerrainCell(tilePos);
		}
		else
		{
			if (_plantGrid[tilePos.X, tilePos.Y].GetPlantType() == null)
			{
				_plantGrid[tilePos.X, tilePos.Y].SowPlant(_plants[0]);
				DayPassed += _plantGrid[tilePos.X, tilePos.Y].TurnDay;
			}
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
		}

		//Idea get sun and moisture by using pos x, y, day, plant type, soil type, and run seed as seed, so it is "random" but deterministic, instead of randomly doing it and loading for each tile.
		private string daySeedString(int day) //standardize method
		{
			return $"{Global.Seed}{day}{_plantType.Name}{_soilType}{_tilePos.X}{_tilePos.Y}";
		}
		private float randomLevel(int day) {
			double floatValue = new Random(GD.Hash(daySeedString(day))).NextDouble();
			/*string baseString = $"{day}{_plantType.Name}{_soilType}{_tilePos.X}{_tilePos.Y}";
			float floatValue = 0;
			if (UInt32.TryParse(baseString, out uint uintValue)) {
				uintValue = uintValue << (day * _tilePos.X * _tilePos.Y) % 31;
				floatValue = (float)uintValue;
			}else {
				GD.Print("String can't be convereted to a Uint32");
			}
			floatValue /= (float)UInt32.MaxValue;*/
			return (float)floatValue;
		}
		
		private float CalcMoisture(int day)
		{
			_moistureLevel += randomLevel(day) * _soilType * .3f;
			return _moistureLevel;
		}
		private float CalcSun(int day)
		{
			return randomLevel(day);
		}

		public void SowPlant(Plant plantType)
		{
			_plantType = plantType;
			_growthStage = 0;
			Texture = GD.Load<Texture2D>(plantType.TexturePaths[_growthStage]);
			Position = new Vector2( _tilePos.X * Global.TileWidth, _tilePos.Y * Global.TileHeight) + Global.SpriteOffset;
		}

		public void TurnDay(int day)
		{
			if (_plantType.GrowthCheck(CalcSun(day), CalcMoisture(day), null)) 
			{
				Grow();
			}
		}

		private void Grow()
		{
			if (_growthStage >= _plantType.GrowthStages - 1) return;
			//TODO: Make growth reduce moisture?
			_growthStage++;
			Texture = GD.Load<Texture2D>(_plantType.TexturePaths[_growthStage]);
		}
		
		public Plant GetPlantType()
		{
			return _plantType;
		}
	}
}
