﻿using CSharpPlatform.GL.Utils;
using System;
using System.Reflection;
using CSharpUtils.Extensions;

namespace CSPspEmu.Gui.XBR.Shader
{
    public class XBRShader : IDisposable
    {
        GLShader Shader;
        GLShaderFilter Filter;

        public XBRShader()
        {
            Console.WriteLine("{0}", string.Join("\n", Assembly.GetExecutingAssembly().GetManifestResourceNames()));
            Shader = new GLShader(
                GLShaderFilter.DefaultVertexShader,
                Assembly.GetExecutingAssembly().GetManifestResourceStream("CSPspEmu.Gui.XBR.Shader.Shader_2xBR.frag")
                    .ReadAllContentsAsString()
            );
            Filter = GLShaderFilter.Create(1, 1, Shader);
        }

        public GLTexture Process(GLTexture Input)
        {
            Filter.SetSize(Input.Width * 2, Input.Height * 2);
            return Filter.Process((_Shader) =>
            {
                _Shader.GetUniform("OGL2Texture").Set(GLTextureUnit.CreateAtIndex(0).SetWrap(GLWrap.ClampToEdge)
                    .SetFiltering(GLScaleFilter.Nearest).SetTexture(Input));
            });
        }

        public void Dispose()
        {
            Shader.Dispose();
            Filter.Dispose();
        }
    }
}