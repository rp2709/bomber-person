using System;
using BomberPerson.Core.Client;
using BomberPerson.Core.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BomberPerson.Core.Scene;

    public class MainMenuScene(ClientStateController clientStateController) : Scene(clientStateController)
    {
    private SpriteFont  fontTitle;
    private SpriteFont  fontButton;
    private Texture2D   pixel;
    private Button      btnHost;
    private Button      btnJoin;

    private const string Title = "Bomberman";

    private string errorMessage => ClientStateController.StatusMessage?.Importance == StatusMessage.ImportanceLevels.Error 
        ? ClientStateController.StatusMessage.Message 
        : null;

    private string infoMessage => ClientStateController.StatusMessage?.Importance != StatusMessage.ImportanceLevels.Error 
        ? ClientStateController.StatusMessage?.Message 
        : null;
    
    public override void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
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
        
        btnHost.OnClick += ClientStateController.GoToHostMenu;
        btnJoin.OnClick += ClientStateController.GoToJoinMenu;
    }

    public override void UnloadContent()
    {
        btnHost.OnClick -= ClientStateController.GoToHostMenu;
        btnJoin.OnClick -= ClientStateController.GoToJoinMenu;
    }

    public override void Update(GameTime gameTime)
    {
        btnHost?.Update(gameTime);
        btnJoin?.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Viewport viewport = spriteBatch.GraphicsDevice.Viewport;

        // Titre centré en haut
        Vector2 titleSize = fontTitle.MeasureString(Title);
        Vector2 titlePos  = new ( (viewport.Width  - titleSize.X) / 2f, viewport.Height * 0.25f);
        spriteBatch.DrawString(fontTitle, Title, titlePos, Color.White);

        // Boutons
        btnHost.Draw(spriteBatch, pixel);
        btnJoin.Draw(spriteBatch, pixel);

        if (!string.IsNullOrEmpty(errorMessage))
        {
            Vector2 errSize = fontButton.MeasureString(errorMessage);
            Vector2 errPos  = new Vector2((viewport.Width - errSize.X) / 2f, titlePos.Y + titleSize.Y + 10);
            spriteBatch.DrawString(fontButton, errorMessage, errPos, Color.Red);
        }
        else if (!string.IsNullOrEmpty(infoMessage))
        {
            Vector2 infoSize = fontButton.MeasureString(infoMessage);
            Vector2 infoPos  = new Vector2((viewport.Width - infoSize.X) / 2f, titlePos.Y + titleSize.Y + 10);
            spriteBatch.DrawString(fontButton, infoMessage, infoPos, Color.Yellow);
        }
    }

}