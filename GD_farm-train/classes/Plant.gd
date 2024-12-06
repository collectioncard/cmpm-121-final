class_name Plant extends RefCounted

var id : int;
var name : String;
var offspring : Array[int];

var growth_stages : int;
var texture_paths : Array[String];

var valid_soil_types : Array[int];
var needed_neighbors : Array[int];
var sun_needed : Vector2i;
var moisture_needed : int;

static var CROSSBREED = Plant.new(
	0,
	"Crossbreed",
	[1,2,3,4],
	["res://assets/plants/idkwhattocallthis.png"],
	-1,
	[],
	Vector2i(-1, -1),
	-1
);

#Some godot stuff, added allowance for empty constructor out of safety, and occasional convenience.
func _init(_id: int = -1, _name:String = "Error", _valid_soils : Array[int] = [], _texturePaths : Array[String] = ["res://assets/trowel.png"], _growthStages : int = 0, _neigbors : Array[int] = [], _sun : Vector2i = Vector2i.ZERO, _moist : int = 0):
	id = _id;
	name = _name;
	valid_soil_types = _valid_soils;
	texture_paths = _texturePaths;
	growth_stages = _growthStages;
	needed_neighbors = _neigbors;
	needed_neighbors.sort();
	sun_needed = _sun;
	moisture_needed = _moist;
	offspring.push_back(self.id);
	
		
func get_type_name() -> int:
	return id;
	
func planting_check(soil_type: int) -> bool:
	return valid_soil_types.has(soil_type);
	
func growth_check(sun : int, moisture: int, _neighbors : Array = []):
	
	if (!(sun > sun_needed.x && sun < sun_needed.y)):
		return false;
	if (!(moisture >= moisture_needed)):
		return false;
	if needed_neighbors.is_empty():
		return true;
	#If no checks above fail, check if for each neighbor in needed_neighbors, the actual neighbors have atleast that many neighbors.
	return needed_neighbors.all(func(val):
		return _neighbors.count(val) >= needed_neighbors.count(val);
	);
		
	
func add_offspring(newOffspringID : int) -> void:
	offspring.push_back(newOffspringID); #Could check for redundancies, but this could have the extra benefit of being a naive way to have some offspring be more likely.

static func choose_offspring(parentA : Plant, parentB : Plant) -> Plant:
	var validOffspring : Array[int];
	#TODO: Not O(n^2) solution
	for _offspring in parentA.offspring:
		if (parentB.offspring.has(_offspring)):
			validOffspring.push_back(_offspring);
	if (validOffspring.is_empty()):
		return parentA;
	var rIndex : int = Global.rng.randi_range(0, validOffspring.size() - 1);
	return Global.Plants[validOffspring[rIndex]];
