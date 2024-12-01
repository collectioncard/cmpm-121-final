class_name Special_Rules extends Tiled_Rules
@export var tile_id_of : Dictionary;
#tile structure (index should be the same as id) {"atlas_coords":Vector2, "cardinality":0 thorugh 7,  "weight":int}
#neighbors structure: {"tile1":tile_id, "tile2":tile_id, "direction":int} #direction should only be left
