using UnityEngine;

public enum PlaceResult
{
    Miss,
    Perfect,
    Cut,
}

public class StackController : MonoBehaviour
{
    [SerializeField] private Block blockPrefab;
    [SerializeField] private Block baseBlock;
    [SerializeField] private ThemeController theme;
    [SerializeField] private float blockHeight = 0.5f;
    [SerializeField] private float spawnOffset = 4f;
    [SerializeField] private float moveRange = 4f;
    [SerializeField] private float speedStart = 3f;
    [SerializeField] private float speedStep = 0.05f;
    [SerializeField] private float speedMax = 7f;
    [SerializeField] private float perfectTolerance = 0.1f;
    [SerializeField] private float cutLifetime = 3f;

    private Block top;
    private Block moving;
    private Color movingColor;
    private int placedCount;
    private float direction = 1f;

    private bool MovesOnX => placedCount % 2 == 0;
    private float Speed => Mathf.Min(speedStart + speedStep * placedCount, speedMax);

    // 시작 지점 대비 탑이 올라간 높이.
    public float TopHeight => top.transform.position.y - baseBlock.transform.position.y;

    // 방금 쌓은(가장 위) 블록. 위치·크기는 이 클래스만 바꾼다.
    public Block Top => top;

    private void Awake()
    {
        top = baseBlock;
        baseBlock.Paint(theme.BlockColor(0));
    }

    private void Update()
    {
        if (moving != null)
            Move();
    }

    public void Begin()
    {
        SpawnMoving();
    }

    public void ResetStack()
    {
        foreach (Transform child in transform)
            Destroy(child.gameObject);

        moving = null;
        top = baseBlock;
        placedCount = 0;
        baseBlock.Paint(theme.BlockColor(0));
    }

    private void Move()
    {
        Vector3 topPos = top.transform.position;
        Vector3 pos = moving.transform.position;
        float axisPos = (MovesOnX ? pos.x - topPos.x : pos.z - topPos.z) + direction * Speed * Time.deltaTime;

        if (Mathf.Abs(axisPos) >= moveRange)
        {
            axisPos = Mathf.Clamp(axisPos, -moveRange, moveRange);
            direction = -direction;
        }

        if (MovesOnX) pos.x = topPos.x + axisPos;
        else pos.z = topPos.z + axisPos;
        moving.transform.position = pos;
    }

    public PlaceResult Place()
    {
        bool onX = MovesOnX;
        Vector3 topPos = top.transform.position;
        Vector3 topScale = top.transform.localScale;
        Vector3 movingPos = moving.transform.position;

        float topCenter = onX ? topPos.x : topPos.z;
        float topSize = onX ? topScale.x : topScale.z;
        float delta = (onX ? movingPos.x : movingPos.z) - topCenter;
        float abs = Mathf.Abs(delta);

        if (abs >= topSize)
        {
            moving.Drop(cutLifetime);
            moving = null;
            return PlaceResult.Miss;
        }

        if (abs <= perfectTolerance)
        {
            SetAxis(moving.transform, onX, topCenter, topSize);
            Advance();
            return PlaceResult.Perfect;
        }

        SetAxis(moving.transform, onX, topCenter + delta * 0.5f, topSize - abs);

        Block piece = Instantiate(blockPrefab, moving.transform.position, Quaternion.identity, transform);
        piece.transform.localScale = moving.transform.localScale;
        piece.Paint(movingColor);
        SetAxis(piece.transform, onX, topCenter + Mathf.Sign(delta) * topSize * 0.5f + delta * 0.5f, abs);
        piece.Drop(cutLifetime);

        Advance();
        return PlaceResult.Cut;
    }

    private void Advance()
    {
        top = moving;
        placedCount++;
        SpawnMoving();
    }

    private void SpawnMoving()
    {
        Vector3 pos = top.transform.position;
        pos.y += blockHeight;

        // 시작 쪽(- 또는 +)을 무작위로 고르고, 중앙을 향해 먼저 움직인다.
        float side = Random.value < 0.5f ? -1f : 1f;
        if (MovesOnX) pos.x += side * spawnOffset;
        else pos.z += side * spawnOffset;

        moving = Instantiate(blockPrefab, pos, Quaternion.identity, transform);
        moving.transform.localScale = top.transform.localScale;
        movingColor = theme.BlockColor(placedCount + 1);
        moving.Paint(movingColor);
        direction = -side;
    }

    private static void SetAxis(Transform block, bool onX, float center, float size)
    {
        Vector3 pos = block.position;
        Vector3 scale = block.localScale;
        if (onX) { pos.x = center; scale.x = size; }
        else { pos.z = center; scale.z = size; }
        block.position = pos;
        block.localScale = scale;
    }
}
