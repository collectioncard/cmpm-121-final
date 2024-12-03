class_name SaveFile extends Resource

@export var undo_stack : Array[GameState];
@export var redo_stack : Array[GameState];

func _init(_undo_stack : Array[GameState] = [], _redo_stack : Array[GameState] = []) -> void:
	undo_stack = _undo_stack.duplicate();
	redo_stack = _undo_stack.duplicate();

func add_state(data : PackedByteArray, day : int):
	undo_stack.push_back(GameState.new().create(data, day));
	redo_stack.clear();
	
func current_state() -> GameState:
	if (undo_stack.is_empty()):
		return GameState.new().create();
	return undo_stack.back();
	
func undo() -> GameState:
	if (undo_stack.is_empty()):
		return null;
	var temp : GameState = undo_stack.pop_back();
	redo_stack.push_back(temp);
	if (undo_stack.is_empty()):
		return GameState.new().create();
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
