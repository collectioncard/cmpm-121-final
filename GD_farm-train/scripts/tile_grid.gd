class_name TileGrid extends TileMapLayer

var PlantDataManager : TileDataManager = TileDataManager.new();
static var cur_day : int = 0;
var tile_sprites : Array[Sprite2D];


@warning_ignore("unused_signal")
signal Unlock;

#region Plants
func get_plant_type(tileInfo : TileDataManager.TileInfo) -> Plant:
	if (tileInfo.plant_id == -1 or tileInfo.soil_type == 0):
		return null;
	return Global.Plants[tileInfo.plant_id];
	
func is_mature(tileInfo : TileDataManager.TileInfo) -> bool:
	return tileInfo.plant_id > 0 && tileInfo.growth_stage == get_plant_type(tileInfo).growth_stages;

#region Daily Conditions
func turn_day(tileInfo : TileDataManager.TileInfo) -> int:
	if (tileInfo.plant_id == -1):
		return -1;
	if (get_plant_type(tileInfo) == Plant.CROSSBREED):
		return propagate(tileInfo);
	grow(tileInfo);
	return 0;

func grow(tileInfo : TileDataManager.TileInfo):
	if (is_mature(tileInfo)):
		return;
	if (!get_plant_type(tileInfo).growth_check(calc_sun(tileInfo), calc_moisture(tileInfo), calc_neighbors(tileInfo))):
		return;
	print_debug("Grow");
	print_debug("Current stage: " + str(tileInfo.growth_stage));
	PlantDataManager.set_property_value_at_coord(
		TileDataManager.properties.GROWTH_STAGE,
		tileInfo.get_coordinates(),
		tileInfo.growth_stage + 1
	)
	tile_sprites[Utils.index_from_vec(tileInfo.get_coordinates())].texture = load(get_plant_type(tileInfo).texture_paths[tileInfo.growth_stage]);
		
func days_seed_of(tileInfo: TileDataManager.TileInfo) -> String:
	var result : String = "{0}{1}{2}{3}{4}";
	return result.format([Global.Seed, cur_day, tileInfo.soil_type, tileInfo.coord_x, tileInfo.coord_y]);
func random_level(tileInfo : TileDataManager.TileInfo) -> int:
	var temp_rng : RandomNumberGenerator = RandomNumberGenerator.new();
	temp_rng.seed = hash(days_seed_of(tileInfo));
	return temp_rng.randi_range(0, 100);
	
func calc_sun(tileInfo : TileDataManager.TileInfo) -> int:
	return random_level(tileInfo);
func calc_moisture(tileInfo : TileDataManager.TileInfo) -> int:
	var result : int = tileInfo.moisture_level;
	result += random_level(tileInfo);
	PlantDataManager.set_property_value_at_coord(
		TileDataManager.properties.MOISTURE_LEVEL,
		tileInfo.get_coordinates(),
		result
	);
	return result;
func calc_neighbors(tileInfo : TileDataManager.TileInfo) -> Array[int]:
	var result : Array[int];
	if (BetterTerrain.get_cell(self, (tileInfo.get_coordinates() as Vector2i) + Vector2i.LEFT) == 5):
		result.push_back(PlantDataManager.get_property_value_at_coord(TileDataManager.properties.PLANT_ID, tileInfo.get_coordinates() + Vector2.LEFT));
	if (BetterTerrain.get_cell(self, (tileInfo.get_coordinates() as Vector2i) + Vector2i.UP) == 5):
		result.push_back(PlantDataManager.get_property_value_at_coord(TileDataManager.properties.PLANT_ID, tileInfo.get_coordinates() + Vector2.UP));
	if (BetterTerrain.get_cell(self, (tileInfo.get_coordinates() as Vector2i) + Vector2i.RIGHT) == 5):
		result.push_back(PlantDataManager.get_property_value_at_coord(TileDataManager.properties.PLANT_ID, tileInfo.get_coordinates() + Vector2.RIGHT));
	if (BetterTerrain.get_cell(self, (tileInfo.get_coordinates() as Vector2i) + Vector2i.DOWN) == 5):
		result.push_back(PlantDataManager.get_property_value_at_coord(TileDataManager.properties.PLANT_ID, tileInfo.get_coordinates() + Vector2.DOWN));
	return result;

