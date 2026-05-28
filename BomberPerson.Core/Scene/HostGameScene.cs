
using System.Threading.Tasks;
using BomberPerson.Core.Client;
using BomberPerson.Core.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BomberPerson.Core.Scene;

public class HostGameScene(ClientStateController clientStateController) : Scene(clientStateController)
{
    private SpriteFont fontTitle;
    private SpriteFont fontUI;
    private Texture2D  pixel;

    private TextField fieldGameName;
    private TextField fieldPort;
    private TextField fieldPassword;
    private Button    btnCreate;
    private Button    btnBack;
    
    private string errorMessage => ClientStateController.StatusMessage?.Importance == StatusMessage.ImportanceLevels.Error 
        ? ClientStateController.StatusMessage.Message 
        : null;

    private string infoMessage => ClientStateController.StatusMessage?.Importance != StatusMessage.ImportanceLevels.Error 
        ? ClientStateController.StatusMessage?.Message 
        : null;

    public override void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        fontTitle = content.Load<SpriteFont>("Fonts/TitleFont");
        fontUI    = content.Load<SpriteFont>("Fonts/ButtonFont");

        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        int screenW  = graphicsDevice.Viewport.Width;
        int screenH  = graphicsDevice.Viewport.Height;
        int fieldW   = 400;
        int fieldH   = 44;
        int labelH   = 28;  // hauteur du label au dessus du champ
        int fieldX   = (screenW - fieldW) / 2;
        int spacing  = fieldH + labelH + 16; // champ + label + marge entre champs
        int groupH   = spacing * 3 + 60;     // hauteur totale du groupe (3 champs + boutons)
        int startY   = (screenH - groupH) / 2 + labelH; // centré verticalement

        fieldGameName = new TextField("Nom de la partie", new Rectangle(fieldX, startY, fieldW, fieldH), fontUI, maxLength: 24);
        fieldPort     = new TextField("Port", new Rectangle(fieldX, startY + spacing, fieldW, fieldH), fontUI, maxLength: 5);
        fieldPassword = new TextField("Mot de passe (optionnel)", new Rectangle(fieldX, startY + spacing * 2, fieldW, fieldH), fontUI, isPassword: true, maxLength: 20);

        int btnW = 180;
        int btnH = 50;
        int btnY = startY + spacing * 3 + 10;

        btnCreate = new Button("Créer", new Rectangle(fieldX + fieldW - btnW, btnY, btnW, btnH), fontUI);
        btnBack   = new Button("Retour", new Rectangle(fieldX, btnY, btnW, btnH), fontUI);

        btnCreate.OnClick += OnCreateClicked;
        btnBack.OnClick   += OnBackClicked;

        fieldGameName.SetValue("TestGame");
        fieldPort.SetValue("7777");
    }

    public override void UnloadContent()
    {
        btnCreate.OnClick -= OnCreateClicked;
        btnBack.OnClick   -= OnBackClicked;
    }

    public override void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();
        MouseState mouse    = Mouse.GetState();

        fieldGameName.Update(gameTime, keyboard, mouse);
        fieldPort.Update(gameTime, keyboard, mouse);
        fieldPassword.Update(gameTime, keyboard, mouse);
        btnCreate.Update(gameTime);
        btnBack.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Viewport viewport  = spriteBatch.GraphicsDevice.Viewport;
        Vector2 titleSize = fontTitle.MeasureString("Host Game");
        Vector2 titlePos  = new Vector2((viewport.Width - titleSize.X) / 2f, viewport.Height * 0.12f);

        spriteBatch.DrawString(fontTitle, "Host Game", titlePos, Color.White);

        fieldGameName.Draw(spriteBatch, pixel);
        fieldPort.Draw(spriteBatch, pixel);
        fieldPassword.Draw(spriteBatch, pixel);
        btnCreate.Draw(spriteBatch, pixel);
        btnBack.Draw(spriteBatch, pixel);

        if (!string.IsNullOrEmpty(errorMessage))
        {
            Vector2 errSize = fontUI.MeasureString(errorMessage);
            Vector2 errPos  = new Vector2((viewport.Width - errSize.X) / 2f, titlePos.Y + titleSize.Y + 10);
            spriteBatch.DrawString(fontUI, errorMessage, errPos, Color.Red);
        }
        else if (!string.IsNullOrEmpty(infoMessage))
        {
            Vector2 infoSize = fontUI.MeasureString(infoMessage);
            Vector2 infoPos  = new Vector2((viewport.Width - infoSize.X) / 2f, titlePos.Y + titleSize.Y + 10);
            spriteBatch.DrawString(fontUI, infoMessage, infoPos, Color.Yellow);
        }
    }

    private void OnBackClicked()
    {
        ClientStateController.GoToMainMenu();
    }

    private void OnCreateClicked()
    {
        if (string.IsNullOrWhiteSpace(fieldGameName.Value))
        {
            // Error handling could also be moved to controller but for now we keep simple validation here
            return;
        }

        if (!int.TryParse(fieldPort.Value, out int port) || port < 1024 || port > 65535)
        {
            return;
        }

        ClientStateController.HostGame(fieldGameName.Value, port, fieldPassword.Value);
    }
}