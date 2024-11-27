class_name SaveFile extends Resource

class GameState extends Resource:
	@export var tile_info : PackedByteArray = PackedByteArray();
	@export var day : int;
	func _init(data : PackedByteArray, _day : int = 0) -> void:
		tile_info = data.duplicate();
		day = _day;


@export var undo_stack : Array[GameState];
@export var redo_stack : Array[GameState];


func add_state(data : PackedByteArray, day : int):
	undo_stack.push_back(GameState.new(data, day));
	redo_stack.clear();
	
func current_state() -> GameState:
	if (undo_stack.is_empty()):
		return GameState.new([]);
	return undo_stack.back();
	
func undo() -> GameState:
	if (undo_stack.is_empty()):
		return null;
	var temp : GameState = undo_stack.pop_back();
	redo_stack.push_back(temp);
	if (undo_stack.is_empty()):
		return GameState.new([])
	else:
		return undo_stack.back();
		
func redo() -> GameState:
	if (redo_stack.is_empty()):
		return null;
	var temp : GameState = redo_stack.pop_back();
	undo_stack.push_back(temp);
	return temp;
	
func OverwriteWith(writefrom : SaveFile) -> void:
	#TODO: Double check if deep copy needs true
	undo_stack = writefrom.undo_stack.duplicate();
	redo_stack = writefrom.redo_stack.duplicate();
