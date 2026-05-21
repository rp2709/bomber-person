using System.Collections.Generic;
using BomberPerson.Core.Lobby;
using BomberPerson.Core.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BomberPerson.Core.Scene;

public class LobbyScene : IScene
{
    public SceneManager SceneManager { get; private set; }
    private string gameName = "Bomber Person";

    private SpriteFont fontTitle;
    private SpriteFont fontUI;
    private Texture2D  pixel;

    private Button btnReady;
    private Button btnStart;
    private Button btnQuit;

    private bool localReady = false;

    private readonly Color colorReady    = new Color(80,  200, 80);
    private readonly Color colorNotReady = new Color(200, 80,  80);
    private readonly Color colorHost     = new Color(220, 180, 0);
    private readonly Color colorRowEven  = new Color(35,  35,  45);
    private readonly Color colorRowOdd   = new Color(28,  28,  38);

    public LobbyScene()
    {
        //this.gameName = gameName;
        LobbyManager.Instance.Reset();
    }

    public void SetName(string name)
    {
        gameName = name;
    }

    public void LoadContent(ContentManager content, GraphicsDevice graphicsDevice, SceneManager sceneManager)
    {
        SceneManager = sceneManager;

        fontTitle = content.Load<SpriteFont>("Fonts/TitleFont");
        fontUI    = content.Load<SpriteFont>("Fonts/ButtonFont");

        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData(new[] { Color.White });

        int screenW = graphicsDevice.Viewport.Width;
        int screenH = graphicsDevice.Viewport.Height;
        int btnW    = 200;
        int btnH    = 50;
        int btnY    = screenH - 80;
        int centerX = screenW / 2;

        btnReady = new Button("Prêt", new Rectangle(centerX - btnW - 20, btnY, btnW, btnH), fontUI);
        btnReady.OnClick += OnReadyClicked;
        /*
        if (NetworkManager.Instance.IsServer)
        {
            btnStart = new Button("Lancer", new Rectangle(centerX + 20, btnY, btnW, btnH), fontUI);
            btnStart.OnClick += OnStartClicked;
        }
        */
        btnQuit = new Button("Quitter", new Rectangle(20, 20, 120, 40), fontUI);
        btnQuit.OnClick += OnQuitClicked;
    }

    public void UnloadContent()
    {
        btnReady.OnClick -= OnReadyClicked;
        btnQuit.OnClick -= OnQuitClicked;
    }

    public void Update(GameTime gameTime)
    {
        // Réagir aux changements du LobbyManager mis à jour par les AuthorityTask
        
        if (LobbyManager.Instance.HostLeft)
        {
            //NetworkManager.Instance.Disconnect();
            SceneManager.LoadScene(EScene.MainMenu);
            return;
        }
        
        
        /*
        if (LobbyManager.Instance.GameStarting)
        {
            // TODO : SceneManager.NavigateTo(new GameScene());
            return;
        }
        */
        
        btnReady.Update(gameTime);
        btnQuit.Update(gameTime);

        //if (NetworkManager.Instance.IsServer) btnStart.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        Viewport viewport = spriteBatch.GraphicsDevice.Viewport;
        int      screenW  = viewport.Width;

        // Titre
        Vector2 titleSize = fontTitle.MeasureString("Lobby");
        Vector2 titlePos  = new Vector2((screenW - titleSize.X) / 2f, 30);
        spriteBatch.DrawString(fontTitle, "Lobby", titlePos, Color.White);

        // Nom de la partie
        Vector2 nameSize = fontUI.MeasureString(gameName);
        Vector2 namePos  = new Vector2((screenW - nameSize.X) / 2f, titlePos.Y + titleSize.Y + 6);
        spriteBatch.DrawString(fontUI, gameName, namePos, new Color(160, 160, 160));

        // Message d'erreur
        
        if (!string.IsNullOrEmpty(LobbyManager.Instance.ErrorMessage))
        {
            Vector2 errSize = fontUI.MeasureString(LobbyManager.Instance.ErrorMessage);
            Vector2 errPos  = new Vector2((screenW - errSize.X) / 2f, namePos.Y + nameSize.Y + 6);
            spriteBatch.DrawString(fontUI, LobbyManager.Instance.ErrorMessage, errPos, Color.Red);
        }
        
        DrawPlayerList(spriteBatch, viewport, (int)(namePos.Y + nameSize.Y + 40));

        btnReady.Draw(spriteBatch, pixel);
        btnQuit.Draw(spriteBatch, pixel);

        //if (NetworkManager.Instance.IsServer) btnStart.Draw(spriteBatch, pixel);
    }

    // -------------------------
    // DESSIN LISTE JOUEURS
    // -------------------------

    private void DrawPlayerList(SpriteBatch spriteBatch, Viewport viewport, int startY)
    {
        int listW   = 600;
        int rowH    = 54;
        int listX   = (viewport.Width - listW) / 2;
        int padding = 16;
        
        List<NetworkPlayer> players = LobbyManager.Instance.Players;

        for (int i = 0; i < players.Count; i++)
        {
            NetworkPlayer player  = players[i];
            Rectangle     rowRect = new Rectangle(listX, startY + i * (rowH + 4), listW, rowH);

            Color rowColor = i % 2 == 0 ? colorRowEven : colorRowOdd;
            spriteBatch.Draw(pixel, rowRect, rowColor);

            Color sideColor = player.IsReady ? colorReady : colorNotReady;
            spriteBatch.Draw(pixel, new Rectangle(rowRect.X, rowRect.Y, 4, rowRect.Height), sideColor);

            Vector2 namePos = new Vector2(rowRect.X + padding + 8, rowRect.Y + (rowH - fontUI.LineSpacing) / 2f);
            spriteBatch.DrawString(fontUI, player.Name, namePos, Color.White);

            if (player.IsHost)
            {
                Vector2 hostPos = new Vector2(
                    rowRect.X + padding + 8 + fontUI.MeasureString(player.Name).X + 12,
                    namePos.Y
                );
                spriteBatch.DrawString(fontUI, "HOST", hostPos, colorHost);
            }

            string  statusText  = player.IsReady ? "Prêt" : "Pas prêt";
            Color   statusColor = player.IsReady ? colorReady : colorNotReady;
            Vector2 statusSize  = fontUI.MeasureString(statusText);
            Vector2 statusPos   = new Vector2(rowRect.Right - statusSize.X - padding, namePos.Y);
            spriteBatch.DrawString(fontUI, statusText, statusPos, statusColor);
        }
        
    }

    // -------------------------
    // ACTIONS BOUTONS
    // Les scènes envoient des RequestTask — pas de logique ici
    // -------------------------

    private void OnReadyClicked()
    {
        localReady = !localReady;
        btnReady.SetLabel(localReady ? "Pas prêt" : "Prêt");

        //NetworkManager.Instance.SendToServer( localReady ? new PlayerReadyRequestTask() : new PlayerNotReadyRequestTask() );
    }

    private void OnStartClicked()
    {
        //NetworkManager.Instance.SendToServer(new GameStartRequestTask());
    }

    private void OnQuitClicked()
    {
        //NetworkManager.Instance.SendToServer(new PlayerLeftRequestTask());
        //NetworkManager.Instance.Disconnect();
        SceneManager.LoadScene(EScene.MainMenu);
    }
}