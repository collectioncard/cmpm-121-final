class_name MouseInputManager extends Node2D

var tile_cursor : Sprite2D;
var player : AnimatedSprite2D;
var player_pos_tween : Tween;

var Tools : Array[Tool];
var cur_tool_idx : int;

signal Tile_Click;

func make_tile_cursor() -> void:
	tile_cursor = Sprite2D.new();
	add_child(tile_cursor);
	tile_cursor.texture = load("res://assets/tileCursor.png");
	
func make_player() -> void:
	player = AnimatedSprite2D.new();
	add_child(player);
	player.position = Vector2(56, 56);
	player.sprite_frames = load("res://assets/playerAnims.tres");
	player.play();
	
func initialize_tools() -> void:
	Tools = [
		Tool.new("Open_Hand", false),
		Tool.new("Hoe", false, "res://assets/trowel.png"),
		Tool.new("Cross_Breed_Tool", false, "res://assets/plants/idkwhattocallthis.png"),
		Tool.new("Seed_One", false, "res://assets/plants/plant1-0.png"),
		Tool.new("Seed_two", true, "res://assets/plants/plant2-0.png")];
	cur_tool_idx = 0;
	
func connect_to_grid() -> void:
	Tile_Click.connect(StateManager.cur_tile_grid.tile_click);
	StateManager.cur_tile_grid.Unlock.connect(unlock);
	
func _ready() -> void:
	StateManager.cur_tile_grid = get_node("TileGrid");
	connect_to_grid();
	make_tile_cursor();
	make_player();
	initialize_tools();
	
func _process(_delta: float) -> void:
	if (Input.is_action_pressed("action1")):
		if (!tile_cursor.visible):
			return;
		Tile_Click.emit(get_global_mouse_position(), Tools[cur_tool_idx]);
		player.flip_h = get_global_mouse_position().x < player.position.x;
		
func move_player(new_pos : Vector2) -> void:
	if (player_pos_tween != null):
		player_pos_tween.kill();
	player_pos_tween = player.create_tween();
	player_pos_tween.tween_property(
		player,
		"position",
		Utils.tile_from_vec(new_pos) + Global.SPRITE_OFFSET,
		player.position.distance_to(Utils.tile_from_vec(new_pos) + Global.SPRITE_OFFSET) / Global.PLAYER_SPEED
	);
	player.flip_h = get_global_mouse_position().x < player.position.x;
	
func _input(event: InputEvent) -> void:
	if (event is InputEventMouseMotion):
		tile_cursor.position = Utils.tile_from_vec(event.position) + Global.SPRITE_OFFSET;
		tile_cursor.visible = tile_cursor.position.distance_to(player.position) < Global.INTERACTION_RADIUS;
	#move from mouse click
	elif (event.is_action_pressed("action2")):
		move_player(get_global_mouse_position());
	#move from wasd
	elif (event.is_action_pressed("up")):
		var new_pos : Vector2 = player.position + (Vector2.UP * Global.TILE_HEIGHT);
		move_player(new_pos);
	elif (event.is_action_pressed("down")):
		var new_pos : Vector2 = player.position + (Vector2.DOWN * Global.TILE_HEIGHT);
		move_player(new_pos);
	elif (event.is_action_pressed("left")):
		var new_pos : Vector2 = player.position + (Vector2.LEFT * Global.TILE_WIDTH);
		move_player(new_pos);
	elif (event.is_action_pressed("right")):
		var new_pos : Vector2 = player.position + (Vector2.RIGHT * Global.TILE_WIDTH);
		move_player(new_pos);
	elif (event.is_action_pressed("toolup")):
		cur_tool_idx = (cur_tool_idx + 1) % Tools.size();
		while (Tools[cur_tool_idx].disabled):
			cur_tool_idx = (cur_tool_idx + 1) % Tools.size();
		get_node("%ToolTexture").texture = Tools[cur_tool_idx].texture;
	elif (event.is_action_pressed("tooldown")):
		cur_tool_idx = (cur_tool_idx - 1) % Tools.size();
		while (Tools[cur_tool_idx].disabled):
			cur_tool_idx = (cur_tool_idx - 1) % Tools.size();
		get_node("%ToolTexture").texture = Tools[cur_tool_idx].texture;

	
func unlock(unlockee : int) -> void:
	match unlockee:
		2:
			Tools[4].disabled = false;
			get_node("Win").playerWin();
		
