class_name Plant extends RefCounted

var id : int;
var offspring : Array[Plant];
var growth_rate : float;

var growth_stages : int;
var texture_paths : Array[String];

var sun_needed : int = 50;
var moisture_needed : int = 50;

static var CROSSBREED = Plant.new(
	0,
	["res://assets/plants/idkwhattocallthis.png"],
	0,
	-1
						);

func _init(_id: int, _texturePaths : Array[String], _growthRate : float, _growthStages : int):
	id = _id;
	texture_paths = _texturePaths;
	growth_stages = _growthStages;
	growth_rate = _growthRate;
	offspring.push_back(self);
	
		
func get_type_name() -> int:
	return id;
	
func growth_check(sun : int, moisture: int, _neighbors : Array = []):
	var moisture_check : bool = false;
	var sun_check : bool = false;
	
	if (sun >= sun_needed):
		sun_check = true;
	if (moisture >= moisture_needed):
		moisture_check = true;
		
	return sun_check && moisture_check;
	
func add_offspring(newOffspring : Plant) -> void:
	offspring.push_back(newOffspring);

static func choose_offspring(parentA : Plant, parentB : Plant) -> Plant:
	var validOffspring : Array[Plant];
	#TODO: Not O(n^2) solution
	for _offspring in parentA.offspring:
		if (parentB.offspring.has(_offspring)):
			validOffspring.push_back(_offspring);
	if (validOffspring.is_empty()):
		return parentA;
	var rIndex : int = Global.rng.randi_range(0, validOffspring.size() - 1);
	return validOffspring[rIndex];
