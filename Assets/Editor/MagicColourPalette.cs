using UnityEditor;
using UnityEngine;

/// <summary>
/// A simple editor window that allows you to generate colour palettes based on a base colour and a colour harmony rule.
/// </summary>
/// <remarks>
/// The maths behind the colour generation is based on the CMY colour wheel which is used by other colour palette generators such as Adobe Color (https://color.adobe.com/create/color-wheel).<br/>
/// Please report any bugs on the repo: https://github.com/CalebGriffin/Magic-Colour-Palette <br/>
/// </remarks>
public class MagicColourPalette : EditorWindow
{
    /// <summary>
    /// The different colour harmony rules that can be used to generate a colour palette.
    /// </summary>
    private enum ColourHarmonyRule
    {
        Analogous,
        Monochromatic,
        Triad,
        Complementary,
        SplitComplementary,
        DoubleSplitComplementary,
        Square,
        Compound,
        Shades,
        Custom
    }

    /// <summary>
    /// The current colour harmony rule that is being used to generate the colour palette.
    /// </summary>
    private ColourHarmonyRule colourHarmonyRule = ColourHarmonyRule.Analogous;

    /// <summary>
    /// The previous colour harmony rule, used to check if the rule has changed.
    /// </summary>
    private ColourHarmonyRule oldColourHarmonyRule = ColourHarmonyRule.Analogous;

    /// <summary>
    /// The base colour that is used to generate the colour palette.
    /// </summary>
    private Color baseColour = Color.white;

    /// <summary>
    /// The previous base colour, used to check if the base colour has changed.
    /// </summary>
    private Color oldBaseColour = Color.white;

    /// <summary>
    /// One of the four colours that make up the colour palette.
    /// </summary>
    private Color colour1, colour2, colour3, colour4;

    /// <summary>
    /// A lookup table that converts between CMY and RGB hues.
    /// </summary>
    /// <remarks>
    /// The first value in each array is the CMY hue, and the second value is the RGB hue. <br/>
    /// The values were found here: https://computergraphics.stackexchange.com/questions/1748/function-to-convert-hsv-angle-to-ryb-angle
    /// </remarks>
    private float[][] CMYtoRGB = new float[][]
    {
        new float[] {0f, 0f},
        new float[] {60f, 35f},
        new float[] {122f, 60f},
        new float[] {165f, 120f},
        new float[] {218f, 180f},
        new float[] {275f, 240f},
        new float[] {330f, 300f},
        new float[] {360f, 360f}
    };

    /// <summary>
    /// The scroll position of the window.
    /// </summary>
    private Vector2 scrollPos;

    /// <summary>
    /// Opens the colour palette window when the menu item is clicked.
    /// </summary>
    [MenuItem("Window/Magic Colour Palette")]
    public static void ShowWindow()
    {
        MagicColourPalette window = GetWindow<MagicColourPalette>("Magic Colour Palette");
        window.GenerateRandomStartingColour();
    }

    /// <summary>
    /// Generates a random starting colour for the colour palette.
    /// </summary>
    /// <remarks>
    /// This is called when the window is opened and the generated colour is weighted towards brighter colours.
    /// </remarks>
    private void GenerateRandomStartingColour()
    {
        float H = Random.Range(0f, 360f);
        float S = Random.Range(75f, 100f);
        float V = Random.Range(75f, 100f);

        baseColour = Color.HSVToRGB(H / 360f, S / 100f, V / 100f);
        oldBaseColour = baseColour;

        // The colours need to be recalculated when the base colour is changed, this will be done automatically when the base colour is set in the editor
        RecalculateColours();
    }

    /// <summary>
    /// Draws the colour palette window.
    /// </summary>
    private void OnGUI()
    {
        // Set up the various GUI styles and options
        // This makes the window look a bit nicer and prevents repeating these options for each GUI element
        GUIStyle ruleLabelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Italic, fontSize = 12 };

