extends Node

#TODO: Spread these vars to the classes that actually need them
var Seed : int = 0;
var Plants : Array[Plant];
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
	#TODO: Rework later
	Plants.push_front(Plant.CROSSBREED);
	var plant1Textures : Array[String] = ["res://assets/plants/plant1-1.png",
										 "res://assets/plants/plant1-2.png",
										 "res://assets/plants/plant1-3.png"];
	Plants.push_back(Plant.new(1, plant1Textures, 1, 3));
	var plant2Textures : Array[String] = ["res://assets/plants/plant2-1.png", 
										 "res://assets/plants/plant2-2.png", 
										 "res://assets/plants/plant2-3.png"];
	Plants.push_back(Plant.new(2, plant2Textures, 1, 3));
	Plants[1].add_offspring(Plants[2]);
