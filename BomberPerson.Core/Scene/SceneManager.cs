using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BomberPerson.Core.Scene;
public class SceneManager(ContentManager content, GraphicsDevice graphicsDevice)
{
    private Scene currentScene;
    public Scene LoadScene(Scene scene)
    {
        Scene previousScene = currentScene;
        
        scene.LoadContent(content, graphicsDevice);
        currentScene = scene;
        previousScene?.UnloadContent();
        
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