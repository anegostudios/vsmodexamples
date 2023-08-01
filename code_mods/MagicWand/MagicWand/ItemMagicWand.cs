using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace MagicWand
{
    class ItemMagicWand : Item
    {
        public static SimpleParticleProperties particles = new SimpleParticleProperties(
            1, 1,
            ColorUtil.ToRgba(50, 220, 220, 220),
            new Vec3d(),
            new Vec3d(),
            new Vec3f(-0.25f, 0.1f, -0.25f),
            new Vec3f(0.25f, 0.1f, 0.25f),
            1.5f,
            -0.075f,
            0.25f,
            0.25f,
            EnumParticleModel.Quad
        );

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            handling = EnumHandHandling.Handled;
        }

        public override bool OnHeldInteractStep(float secondsUsed, ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel, EntitySelection entitySel)
        {
            if (byEntity.World is IClientWorldAccessor)
            {
                ModelTransform tf = new ModelTransform();
                tf.EnsureDefaultValues();

                tf.Origin.Set(0, -1, 0);
                tf.Rotation.Z = Math.Min(30, secondsUsed * 40);
                byEntity.Controls.UsingHeldItemTransformAfter = tf;

                if (secondsUsed > 0.6)
                {
                    Vec3d pos =
                            byEntity.Pos.XYZ.Add(0, byEntity.LocalEyePos.Y, 0)
                            .Ahead(1f, byEntity.Pos.Pitch, byEntity.Pos.Yaw)
                        ;

                    Vec3f speedVec = new Vec3d(0, 0, 0).Ahead(5, byEntity.Pos.Pitch, byEntity.Pos.Yaw).ToVec3f();
                    particles.MinVelocity = speedVec;
                    Random rand = new Random();
                    particles.Color = ColorUtil.ToRgba(255, rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255));
                    particles.MinPos = pos.AddCopy(-0.05, -0.05, -0.05);
                    particles.AddPos.Set(0.1, 0.1, 0.1);
                    particles.MinSize = 0.1F;
                    particles.SizeEvolve = EvolvingNatFloat.create(EnumTransformFunction.SINUS, 10);
                    byEntity.World.SpawnParticles(particles);
                }
            }
            return true;
        }
    }
}