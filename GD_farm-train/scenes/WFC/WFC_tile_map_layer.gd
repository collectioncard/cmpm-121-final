extends TileMapLayer

var tileset : TileSet = self.tile_set;
var tile : Vector2;

func _ready() -> void:
	Global.test_tilemap = self;
	tile = get_cell_atlas_coords(Vector2(0, 1))
	#1 rot TileSetAtlasSource.TRANSFORM_TRANSPOSE | TileSetAtlasSource.TRANSFORM_FLIP_V
	#2 rot TileSetAtlasSource.TRANSFORM_FLIP_H | TileSetAtlasSource.TRANSFORM_FLIP_V
	#3 rot TileSetAtlasSource.TRANSFORM_TRANSPOSE | TileSetAtlasSource.TRANSFORM_FLIP_H
	#refl  TileSetAtlasSource.TRANSFORM_FLIP_H
	#1 rot refl TileSetAtlasSource.TRANSFORM_TRANSPOSE | TileSetAtlasSource.TRANSFORM_FLIP_H | TileSetAtlasSource.TRANSFORM_FLIP_V
	#2 rot refl TileSetAtlasSource.TRANSFORM_FLIP_V
	#3 rot refl TileSetAtlasSource.TRANSFORM_TRANSPOSE

	#set_cell(Vector2(0, 0), 0, Vector2(2, 0));
	#tile = get_cell_tile_data(Vector2(0, 0));
	#print_debug(tile);
