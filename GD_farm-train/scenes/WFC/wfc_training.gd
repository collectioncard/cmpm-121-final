extends Node2D
@onready var sample_layer : TileMapLayer = get_node("%sample");
@onready var output_layer : TileMapLayer = get_node("%output");

@onready var WFC : WFC_Solver = get_node("%WFC");

var defn_maker : Defn_Factory;
var defn : Tiled_Rules;
var model : Tiled_Model;
var tm: TimeMachine;

@export var tile_data : PackedByteArray;


var time_stamp : int;
var weight_even : bool = true;

func _ready() -> void:
	#create_def();
	#create_special_model();
	do_WFC();
	
		
func _input(event: InputEvent) -> void:
	if (event.is_action_pressed("nextDay")):
		BetterTerrain.update_terrain_area(output_layer, Rect2(0, 0, 40, 23), true);
		#tile_data = output_layer.get_tile_map_data_as_array();
		

func create_def() -> void:
	print_debug("Beginning definition building!");
	time_stamp = Time.get_ticks_msec();
	defn_maker = Defn_Factory.new(sample_layer, Vector2(17, 3), weight_even, false);
	defn = defn_maker.setup();
	ResourceSaver.save(defn_maker.new_definition, "res://scenes/WFC/Output/world_map_defn.tres");
	print_debug("Finished building definition in:");
	print_debug(Time.get_ticks_msec() - time_stamp);
	
func create_special_model() -> void:
	print_debug("Beginning rules building!");
	time_stamp = Time.get_ticks_msec();
	defn = load("res://scenes/WFC/Output/world_map_defn.tres");
	model = Special_Model.new(defn, Global.TILE_MAP_SIZE.x, Global.TILE_MAP_SIZE.y);
	model.setup();
	ResourceSaver.save(model, "res://scenes/WFC/Output/world_map_model.tres");
	print_debug("Finished building rules in:");
	print_debug(Time.get_ticks_msec() - time_stamp);
	
func do_WFC() -> void:
	print_debug("Beginning WFC!");
	time_stamp = Time.get_ticks_msec();
	model = load("res://scenes/WFC/Output/world_map_model.tres");
	
	WFC.pre_initialize(model, output_layer);
	WFC.populate_WFC(output_layer);
	tm = WFC.generate_with_time_machine(Global.Seed);
	tm.draw_map()
	print_debug("First run of WFC finished in:");
	print_debug(Time.get_ticks_msec() - time_stamp);
	pass
