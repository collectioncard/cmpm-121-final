using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FarmTrain.classes;
using Godot;
using Array = System.Array;

public partial class Plant : Resource
{
    private int _id;
    private Plant[] _offspring;
    private float _growthRate;

    private string[][] _growthConditions = new string[][]
    {
        new string[] { "soil", "2" },
        new string[] { "sun", "high" },
        new string[] { "moisture", "high" },
    };

    //example _growthConditions = [["soil", "2"],["sun", "high"],["moisture", "low"],["neighbors", "2,1,1,3"]]
    //neighbors portion translates to: 2 plants of type 1 AND 1 type of plant 2
    private Dictionary<string, float> LevelToValue = new Dictionary<string, float>
    {
        { "high", 80f },
        { "medium", 50f },
        { "low", 20f },
    };
    public int GrowthStages;
    public string[] TexturePaths;
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

    public bool GrowthCheck(float sun, float moisture, int soilType, TileInfo[] neighbors)
    {
        Dictionary<string, bool> conditionTypes = GatherConditionTypes();
        if (_growthConditions == null)
        {
            GD.Print("conditons are null");
            return true;
        }
        GD.Print("condition not null");
        for (int i = 0; i < _growthConditions.Length; i++)
        {
            string itemConditionType = _growthConditions[i][0];
            if (!ConditionsHasKeyAndIsFalse(itemConditionType, conditionTypes))
            {
                continue;
            }
            switch (itemConditionType)
            {
                case "soil":
                    if (int.Parse(_growthConditions[i][1]) == soilType)
                    {
                        conditionTypes[itemConditionType] = true;
                    }
                    break;
                case "sun":
                    GD.Print("sun" + sun);
                    if (sun >= LevelToValue[_growthConditions[i][1]])
                    {
                        conditionTypes[itemConditionType] = true;
                    }
                    break;
                case "moisture":
                    if (moisture >= LevelToValue[_growthConditions[i][1]])
                    {
                        conditionTypes[itemConditionType] = true;
                    }
                    break;
                case "neighbors":
                    string[] neighborConditions = _growthConditions[i][1].Split(",");
                    Dictionary<int, int> plantTypeAmountsRequired = new Dictionary<int, int>();
                    if (!(neighborConditions.Length % 2 == 0))
                    {
                        //If the amount of elements in NeighborConditions is not divisible by 2 and thus each plant doesnt have an amount needed, continue;
                        continue;
                    }
                    foreach (TileInfo neighbor in neighbors)
                    {
                        int neighborPlantType = neighbor.PlantId;
                        if (!plantTypeAmountsRequired.ContainsKey(neighborPlantType))
                        {
                            plantTypeAmountsRequired[neighborPlantType] += 1;
                        }
                    }
                    bool allPlantTypeRequirementsPassed = true;
                    for (int plantTypeIndex = 1; plantTypeIndex < neighborConditions.Length; i += 2)
                    {
                        int plantTypeInQuestion = int.Parse(neighborConditions[plantTypeIndex]);
                        if (plantTypeAmountsRequired.ContainsKey(plantTypeInQuestion))
                        {
                            if (
                                int.Parse(neighborConditions[plantTypeIndex - 1])
                                < plantTypeAmountsRequired[plantTypeInQuestion]
                            )
                            {
                                //If the amount of that plant type currently amongst neighbors is less than the amount required, the plant check is false
                                allPlantTypeRequirementsPassed = false;
                            }
                        }
                    }
                    conditionTypes[itemConditionType] = allPlantTypeRequirementsPassed;
                    break;
            }
        }
        return CheckIfAllConditionsPassed(conditionTypes);
    }

    private Dictionary<string, bool> GatherConditionTypes()
    {
        Dictionary<string, bool> conditionTypes = new Dictionary<string, bool>();
        if (_growthConditions == null)
        {
            return conditionTypes;
        }
        for (int i = 0; i < _growthConditions.Length; i++)
        {
            string itemConditionType = _growthConditions[i][0];
            if (!conditionTypes.ContainsKey(itemConditionType))
            {
                conditionTypes.Add(itemConditionType, false);
            }
        }
        return conditionTypes;
    }

    private bool ConditionsHasKeyAndIsFalse(
        string itemConditionType,
        Dictionary<string, bool> conditionTypes
    )
    {
        if (
            conditionTypes.ContainsKey(itemConditionType)
            && conditionTypes[itemConditionType] == false
        )
        {
            return true;
        }
        return false;
    }

    private bool CheckIfAllConditionsPassed(Dictionary<string, bool> conditionTypes)
    {
        bool conditionsPassed = conditionTypes.Values.All(value => value == true);
        return conditionsPassed;
    }
}
