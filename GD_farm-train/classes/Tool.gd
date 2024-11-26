class_name Tool extends Resource
var tool_name : String;
var disabled : bool;
var texture : Texture2D;

func _init(_tool_name : String, start_disabled : bool, texture_path : String) -> void:
	if (texture_path.is_empty()):
		texture = null;
	else: 
		texture = load(texture_path);
	tool_name = _tool_name;
	disabled = start_disabled;