func propagate(tileInfo : TileDataManager.TileInfo) -> int:
	print_debug("Prop attempt!");
	if (random_level(tileInfo) < .5):
		return -1;
	var neighbors : Array[TileDataManager.TileInfo];
	var tilePos : Vector2i = tileInfo.get_coordinates();
	neighbors = [
		get_plant_tile((tilePos + Vector2i.LEFT).x, (tilePos + Vector2i.LEFT).y),
		get_plant_tile((tilePos + Vector2i.UP).x, (tilePos + Vector2i.UP).y),
		get_plant_tile((tilePos + Vector2i.DOWN).x, (tilePos + Vector2i.DOWN).y),
		get_plant_tile((tilePos + Vector2i.RIGHT).x, (tilePos + Vector2i.RIGHT).y),
	]
	var valid_neighbors : Array[TileDataManager.TileInfo] = neighbors.filter(func(tile) : return is_mature(tile));
	match valid_neighbors.size():
		0:
			return -1;
		1:
			sow_plant(tileInfo, get_plant_type(valid_neighbors[0]));
		_:
			valid_neighbors.sort_custom(func(_a, _b): Global.rng.randf());
			sow_plant(tileInfo, Plant.choose_offspring(get_plant_type(valid_neighbors[0]), get_plant_type(valid_neighbors[1]))
			);
		
	return PlantDataManager.get_property_value_at_coord(TileDataManager.properties.PLANT_ID, tileInfo.get_coordinates());
	
#endregion

func sow_plant(tileInfo : TileDataManager.TileInfo, plant_type : Plant) -> void:
	var plant_idx : int = plant_type.get_type_name();
	PlantDataManager.set_property_value_at_coord(
		TileDataManager.properties.PLANT_ID,
		tileInfo.get_coordinates(),
		plant_idx
	);
	PlantDataManager.set_property_value_at_coord(
		TileDataManager.properties.GROWTH_STAGE,
		tileInfo.get_coordinates(),
		1
	);
	if (tile_sprites[Utils.index_from_vec(tileInfo.get_coordinates())] == null):
		tile_sprites[Utils.index_from_vec(tileInfo.get_coordinates())] = Sprite2D.new();
		add_child(tile_sprites[Utils.index_from_vec(tileInfo.get_coordinates())]);
	tile_sprites[Utils.index_from_vec(tileInfo.get_coordinates())].texture = load(plant_type.texture_paths[0]);
	tile_sprites[Utils.index_from_vec(tileInfo.get_coordinates())].position = tileInfo.get_coordinates() * Global.TILE_HEIGHT + Global.SPRITE_OFFSET;
	
func harvest(tileInfo : TileDataManager.TileInfo) -> void:
	PlantDataManager.set_property_value_at_coord(
		TileDataManager.properties.PLANT_ID,
		tileInfo.get_coordinates(),
		-1
	);
	PlantDataManager.set_property_value_at_coord(
		TileDataManager.properties.GROWTH_STAGE,
		tileInfo.get_coordinates(),
		-1
	);
	tile_sprites[Utils.index_from_vec(tileInfo.get_coordinates())].texture = null;
#endregion

func day_passed() -> void:
	for i in (Global.TILE_MAP_SIZE.x * Global.TILE_MAP_SIZE.y):
		if (PlantDataManager.get_property_value_at_coord(TileDataManager.properties.SOIL_TYPE, Utils.vec_from_idx(i)) == 0):
			continue; #soiltype = 2 means uninit
		if (PlantDataManager.get_property_value_at_coord(TileDataManager.properties.PLANT_ID,Utils.vec_from_idx(i)) == -1):
			continue; #plant id -1 means no valid plant
		var queue_unlock : int = turn_day(PlantDataManager.get_tile_info_at_coord(Utils.vec_from_idx(i)));
		emit_signal("Unlock", queue_unlock);
	cur_day += 1;
	StateManager.save_auto(PlantDataManager.export_tile_data(), cur_day);
	
func get_plant_tile(x : int, y : int) -> TileDataManager.TileInfo:
	if (has_tile(x, y)):
		return PlantDataManager.get_tile_info_at_coord(Vector2i(x, y));
	return TileDataManager.TileInfo.new(-1, -1, -1, -1, -1, -1);

func has_tile(x : int, y : int) -> bool:
	if (x < 0 || x >= Global.TILE_MAP_SIZE.x || y < 0 || y >= Global.TILE_MAP_SIZE.y):
		return false;
	return PlantDataManager.get_tile_info_at_coord(Vector2i(x, y)).soil_type != 0;
	
var initial_tiles : PackedByteArray;

# Called when the node enters the scene tree for the first time.
func _ready() -> void:
	tile_sprites.resize(Global.TILE_MAP_SIZE.x * Global.TILE_MAP_SIZE.y);
	initial_tiles = get_tile_map_data_as_array();
	
