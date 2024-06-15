using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace sandbx
{
    public class GameProperties
    {
        // Simulation parameters
        public static int Width = 800;
        public static int Height = 600;
        public static int CellSize = 5; // Size of each cell in pixels

        public static float canvasGravity = 0.1f;
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont mainFont;

        private Texture2D _sandTexture;
        public static Dictionary<Point, SandbxElement> _activeElements;
        private Random random;

        private List<SandbxElement> availableElements = new List<SandbxElement>()
        {
            new PowderElement { baseColor = Color.Yellow, idName = "sand-powder", displayName = "Sand", },
            new LiquidElement { baseColor = Color.DodgerBlue, idName = "water", displayName = "Water", },
            new PowderElement { baseColor = Color.DarkGray, idName = "metal-scraps", displayName = "Metal Scraps", yVelocity = 3f, powderityMin = 0.5f, powderityMax = 3f, },
        };

        private int currentElementIndex = 0;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = GameProperties.Width;
            _graphics.PreferredBackBufferHeight = GameProperties.Height;
        }

        protected override void Initialize()
        {
            base.Initialize();

            random = new Random();
            // Initialize the elements array
            InitializeElements();
            SimulationLogic.InitSimulation();

            _sandTexture = new Texture2D(GraphicsDevice, GameProperties.Width / GameProperties.CellSize, GameProperties.Height / GameProperties.CellSize);
        }

        private void InitializeElements()
        {
            // Example: Initializing elements array with default sand properties
            _activeElements = new Dictionary<Point, SandbxElement>();

            // Example: Adding initial sand elements
            //_activeElements.Add(new Point(Width / 2 / CellSize, Height / 2 / CellSize), new SandbxElement { BaseColor = Color.Yellow });

        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            mainFont = Content.Load<SpriteFont>("font");
        }

        // used for pausing input
        private KeyboardState previousKeyboardState;

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // User Interaction
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed && !Utilities.System.IsMouseOutOfBounds(mouseState))
            {
                int mouseX = mouseState.X / GameProperties.CellSize;
                int mouseY = mouseState.Y / GameProperties.CellSize;

                Point position = new Point(mouseX, mouseY);
                if (!_activeElements.ContainsKey(position))
                {
                    _activeElements[position] = availableElements[currentElementIndex].Clone();
                    //_activeElements[position] = new SandbxElement { baseColor = Color.Yellow, idName = "sand-powder" }; // Add new element
                    //_activeElements[position] = new SandbxElement { baseColor = Color.DodgerBlue, idName = "water" }; // Add new element
                }
            }

            KeyboardState currentKeyboardState = Keyboard.GetState();

            // Check if the Right arrow key was just pressed
            if (currentKeyboardState.IsKeyDown(Keys.Right) && !previousKeyboardState.IsKeyDown(Keys.Right))
            {
                currentElementIndex++;
                if (currentElementIndex >= availableElements.Count)
                {
                    currentElementIndex = 0; // Wrap around to the first element
                }
            }

            // Check if the Left arrow key was just pressed
            if (currentKeyboardState.IsKeyDown(Keys.Left) && !previousKeyboardState.IsKeyDown(Keys.Left))
            {
                currentElementIndex--;
                if (currentElementIndex < 0)
                {
                    currentElementIndex = availableElements.Count - 1; // Wrap around to the last element
                }
            }


            // Store the current state for the next frame
            previousKeyboardState = currentKeyboardState;

            // Sand update logic here
            UpdateSand();

            base.Update(gameTime);
        }

        private void UpdateSand()
        {
            var keys = _activeElements.Keys.ToList(); // Avoid modifying collection while iterating

            foreach (var key in keys)
            {
                var position = key;
                var element = _activeElements[position];

                SimulationLogic.UpdateElement(position, element);
            }

            // Update _sandTexture based on _activeElements
            UpdateSandTexture();
        }

        

        private void UpdateSandTexture()
        {
            Color[] colors = new Color[GameProperties.Width / GameProperties.CellSize * GameProperties.Height / GameProperties.CellSize];

            foreach (var kvp in _activeElements)
            {
                var position = kvp.Key;
                var element = kvp.Value;

                int index = position.X + position.Y * (GameProperties.Width / GameProperties.CellSize);

                if (index >= 0 && index < colors.Length)
                {
                    if (element != null)
                    {
                        colors[index] = element.baseColor;
                    }
                    else
                    {
                        colors[index] = Color.Black;
                    }
                }
            }

            _sandTexture.SetData(colors);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            _spriteBatch.Begin();
            _spriteBatch.Draw(_sandTexture, new Rectangle(0, 0, GameProperties.Width, GameProperties.Height), Color.White);
            _spriteBatch.DrawString(mainFont, availableElements[currentElementIndex].displayName, new Vector2(10, 10), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }


        
    }
}
