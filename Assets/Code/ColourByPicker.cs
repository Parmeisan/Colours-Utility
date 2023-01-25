using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ColourByPicker : Colourized
{
    public FlexibleColorPicker colour_picker_cyan;
    public FlexibleColorPicker colour_picker_magenta;
    public FlexibleColorPicker colour_picker_yellow;
    public FlexibleColorPicker colour_picker_grey;

    public new void Start()
    {
        Image img = GetComponent<Image>();
        orig = GetComponent<Image>().sprite;
        colour_picker_cyan.ToggleFull(false);
        colour_picker_magenta.ToggleFull(false);
        colour_picker_yellow.ToggleFull(false);
        colour_picker_grey.ToggleFull(false);
        DoColourChanges();
    }

    public new void DoColourChanges()
    {
        Color c_c = colour_picker_cyan.GetColor();
        Color c_m = colour_picker_magenta.GetColor();
        Color c_y = colour_picker_yellow.GetColor();
        Color c_g = colour_picker_grey.GetColor();
        bool changes = (c_c != cColour || c_m != mColour || c_y != yColour || c_g != greyColour);
        if (changes)
        {
            cColour = c_c;
            mColour = c_m;
            yColour = c_y;
            greyColour = c_g;
            base.DoColourChanges();
        }
    }

}