        GUIStyle colourLabelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 15 };

        GUIStyle baseColourLabelStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 15 };
        
        GUILayoutOption[] labelFieldOptions = new GUILayoutOption[] { GUILayout.MinWidth(110f), GUILayout.MaxWidth(150f), GUILayout.MinHeight(50f) };

        GUILayoutOption[] hexLabelFieldOptions = new GUILayoutOption[] { GUILayout.Width(90f), GUILayout.MinHeight(50f) };

        GUILayoutOption[] ruleLabelFieldOptions = new GUILayoutOption[] { GUILayout.MinWidth(120f), GUILayout.MaxWidth(150f) };

        GUILayoutOption[] colourFieldOptions = new GUILayoutOption[] { GUILayout.MinHeight(50f), GUILayout.ExpandWidth(true) };

        GUILayoutOption[] enumPopupOptions = new GUILayoutOption[] { GUILayout.ExpandWidth(true) };

        // Set up the scroll view
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, false, false, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Colour Harmony Rule", ruleLabelStyle, ruleLabelFieldOptions);
        colourHarmonyRule = (ColourHarmonyRule)EditorGUILayout.EnumPopup(colourHarmonyRule, enumPopupOptions);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Colour 1", colourLabelStyle, labelFieldOptions);
        colour1 = EditorGUILayout.ColorField(colour1, colourFieldOptions);
        EditorGUILayout.SelectableLabel("#" + ColorUtility.ToHtmlStringRGB(colour1), colourLabelStyle, hexLabelFieldOptions);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Colour 2", colourLabelStyle, labelFieldOptions);
        colour2 = EditorGUILayout.ColorField(colour2, colourFieldOptions);
        EditorGUILayout.SelectableLabel("#" + ColorUtility.ToHtmlStringRGB(colour2), colourLabelStyle, hexLabelFieldOptions);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Base Colour", baseColourLabelStyle, labelFieldOptions);
        baseColour = EditorGUILayout.ColorField(baseColour, colourFieldOptions);
        EditorGUILayout.SelectableLabel("#" + ColorUtility.ToHtmlStringRGB(baseColour), baseColourLabelStyle, hexLabelFieldOptions);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Colour 3", colourLabelStyle, labelFieldOptions);
        colour3 = EditorGUILayout.ColorField(colour3, colourFieldOptions);
        EditorGUILayout.SelectableLabel("#" + ColorUtility.ToHtmlStringRGB(colour3), colourLabelStyle, hexLabelFieldOptions);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Colour 4", colourLabelStyle, labelFieldOptions);
        colour4 = EditorGUILayout.ColorField(colour4, colourFieldOptions);
        EditorGUILayout.SelectableLabel("#" + ColorUtility.ToHtmlStringRGB(colour4), colourLabelStyle, hexLabelFieldOptions);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        EditorGUILayout.EndScrollView();

        // Check if the base colour or colour harmony rule has changed
        if (baseColour != oldBaseColour)
        {
            OnBaseColourChanged();
            oldBaseColour = baseColour;
        }

        if (colourHarmonyRule != oldColourHarmonyRule)
        {
            OnColourHarmonyRuleChanged();
            oldColourHarmonyRule = colourHarmonyRule;
        }
    }

    /// <summary>
    /// Called when the base colour is changed.
    /// </summary>
    private void OnBaseColourChanged() => RecalculateColours();

    /// <summary>
    /// Called when the colour harmony rule is changed.
    /// </summary>
    private void OnColourHarmonyRuleChanged() => RecalculateColours();

    /// <summary>
    /// Recalculates the colours in the colour palette based on the base colour and the chosen colour harmony rule.
    /// </summary>
    private void RecalculateColours()
    {
        // Values for converting the base colour and the old base colour between RGB and HSV
        float H, S, V;
        float OldH, OldS, OldV;

        // Convert the base colour and the old base colour to HSV
        Color.RGBToHSV(baseColour, out H, out S, out V);
        Color.RGBToHSV(oldBaseColour, out OldH, out OldS, out OldV);

        // If the saturation or value of the base colour is too low, then revert to the old base colour
        // This prevents the default behaviour of resetting the hue when the saturation or value is 0 which can cause the colour palette to change drastically
        if (S < 0.01f || V < 0.01f)
        {
            H = OldH;
            S = OldS;
            V = OldV;
        }
        
        // Set the base colour to reflect the new HSV values above
        baseColour = Color.HSVToRGB(H, S, V);

        // Multiply the HSV values to make manipulating them simpler
        H *= 360f;
        S *= 100f;
        V *= 100f;

        // Create arrays for the hue, saturation and value of each colour in the palette
        // The default values are the same as the base colour
        float[] HArr = new float[4] { H, H, H, H };
        float[] SArr = new float[4] { S, S, S, S };
        float[] VArr = new float[4] { V, V, V, V };

        // Manipulate the HSV values for each colour based on the chosen colour harmony rule
        switch (colourHarmonyRule)
        {
            case ColourHarmonyRule.Analogous:
                // Convert from RGB to RYB
                H = ConvertRGBHuetoCMYHue(H);
                HArr[0] = ConvertCMYHuetoRGBHue(H + 30f);
                HArr[1] = ConvertCMYHuetoRGBHue(H + 15f);
                HArr[2] = ConvertCMYHuetoRGBHue(H - 15f);
                HArr[3] = ConvertCMYHuetoRGBHue(H - 30f);

                float NewS;
                if (S > 95f)
                    NewS = S - 5f;
                else if (S < 5f)
                    NewS = 10f;
                else
                    NewS = S + 5f;

                SArr[0] = SArr[1] = SArr[2] = SArr[3] = NewS;

                float NewV1 = Mathf.Clamp(V + 5f, 20f, 100f);

                float NewV2;
                if (V > 90f)
                    NewV2 = V - 10f;
                else if (V < 10f)
                    NewV2 = 20f;
                else
                    NewV2 = V + 10f;
                
                VArr[0] = VArr[3] = NewV1;
                VArr[1] = VArr[2] = NewV2;
                break;
            
            case ColourHarmonyRule.Monochromatic:
                HArr[0] = HArr[1] = HArr[2] = HArr[3] = H;

                NewS = S < 40f ? S + 30f : S - 30f;

                SArr[1] = NewS;
                SArr[2] = NewS;

                NewV1 = V > 70f ? V - 50f : V + 30f;
                
                NewV2 = V < 10f ? 20f : Remap(V, 1f, 100f, 20f, 100f);
                
                float NewV3 = V < 41f ? V + 60f : V - 20f;

                VArr[0] = NewV1;
                VArr[1] = NewV2;
                VArr[2] = NewV1;
                VArr[3] = NewV3;
                break;
            
            case ColourHarmonyRule.Triad:
                H = ConvertRGBHuetoCMYHue(H);
                HArr[1] = ConvertCMYHuetoRGBHue(H + 120f);
                HArr[2] = HArr[3] = ConvertCMYHuetoRGBHue(H - 120f);

                float NewS1 = S < 91f ? S + 10f : S - 10f;
                
                float NewS2 = S > 19f ? S - 10f : S + 10f;
                
                float NewS3;
                if (S > 95f)
                    NewS3 = S - 5f;
                else if (S < 5f)
                    NewS3 = 10f;
                else
                    NewS3 = S + 5f;
                
                SArr[0] = NewS1;
                SArr[1] = NewS2;
                SArr[2] = NewS1;
                SArr[3] = NewS3;

                NewV1 = V < 50f ? V + 30f : V - 30f;
                
                VArr[0] = VArr[3] = NewV1;
                break;
            
            case ColourHarmonyRule.Complementary:
                H = ConvertRGBHuetoCMYHue(H);
                HArr[2] = HArr[3] = ConvertCMYHuetoRGBHue(H + 180f);

                NewS1 = S < 81f ? S + 10f : Remap(S, 80f, 100f, 90f, 100f);
                
                NewS2 = Mathf.Clamp(S - 10f, 0f, 90f);

                NewS3 = Mathf.Clamp(S + 20f, 20f, 100f);

                SArr[0] = NewS1;
                SArr[1] = NewS2;
                SArr[2] = NewS3;

                NewV1 = S < 50f ? V + 30f : V - 30f;
                
                NewV2 = Mathf.Clamp(V + 30f, 30f, 100f);

                VArr[0] = VArr[2] = NewV1;
                VArr[1] = NewV2;
                break;

            case ColourHarmonyRule.SplitComplementary:
                H = ConvertRGBHuetoCMYHue(H);
                HArr[0] = HArr[1] = ConvertCMYHuetoRGBHue(H + 150f);
                HArr[2] = HArr[3] = ConvertCMYHuetoRGBHue(H - 150f);

                NewS1 = S > 19f ? S - 10f : S + 10f;

                NewS2 = S > 14f || S < 5f ? S - 5f : S + 5f;

                NewS3 = S < 91f ? S + 10f : S - 10f;

                float NewS4;
                if (S > 95f)
                    NewS4 = S - 5f;
                else if (S < 5f)
                    NewS4 = 10f;
                else
                    NewS4 = S + 5f;
                
                SArr[0] = NewS1;
                SArr[1] = NewS2;
                SArr[2] = NewS3;
                SArr[3] = NewS4;

                NewV1 = V < 50f ? V + 30f : V - 30f;

                VArr[0] = VArr[2] = NewV1;
                break;

            case ColourHarmonyRule.DoubleSplitComplementary:
                H = ConvertRGBHuetoCMYHue(H);
                HArr[0] = ConvertCMYHuetoRGBHue(H + 30f);
                HArr[1] = ConvertCMYHuetoRGBHue(H + 150f);
                HArr[2] = ConvertCMYHuetoRGBHue(H - 150f);
                HArr[3] = ConvertCMYHuetoRGBHue(H - 30f);

                NewS1 = S > 14f || S < 5f ? S - 5f : S + 5f;

                NewS2 = S > 19f ? S - 10f : S + 10f;

                NewS3 = S < 91f ? S + 10f : S - 10f;

                if (S > 95f)
                    NewS4 = S - 5f;
                else if (S < 5f)
                    NewS4 = 10f;
                else
                    NewS4 = S + 5f;
                
                SArr[0] = NewS1;
                SArr[1] = NewS2;
                SArr[2] = NewS3;
                SArr[3] = NewS4;
                break;

            case ColourHarmonyRule.Square:
                H = ConvertRGBHuetoCMYHue(H);
                HArr[1] = ConvertCMYHuetoRGBHue(H + 90f);
                HArr[2] = ConvertCMYHuetoRGBHue(H + 180f);
                HArr[3] = ConvertCMYHuetoRGBHue(H - 90f);

                NewS1 = S < 91f ? S + 10f : S - 10f;

                NewS2 = S > 19f ? S - 10f : S + 10f;
                
                NewS3 = S > 14f || S < 5f ? S - 5f : S + 5f;
                
                SArr[0] = SArr[2] = NewS1;
                SArr[1] = NewS2;
                SArr[3] = NewS3;
                break;
            
            case ColourHarmonyRule.Compound:
                H = ConvertRGBHuetoCMYHue(H);
                HArr[0] = HArr[1] = ConvertCMYHuetoRGBHue(H + 30f);
                HArr[2] = ConvertCMYHuetoRGBHue(H + 165f);
                HArr[3] = ConvertCMYHuetoRGBHue(H + 150f);

                NewS1 = S < 91f ? S + 10f : S - 10f;

                NewS2 = S > 50f ? S - 40f : S + 40f;
                
                NewS3 = S < 36f ? S + 25f : S - 25f;

                SArr[0] = SArr[3] = NewS1;
                SArr[1] = NewS2;

                NewV1 = V < 81f ? V + 20f : V - 20f;

                NewV2 = V > 60f ? V - 40f : V + 40f;
                
                if (V < 16f)
                    NewV3 = 20f;
                else if (V < 65f)
                    NewV3 = V + 5f;
                else
                    NewV3 = Remap(V, 65f, 100f, 69f, 100f);
                
                VArr[0] = VArr[3] = NewV1;
                VArr[1] = NewV2;
                VArr[2] = NewV3;
                break;
            
            case ColourHarmonyRule.Shades:
                NewV1 = V < 45f ? V + 55f : V - 25f;

                NewV2 = V < 71f ? V + 30f : V - 50f;
                
                if (V < 15f)
                    NewV3 = 20f;
                else if (V < 96f)
                    NewV3 = V + 5f;
                else
                    NewV3 = V - 75f;

                float NewV4 = Mathf.Clamp(V - 10f, 20f, 90f);
                
                VArr[0] = NewV1;
                VArr[1] = NewV2;
                VArr[2] = NewV3;
                VArr[3] = NewV4;
                break;

            default:
                break;
        }

        // If the colour harmony rule is set to custom, then the colours are not recalculated
        if (colourHarmonyRule == ColourHarmonyRule.Custom)
            return;

        // Set the colours in the palette based on the HSV values
        colour1 = Color.HSVToRGB(HArr[0] / 360f, SArr[0] / 100f, VArr[0] / 100f);
        colour2 = Color.HSVToRGB(HArr[1] / 360f, SArr[1] / 100f, VArr[1] / 100f);
        colour3 = Color.HSVToRGB(HArr[2] / 360f, SArr[2] / 100f, VArr[2] / 100f);
        colour4 = Color.HSVToRGB(HArr[3] / 360f, SArr[3] / 100f, VArr[3] / 100f);
    }

    /// <summary>
    /// Converts a hue value from the RGB colour wheel to the CMY colour wheel.
    /// </summary>
    /// <param name="H">The RGB hue value to convert.</param>
    /// <returns>The converted CMY hue value as a float.</returns>
    private float ConvertRGBHuetoCMYHue(float H)
    {
        // If the hue is negative, then add 360 to it
        if (H < 0f)
            H += 360f;

        H = H % 360f;

        // Loop through the RYBtoRGB array to find the first value that is greater than H
        for (int i = 0; i < CMYtoRGB.Length; i++)
        {
            // If the value is equal to H, then return the corresponding CMY hue
            if (CMYtoRGB[i][1] == H)
                return CMYtoRGB[i][0];
            else if (CMYtoRGB[i][1] > H)
            {
                // If the value is greater than H, then we need to interpolate between the current and previous values
                float prevH = i - 1 < 0 ? 0f : CMYtoRGB[i - 1][1];
                float prevRYBHue = i - 1 < 0 ? 0f : CMYtoRGB[i - 1][0];
                float nextH = CMYtoRGB[i][1];
                float nextRYBHue = CMYtoRGB[i][0];

                return Remap(H, prevH, nextH, prevRYBHue, nextRYBHue);
            }
        } 

        // If the value is not found, then return 0
        // This should never happen, but it's here just in case
        return 0f;
    }

    /// <summary>
    /// Converts a hue value from the CMY colour wheel to the RGB colour wheel.
    /// </summary>
    /// <param name="H">The CMY hue value to convert.</param>
    /// <returns>The converted RGB hue value as a float.</returns>
    private float ConvertCMYHuetoRGBHue(float H)
    {
        // If the hue is negative, then add 360 to it
        if (H < 0f)
            H += 360f;

        H = H % 360f;

        // Loop through the RYBtoRGB array to find the first value that is greater than H
        for (int i = 0; i < CMYtoRGB.Length; i++)
        {
            // If the value is equal to H, then return the corresponding RGB hue
            if (CMYtoRGB[i][0] == H)
                return CMYtoRGB[i][1];
            else if (CMYtoRGB[i][0] > H)
            {
                // If the value is greater than H, then we need to interpolate between the current and previous values
                float prevH = i - 1 < 0 ? 0f : CMYtoRGB[i - 1][0];
                float prevRYBHue = i - 1 < 0 ? 0f : CMYtoRGB[i - 1][1];
                float nextH = CMYtoRGB[i][0];
                float nextRYBHue = CMYtoRGB[i][1];

                return Remap(H, prevH, nextH, prevRYBHue, nextRYBHue);
            }
        }

        // If the value is not found, then return 0
        // This should never happen, but it's here just in case
        return 0f;
    }

    /// <summary>
    /// Remaps a floating point value from one range to another.
    /// </summary>
    /// <param name="value">The value to remap.</param>
    /// <param name="from1">The start of the original range.</param>
    /// <param name="to1">The end of the original range.</param>
    /// <param name="from2">The start of the new range.</param>
    /// <param name="to2">The end of the new range.</param>
    /// <returns>The remapped value as a float.</returns>
    private float Remap(float value, float from1, float to1, float from2, float to2)
    {
        return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
    }
}