using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Array = System.Array;

public partial class Plant : Resource
{
    public string Name;
    private Plant[] _offspring;
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
        _offspring = new Plant[1];
    }

    public Plant Init(string name, string[] texturePaths, float growthRate, int growthStages)
    {
        _offspring[0] = this;
        TexturePaths = texturePaths;
        _growthRate = growthRate;
        GrowthStages = growthStages;
        return this;
    }

    public void addOffspring(Plant newOffspring)
    {
        if (_offspring.Contains(newOffspring))
            return;
        Array.Resize(ref _offspring, _offspring.Length + 1);
        _offspring[_offspring.Length - 1] = newOffspring;
    }

    private Plant Clone()
    {
        return (Plant)this.MemberwiseClone(); //Don't envision using this, but eventually make this a deep copy.
    }

    public static Plant ChooseOffspring(Plant parentA, Plant parentB)
    {
        // Some code to determine which values the plant offspring will have from parents
        Plant[] validOffspring = parentA._offspring.Intersect(parentB._offspring).ToArray();
        if (validOffspring.Length == 0)
            return parentA;
        int rIndex = GD.RandRange(0, validOffspring.Length - 1); //TODO: Effective seeding
        return validOffspring[rIndex];
    }

    public bool GrowthCheck(float sun, float moisture, Plant[] nearbyPlants)
    {
        bool checkSunLevels = false;
        bool checkMoistureLevels = false;
        bool checkNearbyPlants = false;

        if (sun >= this._sunNeeded)
        {
            checkSunLevels = true;
        }
        if (moisture >= this._moistureNeeded)
        {
            checkMoistureLevels = true;
        }
        if (true || nearbyPlants.Length >= this._numPlantsNeeded)
        {
            checkNearbyPlants = true;
        }

        return checkSunLevels && checkMoistureLevels && checkNearbyPlants;
    }
}
