using System.Collections.Concurrent;
using System.Reflection;
using Caliban.Core.World;
using OpenTK.Mathematics;

namespace Caliban.Graphics.Utility;

public sealed class Palette
{
    public Vector3[] Colors { get; }

    public Palette(Vector3[] colors)
    {
        Colors = colors;
    }
}

public static class PaletteManager
{
    private static readonly Assembly Assembly = typeof(PaletteManager).Assembly;

    private static readonly ConcurrentDictionary<string, Lazy<Palette?>> Cache = new();

    public static Palette? GetPalette(string id, Biome biome)
    {
        string key = $"{id}/{biome}";

        return Cache.GetOrAdd(
                key,
                _ => new Lazy<Palette?>(
                    () => LoadPalette(id, biome),
                    LazyThreadSafetyMode.ExecutionAndPublication))
            .Value;
    }

    private static Palette? LoadPalette(string id, Biome biome)
    {
        string resourceSuffix =
            $"Assets.Palettes.{id}.{biome}.palette.bmp";

        string? resourceName = Assembly
            .GetManifestResourceNames()
            .FirstOrDefault(x =>
                x.EndsWith(resourceSuffix, StringComparison.OrdinalIgnoreCase));

        if (resourceName == null)
        {
            Console.Error.WriteLine(
                $"[PaletteManager] Palette not found: {id}/{biome}");

            return null;
        }

        using Stream? stream = Assembly.GetManifestResourceStream(resourceName);

        if (stream == null)
        {
            Console.Error.WriteLine(
                $"[PaletteManager] Could not open resource: {resourceName}");

            return null;
        }

        return ReadBmp(stream);
    }

    private static Palette ReadBmp(Stream stream)
    {
        using var reader = new BinaryReader(stream);

        // BMP header
        ushort signature = reader.ReadUInt16();

        if (signature != 0x4D42) // "BM"
            throw new InvalidDataException("Invalid BMP file.");

        reader.ReadUInt32(); // file size
        reader.ReadUInt16(); // reserved
        reader.ReadUInt16(); // reserved

        uint pixelOffset = reader.ReadUInt32();

        // DIB header
        uint dibSize = reader.ReadUInt32();

        if (dibSize < 40)
            throw new InvalidDataException("Unsupported BMP format.");

        int width = reader.ReadInt32();
        int height = reader.ReadInt32();

        ushort planes = reader.ReadUInt16();
        ushort bitsPerPixel = reader.ReadUInt16();

        uint compression = reader.ReadUInt32();

        if (planes != 1)
            throw new InvalidDataException("Unsupported BMP: invalid plane count.");

        if (compression != 0)
            throw new InvalidDataException(
                "Unsupported BMP: compressed BMPs are not supported.");

        if (bitsPerPixel != 24 && bitsPerPixel != 32)
            throw new InvalidDataException(
                $"Unsupported BMP: {bitsPerPixel} bits per pixel.");

        // Skip remaining DIB header fields.
        stream.Position = 14 + dibSize;

        bool bottomUp = height > 0;
        int actualHeight = Math.Abs(height);

        Vector3[] colors = new Vector3[width * actualHeight];

        int bytesPerPixel = bitsPerPixel / 8;

        // BMP rows are padded to 4-byte boundaries.
        int rowSize = ((width * bytesPerPixel + 3) / 4) * 4;

        byte[] row = new byte[rowSize];

        stream.Position = pixelOffset;

        for (int y = 0; y < actualHeight; y++)
        {
            stream.ReadExactly(row);

            int destinationY = bottomUp
                ? actualHeight - 1 - y
                : y;

            for (int x = 0; x < width; x++)
            {
                int offset = x * bytesPerPixel;

                // BMP stores BGR(A), not RGB(A).
                byte b = row[offset];
                byte g = row[offset + 1];
                byte r = row[offset + 2];

                colors[destinationY * width + x] = new Vector3(
                    r / 255.0f,
                    g / 255.0f,
                    b / 255.0f);
            }
        }

        return new Palette(colors);
    }
}