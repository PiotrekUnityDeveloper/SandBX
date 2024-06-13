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
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private SpriteFont mainFont;

        // Simulation parameters
        private const int Width = 800;
        private const int Height = 600;
        private const int CellSize = 5; // Size of each cell in pixels

        private const float canvasGravity = 0.1f;

        private Texture2D _sandTexture;
        private Dictionary<Point, SandbxElement> _activeElements;
        private Random random;

        private List<SandbxElement> availableElements = new List<SandbxElement>()
        {
            new PowderElement { baseColor = Color.Yellow, idName = "sand-powder", displayName = "Sand" },
            new LiquidElement { baseColor = Color.DodgerBlue, idName = "water", displayName = "Water" },
        };

        private int currentElementIndex = 0;

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
            if (mouseState.LeftButton == ButtonState.Pressed && !IsMouseOutOfBounds(mouseState))
            {
                int mouseX = mouseState.X / CellSize;
                int mouseY = mouseState.Y / CellSize;

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

                    SandbxElement elem;
                    // Check if the element will collide in the next frame or is out of bounds
                    if (isColliding(new Point(position.X, position.Y + 1), out elem) || IsOutOfBounds(position.X * CellSize, (position.Y + 1) * CellSize, Width, Height))
                    {
                        if(elem != null && elem.elementType == ElementType.Liquid)
                        {
                            SwapElements(position, new Point(position.X, position.Y + 1));
                            elem.isFalling = true;
                            elem.xVelocity = 0;
                            elem.yVelocity = 0;
                        }
                        else
                        {
                            element.isFalling = false;
                        }
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
                        SandbxElement blockingelem;
                        Point availablePosition = isCollidingInLine(position, newPosition, out blockingelem);

                        if (element.isFalling)
                        {
                            if(blockingelem != null && blockingelem.elementType == ElementType.Liquid)
                            {
                                /*
                                blockingelem.RecieveVelocity(element.xVelocity * blockingelem.energyConsumption * - 1, element.yVelocity * blockingelem.energyConsumption * -1);
                                Point? p = GetKeyByElement(blockingelem);
                                if(p != null)
                                {
                                    SwapElements(position, p.Value);
                                }*/
                            }
                            else
                            {
                                MoveElement(position, availablePosition);
                            }
                        }
                    }
                    else
                    {

                        SandbxElement blockingelem;
                        Point availablePosition = isCollidingInLine(position, newPosition, out blockingelem);

                        if (element.isFalling)
                        {
                            if (blockingelem != null && blockingelem.elementType == ElementType.Liquid)
                            {
                                /*
                                blockingelem.RecieveVelocity(element.xVelocity * blockingelem.energyConsumption * -1, element.yVelocity * blockingelem.energyConsumption * -1);
                                Point? p = GetKeyByElement(blockingelem);
                                if (p != null)
                                {
                                    SwapElements(position, p.Value);
                                }*/
                            }
                            else
                            {
                                MoveElement(position, availablePosition);
                            }
                        }

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
                            MoveElement(position,newHorizontalPosition);
                        }

                        /*
                        if (!IsOutOfBounds(position.X * CellSize, (position.Y + 1) * CellSize, Width, Height))
                        {
                            SandbxElement sandbxelem;
                            if (isColliding(new Point(position.X, position.Y + 1), out sandbxelem))
                            {
                                if(sandbxelem.elementType == ElementType.Liquid)
                                {
                                    SwapElements(position, new Point(position.X, position.Y + 1), element, sandbxelem);
                                    //sandbxelem.RecieveVelocity(element.xVelocity * sandbxelem.energyConsumption, lastYvelocity * sandbxelem.energyConsumption * -1);
                                }
                            }
                        }*/
                    }

                    // Handle the case where the element was falling but is now not falling
                    if (!element.isFalling && wasFalling && Math.Abs(lastYvelocity) >= 1.5f)
                    {
                        //Console.WriteLine("boom");
                        // Randomly move left or right a couple of cells
                        if (rand == 0)
                        {
                            element.xVelocity += element.bounciness * (float)random.NextDouble();

                            /*
                            if (!isColliding(new Point(position.X + 1, position.Y)))
                            {
                                element.xVelocity += element.bounciness * (float)random.NextDouble();
                            }
                            else
                            {
                                element.xVelocity -= element.bounciness * (float)random.NextDouble();
                            }*/
                        }
                        else
                        {
                            element.xVelocity -= element.bounciness * (float)random.NextDouble();

                            /*
                            if (!isColliding(new Point(position.X - 1, position.Y)))
                            {
                                element.xVelocity -= element.bounciness * (float)random.NextDouble();
                            }
                            else
                            {
                                element.xVelocity += element.bounciness * (float)random.NextDouble();
                            }*/
                        }
                    }
                }
                else if (element.idName == "water")
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
                    if (!isColliding(new Point(position.X, position.Y + 1)) && !IsOutOfBounds(position.X * CellSize, (position.Y + 1) * CellSize, Width, Height))
                    {
                        element.isFalling = true;
                    }
                    else
                    {
                        element.isFalling = false;
                    }

                    // Calculate new position based on velocity
                    Point newPosition = new Point(
                        position.X + (int)Math.Round(element.xVelocity),
                        position.Y + (int)Math.Round(element.yVelocity)
                    );

                    SandbxElement blocker;

                    // Ensure the new position is not out of bounds or colliding
                    if (!IsOutOfBounds(newPosition.X * CellSize, newPosition.Y * CellSize, Width, Height) &&
                        !isColliding(newPosition, true, position))
                    {
                        Point availablePosition = isCollidingInLine(position, newPosition);

                        if (element.isFalling)
                        {
                            MoveElement(position, availablePosition);
                        }
                    }
                    else
                    {
                        Point availablePosition = isCollidingInLine(position, newPosition, out blocker);

                        if (element.isFalling)
                        {
                            MoveElement(position, availablePosition);
                        }

                        if(availablePosition != newPosition && blocker != null)
                        {
                            blocker.RecieveVelocity(element.xVelocity, element.yVelocity);
                        }

                        element.xVelocity = (element.xVelocity * -1) / element.energyConsumption;
                        //element.yVelocity = (element.yVelocity * -1) / element.energyConsumption;

                        // Reset velocities if collision detected
                    }

                    int rand = random.Next(0, 2);

                    if (!element.isFalling)
                    {
                        Point newHorizontalPosition;

                        if (rand == 0)
                        {
                            newHorizontalPosition = new Point(position.X - 1, position.Y);
                        }
                        else
                        {
                            newHorizontalPosition = new Point(position.X + 1, position.Y);
                        }

                        // Ensure the new position is not out of bounds or colliding
                        if (!IsOutOfBounds(newHorizontalPosition.X * CellSize, newHorizontalPosition.Y * CellSize, Width, Height) &&
                            !isColliding(newHorizontalPosition))
                        {
                            MoveElement(position, newHorizontalPosition);
                        }
                    }

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
            _spriteBatch.DrawString(mainFont, availableElements[currentElementIndex].displayName, new Vector2(10, 10), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }


        // util
        public Point? GetKeyByElement(SandbxElement element)
        {
            foreach (var kvp in _activeElements)
            {
                if (kvp.Value == element)
                {
                    return kvp.Key;
                }
            }

            return null;
        }

        public void MoveElement(Point start, Point end) // to be tested
        {
            SandbxElement element = _activeElements[start];
            _activeElements.Remove(start);
            _activeElements[end] = element;
        }

        public void MoveElement(Point start, Point end, SandbxElement element)
        {
            _activeElements.Remove(start);
            _activeElements[end] = element;
        }

        public void SwapElements(Point start, Point end)
        {
            SandbxElement startElement = _activeElements[start];
            SandbxElement endElement = _activeElements[end];

            _activeElements.Remove(start);
            _activeElements.Remove(end);
            _activeElements[start] = endElement;
            _activeElements[end] = startElement;
        }

        public void SwapElements(Point start, Point end, SandbxElement startElement, SandbxElement endElement)
        {
            _activeElements.Remove(start);
            _activeElements.Remove(end);
            _activeElements[start] = endElement;
            _activeElements[end] = startElement;
        }

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

        public bool isColliding(Point destination, out SandbxElement blockingElement, bool ignoreSelf = false, Point? selfPosition = null)
        {
            if (ignoreSelf && destination == selfPosition)
            {
                blockingElement = null;
                return false;
            }
            else
            {
                if (_activeElements.ContainsKey(destination))
                {
                    blockingElement = _activeElements[destination];
                    return true;
                }
                else
                {
                    blockingElement = null;
                    return false;
                }
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

        public Point isCollidingInLine(Point start, Point end, out SandbxElement blockingElement)
        {
            Point lastPosition = start;

            if (start.X == end.X)
            {
                for (int y = Math.Min(start.Y, end.Y); y <= Math.Max(start.Y, end.Y); y++)
                {
                    Point currentPosition = new Point(start.X, y);
                    if (isColliding(currentPosition, true, start) || IsOutOfBounds(currentPosition.X * CellSize, currentPosition.Y * CellSize, Width, Height))
                    {
                        if(!IsOutOfBounds(currentPosition.X * CellSize, currentPosition.Y * CellSize, Width, Height))
                        {
                            blockingElement = _activeElements[currentPosition];
                        }
                        else
                        {
                            blockingElement = null;
                        }
                        return lastPosition;
                    }
                    lastPosition = currentPosition;
                }
                blockingElement = null;
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
                        if (!IsOutOfBounds(currentPosition.X * CellSize, currentPosition.Y * CellSize, Width, Height))
                        {
                            blockingElement = _activeElements[currentPosition];
                        }
                        else
                        {
                            blockingElement = null;
                        }
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
                        if (!IsOutOfBounds(currentPosition.X * CellSize, currentPosition.Y * CellSize, Width, Height))
                        {
                            blockingElement = _activeElements[currentPosition];
                        }
                        else
                        {
                            blockingElement = null;
                        }
                        return lastPosition;
                    }
                    lastPosition = currentPosition;
                    currentY -= slope;
                }
            }

            blockingElement = null;
            return lastPosition;
        }

        private bool IsMouseOutOfBounds(MouseState mouseState)
        {
            return mouseState.X < 0 || mouseState.Y < 0 ||
                   mouseState.X > GraphicsDevice.Viewport.Width ||
                   mouseState.Y > GraphicsDevice.Viewport.Height;
        }

        public int CalculateDifference(int number1, int number2)
        {
            int difference = Math.Abs(number1 - number2);
            return difference;
        }




    }
}
