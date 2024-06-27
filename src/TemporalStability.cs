using livemap.render;
using Vintagestory.API.Common;
using Vintagestory.API.Server;

namespace livemap.temporalstability;

public class TemporalStability : ModSystem {
    private Renderer? _renderer;

    public override void StartServerSide(ICoreServerAPI api) {
        // wait a bit for livemap to fully startup
        api.Event.RegisterCallback(_ => {
            _renderer = new StabilityHeatmap(api);
            LiveMap.Api.RendererRegistry.Register(_renderer);
        }, 500);
    }

    public override void Dispose() {
        if (_renderer != null) {
            LiveMap.Api.RendererRegistry.Unregister(_renderer);
        }
    }
}
