using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace MonogameDesktop.Gui.DearImGui {

    public sealed class ImGuiRenderer {

        public GraphicsDevice GraphicsDevice => Game.GraphicsDevice;
        public Game Game { get; }

        public RasterizerState RasterizerState { get; }

        private readonly Dictionary<IntPtr, Texture2D> loadedTextures = new Dictionary<IntPtr, Texture2D>();
        private readonly List<int> keys = new List<int>();

        private BasicEffect effect;

        private byte[] vertexData;
        private VertexBuffer vertexBuffer;
        private int vertexBufferSize;

        private byte[] indexData;
        private IndexBuffer indexBuffer;
        private int indexBufferSize;

        private int textureId;
        private IntPtr? fontTextureId;
        private int scrollWheelValue;

        public ImGuiRenderer(Game game) {
            IntPtr context = ImGui.CreateContext();

            ImGui.SetCurrentContext(context);

            Game = game ?? throw new ArgumentNullException(nameof(game));

            RasterizerState = new RasterizerState() {
                CullMode = CullMode.None,
                DepthBias = 0,
                FillMode = FillMode.Solid,
                MultiSampleAntiAlias = false,
                ScissorTestEnable = true,
                SlopeScaleDepthBias = 0
            };

            SetupInput();
        }

        public unsafe void RebuildFontAtlas() {
            ImGuiIOPtr io = ImGui.GetIO();
            io.Fonts.GetTexDataAsRGBA32(out byte* pixelData, out int width, out int height, out int bytesPerPixel);

            byte[] pixels = new byte[width * height * bytesPerPixel];
            unsafe { 
                Marshal.Copy(new IntPtr(pixelData), pixels, 0, pixels.Length); 
            }

            Texture2D tex2d = new Texture2D(GraphicsDevice, width, height, false, SurfaceFormat.Color);
            tex2d.SetData(pixels);

            if(fontTextureId.HasValue) UnbindTexture(fontTextureId.Value);

            fontTextureId = BindTexture(tex2d);

            io.Fonts.SetTexID(fontTextureId.Value);
            io.Fonts.ClearTexData();
        }

        public IntPtr BindTexture(Texture2D texture) {
            if(loadedTextures.ContainsValue(texture)) {
                return loadedTextures.FirstOrDefault(pair => pair.Value == texture).Key;
            }

            IntPtr id = new IntPtr(textureId++);

            loadedTextures.Add(id, texture);

            return id;
        }

        public void UnbindTexture(IntPtr textureId) {
            loadedTextures.Remove(textureId);
        }

        public void Begin(GameTime gameTime) {
            ImGui.GetIO().DeltaTime = (float) gameTime.ElapsedGameTime.TotalSeconds;

            UpdateInput();

            ImGui.NewFrame();
        }

        public void End() {
            ImGui.Render();

            unsafe { 
                RenderDrawData(ImGui.GetDrawData()); 
            }
        }

        private void SetupInput() {
            ImGuiIOPtr io = ImGui.GetIO();

            keys.Add(io.KeyMap[(int) ImGuiKey.Tab] = (int) Keys.Tab);
            keys.Add(io.KeyMap[(int) ImGuiKey.LeftArrow] = (int) Keys.Left);
            keys.Add(io.KeyMap[(int) ImGuiKey.RightArrow] = (int) Keys.Right);
            keys.Add(io.KeyMap[(int) ImGuiKey.UpArrow] = (int) Keys.Up);
            keys.Add(io.KeyMap[(int) ImGuiKey.DownArrow] = (int) Keys.Down);
            keys.Add(io.KeyMap[(int) ImGuiKey.PageUp] = (int) Keys.PageUp);
            keys.Add(io.KeyMap[(int) ImGuiKey.PageDown] = (int) Keys.PageDown);
            keys.Add(io.KeyMap[(int) ImGuiKey.Home] = (int) Keys.Home);
            keys.Add(io.KeyMap[(int) ImGuiKey.End] = (int) Keys.End);
            keys.Add(io.KeyMap[(int) ImGuiKey.Delete] = (int) Keys.Delete);
            keys.Add(io.KeyMap[(int) ImGuiKey.Backspace] = (int) Keys.Back);
            keys.Add(io.KeyMap[(int) ImGuiKey.Enter] = (int) Keys.Enter);
            keys.Add(io.KeyMap[(int) ImGuiKey.Escape] = (int) Keys.Escape);
            keys.Add(io.KeyMap[(int) ImGuiKey.Space] = (int) Keys.Space);
            keys.Add(io.KeyMap[(int) ImGuiKey.A] = (int) Keys.A);
            keys.Add(io.KeyMap[(int) ImGuiKey.C] = (int) Keys.C);
            keys.Add(io.KeyMap[(int) ImGuiKey.V] = (int) Keys.V);
            keys.Add(io.KeyMap[(int) ImGuiKey.X] = (int) Keys.X);
            keys.Add(io.KeyMap[(int) ImGuiKey.Y] = (int) Keys.Y);
            keys.Add(io.KeyMap[(int) ImGuiKey.Z] = (int) Keys.Z);

            Game.Window.TextInput += (sender, args) => {
                if(args.Character == '\t') return;

                io.AddInputCharacter(args.Character);
            };

            ImGui.GetIO().Fonts.AddFontDefault();
        }

        private Effect UpdateEffect(Texture2D texture) {
            effect ??= new BasicEffect(GraphicsDevice);

            ImGuiIOPtr io = ImGui.GetIO();

            effect.World = Matrix.Identity;
            effect.View = Matrix.Identity;
            effect.Projection = Matrix.CreateOrthographicOffCenter(0f, io.DisplaySize.X, io.DisplaySize.Y, 0f, -1f, 1f);
            effect.TextureEnabled = true;
            effect.Texture = texture;
            effect.VertexColorEnabled = true;

            return effect;
        }

        private void UpdateInput() {
            if(!Game.IsActive) return;

            ImGuiIOPtr io = ImGui.GetIO();

            MouseState mouse = Mouse.GetState();
            KeyboardState keyboard = Keyboard.GetState();

            for(int i = 0; i < keys.Count; i++) {
                io.KeysDown[keys[i]] = keyboard.IsKeyDown((Keys) keys[i]);
            }

            io.KeyShift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
            io.KeyCtrl = keyboard.IsKeyDown(Keys.LeftControl) || keyboard.IsKeyDown(Keys.RightControl);
            io.KeyAlt = keyboard.IsKeyDown(Keys.LeftAlt) || keyboard.IsKeyDown(Keys.RightAlt);
            io.KeySuper = keyboard.IsKeyDown(Keys.LeftWindows) || keyboard.IsKeyDown(Keys.RightWindows);

            io.DisplaySize = new System.Numerics.Vector2(GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);
            io.DisplayFramebufferScale = new System.Numerics.Vector2(1f, 1f);

            io.MousePos = new System.Numerics.Vector2(mouse.X, mouse.Y);

            io.MouseDown[0] = mouse.LeftButton == ButtonState.Pressed;
            io.MouseDown[1] = mouse.RightButton == ButtonState.Pressed;
            io.MouseDown[2] = mouse.MiddleButton == ButtonState.Pressed;

            int scrollDelta = mouse.ScrollWheelValue - scrollWheelValue;
            io.MouseWheel = scrollDelta > 0 ? 1 : scrollDelta < 0 ? -1 : 0;
            scrollWheelValue = mouse.ScrollWheelValue;
        }

        private void RenderDrawData(ImDrawDataPtr drawData) {
            Viewport lastViewport = GraphicsDevice.Viewport;
            Rectangle lastScissorBox = GraphicsDevice.ScissorRectangle;

            GraphicsDevice.BlendFactor = Color.White;
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.RasterizerState = RasterizerState;
            GraphicsDevice.DepthStencilState = DepthStencilState.DepthRead;

            drawData.ScaleClipRects(ImGui.GetIO().DisplayFramebufferScale);

            GraphicsDevice.Viewport = new Viewport(0, 0, GraphicsDevice.PresentationParameters.BackBufferWidth, GraphicsDevice.PresentationParameters.BackBufferHeight);

            UpdateBuffers(drawData);

            RenderCommandLists(drawData);

            GraphicsDevice.Viewport = lastViewport;
            GraphicsDevice.ScissorRectangle = lastScissorBox;
        }

        private unsafe void UpdateBuffers(ImDrawDataPtr drawData) {
            if(drawData.TotalVtxCount == 0) return;

            if(drawData.TotalVtxCount > vertexBufferSize) {
                vertexBuffer?.Dispose();

                vertexBufferSize = (int) (drawData.TotalVtxCount * 1.5f);
                vertexBuffer = new VertexBuffer(GraphicsDevice, DrawVertexDeclaration.Declaration, vertexBufferSize, BufferUsage.None);
                vertexData = new byte[vertexBufferSize * DrawVertexDeclaration.Size];
            }

            if(drawData.TotalIdxCount > indexBufferSize) {
                indexBuffer?.Dispose();

                indexBufferSize = (int) (drawData.TotalIdxCount * 1.5f);
                indexBuffer = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, indexBufferSize, BufferUsage.None);
                indexData = new byte[indexBufferSize * sizeof(ushort)];
            }

            int vtxOffset = 0;
            int idxOffset = 0;

            for(int n = 0; n < drawData.CmdListsCount; n++) {
                ImDrawListPtr cmdList = drawData.CmdListsRange[n];

                fixed(void* vtxDstPtr = &vertexData[vtxOffset * DrawVertexDeclaration.Size])
                fixed(void* idxDstPtr = &indexData[idxOffset * sizeof(ushort)]) {
                    Buffer.MemoryCopy((void*) cmdList.VtxBuffer.Data, vtxDstPtr, vertexData.Length, cmdList.VtxBuffer.Size * DrawVertexDeclaration.Size);
                    Buffer.MemoryCopy((void*) cmdList.IdxBuffer.Data, idxDstPtr, indexData.Length, cmdList.IdxBuffer.Size * sizeof(ushort));
                }

                vtxOffset += cmdList.VtxBuffer.Size;
                idxOffset += cmdList.IdxBuffer.Size;
            }

            vertexBuffer.SetData(vertexData, 0, drawData.TotalVtxCount * DrawVertexDeclaration.Size);
            indexBuffer.SetData(indexData, 0, drawData.TotalIdxCount * sizeof(ushort));
        }

        private unsafe void RenderCommandLists(ImDrawDataPtr drawData) {
            GraphicsDevice.SetVertexBuffer(vertexBuffer);
            GraphicsDevice.Indices = indexBuffer;

            int vtxOffset = 0;
            int idxOffset = 0;

            for(int n = 0; n < drawData.CmdListsCount; n++) {
                ImDrawListPtr cmdList = drawData.CmdListsRange[n];

                for(int cmdi = 0; cmdi < cmdList.CmdBuffer.Size; cmdi++) {
                    ImDrawCmdPtr drawCmd = cmdList.CmdBuffer[cmdi];

                    if(!loadedTextures.ContainsKey(drawCmd.TextureId)) {
                        throw new InvalidOperationException($"Could not find a texture with id '{drawCmd.TextureId}', please check your bindings");
                    }

                    GraphicsDevice.ScissorRectangle = new Rectangle(
                        (int) drawCmd.ClipRect.X,
                        (int) drawCmd.ClipRect.Y,
                        (int) (drawCmd.ClipRect.Z - drawCmd.ClipRect.X),
                        (int) (drawCmd.ClipRect.W - drawCmd.ClipRect.Y)
                    );

                    Effect effect = UpdateEffect(loadedTextures[drawCmd.TextureId]);

                    foreach(EffectPass pass in effect.CurrentTechnique.Passes) {
                        pass.Apply();

#pragma warning disable CS0618
                        GraphicsDevice.DrawIndexedPrimitives(
                            primitiveType: PrimitiveType.TriangleList,
                            baseVertex: vtxOffset,
                            minVertexIndex: 0,
                            numVertices: cmdList.VtxBuffer.Size,
                            startIndex: idxOffset,
                            primitiveCount: (int) drawCmd.ElemCount / 3
                        );
#pragma warning restore CS0618
                    }

                    idxOffset += (int) drawCmd.ElemCount;
                }

                vtxOffset += cmdList.VtxBuffer.Size;
            }
        }

    }

}
