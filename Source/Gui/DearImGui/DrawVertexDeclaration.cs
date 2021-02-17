using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;

namespace MonogameDesktop.Gui.DearImGui {

    public static class DrawVertexDeclaration {

        public static VertexDeclaration Declaration { get; }

        public static int Size { get; }

        static DrawVertexDeclaration() {
            unsafe { 
                Size = sizeof(ImDrawVert); 
            }

            Declaration = new VertexDeclaration(
                Size,

                new VertexElement(0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),

                new VertexElement(8, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),

                new VertexElement(16, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            );
        }

    }

}
