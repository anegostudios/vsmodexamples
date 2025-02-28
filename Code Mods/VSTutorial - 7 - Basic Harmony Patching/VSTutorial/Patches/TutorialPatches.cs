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
        //https://github.com/anegostudios/vssurvivalmod/blob/81e52e9a61eabd6aa2131fb255f072feaf9901f1/Item/ItemAxe.cs#L99
        //https://harmony.pardeike.net/articles/patching-postfix.html
        [HarmonyPostfix()]
        [HarmonyPatch(typeof(ItemAxe), "OnBlockBrokenWith")]
        public static void AfterBreakingBlockWithAxe(IWorldAccessor world, Entity byEntity, ItemSlot itemslot, BlockSelection blockSel, float dropQuantityMultiplier = 1)
        {
            byEntity.Ignite();
        }

        //https://github.com/anegostudios/vsapi/blob/1ac01da5bd25b902ea92c8faa709b576432c1d35/Common/Collectible/Collectible.cs#L1756
        [HarmonyPostfix()]
        [HarmonyPatch(typeof(CollectibleObject), "GetItemDescText")]
        public static void AddToItemDescription(CollectibleObject __instance, ref string __result)
        {
            if (__instance.Id % 2 == 0)
            {
                __result += "This is a patched description!\n";
            }
        }


        //https://github.com/anegostudios/vssurvivalmod/blob/81e52e9a61eabd6aa2131fb255f072feaf9901f1/Block/BlockBed.cs#L56
        //https://harmony.pardeike.net/articles/patching-prefix.html
        [HarmonyPrefix()]
        [HarmonyPatch(typeof(EntityBehaviorHealth), "OnEntityReceiveDamage")]
        public static bool IgnoreDamageChance(EntityBehaviorHealth __instance, DamageSource damageSource, ref float damage)
        {
            if (__instance.entity is EntityPlayer player && __instance.entity.Api is ICoreServerAPI sapi && damage > 0)
            {
                if (__instance.entity.World.Rand.NextSingle() < 0.5f) 
                {
                    sapi.SendMessage(player.Player, GlobalConstants.GeneralChatGroup, "Lucky!", EnumChatType.Notification);
                    return false;
                }
            }
            return true;
        }

    }
}
