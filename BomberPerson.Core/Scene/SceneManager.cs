using System.Collections.Generic;
using BomberPerson.Core.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Input;

namespace BomberPerson.Core.Scene;

public enum EScene
{
    MainMenu,
    HostMenu,
    JoinMenu,
    LobbyMenu,
}

public class SceneManager(ContentManager content, GraphicsDevice graphicsDevice)
{
    private Scene currentScene;
    public Scene LoadScene(Scene scene)
    {
        currentScene?.UnloadContent();
        currentScene = scene;
        currentScene?.LoadContent(content, graphicsDevice);
        return currentScene;
    }

    public void Update(GameTime gameTime)
    {
        currentScene?.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        currentScene?.Draw(spriteBatch);
    }
}