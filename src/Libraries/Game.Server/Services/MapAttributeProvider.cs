using System.Buffers.Binary;
using System.Numerics;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using QuantumCore.API;
using QuantumCore.API.Game.Types;
using QuantumCore.Core.Types;
using QuantumCore.Game.World;

namespace QuantumCore.Game.Services;

internal sealed class MapAttributeProvider : IMapAttributeProvider
{
    private readonly IFileProvider _fileProvider;
    private readonly ILogger<MapAttributeProvider> _logger;

    public MapAttributeProvider(IFileProvider fileProvider, ILogger<MapAttributeProvider> logger)
    {
        _fileProvider = fileProvider;
        _logger = logger;
    }

    public async Task<IMapAttributeSet?> GetAttributesAsync(string mapName, Coordinates position, uint mapWidth,
        uint mapHeight, CancellationToken cancellationToken = default)
    {
        var attrPath = $"maps/{mapName}/server_attr";
        var file = _fileProvider.GetFileInfo(attrPath);
        if (!file.Exists)
        {
            _logger.LogDebug("No server_attr file found for map {Map}", mapName);
            return null;
        }

        _logger.LogDebug("Loading cell attributes from file {ServerAttrPath}", attrPath);

        await using var stream = file.CreateReadStream();

        // header contains length and width in sectree units (which are squares of 128x128 attr cells)
        var headerBuffer = new byte[2 * sizeof(int)];
        await stream.ReadExactlyAsync(headerBuffer, cancellationToken);
        var sectreesWidth = BinaryPrimitives.ReadInt32LittleEndian(headerBuffer.AsSpan(0 * sizeof(int), sizeof(int)));
        var sectreesHeight = BinaryPrimitives.ReadInt32LittleEndian(headerBuffer.AsSpan(1 * sizeof(int), sizeof(int)));

        var expectedSectreesWidth = (int)(mapWidth * Map.MapUnit / MapAttributeSet.SectreeSize);
        var expectedSectreesHeight = (int)(mapHeight * Map.MapUnit / MapAttributeSet.SectreeSize);
        if (sectreesWidth != expectedSectreesWidth || sectreesHeight != expectedSectreesHeight)
        {
            _logger.LogWarning(
                "{ServerAttrPath} dimensions ({SectreeWidth}x{SectreeHeight}) do not match atlasinfo.txt dimensions ({ExpectedWidth}x{ExpectedHeight})",
                attrPath, sectreesWidth, sectreesHeight, expectedSectreesWidth, expectedSectreesHeight);
        }

        var sectrees = new MapAttributeSectree?[sectreesHeight, sectreesWidth];
        var lzoDecompressor = new Lzo(MapAttributeSet.CellsPerSectree * sizeof(uint));
        var intBuffer = new byte[4];

        // info for debug logging only
        long cellsWithKnownFlags = 0;
        var unknownFlagCounts = new Dictionary<int, long>();
        var flagCountsDebugInfo = Enum.GetValues<EMapAttributes>()
            .Where(x => x != EMapAttributes.None)
            .ToDictionary(attr => attr, _ => 0L);

        for (var y = 0; y < sectreesHeight; y++)
        {
            for (var x = 0; x < sectreesWidth; x++)
            {
                await stream.ReadExactlyAsync(intBuffer, cancellationToken);
                var blockSize = BinaryPrimitives.ReadInt32LittleEndian(intBuffer);
                
                var compressedSectree = new byte[blockSize];
                await stream.ReadExactlyAsync(compressedSectree, cancellationToken);
                
                var decompressedSectree = lzoDecompressor.DecodeRaw(compressedSectree);
                if (decompressedSectree.Length != MapAttributeSet.CellsPerSectree * sizeof(uint))
                {
                    _logger.LogWarning("{ServerAttrPath} failed to decode sectree ({X}, {Y}): unexpected size {DecompressedSectorLength}", attrPath, x, y, decompressedSectree.Length);
                    sectrees[y, x] = MapAttributeSectree.Empty;
                    continue;
                }

                var allCellsAttrs = new EMapAttributes[MapAttributeSet.CellsPerSectree];
                for (var i = 0; i < allCellsAttrs.Length; i++)
                {
                    var rawCellFlags = BinaryPrimitives.ReadUInt32LittleEndian(
                        decompressedSectree.AsSpan(i * sizeof(uint), sizeof(uint))
                    );
                    // all set bits of `rawCellFlags` are preserved when cast to enum - individual flags can be extracted with HasFlag()
                    var cellAttrFlags = (EMapAttributes)rawCellFlags;
                    allCellsAttrs[i] = cellAttrFlags;

                    // saving debug info if server_attr is corrupted or wrong format
                    if (_logger.IsEnabled(LogLevel.Debug)) {
                        var cellHasValidFlag = false;
                        foreach (var flag in flagCountsDebugInfo.Keys.ToList())
                        {
                            if (cellAttrFlags.HasFlag(flag))
                            {
                                flagCountsDebugInfo[flag]++;
                                cellHasValidFlag = true;
                            }
                        }

                        if (cellHasValidFlag) cellsWithKnownFlags++;

                        var unknownFlags = cellAttrFlags &
                                           ~flagCountsDebugInfo.Keys.Aggregate(EMapAttributes.None,
                                               (acc, attr) => acc | attr);
                        if (unknownFlags != EMapAttributes.None)
                        {
                            var remaining = (uint)unknownFlags;
                            while (remaining != 0)
                            {
                                var lsb = remaining & ~(remaining - 1);
                                var bitPosition = BitOperations.TrailingZeroCount(lsb);
                                unknownFlagCounts.TryGetValue(bitPosition, out var count);
                                unknownFlagCounts[bitPosition] = count + 1;
                                remaining &= remaining - 1;
                            }
                        }
                    }
                }

                sectrees[y, x] = new MapAttributeSectree(allCellsAttrs);
            }
        }

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var totalCells = (long)sectreesHeight * sectreesWidth * MapAttributeSet.CellsPerSectree;
            var percentage = 100.0 * cellsWithKnownFlags / totalCells;
            _logger.LogDebug( "Loaded {ServerAttrPath} {CellSize}x{CellSize2} cell attributes: {Summary} (cells with known flags: {NonZero}/{Total} = {Percentage:F2}%)",
                attrPath, MapAttributeSet.CellSize, MapAttributeSet.CellSize,
                string.Join(" ", flagCountsDebugInfo.Select(kv => $"{kv.Key}={kv.Value}")),
                cellsWithKnownFlags, totalCells, percentage);

            if (unknownFlagCounts.Count > 0)
            {
                _logger.LogWarning("{ServerAttrPath} contains cells with unknown flag bits (LSB being bit 0): {UnknownSummary}",
                    attrPath, string.Join(", ", unknownFlagCounts.Select(kv => $"bit {kv.Key} found in {kv.Value} cells")));
            }
        }

