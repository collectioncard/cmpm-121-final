using Godot;
using System;

public partial class TileGrid : TileMapLayer
{
	private BetterTerrain _bt;
	public override void _Ready()
	{
		_bt = new BetterTerrain(this);
	}

	public void TileClick(Vector2 pos)
	{
		GD.Print((Vector2I)Global.GetTileAtPos(pos));
		_bt.SetCell(LocalToMap(pos), 5);
		_bt.UpdateTerrainCell(LocalToMap(pos));
	}
}
