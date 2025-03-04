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
            //To inflict damage, we first need to create a DamageSource object.
            DamageSource damage = new DamageSource()
            {
                //Different types of damage sometimes do different things. This is just a fairly standard attack.
                Type = EnumDamageType.PiercingAttack,
                //SourceEntity is where the attack comes from, which is the attacker who is holding the blade.
                SourceEntity = byEntity,
                //Knockback should be set to 0, to stop the user being knocked back themselves.
                KnockbackStrength = 0
            };

            //Now to actually inflict the damage on the entity holding the blade.
            byEntity.ReceiveDamage(damage, 0.25f);

            //And finally run any other logic in regard to attacking, which in this case reduces durability.
            base.OnAttackingWith(world, byEntity, attackedEntity, itemslot);            
        }
    }
}
