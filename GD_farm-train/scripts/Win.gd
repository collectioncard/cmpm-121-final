class_name Win extends Label
var timer : float = 0;

func playerWin() -> void:
	visible = true;
	
func _process(_delta: float) -> void:
	timer += _delta;
	if (timer >= .1):
		create_tween().tween_property(self, "modulate", Color.from_hsv(Global.rng.randf(), 1, 1), 0.1);
		timer = 0;
