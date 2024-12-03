class_name  Overlap_Model extends Node #Generates the rules for a tiled definition
#Start with abstract representation
@export var rules_definition : TileMapLayer; #work on better way to do this
@export var final_width : int;
@export var final_height : int;
var directions : int = 4;
var periodic : bool = false;
#Tile as key "x-y-card" with value index id in tiles
var tile_index : Dictionary;
#Tiles are currently in form [Vector2(Atlas Coords), Cardinality]
var tiles : Array; #This will eventually be the array that holds the raw tile data, whether that is tile images, bitmap data, ehwtaver
#For readability I'm gonna make patterns 2d arrays
var num_patterns : int;
var input_width : int = 3;
var input_height : int = 3;
var size_of_pattern : Vector2 = Vector2 (2, 2); #currently requires x == y
var weights: Array[float];

var sample : Array = [];
var propagator : Array = [];

enum NEIGHBOR {NAME, ROTATIONS};
enum CARDINALITY {SAME, ONE_ROTA, TWO_ROTA, THREE_ROTA, REFL, ONE_ROTA_REFL, TWO_ROTA_REFL, THREE_ROTA_REFL};
enum DIRECTIONS {LEFT, DOWN, RIGHT, UP};

signal model_generated;

func _ready() -> void:
	#return
	#var test_arr = [];
	#test_arr.resize(3);
	#var count = 0
	#for i in 3:
		#test_arr[i] = [];
		#test_arr[i].resize(3);
		#for j in 3:
			#test_arr[i][j] = count;
			#count += 1;
	#print_debug(test_arr[0][1])
	generate_sample();
	sample = rotate_pattern
	print_debug(sample[0][1]);
	
#REFACTOR LATER--Patterns---------------------------------------------
var pattern : Callable = func pattern(pattern_func : Callable) -> Array:
	var result : Array = [];
	result.resize(size_of_pattern.x)
	for i in size_of_pattern.x:
		result[i] = []
		result[i].resize(size_of_pattern.y);
	for i in size_of_pattern.x:
		for j in size_of_pattern.y:
			result[i][j] = pattern_func.call(i, j);
	return result;
	
var patternFromSample : Callable = func patternFromSample(x : int, y : int):
	return pattern.call(func(dx : int, dy : int):
		return sample[(x+dx) % input_width][(y+dy) % input_height]
		); 
		
var rotate_pattern = func rotate(p : Array):
	return pattern.call(func(x : int, y : int):
		return p[y][size_of_pattern.x - x - 1];
	);
#--------------------------------------------------------------------------
func setup() -> void:
	generate_model_rules();
	#log_debug_info();
	var rng : RandomNumberGenerator = RandomNumberGenerator.new();
	rng.seed = hash("WORK PLEASE");
	model_generated.emit(0, rng);
var cardinal_tile_mappings : Array = [];
var tile_id_of : Dictionary = {}; #should move away from dictionaries in Godot
var tile_name_of : Array = [];

