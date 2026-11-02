using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private float smoothTime = 0.25f;

    private Camera cam;
    private float baseY;
    private float baseSize;
    private float targetY;
    private float targetSize;
    private float velocityY;
    private float velocitySize;

    private void Awake()
    {
        cam = GetComponent<Camera>();
        baseY = transform.position.y;
        baseSize = cam.orthographicSize;
        targetY = baseY;
        targetSize = baseSize;
    }

    // height: 시작 지점 대비 탑이 올라간 높이.
    public void SetTarget(float height)
    {
        targetY = baseY + height;
    }

    // 게임 오버: 탑 전체가 보이도록 중앙으로 이동하며 줌아웃한다.
    public void ShowWholeTower(float height)
    {
        targetY = baseY + height * 0.5f;
        targetSize = Mathf.Max(baseSize, height * 0.41f + 5f);
    }

    public void ResetView()
    {
        targetY = baseY;
        targetSize = baseSize;
        velocityY = 0f;
        velocitySize = 0f;

        Vector3 pos = transform.position;
        pos.y = baseY;
        transform.position = pos;
        cam.orthographicSize = baseSize;
    }

    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        pos.y = Mathf.SmoothDamp(pos.y, targetY, ref velocityY, smoothTime);
        transform.position = pos;

        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetSize, ref velocitySize, smoothTime);
    }
}
