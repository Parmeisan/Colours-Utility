using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using DilmerGames.Core.Singletons;

public class Library : Singleton<Library>
{

    #region ================== Game ====================================================================================================

    public static void Log(string s, bool method_debug_mod = true)
    {
        // Include debug mode variable so that we can use alternate methods depending on what we're doing.
        // One example is only outputting only once per second in functions that get called on every tick.
        // I'm having it sent as a bool rather than a function so we're only calculating it once per method.
        if (method_debug_mod)
        {
            Debug.Log(s);
            // Doing this repeatedly is very slow, but this is just for debugging anyway
            var log_obj = GameObject.Find("Log");
            if (log_obj)
            {
                var text_output = GameObject.Find("Log").GetComponent<TMPro.TextMeshProUGUI>();
                var all_text = text_output.text;
                all_text = all_text.Substring(all_text.IndexOf("\n") + 1);
                all_text += "\n" + s;
                text_output.text = all_text;
            }
        }
    }
    #endregion

    #region ================== Files / Data ============================================================================================

    public static Dictionary<string, Hashtable> ParseCSV(string file_path)
    {
        Dictionary<string, Hashtable> entity_props = new Dictionary<string, Hashtable>();
        ArrayList prop_names = new ArrayList();
        ArrayList prop_defaults = new ArrayList();
        int line_count = 0;
        TextAsset text_asset = (TextAsset)Resources.Load(file_path);
        foreach (string str_line in text_asset.text.Split('\n'))
        {
            //Log(str_line);
            ArrayList arr_line = SplitCSVRecord(str_line);
            line_count += 1; // Human-readable, so first line is 1
            // Handle the first (header) row by reading the headings into the prop_names
            if (prop_names.Count == 0)
            {
                prop_names = arr_line;
            }
            // Handle the DEFAULT row by reading the values into prop_defaults
            else if (prop_defaults.Count == 0 && (string)arr_line[0] == "DEFAULT")
            {
                prop_defaults = arr_line;
            }
            // Other lines - we should have values for all properties
            // If we have a value, use it. If not, use the default. Then add the whole thing to our List of props
            else if (arr_line.Count >= prop_names.Count)
            {
                Hashtable row_values = new Hashtable();
                for (int i = 0; i < prop_names.Count; i += 1)
                {
                    if (prop_defaults.Count > 0 && arr_line[i] == null)
                    {
                        row_values[prop_names[i]] = prop_defaults[i];
                    }
                    else
                    {
                        row_values[prop_names[i]] = arr_line[i];
                    }
                }
                entity_props[(string)row_values["ID"]] = row_values;
            }
            else// if (arr_line.Count == 1 && ((string)arr_line[0]).Length == 0)
            {
                Log("Line " + line_count + " of " + file_path + " could not be read.");
            }
        }
        if (entity_props.Count == 0) Log("No records found.");
        else Log("First record: " + HashtableToString(FirstInDict(entity_props)));
        return entity_props;
    }
    public static ArrayList SplitCSVRecord(string line)
    {
        // We can't just split on commas because some data has commas in it
        // We can't just split on quote-comma-quote because numeric columns aren't surrounded by quotes
        ArrayList results = new ArrayList();
        bool inquotes = false;
        string cell = null;
        foreach (char c in line)
        {
            bool ignore = false;
            if (c == '"') // Toggle "inquotes" and never, never include in the "cell" string
            {
                ignore = true;
                inquotes = (!inquotes);
            }
            if (c == ',') // If inquotes, this is the same as any other letter.  If outside of quotes, it's a new cell.
            {
                if (!inquotes)
                {
                    ignore = true;
                    results.Add(cell);
                    cell = null;
                }
            }
            if (!ignore) cell += c;
        }
        results.Add(cell); // The last cell won't have a comma
        return results;
    }
    public static string HashtableToString(Hashtable arr)
    {
        string result = "";
        foreach (string k in arr.Keys)
        {
            if (result != "") result += ", ";
            result += k + ":" + arr[k];
        }
        return "Hashtable {" + result + "}";
    }
    public static Dictionary<string, Hashtable> Filter(Dictionary<string, Hashtable> d, string[] prop, string[] value, bool multi, string display_name)
    {
        Dictionary<string, Hashtable> result_list = new Dictionary<string, Hashtable>();
        foreach (Hashtable row in d.Values)
        {
            bool matches = true;
            for (int i = 0; i < prop.Length; i += 1)
            {
                if (row[prop[i]] is null && value[i] is null) { } // Two nulls are equal
                else if ((string)row[prop[i]] != value[i]) { matches = false; }
            }
            if (matches)
            {
                result_list[(string)row["ID"]] = row;
                if (!multi) return result_list;
            }
        }
        if (result_list.Count == 0)
        {
            string n = display_name;
            if (!display_name.EndsWith("s")) n += "s"; // Just grammatical perfectionism
            Log("No " + display_name + " exist with property " + prop + " = " + value);
        }
        return result_list;
    }
    public static Hashtable FirstInDict(Dictionary<string, Hashtable> dict)
    {
        foreach (string key in dict.Keys)
        {
            return dict[key];
        }
        return null;
    }
    #endregion