func tile_click(pos : Vector2, tool : Tool) -> void:
	var tile_pos : Vector2i = local_to_map(pos);
	
	match tool.tool_name:
		"Open_Hand":
			if (has_tile(tile_pos.x, tile_pos.y)) && is_mature(get_plant_tile(tile_pos.x, tile_pos.y)):
				print_debug("Harvest");
				harvest(get_plant_tile(tile_pos.x, tile_pos.y));
				StateManager.save_auto(PlantDataManager.export_tile_data(), cur_day);
		"Hoe":
			if (BetterTerrain.get_cell(self, tile_pos) != 5):
				print_debug("Hoe at:");
				print_debug(tile_pos);
				var tile : TileDataManager.TileInfo = TileDataManager.TileInfo.new(BetterTerrain.get_cell(self, tile_pos), 
				-1, 0, 0, tile_pos.x, tile_pos.y);
				PlantDataManager.set_tile_info_at_coord(tile.get_coordinates(), tile);
				BetterTerrain.set_cell(self, tile_pos, 5);
				BetterTerrain.update_terrain_cell(self, tile_pos);
				StateManager.save_auto(PlantDataManager.export_tile_data(), cur_day);
		"Seed_One":
			plant_seed(tile_pos, 1);
		"Seed_Two":
			plant_seed(tile_pos, 2);
		"Cross_Breed_Tool":
			if (BetterTerrain.get_cell(self, tile_pos) == 5 and get_plant_type(get_plant_tile(tile_pos.x, tile_pos.y)) == null):
				print_debug("Crossbreed planting!");
				sow_plant(PlantDataManager.get_tile_info_at_coord(tile_pos), Plant.CROSSBREED);
				StateManager.save_auto(PlantDataManager.export_tile_data(), cur_day);
				
func _input(event: InputEvent) -> void:
	if (event.is_action_pressed("nextDay")):
		day_passed();
		get_parent().get_node("%DayLabel").text = "Day: " + str(cur_day);
		
func plant_seed(tile_pos : Vector2i, plant_idx : int):
	if (BetterTerrain.get_cell(self, tile_pos) == 5 && get_plant_type(get_plant_tile(tile_pos.x, tile_pos.y)) == null && Global.Plants[plant_idx].planting_check(get_plant_tile(tile_pos.x, tile_pos.y).soil_type)):
		sow_plant(get_plant_tile(tile_pos.x, tile_pos.y), Global.Plants[plant_idx]);
		StateManager.save_auto(PlantDataManager.export_tile_data(), cur_day)

func reload(new_state : PackedByteArray, day : int) -> void:
	#Clear tile grid
	set_tile_map_data_from_array(initial_tiles);
	for sprite in tile_sprites:
		if (sprite != null):
			sprite.texture = null;
	#Copy conditions over
	if (new_state == null):
		PlantDataManager.clear_tile_data();
	else:
		PlantDataManager.overwrite_tile_data(new_state);
	cur_day = day;
	get_parent().get_node("%DayLabel").text = "Day: " + str(cur_day);

	for i in (Global.TILE_MAP_SIZE.x * Global.TILE_MAP_SIZE.y):
		#var curTile = PlantDataManager.get_tile_info_at_coord(Utils.vec_from_idx(i));
		#print_debug(str(i));
		if (PlantDataManager.get_property_value_at_coord(TileDataManager.properties.SOIL_TYPE, Utils.vec_from_idx(i)) == 0):
			continue; #uninit tile
		BetterTerrain.set_cell(self, Utils.vec_from_idx(i), 5);
		BetterTerrain.update_terrain_cell(self, Utils.vec_from_idx(i));
		if (PlantDataManager.get_property_value_at_coord(TileDataManager.properties.PLANT_ID, Utils.vec_from_idx(i)) == -1):
			continue; #tilled but no plant
			
		var cur_pos : Vector2i = Utils.vec_from_idx(i);
		var cur_tile : TileDataManager.TileInfo = get_plant_tile(cur_pos.x, cur_pos.y);
		if (tile_sprites[Utils.index_from_vec(cur_tile.get_coordinates())] == null):
			tile_sprites[Utils.index_from_vec(cur_tile.get_coordinates())] = Sprite2D.new();
			add_child(tile_sprites[Utils.index_from_vec(cur_tile.get_coordinates())]);
		tile_sprites[Utils.index_from_vec(cur_tile.get_coordinates())].texture = load(get_plant_type(cur_tile).texture_paths[cur_tile.growth_stage - 1]);
		tile_sprites[Utils.index_from_vec(cur_tile.get_coordinates())].position = Vector2(cur_tile.coord_x * Global.TILE_WIDTH, cur_tile.coord_y * Global.TILE_HEIGHT) + Global.SPRITE_OFFSET;
		
		
	
	
