using System.Collections;
using UnityEngine;

// 블록 하나의 외형과 연출을 스스로 관리한다. 위치·크기(판정에 쓰이는 값)는 루트 Transform이 갖고,
// 바운스처럼 보이기만 하는 움직임은 자식 visual에만 적용한다.
public class Block : MonoBehaviour
{
    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    [SerializeField] private Transform visual;

    private Renderer visualRenderer;
    private Coroutine bounce;

    private void Awake()
    {
        visualRenderer = visual.GetComponent<Renderer>();
    }

    public void Paint(Color color)
    {
        var props = new MaterialPropertyBlock();
        visualRenderer.GetPropertyBlock(props);
        props.SetColor(BaseColorId, color);
        visualRenderer.SetPropertyBlock(props);
    }

    // depth: 월드 단위로 눌리는 깊이. 눌린 상태에서 시작해 원래 위치로 ease-out 복귀한다.
    public void Bounce(float depth, float time)
    {
        if (bounce != null)
            StopCoroutine(bounce);

        bounce = StartCoroutine(BounceRoutine(depth, time));
    }

    public void Drop(float lifetime)
    {
        gameObject.AddComponent<Rigidbody>();
        Destroy(gameObject, lifetime);
    }

    private IEnumerator BounceRoutine(float depth, float time)
    {
        float localDepth = depth / transform.lossyScale.y;

        for (float t = 0f; t < time; t += Time.deltaTime)
        {
            float remaining = 1f - t / time;
            visual.localPosition = Vector3.down * (localDepth * remaining * remaining);
            yield return null;
        }

        visual.localPosition = Vector3.zero;
        bounce = null;
    }
}
