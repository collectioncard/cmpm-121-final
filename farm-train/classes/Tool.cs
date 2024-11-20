namespace FarmTrain.classes;

using System;
using Godot;

public partial class Tool : Node
{
    public string ToolName { get; private set; }
    public bool IsDisabled { get; set; }

    //This is dumb
    public Tool() { }

    public Tool(string toolName, bool isDisabled)
    {
        ToolName = toolName;
        IsDisabled = isDisabled;
    }
}
