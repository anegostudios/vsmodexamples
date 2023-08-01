using Vintagestory.API.Common;

namespace MagicWand
{
    public class MagicWandModSystem : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterItemClass("ItemMagicWand", typeof(ItemMagicWand));
        }
    }
}
