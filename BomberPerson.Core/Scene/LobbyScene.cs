using System.Collections.Generic;
using BomberPerson.Core.Lobby;
using BomberPerson.Core.Network;
using BomberPerson.Core.State.NetworkMessages;
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

    private Texture2D pastille;

    private readonly Color colorReady    = new Color(80,  200, 80);
    private readonly Color colorNotReady = new Color(200, 80,  80);
    private readonly Color colorHost     = new Color(220, 180, 0);

    // Couleur du carré par emplacement : bleu foncé, rouge, vert, jaune.
    private static readonly Color[] SlotPalette =
    {
        new Color(30,  60,  150),
        new Color(200, 50,  50),
        new Color(60,  180, 70),
        new Color(220, 200, 40),
    };

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

        pastille = CreateCircleTexture(graphicsDevice, 64);

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
        int slots  = SlotPalette.Length;
        int frameW = 180;
        int frameH = 220;
        int gap    = 24;
        int totalW = slots * frameW + (slots - 1) * gap;
        int startX = (viewport.Width - totalW) / 2;

        List<NetworkPlayer> players = LobbyManager.Instance.Players;

        for (int slot = 0; slot < slots; slot++)
        {
            Rectangle     frame  = new Rectangle(startX + slot * (frameW + gap), startY, frameW, frameH);
            NetworkPlayer player = FindBySlot(players, slot);

            DrawFrame(spriteBatch, frame, player != null);

            if (player == null)
            {
                DrawCenteredText(spriteBatch, "En attente...", frame, new Color(110, 110, 120));
                continue;
            }

            // Carré de la couleur du joueur (déterminée par l'emplacement).
            int       square    = 110;
            Rectangle colorRect = new Rectangle(frame.X + (frameW - square) / 2, frame.Y + 26, square, square);
            spriteBatch.Draw(pixel, colorRect, SlotPalette[slot]);

            // Pastille prêt / pas prêt, coin haut droit du cadre.
            int       dot     = 28;
            Rectangle dotRect = new Rectangle(frame.Right - dot - 12, frame.Y + 12, dot, dot);
            spriteBatch.Draw(pastille, dotRect, player.IsReady ? colorReady : colorNotReady);

            Vector2 nameSize = fontUI.MeasureString(player.Name);
            Vector2 namePos  = new Vector2(frame.X + (frameW - nameSize.X) / 2f, colorRect.Bottom + 16);
            spriteBatch.DrawString(fontUI, player.Name, namePos, Color.White);

            if (player.IsHost)
            {
                Vector2 hostSize = fontUI.MeasureString("HOST");
                Vector2 hostPos  = new Vector2(frame.X + (frameW - hostSize.X) / 2f, namePos.Y + nameSize.Y + 6);
                spriteBatch.DrawString(fontUI, "HOST", hostPos, colorHost);
            }
        }
    }

    private static NetworkPlayer FindBySlot(List<NetworkPlayer> players, int slot)
    {
        foreach (NetworkPlayer p in players)
            if (p.Id == slot) return p;
        return null;
    }

    private void DrawFrame(SpriteBatch spriteBatch, Rectangle frame, bool occupied)
    {
        Color bg     = occupied ? new Color(42, 42, 56) : new Color(26, 26, 34);
        Color border = occupied ? new Color(95, 95, 130) : new Color(55, 55, 70);
        int   t      = 2;

        spriteBatch.Draw(pixel, frame, bg);
        spriteBatch.Draw(pixel, new Rectangle(frame.X, frame.Y, frame.Width, t), border);
        spriteBatch.Draw(pixel, new Rectangle(frame.X, frame.Bottom - t, frame.Width, t), border);
        spriteBatch.Draw(pixel, new Rectangle(frame.X, frame.Y, t, frame.Height), border);
        spriteBatch.Draw(pixel, new Rectangle(frame.Right - t, frame.Y, t, frame.Height), border);
    }

    private void DrawCenteredText(SpriteBatch spriteBatch, string text, Rectangle frame, Color color)
    {
        Vector2 size = fontUI.MeasureString(text);
        Vector2 pos  = new Vector2(frame.X + (frame.Width - size.X) / 2f, frame.Y + (frame.Height - size.Y) / 2f);
        spriteBatch.DrawString(fontUI, text, pos, color);
    }

    private static Texture2D CreateCircleTexture(GraphicsDevice device, int diameter)
    {
        Texture2D texture  = new Texture2D(device, diameter, diameter);
        Color[]   data     = new Color[diameter * diameter];
        float     radius   = diameter / 2f;
        float     radiusSq = radius * radius;

        for (int y = 0; y < diameter; y++)
        for (int x = 0; x < diameter; x++)
        {
            float dx = x - radius + 0.5f;
            float dy = y - radius + 0.5f;
            data[y * diameter + x] = dx * dx + dy * dy <= radiusSq ? Color.White : Color.Transparent;
        }

        texture.SetData(data);
        return texture;
    }

    // -------------------------
    // ACTIONS BOUTONS
    // Les scènes envoient des RequestTask — pas de logique ici
    // -------------------------

    private void OnReadyClicked()
    {
        localReady = !localReady;
        btnReady.SetLabel(localReady ? "Pas prêt" : "Prêt");

        // On envoie l'intention au serveur ; la pastille se met à jour au retour du NewState.
        NetworkManager.Instance.Send(new SetReadyMessage(localReady));
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