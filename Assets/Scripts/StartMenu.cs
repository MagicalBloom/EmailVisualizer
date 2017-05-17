using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenu : MonoBehaviour {

    // csv path
    public InputField csvField;

    // options
    public Toggle enableVrToggle;
    public Toggle relativeColorToggle;
    public Slider hueSlider;
    public InputField maxSizeField;
    public InputField minSizeField;
    public InputField incrementField;
    public Toggle relativeSizeToggle;
    public InputField lineWidthField;
    public Toggle drawLinesToggle;
    public InputField attractionField;
    public InputField repulsionField;
    public InputField visualizationSpeedField;


    /// <summary>
    /// Reads all the information from start-menu and loads visualization scene.
    /// </summary>
    public void OnVisualizeButton()
    {
        Settings.CsvPath = csvField.text;
        Settings.EnableVr = enableVrToggle.isOn;
        Settings.RelativeColoring = relativeColorToggle.isOn;
        Settings.Hue = hueSlider.value; // hue value between 0...1
        Settings.MaxSize = float.Parse(maxSizeField.text);
        Settings.MinSize = float.Parse(minSizeField.text);
        Settings.Increment = float.Parse(incrementField.text); // only used if relative sizing is false
        Settings.RelativeSizing = relativeSizeToggle.isOn;
        Settings.ConnectionLineWidth = float.Parse(lineWidthField.text);
        Settings.DrawLines = drawLinesToggle.isOn;
        Settings.Attraction = float.Parse(attractionField.text);
        Settings.Repulsion = float.Parse(repulsionField.text);
        Settings.VisualizationSpeed = float.Parse(visualizationSpeedField.text);

        SceneManager.LoadScene("visualizationScene");
    }

    public void OnSliderChange(float sliderValue)
    {
        ColorBlock cb = hueSlider.colors;
        cb.normalColor = Color.HSVToRGB(sliderValue, 1f, 1f);
        cb.highlightedColor = Color.HSVToRGB(sliderValue, 1f, 1f);
        cb.disabledColor = Color.HSVToRGB(sliderValue, 1f, 1f);
        cb.pressedColor = Color.HSVToRGB(sliderValue, 1f, 1f);
        hueSlider.colors = cb;
    }

    // Set cursor to be visible when startmenu is loaded
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}