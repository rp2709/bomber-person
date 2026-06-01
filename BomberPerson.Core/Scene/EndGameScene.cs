using BomberPerson.Core.Client;
using BomberPerson.Core.State;
using BomberPerson.Core.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BomberPerson.Core.Scene;

public class EndGameScene(ClientStateController clientStateController) : Scene(clientStateController)
{
    private SpriteFont fontTitle;
    private SpriteFont fontUi;
    private Texture2D pixel;

    private Button btnReplay;
    private Button btnQuit;

    private string winnerName;
    private Color winnerColor = Color.White;

    public override void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        fontTitle = content.Load<SpriteFont>("Fonts/TitleFont");
        fontUi = content.Load<SpriteFont>("Fonts/ButtonFont");

        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData([Color.White]);

        int slot = ClientStateController.State?.WinnerSlotId ?? -1;
        IReadOnlyPlayer winner = null;
        if (slot >= 0 && ClientStateController.State != null)
        {
            foreach (var p in ClientStateController.State.Players)
            {
                if (p.Number == slot) { winner = p; break; }
            }
        }

        if (winner != null)
        {
            winnerName = winner.Name;
            winnerColor = new Color(winner.Color.R, winner.Color.G, winner.Color.B);
        }
        else
        {
            winnerName = null;
        }

        int screenW = graphicsDevice.Viewport.Width;
        int screenH = graphicsDevice.Viewport.Height;
        int btnW = 240;
        int btnH = 60;
        int spacing = 20;
        int btnX = (screenW - btnW) / 2;
        int btnY = screenH / 2 + 60;

        btnReplay = new Button("Replay", new Rectangle(btnX, btnY, btnW, btnH), fontUi);
        btnQuit = new Button("Quitter", new Rectangle(btnX, btnY + btnH + spacing, btnW, btnH), fontUi);

        btnReplay.OnClick += ClientStateController.Replay;
        btnQuit.OnClick += ClientStateController.QuitLobby;
    }

    public override void UnloadContent()
    {
        btnReplay.OnClick -= ClientStateController.Replay;
        btnQuit.OnClick -= ClientStateController.QuitLobby;
    }

    public override void Update(GameTime gameTime)
    {
        btnReplay.Update(gameTime);
        btnQuit.Update(gameTime);
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        Viewport viewport = spriteBatch.GraphicsDevice.Viewport;

        const string title = "Fin de partie";
        Vector2 titleSize = fontTitle.MeasureString(title);
        Vector2 titlePos = new((viewport.Width - titleSize.X) / 2f, viewport.Height * 0.2f);
        spriteBatch.DrawString(fontTitle, title, titlePos, Color.White);

        string subtitle;
        Color subtitleColor;
        if (!string.IsNullOrEmpty(winnerName))
        {
            subtitle = $"Vainqueur : {winnerName}";
            subtitleColor = winnerColor;
        }
        else
        {
            subtitle = "Pas de vainqueur";
            subtitleColor = new Color(200, 200, 200);
        }

        Vector2 subSize = fontTitle.MeasureString(subtitle);
        Vector2 subPos = new((viewport.Width - subSize.X) / 2f, titlePos.Y + titleSize.Y + 30);
        spriteBatch.DrawString(fontTitle, subtitle, subPos, subtitleColor);

        btnReplay.Draw(spriteBatch, pixel);
        btnQuit.Draw(spriteBatch, pixel);
    }
}
