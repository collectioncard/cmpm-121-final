extends CanvasLayer

var menu_popup: Popup
var save_selector: OptionButton
var new_game_button: Button
var save_button: Button
var load_button: Button

var _save_slots = ["Prev Auto Save", "Save Slot 1", "Save Slot 2"]

func _ready() -> void:
	menu_popup = $MenuPopup
	save_selector = $MenuPopup/VBoxContainer/SaveSelector
	new_game_button = $MenuPopup/VBoxContainer/NewGameButton
	save_button = $MenuPopup/VBoxContainer/SaveButton
	load_button = $MenuPopup/VBoxContainer/LoadButton
	
	for slot in _save_slots:
		save_selector.add_item(slot)
	save_selector.select(0)

	menu_popup.hide()
	
	save_button.pressed.connect(_on_save_button_button_down);
	load_button.pressed.connect(_on_load_button_button_down);
	new_game_button.pressed.connect(_on_new_game_button_button_down);


func _process(_delta: float) -> void:
	if Input.is_action_just_pressed("menu"):
		if menu_popup.visible:
			menu_popup.hide();
		else:
			menu_popup.show();
			
	Input.mouse_mode = Input.MOUSE_MODE_VISIBLE if menu_popup.visible else Input.MOUSE_MODE_CONFINED
	get_tree().paused = menu_popup.visible
			
			

func _on_load_button_button_down() -> void:
	StateManager.load_from_file(save_selector.get_selected());


func _on_save_button_button_down() -> void:
	StateManager.save_to_file(save_selector.get_selected());


func _on_new_game_button_button_down() -> void:
	print_debug("New game");
	StateManager.new_game();