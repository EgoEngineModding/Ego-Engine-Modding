using System.Buffers.Binary;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace EgoEngineLibrary.Graphics.Pssg;

public static class PssgDataHelper
{
    private static ReadOnlySpan<T> GetArray<T>(this ReadOnlySpan<byte> data, int elementCount = -1)
        where T : unmanaged
    {
        var dataSpan = MemoryMarshal.Cast<byte, T>(data);
        if (elementCount != -1 && dataSpan.Length < elementCount)
        {
            throw new InvalidDataException(
                $"Expected data to have '{elementCount}' elements, but got '{dataSpan.Length}'.");
        }

        return dataSpan;
    }

    private static void SetArray<T>(this Span<byte> data, ReadOnlySpan<T> input)
        where T : unmanaged
    {
        var dataSpan = MemoryMarshal.Cast<byte, T>(data);
        input.CopyTo(dataSpan);
    }

    public static ReadOnlySpan<float> GetFloatArray(this ReadOnlySpan<byte> data, int elementCount = -1)
    {
        if (BitConverter.IsLittleEndian)
        {
            // Values are in big-endian
            // https://github.com/dotnet/runtime/issues/2365#issuecomment-564382166
            var dataSpan = data.GetArray<int>(elementCount);
            var target = new float[dataSpan.Length];
            BinaryPrimitives.ReverseEndianness(dataSpan, MemoryMarshal.Cast<float, int>(target.AsSpan()));
            return target;
        }

        return data.GetArray<float>(elementCount).ToArray();
    }

    public static void SetFloatArray(this Span<byte> data, ReadOnlySpan<float> values)
    {
        if (BitConverter.IsLittleEndian)
        {
            // Values are in big-endian
            // https://github.com/dotnet/runtime/issues/2365#issuecomment-564382166
            var source = MemoryMarshal.Cast<float, int>(values);
            var target = MemoryMarshal.Cast<byte, int>(data);
            BinaryPrimitives.ReverseEndianness(source, target);
            return;
        }

        data.SetArray(values);
    }

    public static Vector3 GetVector3(this ReadOnlySpan<byte> data)
    {
        var floatArray = data.GetFloatArray(3);
        return new Vector3(floatArray);
    }

    public static void SetVector3(this Span<byte> data, Vector3 value)
    {
        var floatArray = MemoryMarshal.CreateReadOnlySpan(ref value.X, 3);
        data.SetFloatArray(floatArray);
    }

    public static Matrix4x4 GetMatrix4(this ReadOnlySpan<byte> data)
    {
        var floatArray = data.GetFloatArray(16);
        return Unsafe.As<float, Matrix4x4>(ref MemoryMarshal.GetReference(floatArray));
    }

    public static void SetMatrix4(this Span<byte> data, Matrix4x4 value)
    {
        var floatArray = MemoryMarshal.CreateReadOnlySpan(ref value.M11, 16);
        data.SetFloatArray(floatArray);
    }
}