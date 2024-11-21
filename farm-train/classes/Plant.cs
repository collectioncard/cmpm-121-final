using System;
using System.Linq;
using Godot;
using Array = System.Array;

public partial class Plant : Resource
{
    private int _id;
    private Plant[] _offspring;
    private float _growthRate;

    //private string _growthConditions;
    public int GrowthStages;
    public string[] TexturePaths;

    private float _sunNeeded = .5f;
    private float _moistureNeeded = .5f;
    private int _numPlantsNeeded;

    public static readonly Plant CROSSBREED = new Plant().Init(
        0,
        new[] { "res://assets/plants/idkwhattocallthis.png" },
        0,
        -1
    );

    //Godot needs a paramless constructor
    public Plant()
    {
        _offspring = new Plant[1];
    }

    public Plant Init(int id, string[] texturePaths, float growthRate, int growthStages)
    {
        _offspring[0] = this;
        _id = id;
        TexturePaths = texturePaths;
        _growthRate = growthRate;
        GrowthStages = growthStages;
        return this;
    }

    public int GetTypeName()
    {
        return _id;
    }

    public void AddOffspring(Plant newOffspring)
    {
        if (_offspring.Contains(newOffspring))
            return;
        Array.Resize(ref _offspring, _offspring.Length + 1);
        _offspring[_offspring.Length - 1] = newOffspring;
    }

    public static Plant ChooseOffspring(Plant parentA, Plant parentB)
    {
        // Some code to determine which values the plant offspring will have from parents
        Plant[] validOffspring = parentA._offspring.Intersect(parentB._offspring).ToArray();
        if (validOffspring.Length == 0)
            return parentA;
        int rIndex = Global.Rng.RandiRange(0, validOffspring.Length - 1);
        return validOffspring[rIndex];
    }

    public bool GrowthCheck(float sun, float moisture)
    {
        bool checkSunLevels = false;
        bool checkMoistureLevels = false;

        if (sun >= _sunNeeded)
        {
            checkSunLevels = true;
        }
        if (moisture >= _moistureNeeded)
        {
            checkMoistureLevels = true;
        }

        return checkSunLevels && checkMoistureLevels;
    }
}
