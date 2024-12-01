#Godot WFC implementation from: https://github.com/SentientDragon5/CMPM-118-WFC
class_name WFC_Solver extends Node #Does the wfc

@export var model : Tiled_Model;
@export var output : TileMapLayer;
var populated_output : bool = false;
var populated_tiles : Array = []; #each is [position index, tile_id]

var weights: Array[float] = [];

var image_width_tiles: int = 0;
var image_height_tiles: int = 0;
var total_tile_count: int = 0;
var total_pattern_count: int = 0;

var pattern_size: int = 0; #Assuming square patterns only in this implementation

var initialized_field: bool = false;
var generation_complete: bool = false;

var wave = null; #Actually Array[Array[bool]] (cant check, blame GDScript)
var compatibility_matrix = null; #Actually Array[Array[Array[int]]] (cant check, blame GDScript)
var log_weighted_sums: Array[float] = [];
var total_log_weighted_sums: float = 0;
var total_weights: float = 0;

var initial_entropy: float = 0;

# Arrays that keep track of data for each tile
var tile_possible_pattern_count: Array[int] = [];
var tile_total_pattern_weights: Array[float] = [];
var tile_pattern_log_weight_sums: Array[float] = [];
var tile_pattern_entropies: Array[float] = [];

#This is the final output of the algorithm
var collapsed_tiles: Array[int] = [];
var possible_pattern_weights: Array[float] = [];

var stack: Array = []; #Actually array of [int, int] (cant check, blame GDScript) (tile index, pattern index)
var stack_size: int = 0;

const DX: Array[int] = [-1,0,1,0];
const DY: Array[int] = [0,1,0,-1];
const OPPOSITE: Array[int] = [2,3,0,1];

var time_machine: TimeMachine = null;

func pre_initialize(wfc_model : Tiled_Model, output_layer : TileMapLayer) -> void:
	if output_layer != null:
		output = output_layer;
	model = wfc_model;
	image_width_tiles = model.final_width;
	image_height_tiles = model.final_height;
	total_tile_count = image_width_tiles * image_height_tiles;
	total_pattern_count = model.num_patterns;

	weights = model.weights;

func populate_WFC(populate_layer : TileMapLayer = null) -> void:
	if populate_layer == null || image_height_tiles == 0 || image_width_tiles == 0:
		return;
	get_pop_tiles(populate_layer);
	populated_output = true;
	
func clear_populated() -> void:
	populated_output = false;
	populated_tiles.resize(0);

func initialize() -> void:
	possible_pattern_weights = []; # of t length
	possible_pattern_weights.resize(model.num_patterns);
	wave = []; # of fmx_x_fmy len
	compatibility_matrix = []; # of fmx_x_fmy len

	for i in range(total_tile_count):
		wave.append([]);
		compatibility_matrix.append([]);
		for _t in total_pattern_count:
			wave[i].append([]);
			wave[i].resize(total_pattern_count);
			compatibility_matrix[i].append([0,0,0,0]);

	log_weighted_sums = [];
	for _t in range(total_pattern_count):
		log_weighted_sums.append(0);
	tile_total_pattern_weights = [];
	total_log_weighted_sums = 0;

	for _t in range(total_pattern_count):
		log_weighted_sums[_t] = (weights[_t] * log(weights[_t]));
		total_weights += weights[_t];
		total_log_weighted_sums += log_weighted_sums[_t];

	initial_entropy = log(total_weights) - total_log_weighted_sums / total_weights;

	tile_possible_pattern_count = [];
	tile_total_pattern_weights = [];
	tile_pattern_log_weight_sums = [];
	tile_pattern_entropies = [];

	for a in range(total_tile_count):
		tile_possible_pattern_count.append(weights.size());
		tile_total_pattern_weights.append(total_weights);
		tile_pattern_log_weight_sums.append(total_log_weighted_sums);
		tile_pattern_entropies.append(initial_entropy);

	stack = [];
	stack_size = 0;

func observe(rng : RandomNumberGenerator):
	var min_noise = 1000;
	var argmin = -1;
	for i in range(total_tile_count):
		@warning_ignore("integer_division")
		if model.on_boundary(i % image_width_tiles, i / image_width_tiles):
			continue;
		var amount = tile_possible_pattern_count[i];
		if amount == 0:
			return false;
		var entropy = tile_pattern_entropies[i];
		if amount>1 && entropy <= min_noise:
			var noise = 0.000001 * rng.randf_range(0,1); # DOUBLE CHECK
			if entropy + noise < min_noise:
				min_noise = entropy + noise;
				argmin = i;
	# search for the minimum entropy.
	if argmin == -1:
		collapsed_tiles = [];
		collapsed_tiles.resize(total_tile_count);
		collapsed_tiles.fill(-1);
		for i in range(total_tile_count):
			for _t in range(total_pattern_count):
				if wave[i][_t]:
					collapsed_tiles[i] = _t;
		return true;
		
	for _t in range(total_pattern_count):
		possible_pattern_weights[_t] = weights[_t] if wave[argmin][_t] else 0.0;
	var r = randomIndice(possible_pattern_weights, rng);
	var w = wave[argmin];
	for _t in range(total_pattern_count):
		if w[_t] != (_t==r):
			ban(argmin, _t);
	return null;

