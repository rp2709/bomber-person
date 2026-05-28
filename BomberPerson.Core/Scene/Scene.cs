using BomberPerson.Core.Client;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BomberPerson.Core.Scene;

public abstract class Scene(ClientStateController clientStateController)
{
    protected ClientStateController ClientStateController { get; } = clientStateController;
    public abstract void LoadContent(ContentManager content, GraphicsDevice graphicsDevice);
    public abstract void UnloadContent();
    public abstract void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch);
}