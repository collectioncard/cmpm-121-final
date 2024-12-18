class_name GameState extends Resource
@export var tile_info : PackedByteArray;
@export var day : int;

func create(data : PackedByteArray = [], _day : int = 0) -> GameState:
	if (data.is_empty()):
		tile_info = PackedByteArray();
		tile_info.resize(Global.TILE_MAP_SIZE.x * Global.TILE_MAP_SIZE.y * 16); #tile_info_size isn't const, so cant grab here.
	else:
		tile_info = data.duplicate();
	day = _day;
	return self;
