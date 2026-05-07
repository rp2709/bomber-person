using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BomberPerson.Core.Scene;

public interface IScene
{
    public SceneManager SceneManager { get; }
    void LoadContent(ContentManager content, GraphicsDevice graphicsDevice, SceneManager sceneManager);
    void UnloadContent();
    void Update(GameTime gameTime);
    void Draw(SpriteBatch spriteBatch);
}