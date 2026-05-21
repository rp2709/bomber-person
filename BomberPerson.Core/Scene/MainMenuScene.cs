using System;
using BomberPerson.Core.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BomberPerson.Core.Scene;

public class MainMenuScene : IScene
{
    public SceneManager SceneManager { get; private set; }

    private SpriteFont  fontTitle;
    private SpriteFont  fontButton;
    private Texture2D   pixel;
    private Button      btnHost;
    private Button      btnJoin;

    private const string Title = "Bomberman";

    public MainMenuScene()
    {
    }
    
    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice, SceneManager sceneManager)
    {
        SceneManager = sceneManager;
        // Polices — à créer dans le Content Pipeline MonoGame
        fontTitle  = content.Load<SpriteFont>("Fonts/TitleFont");
        fontButton = content.Load<SpriteFont>("Fonts/ButtonFont");

        // Texture 1x1 pixel blanc pour dessiner les rectangles
        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData([Color.White]);

        int screenW = graphicsDevice.Viewport.Width;
        int screenH = graphicsDevice.Viewport.Height;
        
        // Dimensions des boutons
        int btnWidth  = 300;
        int btnHeight = 60;
        int btnX      = (screenW - btnWidth) / 2;
        int spacing   = 20;
        int centerY   = screenH / 2;
        
        btnHost = new Button("Host Game", new Rectangle(btnX, centerY, btnWidth, btnHeight), fontButton);
        btnJoin = new Button("Join Game", new Rectangle(btnX, centerY + btnHeight + spacing, btnWidth, btnHeight), fontButton);
        
        btnHost.OnClick += OnHostClicked;
        btnJoin.OnClick += OnJoinClicked;
    }

    public void UnloadContent()
    {
        btnHost.OnClick -= OnHostClicked;
        btnJoin.OnClick -= OnJoinClicked;
    }

    public void Update(GameTime gameTime)
    {
        btnHost?.Update(gameTime);
        btnJoin?.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Viewport viewport = spriteBatch.GraphicsDevice.Viewport;

        // Titre centré en haut
        Vector2 titleSize = fontTitle.MeasureString(Title);
        Vector2 titlePos  = new ( (viewport.Width  - titleSize.X) / 2f, viewport.Height * 0.25f);
        spriteBatch.DrawString(fontTitle, Title, titlePos, Color.White);

        // Boutons
        btnHost.Draw(spriteBatch, pixel);
        btnJoin.Draw(spriteBatch, pixel);
    }

    private void OnHostClicked() => SceneManager.LoadScene(EScene.HostMenu);
    private void OnJoinClicked() => SceneManager.LoadScene(EScene.JoinMenu);
}