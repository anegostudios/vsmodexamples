using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace MyFirstCommand
{
    public class MyFirstCommandModSystem : ModSystem
    {
        // this tells the vs mod loader that this is a client side mod
		public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Client;

		// the vs mod loader uses this to start our mod for the client side
		public override void StartClientSide(ICoreClientAPI api) {
			// this creates a client side command called "hello" with the description "Says Hello!" 
            api.ChatCommands.Create("hello")
            .WithDescription("Says hello!")
            .HandleWith((args)=>{
                return TextCommandResult.Success("Hello!");
            });
		}
    }
}
