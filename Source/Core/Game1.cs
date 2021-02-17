using System;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonogameDesktop.Gui.DearImGui;

namespace MonogameDesktop.Core {

    public sealed class Game1 : Game {

        public GraphicsDeviceManager Graphics { get; }
        public SpriteBatch SpriteBatch => spriteBatch;
        public ImGuiRenderer ImGuiRenderer { get; }
            
        private SpriteBatch spriteBatch;

        public Game1() {
            Graphics = new GraphicsDeviceManager(this);
            ImGuiRenderer = new ImGuiRenderer(this);

            Content.RootDirectory = "Content";
            TargetElapsedTime = TimeSpan.FromSeconds(1f / 144f);

            IsFixedTimeStep = true;
            IsMouseVisible = true;
        }

        protected override void Initialize() {
            base.Initialize();
        }

        protected override void LoadContent() {
            base.LoadContent();

            ImGuiRenderer.RebuildFontAtlas();

            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);

            if(Keyboard.GetState().IsKeyDown(Keys.Escape))  Exit();
        }

        protected override void Draw(GameTime gameTime) {
            base.Draw(gameTime);

            GraphicsDevice.Clear(Color.Black);

            ImGuiRenderer.Begin(gameTime);

            ImGui.ShowDemoWindow();

            ImGuiRenderer.End();
        }

    }

}
