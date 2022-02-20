using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace Ticking
{
    public class Ticking : ModSystem
    {
        public override void Start(ICoreAPI api)
        {
            base.Start(api);
            api.RegisterBlockEntityClass("tickingcounter", typeof(TickingBlockEntity));
        }
    }

    public class TickingBlockEntity : BlockEntity
    {
        private float timer;

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);
            RegisterGameTickListener(OnGameTick, 50);
        }

        public void OnGameTick(float dt)
        {
            timer += dt;
            if(timer >= 3)
            {
                Block block = Api.World.BlockAccessor.GetBlock(Pos);
                if (block.Code.Path.EndsWith("-on"))
                {
                    block = Api.World.GetBlock(block.CodeWithParts("off"));
                }
                else
                {
                    block = Api.World.GetBlock(block.CodeWithParts("on"));
                }

                Api.World.BlockAccessor.SetBlock(block.BlockId, Pos);
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetFloat("timer", timer);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldAccessForResolve)
        {
            base.FromTreeAttributes(tree, worldAccessForResolve);
            timer = tree.GetFloat("timer");
        }
    }
}
