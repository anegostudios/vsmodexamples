using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;

namespace Vintagestory.ModSamples
{
    /// <summary>
    /// Makes you all happy and giddy when you hold a chick in your hands
    /// A sample on how to load your own custom shader and how to render with it with a quad model during the ortho/2d gui render pass
    /// </summary>
    public class ScreenOverlayShaderExample : ModBase
    {
        ICoreClientAPI capi;
        IShaderProgram overlayShaderProg;
        ExampleOverlayRenderer renderer;

        public override ModInfo GetModInfo()
        {
            return null;
        }

        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Client;
        }
        
        public override void StartClientSide(ICoreClientAPI api)
        {
            this.capi = api;

            api.Event.RegisterReloadShaders(LoadShader);
            LoadShader();

            renderer = new ExampleOverlayRenderer(api, overlayShaderProg);
            api.Event.RegisterRenderer(renderer, EnumRenderStage.Ortho);
        }


        public void LoadShader()
        {
            overlayShaderProg = capi.Shader.NewShaderProgram();
            overlayShaderProg.VertexShader = capi.Shader.NewShader(EnumShaderType.VertexShader);
            overlayShaderProg.FragmentShader = capi.Shader.NewShader(EnumShaderType.FragmentShader);

            overlayShaderProg.VertexShader.Code = GetVertexShaderCode();
            overlayShaderProg.FragmentShader.Code = GetFragmentShaderCode();

            capi.Shader.RegisterMemoryShaderProgram("exampleoverlay", overlayShaderProg);
            overlayShaderProg.PrepareUniformLocations("time");
            overlayShaderProg.Compile();

            if (renderer != null)
            {
                renderer.overlayShaderProg = overlayShaderProg;
            }
        }


        #region Shader Code

        public string GetVertexShaderCode()
        {
            return @"
#version 330 core
#extension GL_ARB_explicit_attrib_location: enable

layout(location = 0) in vec3 vertex;

out vec2 uv;

void main(void)
{
    gl_Position = vec4(vertex.xy, 0, 1);
    uv = (vertex.xy + 1.0) / 2.0;
}
";
        }

        public string GetFragmentShaderCode()
        {
            return @"
#version 330 core

in vec2 uv;

out vec4 outColor;

uniform float time;

// from https://gist.github.com/patriciogonzalezvivo/670c22f3966e662d2f83
float mod289(float x){return x - floor(x * (1.0 / 289.0)) * 289.0;}
vec4 mod289(vec4 x){return x - floor(x * (1.0 / 289.0)) * 289.0;}
vec4 perm(vec4 x){return mod289(((x * 34.0) + 1.0) * x);}

float noise(vec3 p){
    vec3 a = floor(p);
    vec3 d = p - a;
    d = d * d * (3.0 - 2.0 * d);

    vec4 b = a.xxyy + vec4(0.0, 1.0, 0.0, 1.0);
    vec4 k1 = perm(b.xyxy);
    vec4 k2 = perm(k1.xyxy + b.zzww);

    vec4 c = k2 + a.zzzz;
    vec4 k3 = perm(c);
    vec4 k4 = perm(c + 1.0);

    vec4 o1 = fract(k3 * (1.0 / 41.0));
    vec4 o2 = fract(k4 * (1.0 / 41.0));

    vec4 o3 = o2 * d.z + o1 * (1.0 - d.z);
    vec2 o4 = o3.yw * d.x + o3.xz * (1.0 - d.x);

    return o4.y * d.y + o4.x * (1.0 - d.y);
}

void main () {
    float r = noise(vec3(uv.x + time, uv.y + time, 0));
    float g = noise(vec3(uv.x + time, uv.y + time, 1));
    float b = noise(vec3(uv.x + time, uv.y + time, 2));
    float a = noise(vec3(uv.x + time, uv.y + time, 3));

    outColor = vec4(r, g, b, a);
}
";
        }

        #endregion

    }


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
            quadMesh.rgba = null;

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