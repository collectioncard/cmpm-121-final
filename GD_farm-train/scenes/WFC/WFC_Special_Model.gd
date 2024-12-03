class_name  Special_Model extends Tiled_Model #Generates the rules for a tiled definition

func _init(definition : Special_Rules = rules_definition, fx : int = final_width, fy : int = final_height) -> void:
	rules_definition = definition;
	final_width = fx;
	final_height = fy;

func setup() -> void:
	_generate_model_rules();
	#log_debug_info();
	model_generated.emit();

func _generate_model_rules() -> void:
	#tiles
	#weights
	#num tiles
	#propagator
	num_patterns = rules_definition.tiles.size();
	weights.resize(num_patterns);
	var index : int = 0;
	for tile in rules_definition.tiles:
		tiles.push_back([tile["atlas_coords"], tile["cardinality"]]);
		weights[index] = tile["weight"];
		index += 1;
		
		
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
		var tile1 : int = rule["tile1"]; #Array in form ["name", "rotations"]
		var tile2 : int = rule["tile2"];
		var dir : int = rule["direction"];
		
		tempPropagator[dir][tile2][tile1] = true;
		
		#right and up rules
		for p in num_patterns:
			for p2 in num_patterns:
				tempPropagator[DIRECTIONS.RIGHT][p][p2] = tempPropagator[DIRECTIONS.LEFT][p2][p]
				tempPropagator[DIRECTIONS.DOWN][p][p2] = tempPropagator[DIRECTIONS.UP][p2][p]
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
	var tile_name : String = Defn_Factory.get_tile_name(tile[0], cardinality);
	if rules_definition.tile_id_of.has(tile_name):
		return rules_definition.tile_id_of[tile_name];
	else:
		print_debug("Tile data not in tiles!");
		print_debug(tile);
		return -1;