    #region ================== Math ====================================================================================================
    public static float getVariance(float n, float v)
    {
        return (1 - v + (Random.value * 2) * v);
    }
    #endregion


    #region ================== Strings =================================================================================================
    public static string padNum(int num, string c, int n)
    {
        return pad(num.ToString(), c, n, true);
    }
    public static string pad(string inString, string c, int n, bool before = false)
    {
        string s = inString;
        while (s.Length < n)
        {
            s = (before ? c + s : s + c);
        }
        return s;
    }
    #endregion

    #region ================== Positional ==============================================================================================

    public static Vector3 mouseToGrid(Vector3 p)
    {
        // Convert based on camera position, except for Z axis
        Vector3 new_posn = p;
        new_posn = Camera.main.ScreenToWorldPoint(new_posn);
        new_posn.z = 0;
        // Now snap to grid.  Shift by half a grid square before and after rounding, as we should be in the center of the grid
        new_posn.x = closest(new_posn.x - 0.5f, 1.0f) + 0.5f;
        new_posn.y = closest(new_posn.y - 0.5f, 1.0f) + 0.5f;
        return new_posn;
    }
    public static Vector3 gridToBoard(Vector3 p)
    {
        // Grid has 0 at center and pieces are centered on their spots, so at half-grid placement
        // Also, grid positve/negative goes by normal math axis, but I want (0, 0) in the top-left
        Vector3 new_posn = p;
        new_posn.x = p.x - 3.5f + 7.0f;
        new_posn.y = 3.5f - p.y;
        new_posn.z = -0.1f;
        return new_posn;
    }

    // Round to the closest of some range, e.g. 1.8 with range 0.5 rounds up to 2.0, but 1.7 would round down to 1.5
    public static float closest(float n, float range)
    {
        int div = 0;
        // Calculate with positive numbers, then figure out if it should have been negative or not
        float mod = Mathf.Abs(n);
        int pos = (n > 0 ? 1 : -1);
        while (mod > range)
        {
            div += 1;
            mod -= range;
        }
        // div is the whole number that fits for sure
        // The rounded bit will round to either 0 or 1, thereby fitting, or not, the next range increment
        return pos * range * (div + Mathf.RoundToInt(mod / range));
    }
    #endregion

    #region ================== Sprites and Objects =====================================================================================

    // SOURCE: https://answers.unity.com/questions/458207/copy-a-component-at-runtime.html
    // Everything inheriting from MonoBehaviour has a "gameObject" child and that's what we need for the destination here.
    public static T CopyComponent<T>(T original, GameObject destination) where T : Component
    {
        Library.Log("copy");
        System.Type type = original.GetType();
        Component copy = destination.AddComponent(type);
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy as T;
    }

    public static void DrawRect(Rect rect, Color colour, int duration)
    {
        float z = -0.1f;
        Vector3 topLeft = new Vector3(rect.x, rect.y, z);
        Vector3 topRight = topLeft + new Vector3(rect.width, 0, 0);
        Vector3 btmLeft = topLeft + new Vector3(0, rect.height, 0);
        Vector3 btmRight = topLeft + new Vector3(rect.width, rect.height, 0);
        Debug.DrawLine(topLeft, topRight, colour, duration);
        Debug.DrawLine(topRight, btmRight, colour, duration);
        Debug.DrawLine(btmRight, btmLeft, colour, duration);
        Debug.DrawLine(btmLeft, topLeft, colour, duration);
    }
    public static void DrawBounds(Bounds bounds, Color colour, int duration)
    {
        Rect rect = new Rect();
        rect.min = bounds.min;
        rect.max = bounds.max;
        DrawRect(rect, colour, duration);
    }

