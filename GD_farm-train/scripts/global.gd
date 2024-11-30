extends Node

#TODO: Spread these vars to the classes that actually need them
var Seed : int = 0;
var rng : RandomNumberGenerator = RandomNumberGenerator.new();
const plant_types : int = 3;
const TILE_WIDTH : int = 16;
const TILE_HEIGHT : int = 16;
const SPRITE_OFFSET : Vector2 = Vector2(8, 8);
const PLAYER_SPEED : int = 100;
const INTERACTION_RADIUS : float = 4 * TILE_HEIGHT;
const TILE_MAP_SIZE : Vector2i = Vector2i(40, 23);

func _ready() -> void:
	Seed = 1;
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
