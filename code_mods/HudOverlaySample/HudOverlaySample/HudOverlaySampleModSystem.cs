using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace HudOverlaySample
{
    /// <summary>
    /// Renders a progress bar hud in the top left corner of the screen
    /// </summary>
    public class HudOverlaySample : ModSystem
    {
        ICoreClientAPI capi;
        WeirdProgressBarRenderer renderer;

        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Client;
        }

        public override void StartClientSide(ICoreClientAPI api)
        {
            this.capi = api;
            renderer = new WeirdProgressBarRenderer(api);

            api.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
        }
    }

}
