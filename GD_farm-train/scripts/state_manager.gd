extends Node

const save_path : String = "user://Saves";
const auto_save_path : String = "/autoSave.tres";
var auto_save : SaveFile;
var current_save : SaveFile;
const slots_path : String = "/Save";

var cur_tile_grid = null;

func _ready() -> void:
	var dir : DirAccess = DirAccess.open(save_path);
	if (dir == null):
		dir = DirAccess.open("user://");
		dir.make_dir("Saves");
		
	if (FileAccess.file_exists(save_path + auto_save_path)):
		auto_save = ResourceLoader.load(save_path + auto_save_path);
	else:
		auto_save = SaveFile.new();
		
	current_save = auto_save;
	#TODO: call_deferred(load)
	
func _input(event: InputEvent) -> void:
	if (event.is_action_pressed("undo")):
		load_state(current_save.undo())
		ResourceSaver.save(current_save, save_path + auto_save_path);
	elif (event.is_action_pressed("redo")):
		load_state(current_save.redo())
		ResourceSaver.save(current_save, save_path + auto_save_path);
		
		
func new_game() -> void:
	current_save = SaveFile.new();
	load_state(current_save.current_state());
	
func save_auto(state : PackedByteArray, day: int) -> void:
	
