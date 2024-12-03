class_name TimeMachine extends Resource

var wfc_history: Array[TimeCapsule] = [];
var current_frame: int = 0;

# Tilemap data required to draw the tiles / sprites
var tilemap: TileMapLayer = null;
var tileset: TileSet = null;
var tileset_id : int = 0;
var map_size_x: int = -1;
var map_size_y: int = -1;
var tiles: Array = [];
var node: Node = null;
var timer: Timer = null;

var playback_speed : int = 4;

var drawn_items: Array = []; # Keeps track of all drawn items so we can clean up after ourselves
var tile_texture_cache: Dictionary = {};

var blend_shader = load("res://shaders/blend.gdshader");

var decor_seed: int = -1;

func _init(map_layer: TileMapLayer, new_tileset, new_tiles, size_x, size_y, parent_node) -> void:
	tilemap = map_layer;
	tileset = new_tileset;
	tiles = new_tiles;
	map_size_x = size_x;
	map_size_y = size_y;
	node = parent_node;
	decor_seed = RandomNumberGenerator.new().randi();

#Animates the WFC algorithm according to captured history
func animate_map(slp_time):
	timer = Timer.new();
	timer.wait_time = slp_time;
	node.add_child(timer);
	timer.start();
	current_frame = 0;
	while current_frame < wfc_history.size() - playback_speed:
		await timer.timeout;
		draw_map(current_frame);
		current_frame += playback_speed;
	current_frame = wfc_history.size() - 1;
	draw_map(current_frame);
	timer.stop();
	
#Replaces the timemachine's tilemap with the output of the wfc algoritm at a given frame
func draw_map(frame: int = wfc_history.size() - 1) -> void:
	tilemap.tile_set = tileset;

	# Get the collapsed tiles from the time capsule
	var collapsed_tiles: Array = wfc_history[frame]._collapsed;

	# Clear the tilemap and old sprites before redrawing
	tilemap.clear();
	delete_all_drawn_items();
	
	var is_collapsed_complete: bool = !collapsed_tiles.has(-1);

	var index: int = 0
	for tile_y in range(map_size_y):
		for tile_x in range(map_size_x):
			if !collapsed_tiles.is_empty() and is_collapsed_complete:
				# Draw directly if all tiles are collapsed
				var tile_data = tiles[collapsed_tiles[index]];
				var transform = Global.cardinality_transformations.get(tile_data[1], 0);
				tilemap.set_cell(Vector2i(tile_x, tile_y), tileset_id, tile_data[0], transform);
			else:
				# Draw from wave if not all tiles are collapsed
				var possible_tiles: Array = wfc_history[frame].get_valid_tiles(index);
				if possible_tiles.size() == 1:
					# If only one tile is possible, draw it
					var tile_data = tiles[possible_tiles[0]];
					var transform = Global.cardinality_transformations.get(tile_data[1], 0);
					tilemap.set_cell(Vector2i(tile_x, tile_y), tileset_id, tile_data[0], transform);
				elif possible_tiles.size() > 1:
					# if multiple tiles are possible, draw a blend of them
					draw_blend_sprites(Vector2(tile_x+.5, tile_y+.5) * 16, possible_tiles);
				else:
					# If no tiles are possible, print a debug message and abort drawing
					print_debug("No possible tiles at position: " + str(Vector2i(tile_x, tile_y)) + " at frame: " + str(frame) + " aborting draw");
					
					var fail_label = Label.new()
					fail_label.text = "Generation Failed"
					fail_label.scale = Vector2(10, 10)
					fail_label.position = Vector2(200, 200) / 2
					fail_label.set("theme_override_colors/font_color", Color(1, 0, 0))
					node.add_child(fail_label)
					drawn_items.append(fail_label);
					return;
			index += 1;
		
func move_forward() -> void:
	if timer:
		timer.stop();
	if current_frame < wfc_history.size() - 1:
		current_frame += 1;
		draw_map(current_frame);
		return;
	print_debug("Cannot go forward in time any further");

func move_backward() -> void:
	if timer:
		timer.stop();
	if current_frame > 0:
		current_frame -= 1;
		draw_map(current_frame);
		return;
	print_debug("Cannot go back in time any further");

func draw_blend_sprites(tile_pos: Vector2, possible_tiles: Array):
	#Create sprite at location of the missing tile
	var blended_sprite: Sprite2D = Sprite2D.new();
	blended_sprite.position = tile_pos;
	blended_sprite.scale = Vector2(.1,.1);
	blended_sprite.texture = load("res://icon.svg");
	
	#Create the shader (or attempt to anyway)
	var blend_material = ShaderMaterial.new();
	blend_material.shader = blend_shader;

	blend_material.set_shader_parameter("tex1", get_cell_texture(tiles[possible_tiles[0]][0]));
	blend_material.set_shader_parameter("tex2", get_cell_texture(tiles[possible_tiles[1]][0]));
	
	#Apply the material and draw the sprite
	blended_sprite.material = blend_material;
	drawn_items.append(blended_sprite);
	node.add_child(blended_sprite);
			

func delete_all_drawn_items():
	if !drawn_items.is_empty():
		for sprite in drawn_items:
			if node.has_node(sprite.get_path()):
				sprite.queue_free();
				node.remove_child(sprite);
		drawn_items.clear();

func get_cell_texture(coord:Vector2i) -> Texture:
	if tile_texture_cache.get(coord) != null:
		return tile_texture_cache.get(coord);

	var source:TileSetAtlasSource = tileset.get_source(0) as TileSetAtlasSource;
	var rect := source.get_tile_texture_region(coord);
	var image:Image = source.texture.get_image();
	var tile_image := image.get_region(rect);
	tile_texture_cache[coord] = ImageTexture.create_from_image(tile_image);
	return tile_texture_cache[coord];

func add_capsule(collapsed_tiles, wfc_wave):
	var new_capsule = TimeCapsule.new(collapsed_tiles, wfc_wave);
	wfc_history.append(new_capsule);
	current_frame += 1;

func _notification(what):
	# Destructor. Clean stuff up.
	if (what == NOTIFICATION_PREDELETE):
		# If node is empty, then we are probably exiting so we dont care
		if !node.get_children():
			return; 
		
		# Clear any drawn sprites
		if !drawn_items.is_empty():
			for sprite in drawn_items:
				if node.has_node(sprite.get_path()):
					sprite.queue_free();
					node.remove_child(sprite);
			drawn_items.clear();

		#Delete the timer and remove it from node
		if timer:
			timer.stop();
			node.remove_child(timer);
			timer.queue_free();

class TimeCapsule:
	var _collapsed: Array = [];
	var _wave: Array = [];

	func _init(collapsed_tiles, wfc_wave):
		_collapsed = collapsed_tiles;
		_wave = wfc_wave;

	#TILE POSITION STARTS FROM 0
	func get_valid_tiles(tile_position: int):
		var valid_tile_ids = [];

		for i in _wave[tile_position].size():
			if _wave[tile_position][i] == true:
				valid_tile_ids.append(i);

		return valid_tile_ids;
