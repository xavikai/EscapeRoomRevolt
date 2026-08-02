using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace EscapeRoomRevolt.Systems.Interaction
{
    /// <summary>Render Graph-native URP outline for the currently focused interactable.</summary>
    public sealed class SelectionMaskOutlineFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            public RenderPassEvent injectionPoint = RenderPassEvent.AfterRenderingTransparents;
            [Range(1, 6)] public int thickness = 2;
            public Color color = new(1f, 0.35f, 0.12f, 1f);
        }

        [SerializeField] private Settings _settings = new();
        private Material _maskMaterial;
        private Material _compositeMaterial;
        private MaskPass _maskPass;
        private CompositePass _compositePass;

        public override void Create()
        {
            var maskShader = Shader.Find("Hidden/EscapeRoom/SelectionMask");
            var compositeShader = Shader.Find("Hidden/EscapeRoom/SelectionOutlineComposite");
            if (maskShader == null || compositeShader == null) return;
            _maskMaterial = CoreUtils.CreateEngineMaterial(maskShader);
            _compositeMaterial = CoreUtils.CreateEngineMaterial(compositeShader);
            _maskPass = new MaskPass(_maskMaterial) { renderPassEvent = _settings.injectionPoint };
            _compositePass = new CompositePass(_compositeMaterial, _settings) { renderPassEvent = _settings.injectionPoint + 1 };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (_maskPass == null || _compositePass == null || renderingData.cameraData.isPreviewCamera) return;
            renderer.EnqueuePass(_maskPass);
            renderer.EnqueuePass(_compositePass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(_maskMaterial);
            CoreUtils.Destroy(_compositeMaterial);
        }

        private sealed class MaskPass : ScriptableRenderPass
        {
            private static readonly int MaskId = Shader.PropertyToID("_ERSelectionMask");
            private readonly Material _material;
            private readonly List<ShaderTagId> _tags = new()
            {
                new("UniversalForwardOnly"), new("UniversalForward"), new("SRPDefaultUnlit"), new("LightweightForward")
            };

            private sealed class PassData { public RendererListHandle rendererList; }

            public MaskPass(Material material) => _material = material;

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var resources = frameData.Get<UniversalResourceData>();
                var cameraData = frameData.Get<UniversalCameraData>();
                var renderingData = frameData.Get<UniversalRenderingData>();
                var lightData = frameData.Get<UniversalLightData>();
                var descriptor = renderGraph.GetTextureDesc(resources.activeColorTexture);
                descriptor.name = "EscapeRoom Selection Mask";
                descriptor.clearBuffer = true;
                descriptor.clearColor = Color.clear;
                descriptor.depthBufferBits = 0;
                descriptor.msaaSamples = MSAASamples.None;
                var mask = renderGraph.CreateTexture(descriptor);

                var drawing = RenderingUtils.CreateDrawingSettings(_tags, renderingData, cameraData, lightData, cameraData.defaultOpaqueSortFlags);
                drawing.overrideMaterial = _material;
                drawing.overrideMaterialPassIndex = 0;
                var filtering = new FilteringSettings(RenderQueueRange.all, -1)
                {
                    renderingLayerMask = SelectionOutlineTarget.RenderingLayerMask
                };
                var rendererList = renderGraph.CreateRendererList(new RendererListParams(renderingData.cullResults, drawing, filtering));

                using var builder = renderGraph.AddRasterRenderPass<PassData>("EscapeRoom Selection Mask", out var passData);
                passData.rendererList = rendererList;
                builder.UseRendererList(rendererList);
                builder.SetRenderAttachment(mask, 0, AccessFlags.Write);
                builder.SetGlobalTextureAfterPass(mask, MaskId);
                builder.SetRenderFunc(static (PassData data, RasterGraphContext context) => context.cmd.DrawRendererList(data.rendererList));
            }
        }

        private sealed class CompositePass : ScriptableRenderPass
        {
            private readonly Material _material;
            private readonly Settings _settings;
            public CompositePass(Material material, Settings settings) { _material = material; _settings = settings; requiresIntermediateTexture = true; }

            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
            {
                var resources = frameData.Get<UniversalResourceData>();
                var source = resources.activeColorTexture;
                var descriptor = renderGraph.GetTextureDesc(source);
                descriptor.name = "EscapeRoom Selection Outline";
                descriptor.clearBuffer = false;
                descriptor.depthBufferBits = 0;
                descriptor.msaaSamples = MSAASamples.None;
                var destination = renderGraph.CreateTexture(descriptor);
                _material.SetColor("_OutlineColor", _settings.color);
                _material.SetFloat("_OutlineThickness", _settings.thickness);
                renderGraph.AddBlitPass(new RenderGraphUtils.BlitMaterialParameters(source, destination, _material, 0), "EscapeRoom Outline Composite");
                renderGraph.AddCopyPass(destination, source, "EscapeRoom Outline Resolve");
            }
        }
    }
}
