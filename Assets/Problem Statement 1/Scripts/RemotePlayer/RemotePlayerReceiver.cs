using UnityEngine;

public class RemotePlayerReceiver : MonoBehaviour
{
    [Header("World Bounds (same as sender)")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;
    public float fixedY = 0f;

    [Header("Smoothing")]
    public float lerpSpeed = 10f; // higher = snappier

    private Vector3 _targetPosition;

    void Start()
    {
        _targetPosition = transform.localPosition;
    }

    public void OnReceivePackedPosition(uint packed)
    {
        Vector3 decodedPos = PositionCompressor.UnpackXZ(
            packed, minX, maxX, minZ, maxZ, fixedY
        );

        _targetPosition = decodedPos;

        // Debug log: received position
        Debug.Log(
            $"[Receiver] Packed: {packed} | " +
            $"Decoded Pos: {_targetPosition}"
        );
    }

    void FixedUpdate()
    {
        // Smoothly move towards target position
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            _targetPosition,
            lerpSpeed * Time.deltaTime
        );
    }
}