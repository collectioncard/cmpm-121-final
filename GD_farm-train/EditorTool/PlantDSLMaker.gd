@tool
class_name PlantDSLMaker extends Node2D

##Only use to remove plants!
@export var plant_dict : Array[PlantDef];
@export var wip_plant : PlantDef;

##Press to validate plant, and if valid, add to dict!
@export var validate : bool = false:
	set(value):
		validate_new_plant(value);
@export var save : bool = false:
	set(value):
		save_plants(value);

func _process(_delta: float) -> void:
	if Engine.is_editor_hint():
		validate = false;
		save = false;
		
		if wip_plant == null:
			return;
		wip_plant.growth_stages = wip_plant.texturePaths.size();
			
		if wip_plant.valid_soils.size() > wip_plant.SoilTypes.size():
			print("Too many valid soil types, avoid redundancy.")
			wip_plant.valid_soils.resize(wip_plant.SoilTypes.size());
			
		if wip_plant.texture_to_add != null:
			validate_texture();
			
		if wip_plant.needed_neighbors.size() > 4:
			print("A plant can only have 4 valid neighbors!")
			wip_plant.needed_neighbors.resize(4);

				
func validate_texture() -> void:
	if wip_plant.texture_to_add is not CompressedTexture2D:
		print("Need to select valid texture!")
		wip_plant.texture_to_add = null;
	var text_path : String = wip_plant.texture_to_add.get_path();
	if !FileAccess.file_exists(text_path):
		print("Unable to find texture!");
		wip_plant.texture_to_add = null;
	else: 
		wip_plant.texturePaths.push_back(text_path);
		print("Added texture!")
		wip_plant.texture_to_add = null;
		
func validate_new_plant(val : bool) -> void:
	if (val):
		if (wip_plant.texturePaths.is_empty() or wip_plant.valid_soils.is_empty()):
			print("Plant seems to be empty, fail validation.")
		else:
			print("Success: Added to plant dict!");
			plant_dict.push_back(wip_plant);
			wip_plant = null;
			
func save_plants(val : bool) -> void:
	if (val):
		var new_dict : PlantDict = PlantDict.new();
		new_dict.plants = plant_dict.duplicate(true);
		ResourceSaver.save(new_dict, Global.plant_dict_path);
		print("Saved plant dict!");
