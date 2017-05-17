using UnityEngine;
using System.Collections;

public class Settings {

    // csv path
    static private string csvPath;

    // vr
    static private bool enableVr;

    // colors
    static private bool relativeColoring;
    static private float hue;

    // sizes
    static private float maxSize;
    static private float minSize;
    static private float increment;
    static private bool relativeSizing;

    // lines
    static private float connectionLineWidth;
    static private bool drawLines;

    // forces
    static private float repulsion;
    static private float attraction;

    // speed
    static private float visualizationSpeed = 5f; // determines the "speed" nodes are reaching their positions. Large value (10 is quite large) might break the physics calculations in bigger graphs.

    // Settings that aren't in the startmenu settings
    public const string GPU_KERNEL = "CSRepulsion";
    public const string GPU_REPULSION = "repulsion";
    public const string GPU_BUFFER = "bufContacts";
    public const string GPU_DELTATIME = "deltaTime";

    static public string CsvPath
    {
        get { return csvPath; }
        set { csvPath = value; }
    }
    static public bool EnableVr
    {
        get { return enableVr; }
        set { enableVr = value; }
    }
    static public bool RelativeColoring
    {
        get { return relativeColoring; }
        set { relativeColoring = value; }
    }
    static public float Hue
    {
        get { return hue; }
        set { hue = value; }
    }
    static public float MaxSize
    {
        get { return maxSize; }
        set { maxSize = value; }
    }
    static public float MinSize
    {
        get { return minSize; }
        set { minSize = value; }
    }
    static public float Increment
    {
        get { return increment; }
        set { increment = value; }
    }
    static public bool RelativeSizing
    {
        get { return relativeSizing; }
        set { relativeSizing = value; }
    }
    static public float ConnectionLineWidth
    {
        get { return connectionLineWidth; }
        set { connectionLineWidth = value; }
    }
    static public bool DrawLines
    {
        get { return drawLines; }
        set { drawLines = value; }
    }
    static public float Repulsion
    {
        get { return repulsion; }
        set { repulsion = value; }
    }
    static public float Attraction
    {
        get { return attraction; }
        set { attraction = value; }
    }
    static public float VisualizationSpeed
    {
        get { return visualizationSpeed; }
        set { visualizationSpeed = value; }
    }
}
