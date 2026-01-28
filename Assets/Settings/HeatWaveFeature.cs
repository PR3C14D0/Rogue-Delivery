using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

public class HeatWaveFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class HeatWaveSettings
    {
        public Material material;
        public RenderPassEvent renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;

        [Header("Layer Exclusion")]
        [Tooltip("The effect won't apply to cameras that render these layers")]
        public LayerMask excludeLayers;
    }

    public HeatWaveSettings settings = new HeatWaveSettings();
    private HeatWavePass _heatWavePass;

    public override void Create()
    {
        _heatWavePass = new HeatWavePass(settings);
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.material == null)
        {
            Debug.LogWarningFormat("Missing HeatWave material");
            return;
        }

        if ((renderingData.cameraData.camera.cullingMask & settings.excludeLayers) != 0)
        {
            return;
        }
        
        renderer.EnqueuePass(_heatWavePass);
    }

    class HeatWavePass : ScriptableRenderPass
    {
        private HeatWaveSettings _settings;

        public HeatWavePass(HeatWaveSettings settings)
        {
            _settings = settings;
            renderPassEvent = settings.renderPassEvent;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if(_settings.material == null) return;

            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();
            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            TextureHandle source = resourceData.activeColorTexture;

            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            TextureHandle dest = UniversalRenderer.CreateRenderGraphTexture(
                renderGraph,
                desc,
                "_TempHeatWave",
                false
            );

            /* Apply heat wave effect */
            using (var builder = renderGraph.AddRasterRenderPass<PassData>("HeatWave Effect", out PassData passData))
            {
                passData.material = _settings.material;
                passData.source = source;
                builder.UseTexture(source, AccessFlags.Read);
                builder.SetRenderAttachment(dest, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), data.material, 0);
                });
            }

            /* Copy back to camera */
            using(var builder = renderGraph.AddRasterRenderPass<PassData>("Copy to Camera", out PassData passData))
            {
                passData.material = null;
                passData.source = dest;
                builder.UseTexture(dest, AccessFlags.Read);
                builder.SetRenderAttachment(source, 0, AccessFlags.Write);

                builder.SetRenderFunc((PassData data, RasterGraphContext context) =>
                {
                    Material copyMat = Blitter.GetBlitMaterial(TextureDimension.Tex2D);
                    Blitter.BlitTexture(context.cmd, data.source, new Vector4(1, 1, 0, 0), copyMat, 0);
                });
            }
        }

        private class PassData
        {
            public Material material;
            public TextureHandle source;
        }
    }
}
