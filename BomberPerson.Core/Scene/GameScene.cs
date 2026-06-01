using System;
using BomberPerson.Core.Client;
using BomberPerson.Core.Server;
using BomberPerson.Core.State;
using BomberPerson.Core.State.NetworkMessages;
using BomberPerson.Core.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace BomberPerson.Core.Scene;

public class GameScene(ClientStateController clientStateController) : Scene(clientStateController)
{
    private Texture2D pixel;
    private Texture2D circle;
    private SpriteFont font;
    private Button quitButton;

    private const int HeaderHeight = 50;

    private static readonly (Keys[] Bindings, MoveMessage.MoveDirection Direction)[] MoveBindings =
    [
        ([Keys.Up,    Keys.W], MoveMessage.MoveDirection.Up),
        ([Keys.Down,  Keys.S], MoveMessage.MoveDirection.Down),
        ([Keys.Left,  Keys.A], MoveMessage.MoveDirection.Left),
        ([Keys.Right, Keys.D], MoveMessage.MoveDirection.Right)
    ];

    public override void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        pixel = new Texture2D(graphicsDevice, 1, 1);
        pixel.SetData([Color.White]);

        circle = CreateCircleTexture(graphicsDevice, Settings.TileSize);
        font = content.Load<SpriteFont>("Fonts/ButtonFont");

        quitButton = new Button("Quit", new Rectangle(10, 10, 80, 30), font);
        quitButton.OnClick += ClientStateController.QuitLobby;
    }

    private Texture2D CreateCircleTexture(GraphicsDevice device, int diameter)
    {
        Texture2D texture = new Texture2D(device, diameter, diameter);
        Color[] colorData = new Color[diameter * diameter];

        float radius = diameter / 2f;
        float center = radius;

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                float dx = x - center;
                float dy = y - center;
                if (dx * dx + dy * dy <= radius * radius)
                {
                    colorData[y * diameter + x] = Color.White;
                }
                else
                {
                    colorData[y * diameter + x] = Color.Transparent;
                }
            }
        }

        texture.SetData(colorData);
        return texture;
    }

    public override void UnloadContent()
    {
        if (quitButton != null)
        {
            quitButton.OnClick -= ClientStateController.QuitLobby;
        }
    }

    public override void Update(GameTime gameTime)
    {
        quitButton?.Update(gameTime);

        KeyboardStateExtended ks = KeyboardExtended.GetState();

        bool anyMoveKeyDown = false;
        bool anyMoveKeyReleased = false;

        foreach (var (bindings, direction) in MoveBindings)
        {
            foreach (var key in bindings)
            {
                if (ks.WasKeyPressed(key))
                    ClientStateController.Move(direction);
                if (ks.IsKeyDown(key))
                    anyMoveKeyDown = true;
                if (ks.WasKeyReleased(key))
                    anyMoveKeyReleased = true;
            }
        }

        if (anyMoveKeyReleased && !anyMoveKeyDown)
            ClientStateController.Stop();

        if (ks.IsKeyDown(Keys.X) || ks.IsKeyDown(Keys.Space))
        {
            ClientStateController.PutBomb();
        }
    }

    public override void Draw(SpriteBatch spriteBatch)
    {
        GraphicsDevice gd = spriteBatch.GraphicsDevice;
        Viewport viewport = gd.Viewport;

        // Draw Header
        spriteBatch.Draw(pixel, new Rectangle(0, 0, viewport.Width, HeaderHeight), Color.DarkGray);
        quitButton?.Draw(spriteBatch, pixel);
        
        if (ClientStateController.State == null) return;
        
        // Use interpolation for smoother movement
        var state = ((State.State)ClientStateController.State).Clone();
        Simulation.Interpolate(state, DateTimeOffset.Now);
        
        // time remaining
        TimeSpan timeSinceGameStart = state.CurrentDate - state.GameStartDate;
        String timerText = $"Time : {timeSinceGameStart.Minutes}:{timeSinceGameStart.Seconds}";
        Vector2 textPos  = new (
            (viewport.Width - font.MeasureString(timerText).X) / 2f,
            HeaderHeight - font.LineSpacing
        );
        spriteBatch.DrawString(font, timerText, textPos, Color.Black);

        // Draw Terrain
        Terrain terrain = state.Terrain;
        if (terrain != null)
        {
            for (uint y = 0; y < terrain.Height; y++)
            {
                for (uint x = 0; x < terrain.Width; x++)
                {
                    Color color = terrain[x, y] switch
                    {
                        Terrain.Type.Solid => Color.Black,
                        Terrain.Type.Box => Color.Brown,
                        Terrain.Type.Border => Color.DarkSlateGray,
                        Terrain.Type.Empty => Color.Green,
                        _ => Color.Gray
                    };

                    spriteBatch.Draw(pixel, new Rectangle((int)x * Settings.TileSize, (int)y * Settings.TileSize + HeaderHeight, Settings.TileSize, Settings.TileSize), color);
                }
            }
        }

        // Draw Bombs
        foreach (var bomb in state.Bombs)
        {
            spriteBatch.Draw(circle, new Rectangle((int)bomb.PositionX * Settings.TileSize, (int)bomb.PositionY * Settings.TileSize + HeaderHeight, Settings.TileSize, Settings.TileSize), Color.Red);
        }

        // Draw Explosions
        foreach (var explosion in state.Explosions)
        {
            spriteBatch.Draw(pixel, new Rectangle((int)explosion.PositionX * Settings.TileSize, (int)explosion.PositionY * Settings.TileSize + HeaderHeight, Settings.TileSize, Settings.TileSize), Color.Orange);
        }

        // Draw Players
        foreach (var player in state.Players)
        {
            if (!player.IsAlive)
                continue;
            // player.Position is Vector2, probably in world coordinates
            Color pColor = new Color(player.Color.R, player.Color.G, player.Color.B);
            spriteBatch.Draw(circle, new Rectangle((int)player.Position.X, (int)player.Position.Y + HeaderHeight, Settings.PlayerRadius*2,Settings.PlayerRadius*2), pColor);
            
            // Draw name
            Vector2 nameSize = font.MeasureString(player.Name);
            spriteBatch.DrawString(font, player.Name, new Vector2(player.Position.X + (Settings.TileSize - nameSize.X) / 2, player.Position.Y + HeaderHeight - 20), Color.White);
        }
    }
}