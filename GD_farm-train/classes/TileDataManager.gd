class_name TileDataManager extends Resource

# The size of our stored data in bytes.
# We're using signed 32 bit integers for each property, so this should be 4 bytes. (can't do sizeOf in GDscript)
const DATA_SIZE := 4;

# The size each tile takes up in the tile_data array. Used to calculate offsets n stuff.
# As of now, we have 6 properties, so this should be 6 * DATA_SIZE = 24 bytes per tile.
# In other words, each tile starts at multiples of 24 bytes in the tile_data array. (0, 24, 48, 72, etc)
var tile_info_size: int = properties.size() * DATA_SIZE;

# This isnt compile time constant because GDscript doesnt support that. Please *DO NOT* add anything to this enum during runtime.
# Cant enforce that though. :(
enum properties {
	SOIL_TYPE = 0 * DATA_SIZE,
	PLANT_ID = 1 * DATA_SIZE,
	MOISTURE_LEVEL = 2 * DATA_SIZE,
	GROWTH_STAGE = 3 * DATA_SIZE,
}

# The GDscript packedByteArray that holds the tile data
var tile_data : PackedByteArray = PackedByteArray();

func _init():
	tile_data.resize(Global.TILE_MAP_SIZE.x * Global.TILE_MAP_SIZE.y * tile_info_size);


####**** Get Tile Data ****####


#Gets the value of a specific property at a given coordinate.
#params:
#property: The property to retrieve the value for.
#tilePos: The position of the tile.
#returns:The value of the specified property at the given coordinate.
func get_property_value_at_coord(property: properties, tilePos: Vector2i) -> int:
	assert(Utils.index_from_vec(tilePos) >= 0 || Utils.index_from_vec(tilePos) < tile_data.size());
	return tile_data.decode_s32(Utils.index_from_vec(tilePos) * tile_info_size + property);

#Gets the tile info at a given coordinate.
#params:
#tilePos: The position of the tile in the world.
func get_tile_info_at_coord(tilePos: Vector2i) -> TileInfo:
	var tileInfo = TileInfo.new(
		get_property_value_at_coord(properties.SOIL_TYPE, tilePos),
		get_property_value_at_coord(properties.PLANT_ID, tilePos),
		get_property_value_at_coord(properties.MOISTURE_LEVEL, tilePos),
		get_property_value_at_coord(properties.GROWTH_STAGE, tilePos),
		tilePos.x,
		tilePos.y
	);

	return tileInfo;

####**** Set Tile Data ****####

#Sets the value of a specific property at a given coordinate.
#params:
#property: The property to set the value for.
#tilePos: The position of the tile in the world.
#value: The int value to set the property to.
func set_property_value_at_coord(property: properties, tilePos: Vector2i, value: int):
	#print_debug(tilePos)
	tile_data.encode_s32(Utils.index_from_vec(tilePos) * tile_info_size + property, value);


#Sets the tile info at a given coordinate.
#params:
#tilePos: The position of the tile in the world.
#tileInfo: The TileInfo object to set the tile to.
func set_tile_info_at_coord(tilePos: Vector2i, tileInfo: TileInfo):
	print_debug(tilePos);
	set_property_value_at_coord(properties.SOIL_TYPE, tilePos, tileInfo.soil_type);
	set_property_value_at_coord(properties.PLANT_ID, tilePos, tileInfo.plant_id);
	set_property_value_at_coord(properties.MOISTURE_LEVEL, tilePos, tileInfo.moisture_level);
	set_property_value_at_coord(properties.GROWTH_STAGE, tilePos, tileInfo.growth_stage);

####**** Export and Clear Tile Data ****####

func export_tile_data() -> PackedByteArray:
	return tile_data.duplicate();

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

	func _init(_soil_type, _plant_id, _moisture_level, _growth_stage, _coord_x, _coord_y):
		self.soil_type = _soil_type;
		self.plant_id = _plant_id;
		self.moisture_level = _moisture_level;
		self.growth_stage = _growth_stage;
		self.coord_x = _coord_x;
		self.coord_y = _coord_y;


	func get_coordinates() -> Vector2:
		return Vector2(coord_x, coord_y);
