extends Button

func _on_button_down() -> void:
	StateManager.undo(); 
