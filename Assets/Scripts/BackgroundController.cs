using UnityEngine;
using UnityEngine.UI;

// 카메라 뒤쪽에 그라데이션과 반짝이 별을 깐다. 월드 오브젝트가 항상 앞에 그려진다.
public class BackgroundController : MonoBehaviour
{
    [SerializeField] private Camera targetCamera;
    [SerializeField] private Color topColor = new Color(0.69f, 0.70f, 1f);
    [SerializeField] private Color middleColor = new Color(0.96f, 0.84f, 0.93f);
    [SerializeField] private Color bottomColor = new Color(0.98f, 0.91f, 0.88f);
    [SerializeField] private float hueDrift = 0.5f;
    [SerializeField] private int sparkleCount = 45;
    [SerializeField] private Vector2 sparkleSize = new Vector2(24f, 110f);

    private const int GradientHeight = 128;
    private const int StarTextureSize = 64;

    private Texture2D gradient;
    private Color[] gradientPixels;
    private Image[] sparkles;
    private float[] sparkleAlpha;
    private float[] sparkleSpeed;
    private float[] sparklePhase;

    private float hueOffset;
    private bool gradientDirty = true;

    // offset: 시작 색조 대비 현재 색조 차이. 값만 저장하고 다음 Update에서 반영한다.
    public void SetHueOffset(float offset)
    {
        hueOffset = offset;
        gradientDirty = true;
    }

    private void Awake()
    {
        BuildCanvas();
    }

    private void Update()
    {
        if (gradientDirty)
        {
            RebuildGradient();
            gradientDirty = false;
        }

        Twinkle();
    }

    private void RebuildGradient()
    {
        float drift = hueOffset * hueDrift;
        Color top = ShiftHue(topColor, drift);
        Color middle = ShiftHue(middleColor, drift);
        Color bottom = ShiftHue(bottomColor, drift);

        for (int y = 0; y < GradientHeight; y++)
        {
            float t = 1f - (float)y / (GradientHeight - 1); // 0 = 위, 1 = 아래
            Color c = Color.Lerp(top, middle, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.4f, 0.72f, t)));
            c = Color.Lerp(c, bottom, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.72f, 1f, t)));
            gradientPixels[y] = c;
        }
        gradient.SetPixels(gradientPixels);
        gradient.Apply();
    }

    private static Color ShiftHue(Color color, float shift)
    {
        Color.RGBToHSV(color, out float h, out float s, out float v);
        return Color.HSVToRGB(Mathf.Repeat(h + shift, 1f), s, v);
    }

    private void BuildCanvas()
    {
        var canvasGO = new GameObject("BackgroundCanvas", typeof(Canvas), typeof(CanvasScaler));
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = targetCamera;
        canvas.planeDistance = 90f;
        canvas.sortingOrder = -100;

        var scaler = canvasGO.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080f, 1920f);
        scaler.matchWidthOrHeight = 0.5f;

        gradient = new Texture2D(1, GradientHeight, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
        };
        gradientPixels = new Color[GradientHeight];

        var bg = new GameObject("Gradient", typeof(RectTransform), typeof(RawImage));
        bg.transform.SetParent(canvasGO.transform, false);
        var bgRect = (RectTransform)bg.transform;
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        var raw = bg.GetComponent<RawImage>();
        raw.texture = gradient;
        raw.raycastTarget = false;

        BuildSparkles(canvasGO.transform);
    }

    private void BuildSparkles(Transform parent)
    {
        Sprite star = CreateStarSprite();

        sparkles = new Image[sparkleCount];
        sparkleAlpha = new float[sparkleCount];
        sparkleSpeed = new float[sparkleCount];
        sparklePhase = new float[sparkleCount];

        for (int i = 0; i < sparkleCount; i++)
        {
            var go = new GameObject("Sparkle", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);

            var rect = (RectTransform)go.transform;
            var anchor = new Vector2(Random.value, Random.value);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            // 작은 별이 많고 큰 별은 드물게.
            float size = Mathf.Lerp(sparkleSize.x, sparkleSize.y, Mathf.Pow(Random.value, 3f));
            rect.sizeDelta = new Vector2(size, size);

            var image = go.GetComponent<Image>();
            image.sprite = star;
            image.raycastTarget = false;

            sparkles[i] = image;
            sparkleAlpha[i] = Random.Range(0.35f, 1f);
            sparkleSpeed[i] = Random.Range(0.6f, 2.2f);
            sparklePhase[i] = Random.value * Mathf.PI * 2f;
        }
    }

    private void Twinkle()
    {
        for (int i = 0; i < sparkles.Length; i++)
        {
            float k = 0.65f + 0.35f * Mathf.Sin(Time.time * sparkleSpeed[i] + sparklePhase[i]);
            sparkles[i].color = new Color(1f, 1f, 1f, sparkleAlpha[i] * k);
        }
    }

    // 에셋 없이 네 갈래 별을 코드로 만든다.
    private static Sprite CreateStarSprite()
    {
        var texture = new Texture2D(StarTextureSize, StarTextureSize, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Bilinear,
        };

        float half = (StarTextureSize - 1) * 0.5f;
        for (int y = 0; y < StarTextureSize; y++)
        {
            for (int x = 0; x < StarTextureSize; x++)
            {
                float nx = Mathf.Abs((x - half) / half);
                float ny = Mathf.Abs((y - half) / half);
                float alpha = Mathf.Clamp01(1f - (Mathf.Sqrt(nx) + Mathf.Sqrt(ny)));
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, StarTextureSize, StarTextureSize), new Vector2(0.5f, 0.5f), 100f);
    }
}
