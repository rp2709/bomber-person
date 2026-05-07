using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace BomberPerson.Core.UI;

public class Button
{
    private string label;
    private readonly Rectangle bounds;
    private readonly SpriteFont font;

    private bool wasPressed = false;

    // Couleurs
    private readonly Color colorNormal   = new Color(60, 60, 60);
    private readonly Color colorHover    = new Color(100, 100, 200);
    private readonly Color colorPressed  = new Color(50, 50, 150);
    private readonly Color colorText     = Color.White;

    private Color currentColor;

    public event Action OnClick;
    
    public Rectangle Bounds => bounds;
    public void SetLabel(string newLabel) => label = newLabel;

    public Button(string label, Rectangle bounds, SpriteFont font)
    {
        this.label        = label;
        this.bounds       = bounds;
        this.font         = font;
        currentColor = colorNormal;
    }

    public void Update(GameTime gameTime)
    {
        MouseStateExtended mouse = MouseExtended.GetState();
        
        //MouseState mouse = Mouse.GetState();
        Point mousePos = new Point(mouse.X, mouse.Y);
        bool isHover = bounds.Contains(mousePos);
        //bool isDown = mouse.LeftButton == ButtonState.Pressed;
        bool isDown = mouse.WasButtonPressed(MouseButton.Left);
        if (isHover && isDown)
        {
            currentColor = colorPressed;
            wasPressed   = true;
        }
        else if (isHover)
        {
            currentColor = colorHover;
            // Click = relâchement après avoir été pressé sur le bouton
            if (wasPressed)
            {
                OnClick?.Invoke();
            }
            wasPressed = false;
        }
        else
        {
            currentColor = colorNormal;
            wasPressed   = false;
        }
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        // Fond du bouton
        spriteBatch.Draw(pixel, bounds, currentColor);

        // Bordure simple
        int border = 2;
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, border), Color.White);
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - border, bounds.Width, border), Color.White);
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, border, bounds.Height), Color.White);
        spriteBatch.Draw(pixel, new Rectangle(bounds.Right - border, bounds.Y, border, bounds.Height), Color.White);

        // Texte centré
        Vector2 textSize = font.MeasureString(label);
        Vector2 textPos  = new ( 
            bounds.X + (bounds.Width  - textSize.X) / 2f,
            bounds.Y + (bounds.Height - textSize.Y) / 2f
        );
        spriteBatch.DrawString(font, label, textPos, colorText);
    }
}