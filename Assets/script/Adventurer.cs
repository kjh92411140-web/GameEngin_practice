using UnityEngine;

public class Adventurer : MonoBehaviour
{
    // Inspector에서 설정할 수 있는 이동 속도
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 3.0f;

    // 벽 감지 거리 (모험가의 크기에 맞게 0.5~0.6 정도로 설정하면 좋습니다.)
    [Header("충돌 감지 설정")]
    [SerializeField] private float raycastDistance = 0.6f;

    // 'Wall' 레이어만 감지하도록 설정하는 레이어 마스크 (Inspector에서 설정)
    [SerializeField] private LayerMask wallLayer;

    // 현재 X축 이동 방향 (1: 오른쪽, -1: 왼쪽)
    private int directionX = 1;

    void Update()
    {
        // 2. 가고 있는 방향에 벽이 있는지 미리 감지하고 방향을 전환한다.
        CheckForWall();

        // 1. X축으로 이동한다.
        Move();
    }

    private void CheckForWall()
    {
        Vector3 rayOrigin = transform.position;
        Vector3 rayDirection = new Vector3(directionX, 0, 0);

        // Raycast를 발사하여 Wall Layer의 오브젝트를 감지합니다.
        if (Physics.Raycast(rayOrigin, rayDirection, raycastDistance, wallLayer))
        {
            // 벽이 감지되면 방향을 반전시킵니다.
            directionX *= -1;

            // 시각적 방향 전환
            // 모험가 모델의 스케일을 반전시켜 방향을 나타낼 수 있습니다.
            // transform.localScale = new Vector3(directionX * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);

            Debug.Log("벽 감지! 방향 전환: " + (directionX == 1 ? "오른쪽" : "왼쪽"));
        }
    }

    private void Move()
    {
        Vector3 movement = new Vector3(directionX, 0, 0) * moveSpeed * Time.deltaTime;
        transform.Translate(movement);
    }
}