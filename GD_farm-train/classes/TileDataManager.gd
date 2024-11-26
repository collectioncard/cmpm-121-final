class_name TileDataManager extends Resource

const DATA_SIZE = 4;


# This isnt compile time constant because GDscript doesnt support that. Please dont add anything to this enum
# during runtime.
enum properties {
	SOIL_TYPE = 1 * DATA_SIZE,
	PLANT_ID = 2 * DATA_SIZE,
	MOISTURE_LEVEL = 3 * DATA_SIZE,
	GROWTH_STAGE = 4 * DATA_SIZE,
	COORD_X = 5 * DATA_SIZE,
	COORD_Y = 6 * DATA_SIZE
}

var tile_info_size: int = properties.size() * DATA_SIZE;



# The GDscript packedByteArray that holds the tile data

@export var tile_data : PackedByteArray = PackedByteArray();


func _init():
	tile_data.resize(Global.TILE_MAP_SIZE.x * Global.TILE_MAP_SIZE.y * tile_info_size);


func get_property_value_at_coord(property: properties, tilePos: Vector2i) -> int:
	return tile_data.decode_s32(Utils.index_from_vec(tilePos) * tile_info_size + property);



func get_tile_info_at_coord(tilePos) -> TileInfo:
	##TODO: Implement this function
	return null;

func set_property_value_at_coord(property, tilePos, value):
	tile_data.encode_s32(Utils.index_from_vec(tilePos) * tile_info_size + property, value);


func set_tile_info_at_coord(tilePos, tileInfo):
	#TODO: Implement this function
	pass;


func export_tile_data() -> PackedByteArray:
	#TODO: Implement this function
	return PackedByteArray();

func clear_tile_data():
	tile_data.clear();

func overwrite_tile_data(data: PackedByteArray):
	if (data.size() != tile_data.size()):
		push_error("Data size mismatch");
		return;

	tile_data = data.duplicate();



class TileInfo:
	var soil_type := 0;
	var plant_id := 0;
	var moisture_level := 0;
	var growth_stage := 0;
	var coord_x := 0;
	var coord_y := 0;

	func _init(soil_type, plant_id, moisture_level, growth_stage, coord_x, coord_y):
		self.soil_type = soil_type;
		self.plant_id = plant_id;
		self.moisture_level = moisture_level;
		self.growth_stage = growth_stage;
		self.coord_x = coord_x;
		self.coord_y = coord_y;


	func get_coordinates() -> Vector2:
		return Vector2(coord_x, coord_y);
