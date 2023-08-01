using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace ScreenOverlayShaderExample
{
    public class ExampleOverlayRenderer : IRenderer
    {
        MeshRef quadRef;
        ICoreClientAPI capi;
        public IShaderProgram overlayShaderProg;


        public ExampleOverlayRenderer(ICoreClientAPI capi, IShaderProgram overlayShaderProg)
        {
            this.capi = capi;
            this.overlayShaderProg = overlayShaderProg;

            MeshData quadMesh = QuadMeshUtil.GetCustomQuadModelData(-1, -1, 0, 2, 2);
            quadMesh.Rgba = null;

            quadRef = capi.Render.UploadMesh(quadMesh);
        }

        public double RenderOrder
        {
            get { return 1.1; }
        }

        public int RenderRange { get { return 1; } }

        public void Dispose()
        {
            capi.Render.DeleteMesh(quadRef);
            overlayShaderProg.Dispose();
        }

        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            ItemStack stack = capi.World.Player.InventoryManager.ActiveHotbarSlot.Itemstack;
            if (stack == null || !stack.Collectible.Code.Path.Contains("chick")) return;

            IShaderProgram curShader = capi.Render.CurrentActiveShader;
            curShader.Stop();

            overlayShaderProg.Use();

            capi.Render.GlToggleBlend(true);
            overlayShaderProg.Uniform("time", capi.World.ElapsedMilliseconds / 1000f);

            capi.Render.RenderMesh(quadRef);
            overlayShaderProg.Stop();


            curShader.Use();
        }
    }
}