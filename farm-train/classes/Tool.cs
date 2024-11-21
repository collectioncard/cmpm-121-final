namespace FarmTrain.classes;

using System;
using Godot;

public partial class Tool : Resource
{
    public string ToolName { get; private set; }
    public bool IsDisabled { get; set; }
    public Texture2D _texture { get; private set; }

    //This is dumb
    public Tool() { }

    public Tool(string toolName, bool isDisabled, string texturePath)
    {
        _texture = texturePath == null ? null : GD.Load<Texture2D>(texturePath);
        ToolName = toolName;
        IsDisabled = isDisabled;
    }
}
