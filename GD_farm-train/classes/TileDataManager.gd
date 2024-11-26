class_name TileDataManager extends Resource

enum properties {SOIL_TYPE, PLANT_ID, MOISTURE_LEVEL, GROWTH_STAGE, COORD_X, COORD_Y}


func get_property_value_at_coord(property, tilePos) -> int:
	##TODO: Implement this function
	return -1;


func get_tile_info_at_coord(tilePos) -> TileInfo:
	##TODO: Implement this function
	return null;

func set_property_value_at_coord(property, tilePos, value):
	#TODO: Implement this function
	pass;

func set_tile_info_at_coord(tilePos, tileInfo):
	#TODO: Implement this function
	pass;


func export_tile_data() -> PackedByteArray:
	#TODO: Implement this function
	return PackedByteArray();

func clear_tile_data():
	#TODO: Implement this function
	pass;

func overwrite_tile_data(data):
	#TODO: Implement this function
	pass;



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
