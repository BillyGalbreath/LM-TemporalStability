using System;
using livemap.data;
using livemap.render;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;
using Vintagestory.Common.Database;
using Vintagestory.GameContent;

namespace livemap.temporalstability;

public class StabilityHeatmap : Renderer {
    private readonly SystemTemporalStability _temporalStabilitySystem;

    public StabilityHeatmap(ICoreServerAPI api) : base("temporalstability") {
        _temporalStabilitySystem = api.ModLoader.GetModSystem<SystemTemporalStability>();
    }

    public override void ScanChunkColumn(ChunkPos chunkPos, BlockData blockData) {
        int startX = chunkPos.X << 5;
        int startZ = chunkPos.Z << 5;
        int endX = startX + 32;
        int endZ = startZ + 32;

        for (int x = startX; x < endX; x++) {
            for (int z = startZ; z < endZ; z++) {
                BlockData.Data data = blockData.Get(x & 511, z & 511)!;
                float stability = _temporalStabilitySystem.GetTemporalStability(x, data.Y, z);
                data.Custom.TryAdd("temporalstability", stability);
            }
        }
    }

    public override void ProcessBlockData(int regionX, int regionZ, BlockData blockData) {
        if (TileImage == null) {
            return;
        }

        for (int x = 0; x < 512; x++) {
            for (int z = 0; z < 512; z++) {
                BlockData.Data? block = blockData.Get(x, z);
                if (block == null) {
                    continue;
                }

                (int id, int y) = ProcessBlock(block);

                Color color = 0;
                if (LiveMap.Api.Colormap.TryGet(id, out uint[]? colors)) {
                    color = colors[GameMath.MurmurHash3Mod(x, y, z, colors.Length)];

                    if (color > 0 && block.Custom.TryGetValue("temporalstability", out object? obj)) {
                        float stability = (1.5F - (float)(obj ?? 0)) * 1.35F;
                        Color lerped = Color.LerpHsb(0x0000FF, 0xFF0000, Math.Clamp(stability, 0F, 1F));
                        color = Color.Blend(lerped, color, 0.25F).Alpha(0xFF);
                    }
                }

                float yDiff = ProcessShadow(x, y, z, blockData);

                TileImage.SetBlockColor(x, z, color, yDiff);
            }
        }
    }
}
