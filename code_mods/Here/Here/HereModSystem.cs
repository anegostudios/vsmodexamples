using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace here
{
    public class hereModSystem : ModSystem
    {
        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Server;
        }

        public override void StartServerSide(ICoreServerAPI api)
        {
            base.StartServerSide(api);
            AssetLocation sound = new AssetLocation("here", "sounds/partyhorn");
            api.ChatCommands.Create("here")
            .WithDescription("spawns particles around the player")
            .RequiresPrivilege(Privilege.chat)
            .RequiresPlayer()
            .HandleWith((args) =>
            {
                var byEntity = args.Caller.Entity;
                byEntity.World.PlaySoundAt(sound, byEntity);
                Vec3d pos = byEntity.Pos.XYZ.Add(0, byEntity.LocalEyePos.Y, 0);
                Random rand = new Random();
                for (int i = 0; i < 100; i++)
                {
                    Vec3d realPos = pos.AddCopy(-0.1 + rand.NextDouble() * 0.2, 0, -0.1 + rand.NextDouble() * 0.2);
                    Vec3f velocity = new Vec3f(-0.2F + (float)rand.NextDouble() * 0.4F, 0.4F + (float)rand.NextDouble() * 2F, -0.2F + (float)rand.NextDouble() * 0.4F);
                    byEntity.World.SpawnParticles(1, ColorUtil.ToRgba(255, rand.Next(0, 255), rand.Next(0, 255), rand.Next(0, 255)),
                        realPos, realPos,
                        velocity, velocity, (float)rand.NextDouble() * 1 + 1, 0.01F,
                        1, EnumParticleModel.Cube);
                }
                return TextCommandResult.Success();
            });
        }
    }
}
