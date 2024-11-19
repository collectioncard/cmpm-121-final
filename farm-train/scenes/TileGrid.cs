using Godot;
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

public partial class TileGrid : TileMapLayer
{
	private int _day = 0;
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
				_plantGrid[x, y].Init(x, y, _bt.GetCell(new Vector2I(x, y)));
			}
		}
	}
	
	public override void _Ready()
	{
		_bt = new BetterTerrain(this);
		InitPlantGrid();
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
			if (_plantGrid[tilePos.X, tilePos.Y].GetPlantType() == -1)
			{
				Sprite2D temp = new Sprite2D();
				AddChild(temp);
				_plantGrid[tilePos.X, tilePos.Y].SowPlant(temp);
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

	private partial class PlantTile : Resource
	{
		private int _soilType;
		private int _plantType = -1;
		private Vector2I _tilePos;
		private Sprite2D _tile;
		private int _growthStage = -1;
		private float _moistureLevel = 0;
		
		public void Init(int x, int y, int soilTypeIn)
		{
			_soilType = soilTypeIn;
			_tilePos = new Vector2I(x, y);
		}
		
		//Currently purely random, will make these mostly deterministic for eventually
		//Idea get sun and moisture by using pos x, y, day, plant type, soil type, and run seed as seed, so it is "random" but deterministic, instead of randomly doing it and loading for each tile.
		private float randomLevel(int day) {
			string baseString = $"{day}{_plantType}{_soilType}{_tilePos.X}{_tilePos.Y}";
			float floatValue = 0;
			if (UInt32.TryParse(baseString, out uint uintValue)) {
				uintValue = uintValue << (day * _tilePos.X * _tilePos.Y) % 31;
				floatValue = (float)uintValue;
			}else {
				GD.Print("String can't be convereted to a Uint32");
			}
			floatValue /= (float)UInt32.MaxValue;
			return floatValue;
		}
		private float CalcMoisture(int day)
		{
			_moistureLevel += (randomLevel(day) * ((float)_soilType) * .3f);
			return _moistureLevel;
		}
		private float CalcSun(int day)
		{
			return randomLevel(day);
		}

		public void SowPlant(Sprite2D existingChild)
		{
			_tile = existingChild;
			_plantType = 0;
			if (randomLevel(50) > 0.5) {
				_plantType = 1;
				_tile.Texture = GD.Load("res://assets/plants/plant1-1.png") as Texture2D;
			} else {
				_plantType = 2;
				_tile.Texture = GD.Load("res://assets/plants/plant2-1.png") as Texture2D;
			}
			GD.Print(randomLevel(50));
			_growthStage = 1;
			_tile.Position = new Vector2( _tilePos.X * Global.TileWidth, _tilePos.Y * Global.TileHeight) + Global.SpriteOffset;
		}

		public void TurnDay(int day)
		{
			float sun = CalcSun(day);
			float moisture = CalcMoisture(day);
			switch (_growthStage) {
				case 1:
					if (sun > 0.5 && moisture > 0.5) {
						Grow();
					}
					break;
				case 2:
					if (sun > 0.75 && moisture > 0.75) {
						Grow();
					}
					break;
			}
		}

		private void Grow()
		{
			_growthStage++;
			switch (_plantType) {
				case 1:
					switch (_growthStage) {
						case 2:
							_tile.Texture = GD.Load("res://assets/plants/plant1-2.png") as Texture2D;
							break;
						case 3:
							_tile.Texture = GD.Load("res://assets/plants/plant1-3.png") as Texture2D;
							break;
					}
					break;
				case 2:
					switch (_growthStage) {
						case 2:
							_tile.Texture = GD.Load("res://assets/plants/plant2-2.png") as Texture2D;
							break;
						case 3:
							_tile.Texture = GD.Load("res://assets/plants/plant2-3.png") as Texture2D;
							break;
					}
					break;
			}
		}
		
		public int GetPlantType()
		{
			return _plantType;
		}
	}
}
