using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.API.Client;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace VSTutorial.Patches
{
    /// <summary>
    /// Patch classes should use a patch category attribute. The category string is often your mod ID, however it can be anything you want.
    /// It is recommended to use the normal naming method of {modid}.{category} to avoid compatibility issues.
    /// </summary>
    [HarmonyPatchCategory("vstutorial")]
    internal static class TutorialPatches
    {
        /// <summary>
        /// This is a postfix patch, meaning it will be called *after* executing the OnBlockBrokenWith function in the ItemAxe class.
        ///     Essentially, this will be called whenever a block is broken with an axe.
        /// It has access to all the original arguments that are passed to that function.
        /// </summary>
        //https://github.com/anegostudios/vssurvivalmod/blob/81e52e9a61eabd6aa2131fb255f072feaf9901f1/Item/ItemAxe.cs#L99
        //https://harmony.pardeike.net/articles/patching-postfix.html
        [HarmonyPostfix()]
        [HarmonyPatch(typeof(ItemAxe), "OnBlockBrokenWith")]
        public static void AfterBreakingBlockWithAxe(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel, float dropQuantityMultiplier = 1)
        {
            //A somewhat spicy result from breaking a block, but it's easy to see if it works.
            byEntity.Ignite();
        }

        /// <summary>
        /// This is another postfix patch, meaning it will be called after executing the "GetItemDescText" function in CollectibleObject.
        /// In this instance, we're going to use the __instance and __result parameters.
        ///     __instance allows us to get the data of the CollectibleObject that called the function, and __result allows us to view and edit the result of the function.
        /// </summary>
        //https://github.com/anegostudios/vsapi/blob/1ac01da5bd25b902ea92c8faa709b576432c1d35/Common/Collectible/Collectible.cs#L1756
        //https://harmony.pardeike.net/articles/patching-postfix.html
        [HarmonyPostfix()]
        [HarmonyPatch(typeof(CollectibleObject), "GetItemDescText")]
        public static void AddToItemDescription(CollectibleObject __instance, ref string __result)
        {
            //Since every collectible has an ID, we can choose specific ones to patch.
            if (__instance.Id % 2 == 0)
            {
                //By editing the result of GetItemDescText, the description will be added to every other object in the handbook and when hovered over.
                __result += "This is a patched description!\n";
            }
        }

        /// <summary>
        /// This is a prefix patch, meaning it will be called immediately before executing the OnEntityReceiveDamage function in EntityBehaviorHealth.
        /// When using a prefix, you can return a boolean value to control whether the original function will be run.
        /// </summary>
        //https://github.com/anegostudios/vssurvivalmod/blob/81e52e9a61eabd6aa2131fb255f072feaf9901f1/Block/BlockBed.cs#L56
        //https://harmony.pardeike.net/articles/patching-prefix.html
        [HarmonyPrefix()]
        [HarmonyPatch(typeof(EntityBehaviorHealth), "OnEntityReceiveDamage")]
        public static bool IgnoreDamageChance(EntityBehaviorHealth __instance, DamageSource damageSource, ref float damage)
        {
            //A few checks... first, check the entity taking damage is a player. Then check we're on the server. Then ensure the damage dealt is > 0.
            if (__instance.entity is EntityPlayer player && __instance.entity.Api is ICoreServerAPI sapi && damage > 0)
            {
                //A rather simple random check. Hooking this onto the world random saves us from creating a random object for it to immediately get disposed.
                if (__instance.entity.World.Rand.NextSingle() < 0.5f) 
                {
                    //If the random check passes, then tell the player they are lucky...
                    sapi.SendMessage(player.Player, GlobalConstants.GeneralChatGroup, "Lucky!", EnumChatType.Notification);
                    //...and then stop the function from executing to stop processing the normal damage.
                    return false;
                }
            }
            //If any of the checks do not happen, we wil return true to run the original function.
            return true;
        }

    }
}
