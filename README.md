# cmpm-121-final
# Devlog Entry - [11/12/24]
Team Members: Thomas Wessel, Jacqueline Gracey, Jarod Spangler, Jack Wilhelm


## Introducing the team
* Tools Lead - Thomas Wessel: Will help everyone get their tools set up, manage source control, and establish code style conventions.
* Engine Lead - Jacqueline Gracey: Look into alternative engines in case of swapping and be the got to for knowledge about this alternate engine. Manage folder organization among everyone.
* Engine Assistant - Michael Campanile: Assist the lead in researching engines and learning more about these alternatives as well as organize assets and files in accordance with the lead.
* Producer - Jarod Spangler: Will help manage the direction of the game, help manage time and work delegation, as well as a little bit of everything. Help contribute to the style of the game, and help design or decide on art assets, as well as guide the vision for gameplay, and help write code.
* Design Lead- Jack Wilhelm: Will help write code alongside working with artistic and systematic ideas and goals, as well as help guide gameplay.

## Tools and materials
1. Engine: We are choosing to use Godot. The majority of the team primarily only has experience with phaser, but we feel Godot can be more versatile without too extreme of a learning curve. Since Godot is the only other engine some team members have experience with, it shouldn't be too hard to get everyone up to speed.
2. Programming Languages: Godot is mainly compatible with C# and GDScript. We are choosing to use C#, because it would be better to switch from C# to GDScript than the other way around, if necessary.
3. Tools: 
	 - IDEs:
		 - The team will have a choice of using their preferred editor between Godot's default, Jetbrains Rider, or VS Code
	 - Asset Development:
		 - Spritework will be done using [Aseprite](https://www.aseprite.org/) as some of our team has used it before and feels comfortable using and teaching it.
	 - Linters/pre-commit hooks:
		 - For now, we are probably going to use c sharpier. Other tools might be included as we go along.

4. Alternate Platform: The alternate platform would be using Godot with GDScript.
## Outlook
* Unique Goals: We plan to integrate a train into the gameplay, which would be unique. We also plan to integrate procedural generation into generating the 2D tile map.
* Challenges: The biggest challenges we anticipate are making sure everything is coded in ways that are readable and iterable for the entire team. The main programmatic challenge we anticipate is pathing for the train. 
* Learning Goals: Learn to use a more advanced engine like Godot. Learn and practice meaningful programming patterns, while putting them into practice. Learning to better work on a team.

# Devlog Entry - [11/21/24]

## How we satisfied the software requirements
* [F0.a]: In our game the player is moved by left clicking on the map. The player will then move to the tile in which the player clicked. If a new destination is ever selected while the player is en route to a previous destination, the player will automatically reroute to the latest destination. With this system, the player is indeed able to move through the 2D grid of our world.
* [F0.b]: While our player moves in real time as described by the above 2D grid movement, day progression is manual and controlled by pressing the space button. Time movement runs a set of processes over every plant to check if they qualify to grow. This process only happens when a player manually moves the day forward. The current day is also tracked in the upper left-hand corner of the screen. Effectively making each day function as a “turn”.
* [F0.c]: In our game we have an array of tools used for the reaping and sowing system. Using the wheel up and wheel down controls, a player moves between tools. A player may only use a tool on a tile if it is close enough to the player, which is indicated by the tile's border being outlined when the mouse hovers over it. We have a variable ‘InteractionRadius’, which determines the distance a tile can be from the player to interact with a tile. These tools allow the player to sow seeds and later reap them, but only when a tile is within the interaction radius of a tile, thus fulfilling the requirements of F0.c.
* [F0.d]: The increment in which sun and moisture levels are increased is generated each day. These are generated with C#’s Random object, with a specific seed based on tile position, global seed, and day. This way they are generated randomly, but can be calculated deterministically elsewhere without having to access a variable, and so the game can be seeded,this has the added benefit of not needing to store the value in a variable or pass it around. Sun is generated each day and is not carried over between days, it isn’t even stored in a variable. Moisture levels are stored within the plant tile and accumulated each day, the accumulation each day is generated the same way as the sun. This way the incoming sun and water are randomly generated each turn, with sun not accumulating, and water being able to accumulate.
* [F0.e]: We have 3 plant types: plant 1, plant 2, and “crossbreeder”. The crossbreed type is used to crossbreed or clone adjacent plants. The plant type is determined by a property in PlantTile called _plantType of type Plant (which is a separate class). The class PlantTile also includes a property called _growthStage which represents which stage of growth that tile is at. Growth stages are visually represented as different sized/colored plant sprites unique to the plant type. Thus the requirements of distinct types and growth stages are satisfied.
* [F0.f]: When the day changes, each plantTile runs TurnDay which checks if _plantType.GrowthCheck is true. GrowthCheck takes in the 2 calculations of the tiles current sun (via CalcSun(day)) and the accumulated moisture of the tile (via CalcMoisture(day)). GrowthCheck then checks these 2 arguments against the growth conditions, from a PlantTile’s _plantType to see if the current sun and moisture level are satisfactory to allow the plant to increase its growth stage. If a crossbreed type plant is attempting to grow it will try to propagate. During propagation it will check for valid mature neighbors (plants that aren’t null and are at their max growth stage). If there are not enough mature nearby neighbors, propagation will fail. If there are enough, there is a random chance propagation will change the crossbreed plant into a new plant type or a parent plant type. Thus our plants check for spatial rules, sun levels, and water levels to create satisfying conditions for growth.
* [F0.g]: Currently a player wins when they get plant 2 from crossbreeding 2 plant 1 parents. As explained above, when propagation occurs, a new plant is unlocked and grows in place of the crossbreed plant. When this unlock occurs from the successful propagation of plant 2, the player is signaled that they have won in the upper right-hand corner of the screen. 

## Reflection
During the process of making our game we made a couple changes on our outlook. One was that one of our original game goals was to add a train system into our game. Fearing this may be stepping too far and overestimating our abilities we decided that a train would be an addition if/when our other tasks were completed. To keep our game unique however we decided to implement a crossbreeding system because one of our team members was very confident in their ability to easily implement this design. When starting on our project, we quickly decided to split the work amongst branches. Once each member's work was complete, the branches were brought together and merged into main. We implemented some pre-commit hooks to better organize our work, help keep styling across branches more consistent, and make sure code was only pushed when it could build successfully. Our original idea for roles quickly changed as team members found less value offering some goals and turned more towards helping write code. Our goals for using C# in Godot remains the same as our members have become accustomed to the platform. For the future however, we will continue to find ways to face our original outlook challenge and continue to work towards making our code accessible to and readable to new eyes.


# Devlog Entry - [11/27/24]

## How we satisfied the software requirements

### F0 Requirements:
 * **[F0.a]**: This system has not changed in the latest version of our game
 * **[F0.b]**: This system has not changed in the latest version of our game
 * **[F0.c]**: This system has not changed in the latest version of our game
 * **[F0.d]**: This system has not changed in the latest version of our game
 * **[F0.e]**: This system has not changed in the latest version of our game
 * **[F0.f]**: This system has not changed in the latest version of our game    
 * **[F0.g]**: This system has not changed in the latest version of our game
    
### F1 Requirements:
* **[F1.a]**:In our implementation, we used an array of structs style format for our contiguous byte array. This allows us to easily store structs passed to our datastructure and return them when requested. We store every value of the game's core data as a 32 bit signed int which allows for us to easily find an element's offset in the array based on its coordinate on the map + the property's offset given by an enum. Any information that we don't directly store can be derived from the data that we store.
	* The byte Array contains multiple instances of this layout with offsets given by the map coordinate of the tile: 
		```mermaid
		packet-beta
		0-31: "SoilType (int)"
		32-63: "PlantId (int)"
		64-95: "MoistureLevel (int)"
		96-127: "GrowthStage (int)"
		128-159: "CoordX (int)"
		160-191: "CoordY (int)"

		```

* **[F1.b]**: For the manual progression save we have created a menu that the player can bring up with a drop down for the save slots and buttons from New Game, Save, and Load. Save and Load will save/load the game to/from the specific selected save slot. New game will initiate a new game but not erase the old save slots unless the player manually overwrites them. More on how saves work below.
* **[F1.c]**: Our auto save feature activates after any major action, an action which changes the game’s state, such as using a tool or changing the day, simply moving or changing the selected tool does not persist as they do not affect the state of the tile grid. An existing autosave is loaded by default when opening up the game after quitting, and the autosave has a specified save slot in our save menu so the player can still choose to manually save or load it.
* **[F1.d]**: Every state change of the game pushes the necessary data into a GameState object, which holds the data from the contiguous byte array. These GameStates are held in SaveFile objs which have undo and redo stacks, and functions to support undo/redo and saving/loading. Simply put, undoing and redoing will pull a copy of the GameState on the top of either stack, which is removed from that stack, then pushed to the other. The temp GameState is then loaded to rebuild the game’s state with. This way all save files keep their history of GameStates allowing for undoing all the way back to the original.

## Reflection
This was a bit of an undertaking, and with F2 being due so closely after, we had little time to iterate on the overall design. So there have been little to no gameplay changes. No extra player feedback, or anything like that. We already had a sense of eventually needing to make a contiguous byte array, so we already had data stored in an array of structs, so there was just some trouble in trying to switch to a byte[] over the course of multiple smaller refactors. Undo/redo didn’t really fit in our vision of the game, so it functions as a brute force approach of just storing the entire state of the game, and reloading it. Then we had to change how we got the data from our main array to the body of the main game structure (TileGrid). Saving and loading is facilitated by Godot’s resource saver, which makes it very easy to save and load custom objects to just throw in arrays of entire game states. 

# Devlog Entry - [12/02/24]


## How we satisfied the software requirements

### F0 + F1 Requirements:
 * **[F0.a]**: This System has not changed in the latest version of our game
 * **[F0.b]**: This System has not changed in the latest version of our game
 * **[F0.c]**: This System has not changed in the latest version of our game
 * **[F0.d]**: This System has not changed in the latest version of our game
 * **[F0.e]**: This System has not changed in the latest version of our game
 * **[F0.f]**: This System has not changed in the latest version of our game    
 * **[F0.g]**: This System has not changed in the latest version of our game
 * **[F1.a]**: This System has not changed in the latest version of our game
 * **[F1.b]**: This System has not changed in the latest version of our game
 * **[F1.c]**: This System has not changed in the latest version of our game
 * **[F1.d]**: This System has not changed in the latest version of our game


    
### F2 Requirements:
 **[F2.a]**: Our external DSL for scenarios was designed in JSON. From the below example, the way the DSL works is it first defines the seed for the random number generator, or blank for a random seed. It defines what map type, the valid options are “premade” for the map we originally made for earlier stages, “” for a completely randomly generated world, “river” for a world template that starts with a river, and “low_water” for a map that starts with little water. The latter 3 templates are generated with wave function collapse, based on none, or some pre-set tiles. Then the scenarios array holds the main scenarios for the flow of the game. “Weather” defines the weights and what weathers are valid for the scenario, the four shown in the example are the currently implemented weather conditions, omitting any omits that weather from the scenario. The “weather” key is optional, without one all weathers are valid at even weight. The optional “schedule” key defines if there should be any specific weather on specific days, anything not listed just has random, weighted weather. The “win” key defines a win condition, “plant” is the plant that needs to be crossbred, and “amount” defines how many need to be crossbred to win the current scenario. Once a current scenario is won, if there are more scenarios, they will be loaded in and continue, if the completed scenario is the last one the player win message is displayed. E.g. Start a map with a seed of 13, from map type “river” create 1 scenario, where all four weather conditions are present with each of the given weights. Schedule sun for day 1 and rain for day 5. The scenario win conditions are to crossbreed 2 Lilylurk plants.
 
*[External DSL example]*: ```
{
  "seed": 13,
  "map": "river",
  "scenarios": [
	{
	  "weather": {
		"sunny": 0.1,
		"rainy": 0.2,
		"overcast": 0.1,
		"average": 0.6
	  },
	  "schedule": {
		"1": "sunny",
		"5": "rainy"
	  },
	  "win": {
		"plant": "Lilylurk",
		"amount": 2
	  }
	}
  ]
}```

 **[F2.b]**: For the plant DSL we used Godot’s resource system, for its ease of definition and inherent ability to directly store variables. This is a little different than something like JSON because these aren’t as readily directly human writable, they are in a specific format meant to be managed from the engine. To facilitate this we made a tool that can work independently of the rest of the games code to construct them. The tool uses a built in Godot node with properties and code to allow it to validate and build a dictionary of plant definitions to be read during run time to construct all the plant class objects.
* The name is a string that defines the plant’s name.
* The “Texture to Add” is not part of the plant DSL, it's so a designer can copy in a plant sprite asset, which the editor tool will read in, make sure its a valid image in the project, then add as an internal file path for the plant DSL to “Texture Paths”, which can also be edited manually. 
* “Valid soils” is an array in the DSL, but using Godot’s UI you can manually select and add valid soil types visually, as even though it is an array of ints, it's constructed from enums, so a designer can just pick soil names to add. 
* “Growth Stages” are the number of growth stages a plant has, and is based on the number of textures added, as each growth stage needs a texture, though repeats are valid. 
* “Moisture” and “Sun” are enums that can be selected visually, which range from NoPref to High. 
* “Needed Neighbors” and “Offspring” are arrays of strings that are names of plants. Through Godot’s UI these can all be added and edited manually. None of the aspects of the tool need any knowledge of gdscript/C#. 

The validate button validates a plant as having required fields and valid texture paths, and then adds it to the primary plant dict object. Save saves the plant dict object, that way a designer wouldn’t need to write code to save it manually. Plant Def and Plant are mutually exclusive, that way PlantDef’s could be written to without knowledge of the rest of the code, and the code to create a plant class from a PlantDef object can be independently maintained. The editor tool also makes translation from string names and enums to int id's easier. E.g. Make the plant Redroot, with the 3 given textures, and therefore 3 growth stages, its valid soils are dirt and grass, it has a preference for 

*[Internal DSL Constructor Tool]*:

 ![test](https://i.imgur.com/VxeC5J7.png)

*[Internal DSL File Example]*: (Note: the main properties are human readable and editable, but the resource id’s and paths need to be managed in the engine.)
    
    [gd_resource type="Resource" script_class="PlantDict" load_steps=13 format=3 uid="uid://ckdsaq2uc0mkm"]

    [ext_resource type="Script" path="res://EditorTool/PlantDef.gd" id="1_5c6cm"]
    [ext_resource type="Script" path="res://EditorTool/PlantDict.gd" id="2_1oe5o"]
    [sub_resource type="Resource" id="Resource_gvaw3"]
    script = ExtResource("1_5c6cm")
    
    Name = "Redroot"
    texturePaths = Array[String](["res://assets/plants/plant1-1.png", "res://assets/plants/plant1-2.png", "res://assets/plants/plant1-3.png"])
    valid_soils = Array[int]([2, 3])
    growth_stages = 3
    moisture = 1
    sun = 1
    needed_neighbors = Array[String](["Redroot", "Redroot", "Redroot", "Redroot"])
    offspring = Array[String](["Lilylurk", "Mudskipper", "Moonshade", "Pickystick", "Eezeegrow"])
 **[F2.c]**: We were already aware of the possibility to change platforms, so we changed from C# to gdscript. This made the transition very easy. In C#, we tried to use the engine’s api as much as possible, which allowed for almost all code to be easily translated one to one into gdscript. No designs needed to be changed. The few things that needed extra thought were idiomatic C# things, like nullable object types, gdscripts lack of real namespaces and structs and the weird way it treats nested classes. The main thing that needed a bit more attention was the contiguous byte array, but gdscript has its PackByteArray and bindings for dealing with bytes, which we used, we just no longer had the ease a C# span brought for it. Once everything was rewritten in gdscript, aside from a few errors in translation, it worked almost perfectly out of the box.

## Reflection
Our plan from the beginning was the if we were going to change languages we would change C# to GDScript. This is because we believe that C# is a harder language to use as so rather than having us change from an easier language to a harder language, we would change from a harder language to an easier one. This also meant we could frontload learning a language, instead of having to port the project while also learning a new, more complex, language. This also means we are using gdscript for our game for F3, which has true support for web and mobile builds.


# Devlog Entry - [12/06/24]


## How we satisfied the software requirements

### F0 + F1 + F2 Requirements:
 * **[F0.a]**: Movement is now controlled by either tapping buttons on the screen when on a mobile device or by using the w, a, s & d keys when a keyboard is available.
 * **[F0.b]**: This System has not changed in the latest version of our game
 * **[F0.c]**: This System has not changed in the latest version of our game
 * **[F0.d]**: This System has not changed in the latest version of our game
 * **[F0.e]**: This System has not changed in the latest version of our game
 * **[F0.f]**: This System has not changed in the latest version of our game    
 * **[F0.g]**: This System has not changed in the latest version of our game
 * **[F1.a]**: This System has not changed in the latest version of our game
 * **[F1.b]**: This System has not changed in the latest version of our game
 * **[F1.c]**: This System has not changed in the latest version of our game
 * **[F1.d]**: This System has not changed in the latest version of our game
 * **[F2.a]**: This System has not changed in the latest version of our game
 * **[F2.b]**: This System has not changed in the latest version of our game
 * **[F2.c]**: This System has not changed in the latest version of our game

    
### F3 Requirements:
 **[F3.a]**: One of the perks of using Godot and GDScript is that it comes with built in support for translation. In Godot, when attempting to translate all viewable text to a certain language there is a method called TranslationSever.set_locale(string). What this does is the it takes the string we give it (e.g “ja” for japanese) and searches through provided .po files for a matching file and translates viewable text to the language, so long as the text its attempting to translate is provided a translation in the given .po file. For example, if we know the game displays the word “save”, we must give each .po file a translation for “save” or else the word will be printed as shown in the code.

 **[F3.b]**: For the localization of our game the languages we chose to support were English, Spanish (for a second non-logographic and non-right-to-left language), Japanese (for our logographic language), and Arabic (for our right-to-left language). For each of these languages we searched through our code for all instances that displayed text, then for each language we created its own .po file and gave it to Godot. When creating these .po files we referred to Brace for assistance in translation giving it a prompt of all of our words and how to acquire the needed translations. A user selects a language by going into the menu and selecting their language from a dropdown list. The games language will then change by using TranslationSever.set_locale(string) (“en” = english, “es” = spanish, “ja” = japanese, and “ar” = arabic). A note for the arabic setting is that the game will reflect the position of displayed text, so any HUD elements on the left of the screen will flip to the right side of the screen (as one would have in a right-to-left language).

 **[F3.c]**: Thanks to the godot engine, exporting our project to a mobile platform only required adding touch controls to the screen. We decided to export our project to iOS because it was the most convenient platform for us. The resulting product was a native iOS build of the game that is not dependent on any external resources such as the internet. It is not published on the app store and instead must be sideloaded like any other ipa file.

 **[F3.d]**: Because our mobile build of the game is native to iOS it acts like any other game that would be installed via the app store.

## Reflection:
One way these requirements changed how our games work unintentionally is the language settings. Before the changes, certain displayable text elements of our hud were variable like Day: 0, Day: 1, Day: 2, etc. One of the tricky parts about this is the need to give Godot all of the translations for each text. Because we cant provide a .po file with translations for Day: 0 all the way to Day:1000 (or beyond), we’ve had to changer he way certain hud elements display info. For these variable text displays we changed it to using a sprite such as a calendar to represent the day followed by a number. And because of the way Godot mirrors the position of text elements when using a right-to-left language, we’ve had to move certain hud elements to prevent them from being cut off of the screen.

