using Godot;
using System;
using System.Collections.Generic;

public partial class Plant : Resource
{
	public string Name;
	private string[] _offspring;
	private float _growthRate;
	//private string _growthConditions;
	public int GrowthStages;
	public string[] TexturePaths;

	private float _sunNeeded = .5f;
	private float _moistureNeeded = .5f;
	private int _numPlantsNeeded = 0;
	
	//Godot needs a paramless constructor
	public Plant()
	{
		_offspring = new string[1];
	}

	public Plant Init(string name, string[] texturePaths, string[] offspring, float growthRate, int growthStages)
	{
		_offspring[0] = name;
		TexturePaths = texturePaths;
		_growthRate = growthRate;
		GrowthStages = growthStages;
		return this;
	}

	public Plant Clone() 
	{
		Plant childPlant = new Plant().Init(Name, TexturePaths, _offspring, _growthRate, GrowthStages);
		return childPlant;		
	}

	public Plant ChooseOffspring( Plant parentA, Plant parentB)
	{
		// Some code to determine which values the plant offspring will have from parents
		Plant childPlant = new Plant();
		return childPlant;
	}

	public bool GrowthCheck(float sun, float moisture, Plant[] nearbyPlants) 
	{		
		bool checkSunLevels = false;
		bool checkMoistureLevels = false;
		bool checkNearbyPlants = false;

		if(sun >= this._sunNeeded) {
			checkSunLevels = true;
		}
		if(moisture >= this._moistureNeeded) {
			checkMoistureLevels = true;
		}
		if(true || nearbyPlants.Length >= this._numPlantsNeeded) {
			checkNearbyPlants = true;
		}

		return checkSunLevels && checkMoistureLevels && checkNearbyPlants;
	}

}
