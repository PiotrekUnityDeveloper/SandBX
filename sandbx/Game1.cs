using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace sandbx
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        // Simulation parameters
        private const int Width = 800;
        private const int Height = 600;
        private const int CellSize = 5; // Size of each cell in pixels

        private const float canvasGravity = 0.1f;

        private Texture2D _sandTexture;
        private Dictionary<Point, SandbxElement> _activeElements;
        private Random random;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _graphics.PreferredBackBufferWidth = Width;
            _graphics.PreferredBackBufferHeight = Height;
        }

        protected override void Initialize()
        {
            base.Initialize();

            random = new Random();
            // Initialize the elements array
            InitializeElements();

            _sandTexture = new Texture2D(GraphicsDevice, Width / CellSize, Height / CellSize);
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
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // User Interaction
            MouseState mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                int mouseX = mouseState.X / CellSize;
                int mouseY = mouseState.Y / CellSize;

                Point position = new Point(mouseX, mouseY);
                if (!_activeElements.ContainsKey(position))
                {
                    _activeElements[position] = new SandbxElement { baseColor = Color.Yellow, idName = "sand-powder" }; // Add new element
                    //_activeElements[position] = new SandbxElement { baseColor = Color.DodgerBlue, idName = "water" }; // Add new element
                }
            }

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

                if (element.idName == "sand-powder")
                {
                    float lastYvelocity = element.yVelocity;

                    // Apply gravity if falling
                    if (element.isFalling)
                    {
                        element.yVelocity += canvasGravity; // Add a fraction of gravity each frame
                    }
                    else
                    {
                        element.xVelocity *= element.friction;
                    }

                    bool wasFalling = element.isFalling; // Capture the current falling state

                    // Check if the element will collide in the next frame or is out of bounds
                    if (isColliding(new Point(position.X, position.Y + 1)) || IsOutOfBounds(position.X * CellSize, (position.Y + 1) * CellSize, Width, Height))
                    {
                        element.isFalling = false;
                    }
                    else
                    {
                        element.isFalling = true;
                    }

                    // Calculate new position based on velocity
                    Point newPosition = new Point(
                        position.X + (int)Math.Round(element.xVelocity),
                        position.Y + (int)Math.Round(element.yVelocity)
                    );

                    // Ensure the new position is not out of bounds or colliding
                    if (!IsOutOfBounds(newPosition.X * CellSize, newPosition.Y * CellSize, Width, Height) &&
                        !isColliding(newPosition, true, position))
                    {
                        Point availablePosition = isCollidingInLine(position, newPosition);

                        if (element.isFalling)
                        {
                            _activeElements.Remove(position);
                            _activeElements[availablePosition] = element;
                        }
                    }
                    else
                    {
                        // Reset velocities if collision detected
                        element.xVelocity = 0;
                        element.yVelocity = 0;
                    }

                    int rand = random.Next(0, 2);

                    if (!element.isFalling)
                    {
                        Point newHorizontalPosition;
                        Point horizontalBelow;
                        if (rand == 0)
                        {
                            newHorizontalPosition = new Point(position.X - 1, position.Y);
                            horizontalBelow = new Point(position.X - 1, position.Y + 1);
                        }
                        else
                        {
                            newHorizontalPosition = new Point(position.X + 1, position.Y);
                            horizontalBelow = new Point(position.X + 1, position.Y + 1);
                        }

                        // Ensure the new position is not out of bounds or colliding
                        if (!IsOutOfBounds(newHorizontalPosition.X * CellSize, newHorizontalPosition.Y * CellSize, Width, Height) &&
                            !isColliding(newHorizontalPosition) &&
                            !IsOutOfBounds(horizontalBelow.X * CellSize, horizontalBelow.Y * CellSize, Width, Height) &&
                            !isColliding(horizontalBelow))
                        {
                            _activeElements.Remove(position);
                            _activeElements[newHorizontalPosition] = element;
                        }
                    }

                    // Handle the case where the element was falling but is now not falling
                    if (!element.isFalling && wasFalling && Math.Abs(lastYvelocity) >= 0.5f)
                    {
                        Console.WriteLine("boom");
                        // Randomly move left or right a couple of cells
                        if (rand == 0)
                        {
                            element.xVelocity += element.bounciness;
                        }
                        else
                        {
                            element.xVelocity -= element.bounciness;
                        }
                    }
                }
                else if (element.idName == "water")
                {
                    // Water specific logic here
                }
            }

            // Update _sandTexture based on _activeElements
            UpdateSandTexture();
        }



        private void UpdateSandTexture()
        {
            Color[] colors = new Color[Width / CellSize * Height / CellSize];

            foreach (var kvp in _activeElements)
            {
                var position = kvp.Key;
                var element = kvp.Value;

                int index = position.X + position.Y * (Width / CellSize);

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
            _spriteBatch.Draw(_sandTexture, new Rectangle(0, 0, Width, Height), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }


        // util
        public bool IsOutOfBounds(int x, int y, int width, int height)
        {
            // Check if x or y are outside the bounds of the grid
            if (x < 0 || x >= width || y < 0 || y >= height)
            {
                return true; // Position is out of bounds
            }
            return false; // Position is within bounds
        }

        public bool isColliding(Point destination, bool ignoreSelf = false, Point? selfPosition = null)
        {
            if (ignoreSelf && destination == selfPosition)
            {
                return false;
            }
            else
            {
                return _activeElements.ContainsKey(destination);
            }
        }

        public Point isCollidingInLine(Point start, Point end)
        {
            Point lastPosition = start;

            if (start.X == end.X)
            {
                for (int y = Math.Min(start.Y, end.Y); y <= Math.Max(start.Y, end.Y); y++)
                {
                    Point currentPosition = new Point(start.X, y);
                    if (isColliding(currentPosition, true, start) || IsOutOfBounds(currentPosition.X * CellSize, currentPosition.Y * CellSize, Width, Height))
                    {
                        return lastPosition;
                    }
                    lastPosition = currentPosition;
                }
                return lastPosition;
            }

            double slope = (double)(end.Y - start.Y) / (end.X - start.X);
            double currentY = start.Y;

            if (start.X < end.X)
            {
                for (int x = start.X; x <= end.X; x++)
                {
                    int roundedY = (int)Math.Round(currentY);
                    Point currentPosition = new Point(x, roundedY);
                    if (isColliding(currentPosition, true, start) || IsOutOfBounds(currentPosition.X * CellSize, currentPosition.Y * CellSize, Width, Height))
                    {
                        return lastPosition;
                    }
                    lastPosition = currentPosition;
                    currentY += slope;
                }
            }
            else
            {
                for (int x = start.X; x >= end.X; x--)
                {
                    int roundedY = (int)Math.Round(currentY);
                    Point currentPosition = new Point(x, roundedY);
                    if (isColliding(currentPosition, true, start) || IsOutOfBounds(currentPosition.X * CellSize, currentPosition.Y * CellSize, Width, Height))
                    {
                        return lastPosition;
                    }
                    lastPosition = currentPosition;
                    currentY -= slope;
                }
            }

            return lastPosition;
        }




        public int CalculateDifference(int number1, int number2)
        {
            int difference = Math.Abs(number1 - number2);
            return difference;
        }





    }
}
