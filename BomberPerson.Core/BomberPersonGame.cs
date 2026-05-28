using System;
using System.Collections.Generic;
using System.Globalization;
using BomberPerson.Core.Client;
using BomberPerson.Core.Scene;
using BomberPerson.Core.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace BomberPerson.Core
{
    /// <summary>
    /// The main class for the game, responsible for managing game components, settings, 
    /// and platform-specific configurations.
    /// </summary>
    /// <remarks>
    /// This class is the entry point for the game and handles initialization, content loading,
    /// and screen management.
    /// </remarks>}
    public class BomberPersonGame : Game
    {
        // Resources for drawing.
        private GraphicsDeviceManager graphicsDeviceManager;
        private SpriteBatch spriteBatch;

        private SceneManager           sceneManager;
        private ClientStateController clientStateController;

        /// <summary>
        /// Initializes a new instance of the game. Configures platform-specific settings, 
        /// initializes services like settings and leaderboard managers, and sets up the 
        /// screen manager for screen transitions.
        /// </summary>
        public BomberPersonGame()
        {
            graphicsDeviceManager = new GraphicsDeviceManager(this);

            // Share GraphicsDeviceManager as a service.
            Services.AddService(typeof(GraphicsDeviceManager), graphicsDeviceManager);

            IsMouseVisible = true;
            
            Content.RootDirectory = "Content";
            
            graphicsDeviceManager.PreferredBackBufferWidth  = 1280;
            graphicsDeviceManager.PreferredBackBufferHeight = 720;

            // Configure screen orientations.
            graphicsDeviceManager.SupportedOrientations = DisplayOrientation.LandscapeLeft | DisplayOrientation.LandscapeRight;
        }

        /// <summary>
        /// Initializes the game, including setting up localization and adding the 
        /// initial screens to the ScreenManager.
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
        }

        /// <summary>
        /// Loads game content, such as textures and particle systems.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            sceneManager = new SceneManager(Content, GraphicsDevice);
            clientStateController = new ClientStateController(sceneManager);
        }
        
        protected override void Update(GameTime gameTime)
        {
            KeyboardExtended.Update();
            MouseExtended.Update();
            KeyboardStateExtended keyboardState = KeyboardExtended.GetState();
            if (keyboardState.WasKeyPressed(Keys.Escape))
            {
                Exit();
            }
            
            sceneManager.Update(gameTime);

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            // TODO: Add your drawing code here
            spriteBatch.Begin();
            sceneManager.Draw(spriteBatch);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}