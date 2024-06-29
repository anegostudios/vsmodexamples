using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;

namespace VSTutorial.Items
{
    /*
     * As this is an item, you need to inherit the Item class. This gives access to functions within Item, and CollectibleObject.
     * Take a look at https://apidocs.vintagestory.at/api/Vintagestory.API.Common.Item.html#methods and
    *   https://apidocs.vintagestory.at/api/Vintagestory.API.Common.CollectibleObject.html#methods for all the methods that can be overriden.
     */
    internal class ItemThornsBlade : Item
    {
        /*
         * This function is called whenever this item is used by an entity to attack another entity.
         * You have access to the world, the entity who is attacking, the entity who is being attacked, and the held item's data.
         */
        public override void OnAttackingWith(IWorldAccessor world, Entity byEntity, Entity attackedEntity, ItemSlot itemslot)
        {
            DamageSource damage = new DamageSource()
            {
                Type = EnumDamageType.PiercingAttack,
                CauseEntity = byEntity
            };
            byEntity.ReceiveDamage(damage, 0.25f);
            base.OnAttackingWith(world, byEntity, attackedEntity, itemslot);
        }
    }
}
