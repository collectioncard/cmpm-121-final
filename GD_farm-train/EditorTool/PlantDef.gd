class_name PlantDef extends Resource

@export var Name :  String;

##Adds the path of this texture to texture paths, needs to be valid image in project!
@export var texture_to_add : Texture2D = null;

##Do not edit manually!
@export var texturePaths : Array[String];

enum SoilTypes {
	PLACEHOLDERDONOTCHOOSE = 0, #For some reason the first always deafults to 0, but SAND needs to be 1
	SAND = 1,
	GROUND = 2,
	GRASS = 3,
	WATER = 4
}

@export var valid_soils : Array[SoilTypes];

enum MoistureLevels {
	LOW,
	MEDIUM,
	HIGH,
	NOPREF
}

##Must equal the number of texture paths.
@export var growth_stages : int;

@export var moisture : MoistureLevels;

enum SunLevels {
	LOW,
	MEDIUM,
	HIGH,
	NOPREF
}

@export var sun : SunLevels;

##String names of plants (WHICH SHOULD EXIST IN PLANT DICT). Maximum of 4 neighbors. Each entry should be a name. Case-Sensitive
@export var needed_neighbors : Array[String];

##String names of plants (WHICH SHOULD EXIST IN PLANT DICT by compile time).
@export var offspring : Array[String];
