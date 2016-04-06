﻿using OpenTK.Graphics.OpenGL;
using Render.Geometries.Buffers;

namespace Render.RenderObjects.Geometries
{
    // 0 - Position
    internal class FullScreenQuad : GeometryBase
    {
        const string Vert = "vert";


        public FullScreenQuad()
        {
            Vao[Vert]= FullscreenQuadVertices.Value;
            Vao.EnableAttrib(Vert, 0);
        }


        public override void Draw()
        {
            GL.BindVertexArray(Vao.Handle);
            GL.DrawArrays(PrimitiveType.Quads, 0, 4);
        }
    }
}
