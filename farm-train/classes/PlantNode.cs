/*
using Godot;
using System;

public partial class PlantNode : Sprite2D
{
	private Plant[] offspring;
	private float growthRate;
	private string growthConditions;
	private int growthStages;
	private string[] texturePaths;

	private int currGrowthStage = 0;
	float sunNeeded = 3;
	private float moistureNeeded = 3;
	private int numPlantsNeeded = 2;
	
	// Constructor to initialize variables
	public Plant(float growthRate, string growthConditions, int growthStages, string[] texturePaths)
	{
		this.growthRate = growthRate;
		this.growthConditions = growthConditions;
		this.growthStages = growthStages;
		this.texturePaths = texturePaths;

		offspring = new Plant[0];
	}	

	public duplicateSelf() {
		childPlant = new Plant(this.growthRate, this.growthConditions, this.growthStages, this.texturePaths);
		return childPlant;		
	}

	public generateOffspring( Plant parentA, Plant parentB){
		// Some code to determine which values the plant offspring will have from parents

		childPlant = new Plant(this.growthRate, this.growthConditions, this.growthStages, this.texturePaths);
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
*/
