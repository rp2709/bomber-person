using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace BomberPerson.Core.UI;

public class UIManager
{
    private static UIManager instance;
    public static UIManager Instance => instance;
    
    public SpriteFont FontTitle { get; private set; }
    public SpriteFont FontButton { get; private set; }

    public UIManager(ContentManager content)
    {
        instance = this;
        FontTitle = content.Load<SpriteFont>("Fonts/TitleFont");
        FontButton = content.Load<SpriteFont>("Fonts/ButtonFont");
    }
}