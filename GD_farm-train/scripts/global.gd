extends Node

#TODO: Spread these vars to the classes that actually need them
var Seed : int = 1;
var rng : RandomNumberGenerator = RandomNumberGenerator.new();
var Map_Type : String = "";
@export var Scenarios : Array; #yummy array of dictionary dictionaries
var cur_scene : int = 0;

const TILE_WIDTH : int = 16;
const TILE_HEIGHT : int = 16;
const SPRITE_OFFSET : Vector2 = Vector2(8, 8);
const PLAYER_SPEED : int = 100;
const INTERACTION_RADIUS : float = 4 * TILE_HEIGHT;
const TILE_MAP_SIZE : Vector2i = Vector2i(40, 23);

func _ready() -> void:
	read_scenarios();
	rng.set_seed(Seed);
	construct_plants();

## The main array of plat classes used
var Plants : Array[Plant]; 
## number of plants, valid plant_ids should be < this num.
var plant_ids : int = 0;
## Holds plant names as keys and their ids as values, in insertion order.
var plant_name_id_dict : Dictionary;
## Path to the current, necessary, plant dict.
const plant_dict_path : String = "res://assets/plants/plant_dict.tres";

func construct_plants() -> void:
	assert(ResourceLoader.exists(plant_dict_path, "PlantDict"), "PlantDict not found!");
		
	var plant_dict : PlantDict = ResourceLoader.load(plant_dict_path);
	#First pass - Construct dictionary
	plant_name_id_dict.get_or_add(Plant.CROSSBREED.name, plant_ids);
	plant_ids += 1;
	for plant_def in plant_dict.plants:
		assert(!plant_name_id_dict.has(plant_def.Name), "Duplicated name " + plant_def.Name + " in dict!");
		plant_name_id_dict.get_or_add(plant_def.Name, plant_ids);
		plant_ids += 1;
	#Second pass -  construct plant classes
	Plants.push_back(Plant.CROSSBREED);
	for def in plant_dict.plants:
		Plants.push_back(add_plant_class(def));
	
var sun_ranges : Dictionary = {
	PlantDef.SunLevels.LOW: Vector2i(10, 30), #Low
	PlantDef.SunLevels.MEDIUM: Vector2i(30, 65), #Medium
	PlantDef.SunLevels.HIGH: Vector2i(65, 100), #High
	PlantDef.SunLevels.NOPREF: Vector2i(-1, 101)
}
var moistures : Dictionary = {
	PlantDef.MoistureLevels.LOW: 50, #Low
	PlantDef.MoistureLevels.MEDIUM: 100, #Medium
	PlantDef.MoistureLevels.HIGH: 150, #High
	PlantDef.MoistureLevels.NOPREF: -1
}
	
func add_plant_class(defin : PlantDef) -> Plant:
	#calc neihbor bitmask
	var nghbrs : Array[int];
	for plant_name in defin.needed_neighbors:
		if (plant_name.is_empty()):
			continue;
		assert(plant_name_id_dict.has(plant_name), "Plant name: " + plant_name + " in (" + defin.Name + ")'s neighbors not found!")
		nghbrs.push_back(plant_name_id_dict[plant_name]);
	var result : Plant = Plant.new(
		plant_name_id_dict[defin.Name],
		defin.Name,
		defin.valid_soils as Array[int],
		defin.texturePaths,
		defin.growth_stages,
		nghbrs,
		sun_ranges[defin.sun],
		moistures[defin.moisture]
	);
	for offspring_name in defin.offspring:
		assert(plant_name_id_dict.has(offspring_name), "ERROR: In plant: " + defin.Name + " offspring name not found: " + offspring_name);
		result.add_offspring(plant_name_id_dict[offspring_name]);
	
	return result;
	
const cardinality_transformations: Dictionary = {
	1: TileSetAtlasSource.TRANSFORM_TRANSPOSE | TileSetAtlasSource.TRANSFORM_FLIP_V,
	2: TileSetAtlasSource.TRANSFORM_FLIP_H | TileSetAtlasSource.TRANSFORM_FLIP_V,
	3: TileSetAtlasSource.TRANSFORM_TRANSPOSE | TileSetAtlasSource.TRANSFORM_FLIP_H,
	4: TileSetAtlasSource.TRANSFORM_FLIP_H,
	5: TileSetAtlasSource.TRANSFORM_TRANSPOSE | TileSetAtlasSource.TRANSFORM_FLIP_H | TileSetAtlasSource.TRANSFORM_FLIP_V,
	6: TileSetAtlasSource.TRANSFORM_FLIP_V,
	7: TileSetAtlasSource.TRANSFORM_TRANSPOSE
}

var scene_json : String;
@export var scene_dict : Dictionary;

func read_scenarios() -> void:
	if (FileAccess.file_exists("user://mods/scenarios/scenario.json")):
		print_debug("Found external scenarios! Attempting parse!");
		scene_json = FileAccess.get_file_as_string("user://mods/scenarios/scenario.json");
	else:
		print_debug("No external scenarios! Using default!");
		scene_json = FileAccess.get_file_as_string("res://scenes/maps/scenario.json");
		
	scene_dict = JSON.parse_string(scene_json);
	assert(validate_scenarios(scene_dict), "No valid scenarios found!");
	
	Seed = scene_dict.get_or_add("seed", Global.Seed);
	Map_Type = scene_dict.get_or_add("map", Global.Map_Type);
	Scenarios = scene_dict.get("scenarios");
	
	
#TODO: Add better validity checking.
func validate_scenarios(scenes : Dictionary) -> bool:
	return scenes.has("scenarios") && !(scenes["scenarios"] as Array).is_empty();
