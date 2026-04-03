using UnityEngine;

public class MyPlayerSender : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("World Bounds (for compression)")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;

    [Header("Send Settings")]
    public float sendInterval = 0.05f;  // 20 updates/sec
    public float minDistanceToSend = 0.02f;

    [Header("Remote Reference")]
    public RemotePlayerReceiver remotePlayer;

    private float _sendTimer;
    private Vector3 _lastSentPosition;

    void Start()
    {
        _lastSentPosition = transform.localPosition;
    }

    void FixedUpdate()
    {
        HandleInput();
        SimulateSending();
    }

    private void HandleInput()
    {
        float xPos = 0f;
        float zPos = 0f;

        if (Input.GetKey(KeyCode.W)) zPos += 1f;
        if (Input.GetKey(KeyCode.S)) zPos -= 1f;
        if (Input.GetKey(KeyCode.D)) xPos += 1f;
        if (Input.GetKey(KeyCode.A)) xPos -= 1f;

        Vector3 direction = new Vector3(xPos, 0f, zPos).normalized;

        if (direction.sqrMagnitude > 0.0001f)
        {
            transform.localPosition += direction * moveSpeed * Time.deltaTime;
        }
    }

    private void SimulateSending()
    {
        _sendTimer += Time.deltaTime;

        Vector3 currentPos = transform.localPosition;
        float dist = Vector3.Distance(currentPos, _lastSentPosition);

        if (dist < minDistanceToSend && _sendTimer < sendInterval)
            return;

        _sendTimer = 0f;
        _lastSentPosition = currentPos;

        // Compress
        uint packed = PositionCompressor.PackXZ(currentPos, minX, maxX, minZ, maxZ);

        // "Send" locally
        if (remotePlayer != null)
        {
            remotePlayer.OnReceivePackedPosition(packed);

            // Debug log: sent position, size, packed value
            Debug.Log(
                $"[Sender] Sent Pos: {currentPos} | " +
                $"Packed: {packed} | " +
                $"Data Size: 32 bits (packed) vs 96 bits (3 floats)"
            );
        }
    }
}
