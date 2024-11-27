extends Node

func index_from_vec(tilePos : Vector2i) -> int:
	if (
			tilePos.x < 0
			|| tilePos.y < 0
			|| tilePos.x >= Global.TILE_MAP_SIZE.x
			|| tilePos.y >= Global.TILE_MAP_SIZE.y
		):
			push_error("Invalid tilepos OOB");
	return tilePos.x + (tilePos.y * Global.TILE_MAP_SIZE.x);

func vec_from_idx(idx: int) -> Vector2i:
	if (idx < 0 || idx >= Global.TILE_MAP_SIZE.x * Global.TILE_MAP_SIZE.y):
		push_error("Invald idx OOB");
	return Vector2i(idx % Global.TILE_MAP_SIZE.x, idx / Global.TILE_MAP_SIZE.x);

func tile_from_vec(globalPos : Vector2i) -> Vector2i:
		var x : int = floor((globalPos.x) / Global.TILE_WIDTH) * Global.TILE_WIDTH;
		var y : int = floor((globalPos.y) / Global.TILE_HEIGHT) * Global.TILE_HEIGHT;
		return Vector2i(x, y);
