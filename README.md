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
