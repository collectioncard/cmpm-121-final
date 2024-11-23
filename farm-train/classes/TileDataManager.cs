using System;
using System.Runtime.InteropServices;
using Godot;
using static FarmTrain.scripts.Utils;

namespace FarmTrain.classes;

/// <summary>
/// A snapshot of the properties of a tile. Any modifications made to this struct *WILL NOT* be reflected in the game state.
/// To submit your changes, call <see cref="TileDataManager.SetPropertyValueAtCoord"/> with the modified data.
/// </summary>
public struct TileInfo
{
    public Vector2I TilePosition;
    private int SoilType { get; set; }
    private int PlantType { get; set; }
    private int MoistureLevel { get; set; }
    private int GrowthStage { get; set; }
}

/// <summary>
/// Manages the tile data for the game, effectively acting as a 2D array of TileInfo structs.
/// Please note that none of the structs returned by this class allow for modifications to the game state
/// </summary>
public class TileDataManager
{
    // Apply the Singleton pattern to the class to prevent multiple game states from existing at the same time
    private static TileDataManager _instance;
    public static TileDataManager Instance => _instance ??= new TileDataManager();

    private static int _tileInfoSize = DataSize * Enum.GetValues(typeof(Properties)).Length; // The size of each tile info block in bytes

    //Define specific offsets for each piece of information in the byte array
    // Add to this enum to add more properties (it should take care of everything else - I hope...)
    public enum Properties
    {
        SoilType = 0 * DataSize,
        PlantType = 1 * DataSize,
        MoistureLevel = 2 * DataSize,
        GrowthStage = 3 * DataSize,
    }

    // Size of the data to store
    private const int DataSize = 4; // 4 bytes -> 32 bit fields

    // Define properties of the byte array

    // Finally create the byte array
    private byte[] _tileDataStorage = new byte[
        Global.TileMapSize.X * Global.TileMapSize.Y * _tileInfoSize
    ];

    private TileDataManager() { }

    ////**** DATA RETRIEVAL ****////

    /// <summary>
    /// Gets the value of a specific property at a given coordinate.
    /// </summary>
    /// <param name="property">The property to retrieve the value for.</param>
    /// <param name="tilePos">The position of the tile.</param>
    /// <returns>The value of the specified property at the given coordinate.</returns>
    public int GetPropertyValueAtCoord(Properties property, Vector2I tilePos)
    {
        return BitConverter.ToInt32(
            _tileDataStorage,
            CoordsToIndex(tilePos) * _tileInfoSize + (int)property
        );
    }

    /// <summary>
    /// Gets all of the tile properties at a given coordinate.
    /// Returns a copy of the data. DO NOT expect modifications to this data to do anything unless
    /// you call SetTileInfoAtCoord with the modified data.
    /// </summary>
    /// <param name="tilePos">The position of the tile.</param>
    /// <returns>A <see cref="TileInfo"/> struct containing the properties of the tile at the given coordinate.</returns>
    public TileInfo GetTileInfoAtCoord(Vector2I tilePos)
    {
        int startIndex = CoordsToIndex(tilePos) * _tileInfoSize;
        var span = new Span<byte>(_tileDataStorage, startIndex, _tileInfoSize);
        return MemoryMarshal.Read<TileInfo>(span);
    }

    ////**** DATA STORAGE ****////

    /// <summary>
    /// Sets the value of a given property at a given coordinate on the map.
    /// </summary>
    /// <param name="property">The property field to set</param>
    /// <param name="tilePos">The Coordinate of the tile on the game map</param>
    /// <param name="value">The 32-bit integer value to set</param>
    public void SetPropertyValueAtCoord(Properties property, Vector2I tilePos, int value)
    {
        byte[] data = BitConverter.GetBytes(value);
        Array.Copy(
            data,
            0,
            _tileDataStorage,
            CoordsToIndex(tilePos) * _tileInfoSize + (int)property,
            DataSize
        );
    }

    /// <summary>
    /// Sets all of the properties of a tile at a given coordinate. Because structs are pass by value, do not expect any
    /// changes to the input struct *after* this call to be reflected in the game state.
    /// </summary>
    /// <param name="tileInfo"></param>
    public void SetTileInfoAtCoord(TileInfo tileInfo)
    {
        int startIndex = CoordsToIndex(tileInfo.TilePosition) * _tileInfoSize;
        var span = new Span<byte>(_tileDataStorage, startIndex, _tileInfoSize);
        MemoryMarshal.Write(span, ref tileInfo);
    }
}
