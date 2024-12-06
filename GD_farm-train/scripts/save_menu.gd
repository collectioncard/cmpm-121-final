extends CanvasLayer

var menu_popup: Popup
var save_selector: OptionButton
var new_game_button: Button
var save_button: Button
var load_button: Button
var language_selector: OptionButton

var _save_slots = ["Previous Auto Save", "Save Slot 1", "Save Slot 2"]
var _language_slots = ["English", "Español", "日本語", "عربي"]

func _ready() -> void:
	menu_popup = $MenuPopup
	save_selector = $MenuPopup/VBoxContainer/SaveSelector
	new_game_button = $MenuPopup/VBoxContainer/NewGameButton
	save_button = $MenuPopup/VBoxContainer/SaveButton
	load_button = $MenuPopup/VBoxContainer/LoadButton
	language_selector = $MenuPopup/VBoxContainer/LanguageSelector
	
	for slot in _save_slots:
		save_selector.add_item(slot)
	save_selector.select(0)
	
	for slot in _language_slots:
		language_selector.add_item(slot);
	
	language_selector.item_selected.connect(_on_language_selected)

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


func _on_save_menu_button_button_down() -> void:
	if menu_popup.visible:
		menu_popup.hide()
	else:
		menu_popup.show()
		Input.mouse_mode = Input.MOUSE_MODE_VISIBLE
		get_tree().paused = true
	
func _on_language_selected(index) -> void:
	match index:
			0: 
				TranslationServer.set_locale("en")
			1:
				TranslationServer.set_locale("es")
			2:
				TranslationServer.set_locale("ja")
			3:
				TranslationServer.set_locale("ar")