        return new MapAttributeSet(sectreesWidth, sectreesHeight, position, sectrees);
    }

    private sealed class MapAttributeSet : IMapAttributeSet
    {
        internal const int SectreeSize = 6400; // 64m x 64m
        internal const int CellSize = 50; // 50cm x 50cm  - basically a quarter of the size of a full world cell (2m x 2m)
        internal const int CellsPerAxis = SectreeSize / CellSize;
        internal const int CellsPerSectree = CellsPerAxis * CellsPerAxis;

        private readonly MapAttributeSectree?[,] _sectreesAttrs;
        private readonly int _sectreesWidth;
        private readonly int _sectreesHeight;
        private readonly Coordinates _baseCoords;

        public MapAttributeSet(int sectreesWidth, int sectreesHeight, Coordinates basePosition,
            MapAttributeSectree?[,] sectreesAttrs)
        {
            _sectreesWidth = sectreesWidth;
            _sectreesHeight = sectreesHeight;
            _baseCoords = basePosition;
            _sectreesAttrs = sectreesAttrs;
        }

        public EMapAttributes GetAttributesAt(Coordinates coords)
        {
            if (!TryLocate(coords, out var locatedSectreeAttrs, out var cellX, out var cellY) || locatedSectreeAttrs is null)
            {
                return EMapAttributes.None;
            }

            return locatedSectreeAttrs.Get(cellX, cellY);
        }

        private bool TryLocate(Coordinates coords, out MapAttributeSectree? sectreeAttrs, out int cellX, out int cellY)
        {
            sectreeAttrs = null;
            cellX = 0;
            cellY = 0;

            var relativeCoords = coords - _baseCoords;

            // find which sectree covers the x y relative map coords
            var sectreeIndexX = relativeCoords.X / SectreeSize;
            var sectreeIndexY = relativeCoords.Y / SectreeSize;
            if (sectreeIndexX >= _sectreesWidth || sectreeIndexY >= _sectreesHeight)
            {
                return false;
            }
            sectreeAttrs = _sectreesAttrs[sectreeIndexY, sectreeIndexX];
            
            // find which cell of the sectree covers the x y relative map coords
            cellX = (int)((relativeCoords.X % SectreeSize) / CellSize);
            cellY = (int)((relativeCoords.Y % SectreeSize) / CellSize);
            
            return cellX < CellsPerAxis && cellY < CellsPerAxis;
        }
    }

    private sealed class MapAttributeSectree(EMapAttributes[] values)
    {
        public static MapAttributeSectree Empty { get; } = new(new EMapAttributes[MapAttributeSet.CellsPerSectree]);

        public EMapAttributes Get(int x, int y)
        {
            return values[y * MapAttributeSet.CellsPerAxis + x];
        }
    }
}