func propagate() -> void:
	while stack_size > 0:
		var tile_element = stack.pop_front();
		stack_size-=1;

		var tile_index = tile_element[0];
		var tile_x_pos = tile_index % image_width_tiles;
		var tile_y_pos = tile_index / image_width_tiles | 0;

		for direction in range(4):
			var delta_x = DX[direction];
			var delta_y = DY[direction];

			var neighbor_tile_x_pos = tile_x_pos + delta_x;
			var neighbor_tile_y_pos = tile_y_pos + delta_y;

			if model.on_boundary(neighbor_tile_x_pos, neighbor_tile_y_pos):
				continue;
			if neighbor_tile_x_pos < 0:
				neighbor_tile_x_pos += image_width_tiles;
			elif neighbor_tile_x_pos >= image_width_tiles:
				neighbor_tile_x_pos -= image_width_tiles;
			if neighbor_tile_y_pos < 0:
				neighbor_tile_y_pos += image_height_tiles;
			elif neighbor_tile_y_pos >= image_height_tiles:
				neighbor_tile_y_pos -= image_height_tiles;

			var neighbor_tile_index = neighbor_tile_x_pos + neighbor_tile_y_pos * image_width_tiles;
			var possibleTiles = model.propagator[direction][tile_element[1]];
			var compat = compatibility_matrix[neighbor_tile_index];

			for l in range(possibleTiles.size()):
				var patternIndex = possibleTiles[l];
				var comp = compat[patternIndex];
				comp[direction] -= 1;
				if comp[direction] == 0:
					ban(neighbor_tile_index, patternIndex);

func singleIteration(rng : RandomNumberGenerator):
	var result = observe(rng);
	#add_tm_entry();
	if result != null:
		generation_complete = result;
		return result;
	propagate();
	return null;

func iterate(iterations : int, rng) -> bool:
	if wave == null:
		initialize();
	if !initialized_field:
		clear();
	iterations = iterations || 0;
	if rng == null:
		rng = RandomNumberGenerator.new();
	var i = 0;
	while i < iterations || iterations == 0:
		var result = singleIteration(rng);
		if result != null:
			return result;
	return true;

func generate(rng_seed : int = -1) -> bool:
	var rng = RandomNumberGenerator.new();

	if rng_seed == -1:
		rng.seed = hash("WORK PLEASE") * randi_range(0, 6969696969);
	else:
		rng.seed = rng_seed;

	if wave == null:
		initialize();
	clear();
	if populated_output:
		ban_pop_tiles();
	while true:
		var result = singleIteration(rng);
		if result != null:
			print_debug(result);
			return result;
	return false;
	
func generate_with_time_machine(rng_seed : int = -1) -> TimeMachine:
	if time_machine:
		time_machine = null;

	time_machine = TimeMachine.new(
		output, # The tilemaplayer to draw tiles onto
		model.rules_definition.tileset, # The tileset to draw from
		model.tiles, # Tile definitions n rotations
		model.final_width, # The width of the map
		model.final_height, # The height of the map
		get_parent() # The parent node that we should draw sprites on
	);

	generate(rng_seed);
	return time_machine;

func ban(i,_t) -> void:
	var comp = compatibility_matrix[i][_t];
	for d in range(4):
		comp[d] = 0;
	wave[i][_t] = false;

	stack.push_back([i,_t]);
	stack_size+=1;

	tile_possible_pattern_count[i] -= 1;
	#if tppc == 1 collapse tile
	if tile_possible_pattern_count[i] == 1:
		add_tm_entry();
	tile_total_pattern_weights[i] -= weights[_t];
	tile_pattern_log_weight_sums[i] -= log_weighted_sums[_t];

	var sum = tile_total_pattern_weights[i];
	tile_pattern_entropies[i] = log(sum) - tile_pattern_log_weight_sums[i] / sum;

func clear() -> void:
	for i in range(total_tile_count):
		for _t in range(total_pattern_count):
			wave[i][_t] = true;
			for d in range(4):
				compatibility_matrix[i][_t][d] = model.propagator[OPPOSITE[d]][_t].size();
		tile_possible_pattern_count[i] = weights.size();
		tile_total_pattern_weights[i] = total_weights;
		tile_pattern_log_weight_sums[i] = total_log_weighted_sums;
		tile_pattern_entropies[i] = initial_entropy;
		collapsed_tiles.resize(total_tile_count);
		collapsed_tiles.fill(-1);

	initialized_field = true;
	generation_complete = false;

func randomIndice(distrib : Array, rng : RandomNumberGenerator) -> int:
	var random_value : float = rng.randf_range(0, 1);
	var total_weight : float = 0;
	var accumulated_weight : float = 0;
	var index : int = 0;

	for num in distrib:
		total_weight += num;

	random_value *= total_weight;

	while (random_value != 0 and index < distrib.size()):
		accumulated_weight += distrib[index];
		if (random_value <= accumulated_weight):
			return index;
		index+=1;
	return 0;
	
func get_pop_tiles(populate_layer : TileMapLayer) -> void:
	for tile_y in range(model.final_height):
		for tile_x in range (model.final_width):
			var tile_coords : Vector2i =  populate_layer.get_cell_atlas_coords(Vector2i(tile_x, tile_y));
			if tile_coords == Vector2i(-1, -1): #notatile
				continue;
			var tile_id : int = model.id_from_tile_data([tile_coords, populate_layer.get_cell_alternative_tile(Vector2i(tile_x, tile_y))]);
			populated_tiles.push_back([tile_x + tile_y * image_width_tiles, tile_id]);
			
func ban_pop_tiles() -> void:
	for tile in populated_tiles:
		var w = wave[tile[0]]; #get wave of tile's pos index
		for _t in range(total_pattern_count):
			if w[_t] != (_t==tile[1]):
				ban(tile[0], _t);
		propagate();
		
func add_tm_entry():
	if time_machine:
		time_machine.add_capsule(
			collapsed_tiles.duplicate(true),
			wave.duplicate(true)
		);
