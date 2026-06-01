using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input;

namespace BomberPerson.Core.UI;

public class TextField
{
    private readonly string label;
    private readonly Rectangle bounds;
    private readonly SpriteFont font;
    private readonly bool isPassword;
    private readonly int maxLength;

    private string value        = "";
    private bool isFocused    = false;
    private bool wasClicked   = false;

    private KeyboardState prevKeyboard;

    // Curseur clignotant
    private float cursorTimer = 0f;
    private bool  cursorVisible = true;

    private readonly Color colorBackground  = new Color(30, 30, 30);
    private readonly Color colorFocused     = new Color(40, 40, 80);
    private readonly Color colorBorder      = new Color(120, 120, 120);
    private readonly Color colorBorderFocus = new Color(100, 100, 220);
    private readonly Color colorLabel       = new Color(180, 180, 180);
    private readonly Color colorText        = Color.White;
    private readonly Color colorPlaceholder = new Color(80, 80, 80);

    public string Value => value;
    public void SetValue(string newValue) => value = newValue;

    public TextField(string label, Rectangle bounds, SpriteFont font, bool isPassword = false, int maxLength = 32)
    {
        this.label      = label;
        this.bounds     = bounds;
        this.font       = font;
        this.isPassword = isPassword;
        this.maxLength  = maxLength;
    }

    public void Update(GameTime gameTime, KeyboardState keyboard, MouseState mouse)
    {
        // Focus au clic
        Point mousePos  = new Point(mouse.X, mouse.Y);
        bool isClicked = mouse.LeftButton == ButtonState.Pressed;

        if (isClicked && !wasClicked) isFocused = bounds.Contains(mousePos);

        wasClicked = isClicked;
        
        // Curseur clignotant
        if (isFocused)
        {
            cursorTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (cursorTimer >= 0.5f)
            {
                cursorVisible = !cursorVisible;
                cursorTimer   = 0f;
            }
        }
        else
        {
            cursorVisible = false;
        }

        // Saisie clavier
        if (isFocused)
        {
            Keys[] pressedKeys = keyboard.GetPressedKeys();

            foreach (Keys key in pressedKeys)
            {
                // Ignorer les touches déjà enfoncées
                if (prevKeyboard.IsKeyDown(key)) continue;

                if (key == Keys.Back && value.Length > 0)
                {
                    value = value[..^1];
                    continue;
                }

                char? c = KeyToChar(key, keyboard);
                if (c.HasValue && value.Length < maxLength) value += c.Value;
            }
        }

        prevKeyboard = keyboard;
    }

    public void Draw(SpriteBatch spriteBatch, Texture2D pixel)
    {
        // Label au dessus du champ
        Vector2 labelPos = new Vector2(bounds.X, bounds.Y - 36);
        spriteBatch.DrawString(font, label, labelPos, colorLabel);

        // Fond
        spriteBatch.Draw(pixel, bounds, isFocused ? colorFocused : colorBackground);

        // Bordure
        Color borderColor = isFocused ? colorBorderFocus : colorBorder;
        int b = 2;
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, bounds.Width, b), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Bottom - b, bounds.Width, b), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(bounds.X, bounds.Y, b, bounds.Height), borderColor);
        spriteBatch.Draw(pixel, new Rectangle(bounds.Right - b, bounds.Y, b, bounds.Height), borderColor);

        // Texte ou placeholder
        string displayText = value.Length > 0
            ? (isPassword ? new string('*', value.Length) : value)
            : "";

        Color textColor = value.Length > 0 ? colorText : colorPlaceholder;
        Vector2 textPos   = new Vector2(bounds.X + 10, bounds.Y + (bounds.Height - font.LineSpacing) / 2f);

        if (value.Length > 0)
            spriteBatch.DrawString(font, displayText, textPos, textColor);

        // Curseur clignotant
        if (cursorVisible)
        {
            float textWidth = value.Length > 0 ? font.MeasureString(displayText).X : 0;
            int cursorX = (int)(textPos.X + textWidth + 2);
            Rectangle cursorRect = new Rectangle(cursorX, (int)textPos.Y, 2, font.LineSpacing);
            spriteBatch.Draw(pixel, cursorRect, Color.White);
        }
    }

    // -------------------------
    // CONVERSION TOUCHE → CHAR
    // -------------------------
    private static char? KeyToChar(Keys key, KeyboardState keyboard)
    {
        bool shift = keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift);
        return key switch
        {
            Keys.A => shift ? 'A' : 'a',
            Keys.B => shift ? 'B' : 'b',
            Keys.C => shift ? 'C' : 'c',
            Keys.D => shift ? 'D' : 'd',
            Keys.E => shift ? 'E' : 'e',
            Keys.F => shift ? 'F' : 'f',
            Keys.G => shift ? 'G' : 'g',
            Keys.H => shift ? 'H' : 'h',
            Keys.I => shift ? 'I' : 'i',
            Keys.J => shift ? 'J' : 'j',
            Keys.K => shift ? 'K' : 'k',
            Keys.L => shift ? 'L' : 'l',
            Keys.M => shift ? 'M' : 'm',
            Keys.N => shift ? 'N' : 'n',
            Keys.O => shift ? 'O' : 'o',
            Keys.P => shift ? 'P' : 'p',
            Keys.Q => shift ? 'Q' : 'q',
            Keys.R => shift ? 'R' : 'r',
            Keys.S => shift ? 'S' : 's',
            Keys.T => shift ? 'T' : 't',
            Keys.U => shift ? 'U' : 'u',
            Keys.V => shift ? 'V' : 'v',
            Keys.W => shift ? 'W' : 'w',
            Keys.X => shift ? 'X' : 'x',
            Keys.Y => shift ? 'Y' : 'y',
            Keys.Z => shift ? 'Z' : 'z',
            Keys.D0 or Keys.NumPad0 => '0',
            Keys.D1 or Keys.NumPad1 => '1',
            Keys.D2 or Keys.NumPad2 => '2',
            Keys.D3 or Keys.NumPad3 => '3',
            Keys.D4 or Keys.NumPad4 => '4',
            Keys.D5 or Keys.NumPad5 => '5',
            Keys.D6 or Keys.NumPad6 => '6',
            Keys.D7 or Keys.NumPad7 => '7',
            Keys.D8 or Keys.NumPad8 => '8',
            Keys.D9 or Keys.NumPad9 => '9',
            Keys.Space              => ' ',
            Keys.OemMinus           => shift ? '_' : '-',
            Keys.OemComma           => '.',
            _                       => null
        };
    }
}