func generate_model_rules() -> void:
	#conversion from raw data to tiling for game
	generate_sample();

	var rotation_mapping_func : Callable; #function to get a single rotation for the mapping
	var reflection_mapping_func : Callable;#function to get a refl for the mapping
	
	#Pack and create tiles and cardinal variations
	for i in rules_definition.tiles.size(): #Loop through all tiles
		var currentTile : Dictionary = rules_definition.tiles[i];
		var cardinality : int; #num of tile variations a tile should have, based on symmetries
		
		#Set functions to map between transformations for cardinal tiles
		match currentTile["symmetry"]:
			"L":
				cardinality = 4;
				rotation_mapping_func = func(cur_cardinality : int):	
					return (cur_cardinality + 1) % 4;
				reflection_mapping_func = func(cur_cardinality : int): 	
					if (cur_cardinality % 2 == 0):
						return (cur_cardinality + 1);
					else:
						return (cur_cardinality - 1); 
			"T":
				cardinality = 4;
				rotation_mapping_func = func(cur_cardinality : int):	
					return (cur_cardinality + 1) % 4;
				reflection_mapping_func = func(cur_cardinality : int): 
					if (cur_cardinality % 2 == 0):
						return cur_cardinality;
					else:
						return (4 - cur_cardinality);
			"I":
				cardinality = 2;
				rotation_mapping_func = func(cur_cardinality : int):
					return 1 - cur_cardinality;
				reflection_mapping_func = func(cur_cardinality : int):
					return cur_cardinality;
			"\\":
				cardinality = 2;
				rotation_mapping_func = func(cur_cardinality : int):
					return 1 - cur_cardinality;
				reflection_mapping_func = func(cur_cardinality : int):
					return 1 - cur_cardinality;
			"F":
				cardinality = 8;
				rotation_mapping_func = func(cur_cardinality : int):
					if (cur_cardinality < 4):
						return (cur_cardinality+1)%4
					else:
						return 4 + (cur_cardinality -1) % 4;
				reflection_mapping_func = func(cur_cardinality : int):
					if (cur_cardinality < 4):
						return cur_cardinality + 4;
					else:
						return cur_cardinality - 4;
			_:
				cardinality = 1;
				rotation_mapping_func = func (cur_cardinality): 	
					return cur_cardinality;
				reflection_mapping_func = func (cur_cardinality): 	
					return cur_cardinality;
					
		num_patterns = cardinal_tile_mappings.size();
		tile_id_of[currentTile["name"]] = num_patterns;
		var parent_tile_id : int = num_patterns;
		#Create mappings for each cardinal tile
		#Array is in form cardinal_tile_mappings[tile][CARDINALITY], which gives the tile id of the cardinality from tile
		for c in cardinality:
			tiles.push_back([currentTile["atlas_coords"], c]);
			cardinal_tile_mappings.push_back([
				parent_tile_id + c, #id of self
				parent_tile_id + rotation_mapping_func.call(c), #id of 1 rotation
				parent_tile_id + rotation_mapping_func.call(rotation_mapping_func.call(c)), # 2 rota's
				parent_tile_id + rotation_mapping_func.call(rotation_mapping_func.call(rotation_mapping_func.call(c))), #3 rots
				parent_tile_id + reflection_mapping_func.call(c), #refl
				parent_tile_id + reflection_mapping_func.call(rotation_mapping_func.call(c)), #refl of 1 rot
				parent_tile_id + reflection_mapping_func.call(rotation_mapping_func.call(rotation_mapping_func.call(c))), #refl of 2 rot
				parent_tile_id + reflection_mapping_func.call(rotation_mapping_func.call(rotation_mapping_func.call(rotation_mapping_func.call(c)))), #refl of 3 rot
			]);
			tile_name_of.push_back(currentTile.name + str(c));
		
		#Add weights
		for c in cardinality:
			if currentTile.has("weight"):
				weights.push_back(currentTile["weight"])
			else:
				weights.push_back(1);
	num_patterns = cardinal_tile_mappings.size();
	# end section---------------------
	#Create Propagator---------------
		#Structure propagator and tempProp
	propagator.resize(directions);
	var tempPropagator : Array = [];
	tempPropagator.resize(directions);
	
	for d in directions:
		propagator[d] = [];
		propagator[d].resize(num_patterns);
		tempPropagator[d] = [];
		tempPropagator[d].resize(num_patterns)
		for p in num_patterns:
			tempPropagator[d][p] = [];
			tempPropagator[d][p].resize(num_patterns);
			for p2 in num_patterns:
				tempPropagator[d][p][p2] = false;
	
		#generate rules from neighbors
	for rule in rules_definition.neighbors:
		var l_neighbor : Array = rule.left.split(" "); #Array in form ["name", "rotations"]
		var r_neighbor : Array = rule.right.split(" ");
		
		var L_id : int = cardinal_tile_mappings[tile_id_of[l_neighbor[NEIGHBOR.NAME]]][int(l_neighbor[NEIGHBOR.ROTATIONS])];
		var D_id : int = cardinal_tile_mappings[L_id][CARDINALITY.ONE_ROTA];
		var R_id : int = cardinal_tile_mappings[tile_id_of[r_neighbor[NEIGHBOR.NAME]]][int(r_neighbor[NEIGHBOR.ROTATIONS])];
		var U_id : int = cardinal_tile_mappings[R_id][CARDINALITY.ONE_ROTA];
		#left dir rules
		tempPropagator[DIRECTIONS.LEFT][R_id][L_id] = true;
		tempPropagator[DIRECTIONS.LEFT][cardinal_tile_mappings[R_id][CARDINALITY.TWO_ROTA_REFL]][cardinal_tile_mappings[L_id][CARDINALITY.TWO_ROTA_REFL]] = true;
		tempPropagator[DIRECTIONS.LEFT][cardinal_tile_mappings[L_id][CARDINALITY.REFL]][cardinal_tile_mappings[R_id][CARDINALITY.REFL]] = true;
		tempPropagator[DIRECTIONS.LEFT][cardinal_tile_mappings[L_id][CARDINALITY.TWO_ROTA]][cardinal_tile_mappings[R_id][CARDINALITY.TWO_ROTA]] = true;
		#down dir rules
		tempPropagator[DIRECTIONS.DOWN][U_id][D_id] = true;
		tempPropagator[DIRECTIONS.DOWN][cardinal_tile_mappings[D_id][CARDINALITY.TWO_ROTA_REFL]][cardinal_tile_mappings[U_id][CARDINALITY.TWO_ROTA_REFL]] = true;
		tempPropagator[DIRECTIONS.DOWN][cardinal_tile_mappings[U_id][CARDINALITY.REFL]][cardinal_tile_mappings[D_id][CARDINALITY.REFL]] = true;
		tempPropagator[DIRECTIONS.DOWN][cardinal_tile_mappings[D_id][CARDINALITY.TWO_ROTA]][cardinal_tile_mappings[U_id][CARDINALITY.TWO_ROTA]] = true;
		#right and up rules
		for p in num_patterns:
			for p2 in num_patterns:
				tempPropagator[DIRECTIONS.RIGHT][p][p2] = tempPropagator[DIRECTIONS.LEFT][p2][p]
				tempPropagator[DIRECTIONS.UP][p][p2] = tempPropagator[DIRECTIONS.DOWN][p2][p]
		#push rules from temp to propagator
		for d in directions:
			for p1 in num_patterns:
				var rule_list : Array = [];
				var temp_prop_list : Array = tempPropagator[d][p1];
				for p2 in num_patterns:
					if (temp_prop_list[p2]):
						rule_list.append(p2);
				
				propagator[d][p1] = rule_list;
	pass;
	
