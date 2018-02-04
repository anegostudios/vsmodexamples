using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace VSExampleMods
{
    /// <summary>
    /// Puts a stationary basket on the back of every player
    /// </summary>
    public class BackPackRenderer : ModBase, IRenderer
    {
        ICoreClientAPI api;

        MeshRef backPackMeshRef;
        int backPackTextureId;
        float[] tmpMat = Mat4f.Create();

        ModelTransform backPackTransform = new ModelTransform()
        {
            Translation = new Vec3f(-0.34f, -0.5f, -0.6f),
            Rotation = new Vec3f(0, 0, -87),
            Scale = 0.65f
        };


        public double RenderOrder
        {
            get { return 1; }
        }

        public int RenderRange
        {
            get { return 99; }
        }


        public void Dispose()
        {
            api.Render.DeleteMesh(backPackMeshRef);
        }
        
        public override bool ShouldLoad(EnumAppSide side)
        {
            return side == EnumAppSide.Client;
        }


        public override void StartClientSide(ICoreClientAPI api)
        {
            this.api = api;
            api.Event.RegisterRenderer(this, EnumRenderStage.Opaque);
            api.Event.RegisterRenderer(this, EnumRenderStage.ShadowFar);
            api.Event.RegisterRenderer(this, EnumRenderStage.ShadowNear);
        }


        public void OnRenderFrame(float deltaTime, EnumRenderStage stage)
        {
            Block block = api.World.GetBlock(new AssetLocation("stationarybasket-north"));

            backPackMeshRef = api.Render.UploadMesh(api.Tesselator.GetDefaultBlockMesh(block));
            backPackTextureId = api.BlockTextureAtlas.Positions[0].atlasTextureId;
            backPackTransform.Origin = block.GuiTransform.Origin;

            for (int i = 0; i < api.World.AllPlayers.Length; i++)
            {
                IPlayer plr = api.World.AllPlayers[i];
                EntityShapeRenderer rend = plr.Entity.Renderer as EntityShapeRenderer;
                if (rend == null) continue;

                RenderBackPack(plr.Entity, rend, stage != EnumRenderStage.Opaque);
            }
        }
        

        
        
        private void RenderBackPack(EntityPlayer entity, EntityShapeRenderer rend, bool isShadowPass)
        {
            IRenderAPI rpi = api.Render;
            BlendEntityAnimator bea = rend.animator as BlendEntityAnimator;
            AttachmentPointAndPose apap = null;

            bea.AttachmentPointByCode.TryGetValue("Back", out apap);

            if (apap == null || backPackMeshRef == null) return;

            for (int i = 0; i < 16; i++) tmpMat[i] = rend.ModelMat[i];

            AttachmentPoint ap = apap.AttachPoint;

            float[] mat = apap.Pose.AnimModelMatrix;
            float[] orig = new float[16];
            for (int i = 0; i < 16; i++) orig[i] = (float)api.Render.CameraMatrixOrigin[i];

            if (!isShadowPass) Mat4f.Mul(tmpMat, orig, tmpMat);
            Mat4f.Mul(tmpMat, tmpMat, mat);

            IStandardShaderProgram prog = null;

            if (isShadowPass)
            {
                rpi.CurrentActiveShader.BindTexture2D("tex2d", backPackTextureId, 0);
            }
            else
            {
                prog = rpi.PreparedStandardShader((int)entity.Pos.X, (int)entity.Pos.Y, (int)entity.Pos.Z);
                prog.Tex2D = backPackTextureId;
                prog.AlphaTest = 0.01f;
            }
            

            Mat4f.Translate(tmpMat, tmpMat, backPackTransform.Origin.X, backPackTransform.Origin.Y, backPackTransform.Origin.Z);
            Mat4f.Scale(tmpMat, tmpMat, backPackTransform.Scale, backPackTransform.Scale, backPackTransform.Scale);
            Mat4f.Translate(tmpMat, tmpMat, (float)ap.PosX / 16f + backPackTransform.Translation.X, (float)ap.PosY / 16f + backPackTransform.Translation.Y, (float)ap.PosZ / 16f + backPackTransform.Translation.Z);
            Mat4f.RotateX(tmpMat, tmpMat, (float)(ap.RotationX + backPackTransform.Rotation.X) * GameMath.DEG2RAD);
            Mat4f.RotateY(tmpMat, tmpMat, (float)(ap.RotationY + backPackTransform.Rotation.Y) * GameMath.DEG2RAD);
            Mat4f.RotateZ(tmpMat, tmpMat, (float)(ap.RotationZ + backPackTransform.Rotation.Z) * GameMath.DEG2RAD);
            Mat4f.Translate(tmpMat, tmpMat, -(backPackTransform.Origin.X), -(backPackTransform.Origin.Y), -(backPackTransform.Origin.Z));

            if (isShadowPass)
            {
                Mat4f.Mul(tmpMat, api.Render.CurrentShadowProjectionMatrix, tmpMat);
                api.Render.CurrentActiveShader.UniformMatrix("mvpMatrix", tmpMat);
                api.Render.CurrentActiveShader.Uniform("origin", rend.OriginPos);
            }
            else
            {
                prog.ModelViewMatrix = tmpMat;
            }

            api.Render.RenderMesh(backPackMeshRef);

            if (!isShadowPass) prog.Stop();
        }

    }
}
