using System;
using System.Collections.Generic;
using Godot;

public partial class SaveMenu : CanvasLayer
{
    // Called when the node enters the scene tree for the first time.
    private Popup menuPopup;
    private OptionButton saveSelector;
    private Button saveButton;
    private Button loadButton;

    private int saveCounter = 1;

    public override void _Ready()
    {
        menuPopup = GetNode<Popup>("MenuPopup");
        saveSelector = GetNode<OptionButton>("MenuPopup/VBoxContainer/SaveSelector");
        saveButton = GetNode<Button>("MenuPopup/VBoxContainer/SaveButton");
        loadButton = GetNode<Button>("MenuPopup/VBoxContainer/LoadButton");

        saveButton.Pressed += OnSaveButtonPressed;
        loadButton.Pressed += OnLoadButtonPressed;

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

    private void OnSaveButtonPressed()
    {
        /*In order to keep the latest option at the top of the list, the list of
        options is saved, the optionButton is cleared, and the list is reentered
        with the latest option first*/

        string[] allItems = new string[saveSelector.ItemCount + 1];

        for (int i = 0; i < saveSelector.ItemCount; i++)
        {
            allItems[i + 1] = (saveSelector.GetItemText(i));
        }

        allItems[0] = $"Option {saveCounter}"; //Latest save
        saveSelector.Clear();

        foreach (string item in allItems)
        {
            saveSelector.AddItem(item);
        }
        saveCounter++;
    }

    private void OnLoadButtonPressed()
    {
        if (saveSelector.Selected >= 0)
        {
            GD.Print(saveSelector.GetItemText(saveSelector.Selected));
            //Fill with code about loading game from selected option.
        }
    }
}
