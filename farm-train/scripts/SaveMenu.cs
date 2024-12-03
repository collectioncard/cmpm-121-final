using System;
using System.Collections.Generic;
using Godot;

public partial class SaveMenu : CanvasLayer
{
    // Called when the node enters the scene tree for the first time.
    private Popup menuPopup;
    private OptionButton saveSelector;
    private Button NGButton;
    private Button saveButton;
    private Button loadButton;

    private string[] _saveSlots = { "Prev Auto Save", "Save Slot 1", "Save Slot 2" };

    public override void _Ready()
    {
        menuPopup = GetNode<Popup>("MenuPopup");
        NGButton = GetNode<Button>("MenuPopup/VBoxContainer/NewGameButton");
        saveSelector = GetNode<OptionButton>("MenuPopup/VBoxContainer/SaveSelector");
        saveButton = GetNode<Button>("MenuPopup/VBoxContainer/SaveButton");
        loadButton = GetNode<Button>("MenuPopup/VBoxContainer/LoadButton");

        NGButton.Pressed += OnNGButtonPressed;
        saveButton.Pressed += OnSaveButtonPressed;
        loadButton.Pressed += OnLoadButtonPressed;

        foreach (string slot in _saveSlots)
        {
            saveSelector.AddItem(slot);
        }
        saveSelector.Select(0);

        menuPopup.Hide();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
        if (Input.IsActionJustPressed("menu"))
        {
            if (menuPopup.Visible)
            {
                menuPopup.Hide();
            }
            else
            {
                menuPopup.Show();
            }
            //Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Visible ? Input.MouseModeEnum.Confined: Input.MouseModeEnum.Visible;
        }
        Input.MouseMode = menuPopup.Visible
            ? Input.MouseModeEnum.Visible
            : Input.MouseModeEnum.Confined;
        GetTree().Paused = menuPopup.Visible;
    }

    private void OnNGButtonPressed()
    {
        /*In order to keep the latest option at the top of the list, the list of
        options is saved, the optionButton is cleared, and the list is reentered
        with the latest option first*/
        StateManager.NewGame();
    }

    private void OnSaveButtonPressed()
    {
        /*In order to keep the latest option at the top of the list, the list of
        options is saved, the optionButton is cleared, and the list is reentered
        with the latest option first*/
        StateManager.SaveToFile(saveSelector.GetSelected());
    }

    private void OnLoadButtonPressed()
    {
        StateManager.LoadFromFile(saveSelector.GetSelected());
    }
}
