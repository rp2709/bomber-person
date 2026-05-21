using BomberPerson.Core.Lobby;
using BomberPerson.Core.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BomberPerson.Core.Scene;

public class JoinGameScene : IScene
{
    public SceneManager SceneManager { get; private set; }

    private SpriteFont fontTitle;
    private SpriteFont fontUI;
    private Texture2D  pixel;

    private TextField fieldPlayerName;
    private TextField fieldIP;
    private TextField fieldPort;
    private TextField fieldPassword;
    private Button    btnJoin;
    private Button    btnBack;

    private string errorMessage = "";

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice, SceneManager manager)
    {
        SceneManager = manager;

        fontTitle = content.Load<SpriteFont>("Fonts/TitleFont");
        fontUI    = content.Load<SpriteFont>("Fonts/ButtonFont");

        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        int screenW  = graphicsDevice.Viewport.Width;
        int screenH  = graphicsDevice.Viewport.Height;
        int fieldW   = 400;
        int fieldH   = 44;
        int labelH   = 28;
        int fieldX   = (screenW - fieldW) / 2;
        int spacing  = fieldH + labelH + 16;
        int groupH   = spacing * 4 + 60; // 4 champs + boutons
        int startY   = (screenH - groupH) / 2 + labelH;

        fieldPlayerName = new TextField("Nom du joueur", new Rectangle(fieldX, startY,                 fieldW, fieldH), fontUI, maxLength: 20);
        fieldIP         = new TextField("Adresse IP",    new Rectangle(fieldX, startY + spacing,       fieldW, fieldH), fontUI, maxLength: 15);
        fieldPort       = new TextField("Port",          new Rectangle(fieldX, startY + spacing * 2,   fieldW, fieldH), fontUI, maxLength: 5);
        fieldPassword   = new TextField("Mot de passe (optionnel)", new Rectangle(fieldX, startY + spacing * 3, fieldW, fieldH), fontUI, isPassword: true, maxLength: 20);

        int btnW = 180;
        int btnH = 50;
        int btnY = startY + spacing * 4 + 10;

        btnJoin = new Button("Rejoindre", new Rectangle(fieldX + fieldW - btnW, btnY, btnW, btnH), fontUI);
        btnBack = new Button("Retour",    new Rectangle(fieldX, btnY, btnW, btnH), fontUI);

        btnJoin.OnClick += OnJoinClicked;
        btnBack.OnClick += OnBackClicked;

        fieldPlayerName.SetValue("Player2");
        fieldIP.SetValue("127.0.0.1");
        fieldPort.SetValue("7777");
    }

    public void UnloadContent()
    {
        btnJoin.OnClick -= OnJoinClicked;
        btnBack.OnClick -= OnBackClicked;
    }

    public void Update(GameTime gameTime)
    {
        KeyboardState keyboard = Keyboard.GetState();
        MouseState    mouse    = Mouse.GetState();

        fieldPlayerName.Update(gameTime, keyboard, mouse);
        fieldIP.Update(gameTime, keyboard, mouse);
        fieldPort.Update(gameTime, keyboard, mouse);
        fieldPassword.Update(gameTime, keyboard, mouse);
        btnJoin.Update(gameTime);
        btnBack.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Viewport viewport  = spriteBatch.GraphicsDevice.Viewport;
        Vector2  titleSize = fontTitle.MeasureString("Join Game");
        Vector2  titlePos  = new Vector2((viewport.Width - titleSize.X) / 2f, viewport.Height * 0.12f);

        spriteBatch.DrawString(fontTitle, "Join Game", titlePos, Color.White);

        fieldPlayerName.Draw(spriteBatch, pixel);
        fieldIP.Draw(spriteBatch, pixel);
        fieldPort.Draw(spriteBatch, pixel);
        fieldPassword.Draw(spriteBatch, pixel);
        btnJoin.Draw(spriteBatch, pixel);
        btnBack.Draw(spriteBatch, pixel);

        if (!string.IsNullOrEmpty(errorMessage))
        {
            Vector2 errSize = fontUI.MeasureString(errorMessage);
            Vector2 errPos  = new Vector2((viewport.Width - errSize.X) / 2f, titlePos.Y + titleSize.Y + 10);
            spriteBatch.DrawString(fontUI, errorMessage, errPos, Color.Red);
        }
    }

    private async void OnJoinClicked()
    {
        if (string.IsNullOrWhiteSpace(fieldPlayerName.Value))
        {
            errorMessage = "Le nom du joueur est requis.";
            return;
        }

        if (string.IsNullOrWhiteSpace(fieldIP.Value))
        {
            errorMessage = "L'adresse IP est requise.";
            return;
        }

        if (!int.TryParse(fieldPort.Value, out int port) || port < 1024 || port > 65535)
        {
            errorMessage = "Port invalide (1024 - 65535).";
            return;
        }

        errorMessage = "Connexion en cours...";
        /*
        bool connected = await NetworkManager.Instance.ConnectAsync(fieldIP.Value, port, fieldPlayerName.Value, fieldPassword.Value);
        if (!connected)
        {
            errorMessage = "Impossible de se connecter au serveur.";
            return;
        }
        */
        //SceneManager.LoadScene(new LobbyScene(fieldPlayerName.Value));
        LobbyScene scene = (LobbyScene)SceneManager.LoadScene(EScene.LobbyMenu);
        //LobbyManager.Instance.SetLobby(scene, fieldPlayerName.Value);
    }
    
    private void OnBackClicked() => SceneManager.LoadScene(EScene.MainMenu);
}