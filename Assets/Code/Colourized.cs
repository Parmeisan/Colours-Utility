using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Colourized : MonoBehaviour
{
    Sprite orig;
    public FlexibleColorPicker colour_picker_cyan;
    public FlexibleColorPicker colour_picker_magenta;
    public FlexibleColorPicker colour_picker_yellow;
    public FlexibleColorPicker colour_picker_grey;

    #region ==== Initialization ===================================================================
    public void Start()
    {
        Image img = GetComponent<Image>();
        orig = GetComponent<Image>().sprite;
        colour_picker_cyan.ToggleFull(false); // After setting the colour, so the HSV gets moved
        colour_picker_magenta.ToggleFull(false);
        colour_picker_yellow.ToggleFull(false);
        colour_picker_grey.ToggleFull(false);
        DoColourChanges();
    }
    #endregion


    #region ==== Colour Options ===================================================================

    protected Color cColour;
    protected Color mColour;
    protected Color yColour;
    protected Color greyColour;
    public void DoColourChanges()
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
            //Library.Log($"Doing changes with cyan {cColour} magenta {mColour} yellow {yColour} grey {greyColour}");
            GetComponent<Image>().sprite = Library.ConvertColours(orig, cColour, mColour, yColour, greyColour);
        }
    }
    #endregion


}
