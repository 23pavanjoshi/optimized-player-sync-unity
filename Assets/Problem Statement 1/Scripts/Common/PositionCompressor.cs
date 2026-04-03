using UnityEngine;

public static class PositionCompressor
{
    // Quantize float in [min, max] to 0..65535
    public static ushort Quantize(float value, float min, float max)
    {
        value = Mathf.Clamp(value, min, max);
        float normalized = (value - min) / (max - min); // 0..1
        return (ushort)Mathf.RoundToInt(normalized * 65535f);
    }

    // Dequantize 0..65535 back to float in [min, max]
    public static float Dequantize(ushort quantized, float min, float max)
    {
        float normalized = quantized / 65535f;
        return min + normalized * (max - min);
    }

    // Pack X and Z into 1 uint (16 bits each)
    public static uint PackXZ(Vector3 pos, float minX, float maxX, float minZ, float maxZ)
    {
        ushort qx = Quantize(pos.x, minX, maxX);
        ushort qz = Quantize(pos.z, minZ, maxZ);

        uint packed = ((uint)qx << 16) | qz;
        return packed;
    }

    // Unpack X and Z from uint
    public static Vector3 UnpackXZ(uint packed, float minX, float maxX, float minZ, float maxZ, float y = 0f)
    {
        ushort qx = (ushort)(packed >> 16);
        ushort qz = (ushort)(packed & 0xFFFF);

        float x = Dequantize(qx, minX, maxX);
        float z = Dequantize(qz, minZ, maxZ);

        return new Vector3(x, y, z);
    }
}