    public enum ConvColours { MAGENTA, CYAN, YELLOW, GREY };
    // These versions only iterate through the image once even if updating multiple colours.
    public static Sprite ConvertColours(Sprite sprite, Color? cyan = null, Color? magenta = null, Color? yellow = null, Color? grey = null)
    {
        Texture2D texture = ConvertColours(sprite.texture, cyan, magenta, yellow, grey);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }
    public static Texture2D ConvertColours(Texture2D txt, Color? cyan = null, Color? magenta = null, Color? yellow = null, Color? grey = null)
    {
        Resources.UnloadUnusedAssets();
        Texture2D new_texture = new Texture2D(txt.width, txt.height); // We have to do this so we're not changing the asset (and requiring re-import)

        for (int x = 0; x < txt.width; x += 1)
        {
            for (int y = 0; y < txt.height; y += 1)
            {
                Color px = txt.GetPixel(x, y);
                if (isTransparent(px))
                {
                    px = Color.clear;
                }
                else
                {
                    if (!isBlack(px) && !isWhite(px))
                    {
                        if (magenta != null && isMagenta(px))
                        {
                            px = colourize(grayscale(px), (Color)magenta);
                        }
                        if (cyan != null && isCyan(px))
                        {
                            px = colourize(grayscale(px), (Color)cyan);
                        }
                        if (yellow != null && isYellow(px))
                        {
                            px = colourize(grayscale(px), (Color)yellow);
                        }
                        if (grey != null && isGrey(px))
                        {
                            // We can skip the grayscale function on this one since it's already grey
                            px = colourize(px, (Color)grey);
                        }
                    }
                }
                new_texture.SetPixel(x, y, px);
            }
        }

        new_texture.Apply();
        return new_texture;
    }

    protected const float TOLERANCE = .05f;
    protected static bool is00(float c) { return c < TOLERANCE; }
    protected static bool isFF(float c) { return c >= (1f - TOLERANCE); }
    protected static bool isSame(float c1, float c2) { return Mathf.Abs(c1 - c2) < TOLERANCE; }

    public static bool isBlack(Color px)
    {
        return is00(px.r) && is00(px.g) && is00(px.b);
    }
    public static bool isWhite(Color px)
    {
        return isFF(px.r) && isFF(px.g) && isFF(px.b);
    }
    public static bool isTransparent(Color px)
    {
        return px.a < TOLERANCE;
    }
    public static bool isGrey(Color px)
    {
        return isSame(px.r, px.b) && isSame(px.r, px.g);
    }
    // A colour is precisely magenta when RGB is FF00FF
    // It's a ligher shade of magenta if R and B are both FF, but G varies depending on the lightness (where FF becomes white)
    // It's a darker shade of magenta if G is 00 and R/B are the SAME but depend again on darkness (where 00 becomes black)
    public static bool isMagenta(Color px)
    {
        return (isFF(px.r) && isFF(px.b)) || (is00(px.g) && isSame(px.r, px.b));
    }
    public static bool isYellow(Color px)
    {
        return (isFF(px.r) && isFF(px.g)) || (is00(px.b) && isSame(px.r, px.g));
    }
    public static bool isCyan(Color px)
    {
        return (isFF(px.g) && isFF(px.b)) || (is00(px.r) && isSame(px.b, px.g));
    }


    public static Color grayscale(Color px)
    {
        float avg = (px.r + px.g + px.b) / 3f;
        return new Color(avg, avg, avg);
    }
    // VERY close to the colourize operation in GIMP
    protected static float cPixel(float i, float m)
    {
        return i * m * 1.5f;
    }
    protected static Color colourize(Color i, Color m)
    {
        Color result = new Color(cPixel(i.r, m.r),
                                 cPixel(i.g, m.g),
                                 cPixel(i.b, m.b));
        return result;
    }


    // Modified from SOURCE: https://gamedev.stackexchange.com/questions/132569/how-do-i-find-an-object-by-type-and-name-in-unity-using-c
    // Note that this won't find items you've just created, particularly buttons
    // see https://stackoverflow.com/questions/37797071/unity-onclick-addlistener-not-working
    public static T MatchObject<T>(string name) where T : MonoBehaviour
    {
        MonoBehaviour[] list = GameObject.FindObjectsOfType<T>();
        for (var i = 0; i < list.Length; i += 1)
        {
            if (list[i].name == name)
            {
                return (T)list[i];
            }
        }
        return null;
    }
    public static T MatchBehaviour<T>(string name) where T : Behaviour // Wish I could make a generic T1, T2 but I'm getting errors
    {
        Behaviour[] list = GameObject.FindObjectsOfType<T>();
        for (var i = 0; i < list.Length; i += 1)
        {
            if (list[i].name == name)
            {
                return (T)list[i];
            }
        }
        return null;
    }
    public static List<T> MatchAll<T>(string name) where T : MonoBehaviour
    {
        MonoBehaviour[] list = GameObject.FindObjectsOfType<T>();
        List<T> finalList = new List<T>();
        for (var i = 0; i < list.Length; i += 1)
        {
            if (list[i].name == name)
            {
                finalList.Add((T)list[i]);
            }
        }
        return finalList;
    }
    #endregion



}

