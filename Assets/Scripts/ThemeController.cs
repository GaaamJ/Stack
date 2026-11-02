using UnityEngine;

public class ThemeController : MonoBehaviour
{
    [SerializeField] private Light sun;
    [SerializeField] private BackgroundController background;
    [SerializeField] private float hueStart = 0.55f;
    [SerializeField] private float hueStep = 0.02f;
    [SerializeField] private float saturation = 0.35f;
    [SerializeField] private float value = 0.95f;
    [SerializeField] private float smoothTime = 0.6f;

    private float hue;
    private float targetHue;
    private float velocity;

    public Color BlockColor(int layer)
    {
        return Color.HSVToRGB(Mathf.Repeat(hueStart + layer * hueStep, 1f), saturation, value);
    }

    public void Apply(int layer)
    {
        targetHue = hueStart + layer * hueStep;
    }

    public void ResetTheme()
    {
        hue = hueStart;
        targetHue = hueStart;
        velocity = 0f;
        ApplyColors();
    }

    private void Awake()
    {
        ResetTheme();
    }

    private void Update()
    {
        if (Mathf.Approximately(hue, targetHue))
            return;

        hue = Mathf.SmoothDamp(hue, targetHue, ref velocity, smoothTime);
        ApplyColors();
    }

    private void ApplyColors()
    {
        float h = Mathf.Repeat(hue, 1f);

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = Color.HSVToRGB(h, 0.25f, 0.95f);
        RenderSettings.ambientEquatorColor = Color.HSVToRGB(h, 0.15f, 0.85f);
        RenderSettings.ambientGroundColor = Color.HSVToRGB(h, 0.2f, 0.7f);
        sun.color = Color.Lerp(Color.white, Color.HSVToRGB(h, 0.2f, 1f), 0.5f);

        background.SetHueOffset(hue - hueStart);
    }
}
