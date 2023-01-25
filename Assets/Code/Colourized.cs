using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Colourized : MonoBehaviour
{
    protected Sprite orig;
    public Color cColour;
    public Color mColour;
    public Color yColour;
    public Color greyColour;

    public void Start()
    {
        Image img = GetComponent<Image>();
        orig = GetComponent<Image>().sprite;
        DoColourChanges();
    }

    public void DoColourChanges()
    {
        GetComponent<Image>().sprite = Library.ConvertColours(orig, cColour, mColour, yColour, greyColour);
    }

}
