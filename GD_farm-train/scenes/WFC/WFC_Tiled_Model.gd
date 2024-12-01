class_name  Tiled_Model extends Resource #Generates the rules for a tiled definition
#Start with abstract representation
@export var rules_definition : Tiled_Rules; #work on better way to do this
@export var final_width : int;
@export var final_height : int;
@export var directions : int = 4;
#Tiles are currently in form [Vector2(Atlas Coords), Cardinality]
@export var tiles : Array;
@export var num_patterns : int;
@export var weights: Array[float];

@export var propagator : Array = [];

enum NEIGHBOR {NAME, ROTATIONS};
enum CARDINALITY {SAME, ONE_ROTA, TWO_ROTA, THREE_ROTA, REFL, ONE_ROTA_REFL, TWO_ROTA_REFL, THREE_ROTA_REFL};
enum DIRECTIONS {LEFT, DOWN, RIGHT, UP};

const cardinality_transformations: Dictionary = {
	0: 0,
	1: TileSetAtlasSource.TRANSFORM_TRANSPOSE | TileSetAtlasSource.TRANSFORM_FLIP_V,
	2: TileSetAtlasSource.TRANSFORM_FLIP_H | TileSetAtlasSource.TRANSFORM_FLIP_V,
	3: TileSetAtlasSource.TRANSFORM_TRANSPOSE | TileSetAtlasSource.TRANSFORM_FLIP_H,
	4: TileSetAtlasSource.TRANSFORM_FLIP_H,
	5: TileSetAtlasSource.TRANSFORM_TRANSPOSE | TileSetAtlasSource.TRANSFORM_FLIP_H | TileSetAtlasSource.TRANSFORM_FLIP_V,
	6: TileSetAtlasSource.TRANSFORM_FLIP_V,
	7: TileSetAtlasSource.TRANSFORM_TRANSPOSE
}

signal model_generated;

func _init(definition : Tiled_Rules = rules_definition, fx : int = final_width, fy : int = final_height) -> void:
	rules_definition = definition;
	final_width = fx;
	final_height = fy;

func setup() -> void:
	_generate_model_rules();
	#log_debug_info();
	model_generated.emit();
@export var cardinal_tile_mappings : Array = [];
@export var tile_id_of : Dictionary = {}; #should move away from dictionaries in Godot
@export var tile_name_of : Array = [];

func _generate_model_rules() -> void:
	#periodicity?
	#unique tiles?
	#subsets?
	#conversion from raw data to tiling for game

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
			tile_name_of.push_back(currentTile.name + " " + str(c));
		
		#Add weights
		for c in cardinality:
			if currentTile.has("weight"):
				weights.push_back(currentTile["weight"]);
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
	
func id_from_tile_data(tile : Array) -> int: #[atlas_coords, alt_id]
	var cardinality = cardinality_transformations.find_key(tile[1]);
	var tile_name : String;
	for tile_obj in rules_definition.tiles:
		if tile_obj["atlas_coords"] == Vector2(tile[0]):
			tile_name = tile_obj["name"];
			var parent_id : int = tile_id_of[tile_name];
			return cardinal_tile_mappings[parent_id][cardinality];
	print_debug("Tile data not in tiles!");
	return -1;

func on_boundary(x:int, y:int) -> bool: #returns true if the x, y tile is oob
	return !rules_definition.periodic && (x < 0 || y < 0 || x >= final_width || y >= final_height);
	
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
