using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FeedbackFX : MonoBehaviour
{
    [SerializeField] private float bounceDepth = 0.08f;
    [SerializeField] private float bounceTime = 0.12f;
    [SerializeField] private float ringTime = 0.35f;
    [SerializeField] private float ringScale = 1.3f;
    [SerializeField] private float ringWidth = 0.06f;
    [SerializeField] private int maxPitchSteps = 12;
    [SerializeField] private float hitStopTime = 0.04f;

    private const int SampleRate = 44100;

    private AudioSource audioSource;
    private Material ringMaterial;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.clip = CreateBeep(880f, 0.15f);

        ringMaterial = new Material(Shader.Find("Sprites/Default"));
    }

    public void Play(PlaceResult result, int combo, Block block)
    {
        if (result == PlaceResult.Miss)
            return;

        if (result == PlaceResult.Perfect)
        {
            StartCoroutine(Ring(block));
            StartCoroutine(HitStop());
            audioSource.pitch = Mathf.Pow(2f, Mathf.Min(combo, maxPitchSteps) / 12f);
        }
        else
        {
            audioSource.pitch = 1f;
        }

        audioSource.Play();
        block.Bounce(bounceDepth, bounceTime);
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    private IEnumerator HitStop()
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(hitStopTime);
        Time.timeScale = 1f;
    }

    private IEnumerator Ring(Block block)
    {
        Vector3 scale3 = block.transform.localScale;
        var go = new GameObject("PerfectRing");
        go.transform.position = block.transform.position + Vector3.up * (scale3.y * 0.5f + 0.01f);

        var line = go.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = true;
        line.widthMultiplier = ringWidth;
        line.material = ringMaterial;
        line.positionCount = 4;

        float hx = scale3.x * 0.5f;
        float hz = scale3.z * 0.5f;
        line.SetPositions(new[]
        {
            new Vector3(-hx, 0f, -hz),
            new Vector3(-hx, 0f, hz),
            new Vector3(hx, 0f, hz),
            new Vector3(hx, 0f, -hz),
        });

        for (float t = 0f; t < ringTime; t += Time.deltaTime)
        {
            float k = t / ringTime;
            float scale = Mathf.Lerp(1f, ringScale, k);
            go.transform.localScale = new Vector3(scale, 1f, scale);

            var color = new Color(1f, 1f, 1f, 1f - k);
            line.startColor = color;
            line.endColor = color;
            yield return null;
        }

        Destroy(go);
    }

    // 사운드 에셋 없이 짧은 사인파 비프를 코드로 만든다.
    private static AudioClip CreateBeep(float frequency, float length)
    {
        int count = Mathf.CeilToInt(SampleRate * length);
        var samples = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = 1f - (float)i / count;
            samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.4f;
        }

        var clip = AudioClip.Create("Beep", count, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