var bitmask_base : int;
var bitmask_size : float;
	
func generate_sample() -> void: #lowkey don't know if I actually need all this
	sample.resize(input_width);
	for i in input_width:
		sample[i] = [];
		sample[i].resize(input_height);
		for j in input_height:
			#[(x, y)atlas coords, cardinality]
			var cur_tile : Array = [rules_definition.get_cell_atlas_coords(Vector2(i, j)), alt_to_card(rules_definition.get_cell_alternative_tile(Vector2(i, j)))];
			var cur_index : String = str(cur_tile[0].x)+str(cur_tile[0].y)+str(cur_tile[1]); #index as string
			if !tile_index.has(cur_index):
				tile_index[cur_index] = tiles.size();
				tiles.push_back(cur_tile);
			sample[i][j] = cur_tile;
	bitmask_base = tiles.size();
	bitmask_size = pow(bitmask_base, size_of_pattern.x * size_of_pattern.y);
			
func alt_to_card(alt_id : int) -> int:
	match  alt_id:
		TileSetAtlasSource.TRANSFORM_TRANSPOSE | TileSetAtlasSource.TRANSFORM_FLIP_V:
			return 1;
		TileSetAtlasSource.TRANSFORM_FLIP_H | TileSetAtlasSource.TRANSFORM_FLIP_V:
			return 2;
		TileSetAtlasSource.TRANSFORM_TRANSPOSE | TileSetAtlasSource.TRANSFORM_FLIP_H:
			return 3;
		TileSetAtlasSource.TRANSFORM_FLIP_H:
			return 4;
		TileSetAtlasSource.TRANSFORM_TRANSPOSE | TileSetAtlasSource.TRANSFORM_FLIP_H | TileSetAtlasSource.TRANSFORM_FLIP_V:
			return 5;
		TileSetAtlasSource.TRANSFORM_FLIP_V:
			return 6;
		TileSetAtlasSource.TRANSFORM_TRANSPOSE:
			return 7;
		_:
			return 0
	
func on_boundary(x:int, y:int) -> bool: #returns true if the x, y tile is oob
	return !periodic && (x < 0 || y < 0 || x + size_of_pattern.x >= final_width || y + size_of_pattern.y >= final_height);
	
func log_debug_info():
	assert(propagator.size() > 0);
	print_debug("Start log: Propagator");
	for d in directions:
		var direction : String = "left";
		match d:
			1:
				direction = "down";
			2: 
				direction = "right";
			3: 
				direction = "up";
		print_debug("Direction " + direction);
		for p1 in num_patterns:
			print_debug("Tile: " + tile_name_of[p1] + " can be adjacent to:");
			for p2 in propagator[d][p1]:
				print_debug(tile_name_of[p2]);
	Global._draw_debug_tiles(tiles);
