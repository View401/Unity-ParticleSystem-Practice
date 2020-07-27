using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class bloom : PostEffectBase
{
    //分辨率
    public int downSample = 1;
    //采样率
    public int sampleScale = 1;
    public Color colorThreshold = Color.gray;
    [Range(0.0f, 1.0f)]
    public float bloomFactor = 0.5f;
    
    void OnRenderImage(RenderTexture source,RenderTexture destination) {
        if (_Material)
        {
            RenderTexture temp1 = RenderTexture.GetTemporary(source.width >> downSample, source.height >> downSample, 0,source.format);
            RenderTexture temp2 = RenderTexture.GetTemporary(source.width >> downSample, source.height >> downSample, 0, source.format);

            Graphics.Blit(source, temp1);

            _Material.SetVector("_colorThreshold", colorThreshold);
            Graphics.Blit(temp1, temp2, _Material, 0);

            _Material.SetVector("_offsets", new Vector4(0, sampleScale, 0, 0));
            Graphics.Blit(temp2, temp1, _Material, 1);
            _Material.SetVector("_offsets", new Vector4(sampleScale, 0, 0, 0));
            Graphics.Blit(temp1, temp2, _Material, 1);

            _Material.SetTexture("_BlurTex", temp2);
            Graphics.Blit(source, destination, _Material, 2);

            RenderTexture.ReleaseTemporary(temp1);
            RenderTexture.ReleaseTemporary(temp2);
        }
    }
}
