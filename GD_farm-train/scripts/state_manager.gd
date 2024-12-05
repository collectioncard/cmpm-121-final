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
	call_deferred("load_state", current_save.current_state());
	
func _input(event: InputEvent) -> void:
	if (event.is_action_pressed("undo")):
		undo();
	elif (event.is_action_pressed("redo")):
		redo();

func undo() -> void:
	load_state(current_save.undo())
	ResourceSaver.save(current_save, save_path + auto_save_path);
	
func redo() -> void:
	load_state(current_save.redo())
	ResourceSaver.save(current_save, save_path + auto_save_path);
		
func new_game() -> void:
	current_save = SaveFile.new();
	load_state(current_save.current_state());
	
func save_auto(state : PackedByteArray, day: int) -> void:
	current_save.add_state(state, day);
	ResourceSaver.save(current_save, save_path + auto_save_path);
	auto_save.OverwriteWith(current_save);

func save_to_file(slot : int) -> void:
	if (slot == 0):
		ResourceSaver.save(current_save.duplicate(), save_path + auto_save_path);
		return
	ResourceSaver.save(current_save.duplicate(), save_path + slots_path + str(slot) + ".tres");
	
func load_state(loadstate : GameState = current_save.current_state()) -> void:
	if (loadstate == null or cur_tile_grid == null):
		return;
	cur_tile_grid.reload(loadstate.tile_info, loadstate.day);

func load_from_file(slot : int) -> void:
	var temp_savefile : SaveFile;
	if (slot == 0):
		temp_savefile = ResourceLoader.load(save_path + auto_save_path);
	elif (FileAccess.file_exists(save_path + slots_path + str(slot) + ".tres")):
		temp_savefile = ResourceLoader.load(save_path + slots_path + str(slot) + ".tres");
	else:
		temp_savefile = SaveFile.new();
	current_save.OverwriteWith(temp_savefile);
	load_state(temp_savefile.current_state());
