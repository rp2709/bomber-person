using System.Collections.Generic;
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

public class SceneManager
{
    private readonly ContentManager content;
    private readonly GraphicsDevice graphicsDevice;

    private Dictionary<EScene, IScene> scenes = new Dictionary<EScene, IScene>()
    {
        { EScene.MainMenu, new MainMenuScene() },
        { EScene.HostMenu, new HostGameScene() },
        { EScene.LobbyMenu, new LobbyScene() },
    };
    
    private IScene currentScene;

    public SceneManager(ContentManager content, GraphicsDevice graphicsDevice, EScene defaultScene)
    {
        this.content        = content;
        this.graphicsDevice = graphicsDevice;
        LoadScene(defaultScene);
    }

    public IScene LoadScene(EScene scene)
    {
        if (!scenes.TryGetValue(scene, out IScene sceneObject))  return null;
        currentScene?.UnloadContent();
        currentScene = sceneObject;
        currentScene?.LoadContent(content, graphicsDevice, this);
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