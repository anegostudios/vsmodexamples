using Vintagestory.API.Common;

namespace Tunnler
{
    public class TunnlerModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterItemClass("tunnler", typeof(TunnlerItem));
        }
    }
}
