class_name Defn_Factory extends Node
var new_definition : Tiled_Rules;
@export var sample_layer : TileMapLayer;
@export var sample_size : Vector2i;
@export var weight_evenly : bool = false;
@export var periodic : bool = true;

enum DIRECTIONS {LEFT, DOWN, RIGHT, UP};
var tile_id_of : Dictionary = {};
var unique_rules : Dictionary;
var tilemap_as_data : Array = [];

func _init(sample : TileMapLayer = sample_layer, sampl_size : Vector2 = sample_size, even_weights : bool = false, s_periodic : bool = false) -> void:
	sample_layer = sample;
	sample_size = sampl_size;
	weight_evenly = even_weights;
	periodic = s_periodic;

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

func setup() -> Tiled_Rules:
	new_definition = Special_Rules.new();
	#get tile set
	new_definition.tileset = sample_layer.tile_set;
	#Read in tiles and add to tiles[]
	read_tiles();
	#get adjacencies and cardinalities to push to neighbors[]
	get_neighbors();
	new_definition.tile_id_of = tile_id_of;
	return new_definition;
	
func read_tiles() -> void:
	tilemap_as_data.resize(sample_size.x);
	for x in sample_size.x:
		tilemap_as_data[x] = [];
		tilemap_as_data[x].resize(sample_size.y);
		for y in sample_size.y:
			var tile_coords : Vector2 = sample_layer.get_cell_atlas_coords(Vector2(x, y));
			var tile_alt : int = cardinality_transformations.find_key(sample_layer.get_cell_alternative_tile(Vector2(x, y)));
			var tile_name : String = get_tile_name(tile_coords, tile_alt);
			if !tile_id_of.has(tile_name):
				tile_id_of[tile_name] = new_definition.tiles.size();
				new_definition.tiles.push_back({"atlas_coords":tile_coords, "cardinality":tile_alt, "weight": 1});
			else:
				if !weight_evenly:
					new_definition.tiles[tile_id_of[tile_name]]["weight"] += 1;
			tilemap_as_data[x][y] = tile_id_of[tile_name];



func get_neighbors() -> void:#currently generates redundant rules
	#loop through sample x by y
	for x in sample_size.x:
		for y in sample_size.y:
			#for each tile get left and bottom neighbor pair
			#L & r is easy
			var cur_tile : int = tilemap_as_data[x][y];
			#get correct cardinality based on rotations and alt tile id
			if x < sample_size.x - 1: 
				var neighbor : int = tilemap_as_data[x+1][y];
				is_rule_unique(cur_tile, neighbor, DIRECTIONS.LEFT);
			elif periodic:
				var neighbor : int = tilemap_as_data[0][y];
				is_rule_unique(cur_tile, neighbor, DIRECTIONS.LEFT);
				
			if y < sample_size.y - 1:
				var neighbor : int = tilemap_as_data[x][y+1];
				is_rule_unique(cur_tile, neighbor, DIRECTIONS.UP);
			elif periodic:
				var neighbor : int = tilemap_as_data[x][0];
				is_rule_unique(cur_tile, neighbor, DIRECTIONS.UP);

static func get_tile_name(coords:Vector2, card : int) -> String:
	return str(coords.x)+","+str(coords.y) + " " + str(card);
	
func is_rule_unique(tile1 : int, tile2 : int, direction : DIRECTIONS) -> bool: #TODO different, peferably faster, hashing
	
	var rule_name : String = str(tile1) + " " + str(tile2) + " " + str(direction);
	if (unique_rules.has(rule_name)):
		return false;
	else: 
		var result : Dictionary = {"tile1": tile1, "tile2": tile2, "direction":direction};
		unique_rules[rule_name] = true;
		new_definition.neighbors.push_back(result);
		return true;
