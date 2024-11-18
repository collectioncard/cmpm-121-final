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
		
		public void Init(int x, int y, int soilTypeIn)
		{
			_soilType = soilTypeIn;
			_tilePos = new Vector2I(x, y);
		}
		
		//Currently purely random, will make these mostly deterministic for eventually
		//Idea get sun and moisture by using pos x, y, day, plant type, soil type, and run seed as seed, so it is "random" but deterministic, instead of randomly doing it and loading for each tile.
		private float CalcMoisture(int day)
		{
			return GD.Randf();
		}
		private float CalcSun(int day)
		{
			return GD.Randf();
		}

		public void SowPlant(Sprite2D existingChild)
		{
			_plantType = 1;
			_growthStage = 1;
			_tile = existingChild;
			_tile.Texture = GD.Load("res://assets/plants/plant1-1.png") as Texture2D;
			_tile.Position = new Vector2( _tilePos.X * Global.TileWidth, _tilePos.Y * Global.TileHeight) + Global.SpriteOffset;
		}

		public void TurnDay(int day)
		{
			if (CalcSun(day) > .1 && CalcMoisture(day)	> .1)
			{
				Grow();
			}
		}

		private void Grow()
		{
			_growthStage++;
			_tile.Texture = GD.Load("res://assets/plants/plant1-2.png") as Texture2D;
		}
		
		public int GetPlantType()
		{
			return _plantType;
		}
	}
}
