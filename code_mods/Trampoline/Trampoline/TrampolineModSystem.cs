using Vintagestory.API.Common;

namespace Trampoline
{
    public class TrampolineModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockClass("trampoline", typeof(TrampolineBlock));
        }
    }
}
