using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
//using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace sandbx
{
    internal class Utilities
    {
        public class Simulation
        {
            public static float GetRandomPouringValue(SandbxElement element)
            {
                if (element.GetType() == typeof(LiquidElement))
                {
                    float minValue = ((LiquidElement)element).fluidPouringMin;
                    float maxValue = ((LiquidElement)element).fluidPouringMax;

                    float randomValue = Utilities.Mathematic.GetRandomFloat(minValue, maxValue);

                    return randomValue;
                }
                else
                {
                    return 0;
                }
            }

            public static float GetRandomPowderityValue(SandbxElement element)
            {
                if (element.GetType() == typeof(PowderElement))
                {
                    float minValue = ((PowderElement)element).powderityMin;
                    float maxValue = ((PowderElement)element).powderityMax;

                    float randomValue = Utilities.Mathematic.GetRandomFloat(minValue, maxValue);

                    return randomValue;
                }
                else
                {
                    return 0;
                }
            }

            public static Point? GetKeyByElement(SandbxElement element)
            {
                foreach (var kvp in Game1._activeElements)
                {
                    if (kvp.Value == element)
                    {
                        return kvp.Key;
                    }
                }

                return null;
            }

            public static void MoveElement(Point start, Point end) // to be tested
            {
                SandbxElement element = Game1._activeElements[start];
                Game1._activeElements.Remove(start);
                Game1._activeElements[end] = element;
            }

            public static void MoveElement(Point start, Point end, SandbxElement element)
            {
                Game1._activeElements.Remove(start);
                Game1._activeElements[end] = element;
            }

            public static void SwapElements(Point start, Point end)
            {
                SandbxElement startElement = Game1._activeElements[start];
                SandbxElement endElement = Game1._activeElements[end];

                Game1._activeElements.Remove(start);
                Game1._activeElements.Remove(end);
                Game1._activeElements[start] = endElement;
                Game1._activeElements[end] = startElement;
            }

            public static void SwapElements(Point start, Point end, SandbxElement startElement, SandbxElement endElement)
            {
                Game1._activeElements.Remove(start);
                Game1._activeElements.Remove(end);
                Game1._activeElements[start] = endElement;
                Game1._activeElements[end] = startElement;
            }

            public static bool IsPositionOutOfBounds(int x, int y, int Width, int Height)
            {
                // Check if x or y are outside the bounds of the grid
                if (x < 0 || x >= GameProperties.Width || y < 0 || y >= GameProperties.Height)
                {
                    return true; // Position is out of bounds
                }
                return false; // Position is within bounds
            }

            public static bool IsPositionOutOfBounds(Point position, int Width, int Height)
            {
                // Check if x or y are outside the bounds of the grid
                if (position.X < 0 || position.X >= GameProperties.Width || position.Y < 0 || position.Y >= GameProperties.Height)
                {
                    return true; // Position is out of bounds
                }
                return false; // Position is within bounds
            }

            public static bool IsPositionOutOfCanvasBounds(Point position)
            {
                // Check if x or y are outside the bounds of the grid
                if (position.X * GameProperties.CellSize < 0 || 
                    position.X * GameProperties.CellSize >= GameProperties.Width || 
                    position.Y * GameProperties.CellSize < 0 || 
                    position.Y * GameProperties.CellSize >= GameProperties.Height)
                {
                    return true; // Position is out of bounds
                }
                return false; // Position is within bounds
            }

            public static bool IsPositionOccupied(Point destination, bool ignoreSelf = false, Point? selfPosition = null)
            {
                if (ignoreSelf && destination == selfPosition)
                {
                    return false;
                }
                else
                {
                    return Game1._activeElements.ContainsKey(destination);
                }
            }

            public static bool GetElementInPosition(Point destination, out SandbxElement blockingElement, bool ignoreSelf = false, Point? selfPosition = null)
            {
                if (ignoreSelf && destination == selfPosition)
                {
                    blockingElement = null;
                    return false;
                }
                else
                {
                    if (Game1._activeElements.ContainsKey(destination))
                    {
                        blockingElement = Game1._activeElements[destination];
                        return true;
                    }
                    else
                    {
                        blockingElement = null;
                        return false;
                    }
                }
            }

            public static Point GetLastAvaiablePosition(Point start, Point end)
            {
                Point lastPosition = start;

                if (start.X == end.X)
                {
                    for (int y = Math.Min(start.Y, end.Y); y <= Math.Max(start.Y, end.Y); y++)
                    {
                        Point currentPosition = new Point(start.X, y);
                        if (IsPositionOccupied(currentPosition, true, start) || IsPositionOutOfBounds(currentPosition.X * GameProperties.CellSize, currentPosition.Y * GameProperties.CellSize, GameProperties.Width, GameProperties.Height))
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
                        if (IsPositionOccupied(currentPosition, true, start) || IsPositionOutOfBounds(currentPosition.X * GameProperties.CellSize, currentPosition.Y * GameProperties.CellSize, GameProperties.Width, GameProperties.Height))
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
                        if (IsPositionOccupied(currentPosition, true, start) || IsPositionOutOfBounds(currentPosition.X * GameProperties.CellSize, currentPosition.Y * GameProperties.CellSize, GameProperties.Width, GameProperties.Height))
                        {
                            return lastPosition;
                        }
                        lastPosition = currentPosition;
                        currentY -= slope;
                    }
                }

                return lastPosition;
            }

            public static Point GetCollidingElementInLine(Point start, Point end, out SandbxElement blockingElement)
            {
                Point lastPosition = start;

                if (start.X == end.X)
                {
                    for (int y = Math.Min(start.Y, end.Y); y <= Math.Max(start.Y, end.Y); y++)
                    {
                        Point currentPosition = new Point(start.X, y);
                        if (IsPositionOccupied(currentPosition, true, start) || IsPositionOutOfBounds(currentPosition.X * GameProperties.CellSize, currentPosition.Y * GameProperties.CellSize, GameProperties.Width, GameProperties.Height))
                        {
                            if (!IsPositionOutOfBounds(currentPosition.X * GameProperties.CellSize, currentPosition.Y * GameProperties.CellSize, GameProperties.Width, GameProperties.Height))
                            {
                                blockingElement = Game1._activeElements[currentPosition];
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
                        if (IsPositionOccupied(currentPosition, true, start) || IsPositionOutOfBounds(currentPosition.X * GameProperties.CellSize, currentPosition.Y * GameProperties.CellSize, GameProperties.Width, GameProperties.Height))
                        {
                            if (!IsPositionOutOfBounds(currentPosition.X * GameProperties.CellSize, currentPosition.Y * GameProperties.CellSize, GameProperties.Width, GameProperties.Height))
                            {
                                blockingElement = Game1._activeElements[currentPosition];
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
                        if (IsPositionOccupied(currentPosition, true, start) || IsPositionOutOfBounds(currentPosition.X * GameProperties.CellSize, currentPosition.Y * GameProperties.CellSize, GameProperties.Width, GameProperties.Height))
                        {
                            if (!IsPositionOutOfBounds(currentPosition.X * GameProperties.CellSize, currentPosition.Y * GameProperties.CellSize, GameProperties.Width, GameProperties.Height))
                            {
                                blockingElement = Game1._activeElements[currentPosition];
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
        }

        public class Mathematic
        {
            public static float GetRandomFloat(float minValue, float maxValue)
            {
                Random random = new Random();
                double range = maxValue - minValue;
                double sample = random.NextDouble(); // Returns a random number between 0.0 and 1.0
                return (float)(minValue + sample * range);
            }

            public int CalculateDifference(int number1, int number2)
            {
                int difference = Math.Abs(number1 - number2);
                return difference;
            }
        }

        public class System
        {
            public static bool IsMouseOutOfBounds(MouseState mouseState)
            {
                return mouseState.X < 0 || mouseState.Y < 0 ||
                       mouseState.X > GameProperties.Width ||
                       mouseState.Y > GameProperties.Height;
            }
        }
    }
}
