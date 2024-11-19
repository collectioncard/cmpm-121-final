using Godot;
using System;

public partial class PlantNode : Sprite2D
{
	private Plant[] offspring;
	private float growthRate;
	private string growthConditions;
	private int growthStages;
	private textures[] sprites;

	private int currGrowthStage = 0;
	float sunNeeded = 3
	float moistureNeeded = 3
	int numPlantsNeeded = 2
	
	// Constructor to initialize variables
	public Plant(float growthRate, string growthConditions, int growthStages, textures[] sprites)
	{
		this.growthRate = growthRate;
		this.growthConditions = growthConditions;
		this.growthStages = growthStages;
		this.sprites = sprites;

		offspring = new Plant[0];
	}	

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	public duplicateSelf() {
		childPlant = new Plant(this.growthRate, this.growthConditions, this.growthStages, this.sprites);
		return childPlant;		
	}

	public generateOffspring( Plant parentA, Plant parentB){
		// Some code to determine which values the plant offspring will have from parents

		childPlant = new Plant(this.growthRate, this.growthConditions, this.growthStages, this.sprites);
		return childPlant;
	}

	public checkGrowthConditions(float Sun, float Moisture, array[] nearbyPlants) {		
		bool checkSunLevels = false;
		bool checkMoistureLevels = false;
		bool checkNearbyPlants = false;

		if(Sun >= this.sunNeeded) {
			checkSunLevels = true;
		}
		if(Moisture >= this.moistureNeeded) {
			checkMoistureLevels = true;
		}
		if(nearbyPlants.length >= this.numPlantsNeeded) {
			checkNearbyPlants = true;
		}

		return checkSunLevels && checkMoistureLevels && checkNearbyPlants;
	}

}
