using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Point = Microsoft.Xna.Framework.Point;

namespace sandbx
{
    internal class SimulationLogic
    {
        private static Random random;

        SimulationLogic() { random = new Random(); }

        public static void InitSimulation()
        {
            random = new Random();
        }

        public static void UpdateElement(Point position, SandbxElement element)
        {
            if (element.idName == "sand-powder")
            {
                float lastYvelocity = element.yVelocity;

                // Apply gravity if falling
                if (element.isFalling)
                {
                    element.yVelocity += GameProperties.canvasGravity; // Add a fraction of gravity each frame
                }
                else
                {
                    element.xVelocity *= element.friction;
                }

                bool wasFalling = element.isFalling; // Capture the current falling state

                SandbxElement elem;
                // Check if the element will collide in the next frame or is out of bounds
                if (Utilities.Simulation.GetElementInPosition(new Point(position.X, position.Y + 1), out elem) || Utilities.Simulation.IsPositionOutOfBounds(position.X * GameProperties.CellSize, (position.Y + 1) * GameProperties.CellSize, GameProperties.Width, GameProperties.Height))
                {
                    if (elem != null && elem.elementType == ElementType.Liquid)
                    {
                        Utilities.Simulation.SwapElements(position, new Point(position.X, position.Y + 1));
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
                if (!Utilities.Simulation.IsPositionOutOfBounds(newPosition.X * GameProperties.CellSize, newPosition.Y * GameProperties.CellSize, GameProperties.Width, GameProperties.Height) &&
                    !Utilities.Simulation.IsPositionOccupied(newPosition, true, position))
                {
                    SandbxElement blockingelem;
                    Point availablePosition = Utilities.Simulation.GetCollidingElementInLine(position, newPosition, out blockingelem);

                    if (element.isFalling)
                    {
                        if (blockingelem != null && blockingelem.elementType == ElementType.Liquid)
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
                            Utilities.Simulation.MoveElement(position, availablePosition);
                        }
                    }
                }
                else
                {

                    SandbxElement blockingelem;
                    Point availablePosition = Utilities.Simulation.GetCollidingElementInLine(position, newPosition, out blockingelem);

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
                            Utilities.Simulation.MoveElement(position, availablePosition);
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
                    if (!Utilities.Simulation.IsPositionOutOfBounds(newHorizontalPosition.X * GameProperties.CellSize, newHorizontalPosition.Y * GameProperties.CellSize, GameProperties.Width, GameProperties.Height) &&
                    !Utilities.Simulation.IsPositionOccupied(newHorizontalPosition) &&
                    !Utilities.Simulation.IsPositionOutOfBounds(horizontalBelow.X * GameProperties.CellSize, horizontalBelow.Y * GameProperties.CellSize, GameProperties.Width, GameProperties.Height) &&
                        !Utilities.Simulation.IsPositionOccupied(horizontalBelow))
                    {
                        Utilities.Simulation.MoveElement(position, newHorizontalPosition);
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
            else if (element.idName == "metal-scraps")
            {
                float lastYvelocity = element.yVelocity;
                bool wasFalling = element.isFalling; // Capture the current falling state

                UpdateElementGravity(position, element);

                // Check if the element will collide in the next frame or is out of bounds

                /*
                if (!IsPositionOccupied(new Point(position.X, position.Y + 1)) && !IsPositionOutOfBounds(position.X * CellSize, (position.Y + 1) * CellSize, Width, Height))
                {
                    element.isFalling = true;
                }
                else
                {
                    element.isFalling = false;
                }

                /*

                // Calculate new position based on velocity
                Point newPosition = new Point(
                    position.X + (int)Math.Round(element.xVelocity),
                    position.Y + (int)Math.Round(element.yVelocity)
                );

                SandbxElement blocker;

                // Ensure the new position is not out of bounds or colliding
                if (!IsPositionOutOfBounds(newPosition.X * CellSize, newPosition.Y * CellSize, Width, Height) &&
                    !IsPositionOccupied(newPosition, true, position))
                {
                    Point availablePosition = IsCollidingInLine(position, newPosition);

                    if (element.isFalling)
                    {
                        MoveElement(position, availablePosition);
                    }
                }
                else
                {
                    Point availablePosition = GetCollidingElementInLine(position, newPosition, out blocker);

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
                }*/

                int rand = random.Next(0, 2);

                if (!element.isFalling)
                {
                    //Point newHorizontalPosition;

                    if (rand == 0)
                    {
                        //newHorizontalPosition = new Point(position.X - ((LiquidElement)element).fluidPouring, position.Y);
                        if (!Utilities.Simulation.IsPositionOccupied(new Point(position.X - 1, position.Y)) &&
                            element.xVelocity < 1 && element.xVelocity > -1)
                        {
                            element.xVelocity -= Utilities.Simulation.GetRandomPowderityValue(element);
                            //element.xVelocity = Math.Clamp(element.xVelocity -= Utilities.Simulation.GetRandomPouringValue(element), 0, ((LiquidElement)element).fluidPouringMax);
                        }
                        else if (!Utilities.Simulation.IsPositionOccupied(new Point(position.X + 1, position.Y)) &&
                            element.xVelocity < 1 && element.xVelocity > -1)
                        {
                            element.xVelocity += Utilities.Simulation.GetRandomPowderityValue(element);
                            //element.xVelocity = Math.Clamp(element.xVelocity += Utilities.Simulation.GetRandomPouringValue(element), 0, ((LiquidElement)element).fluidPouringMax);
                        }
                        else
                        {
                            element.xVelocity -= Utilities.Simulation.GetRandomPowderityValue(element);
                            //element.xVelocity = Math.Clamp(element.xVelocity -= Utilities.Simulation.GetRandomPouringValue(element), 0, ((LiquidElement)element).fluidPouringMax);
                        }
                    }
                    else if (rand == 1)
                    {
                        //newHorizontalPosition = new Point(position.X + ((LiquidElement)element).fluidPouring, position.Y);
                        if (!Utilities.Simulation.IsPositionOccupied(new Point(position.X + 1, position.Y)) &&
                            element.xVelocity < 1 && element.xVelocity > -1)
                        {
                            element.xVelocity += Utilities.Simulation.GetRandomPowderityValue(element);
                            //element.xVelocity = Math.Clamp(element.xVelocity += Utilities.Simulation.GetRandomPouringValue(element), 0, ((LiquidElement)element).fluidPouringMax);
                        }
                        else if (!Utilities.Simulation.IsPositionOccupied(new Point(position.X - 1, position.Y)) &&
                            element.xVelocity < 1 && element.xVelocity > -1)
                        {
                            element.xVelocity -= Utilities.Simulation.GetRandomPowderityValue(element);
                            //element.xVelocity = Math.Clamp(element.xVelocity -= Utilities.Simulation.GetRandomPouringValue(element), 0, ((LiquidElement)element).fluidPouringMax);
                        }
                        else
                        {
                            element.xVelocity += Utilities.Simulation.GetRandomPowderityValue(element);
                            //element.xVelocity = Math.Clamp(element.xVelocity += Utilities.Simulation.GetRandomPouringValue(element), 0, ((LiquidElement)element).fluidPouringMax);
                        }
                    }
                }

                UpdateElementVelocity(position, element);
            }
        }

        public static void UpdateElementVelocity(Point elementKey, SandbxElement elementDef)
        {
            Point newDesiredPosition = new Point(elementKey.X + (int)Math.Round(elementDef.xVelocity), elementKey.Y + (int)Math.Round(elementDef.yVelocity));
            Point newAvaiablePosition = Utilities.Simulation.GetLastAvaiablePosition(elementKey, newDesiredPosition);

            //Console.WriteLine("DES: " + newDesiredPosition.X + " / " + newDesiredPosition.Y);
            //Console.WriteLine("NEW: " + newDesiredPosition.X + " / " + newDesiredPosition.Y);

            Point belowPosition = new Point(elementKey.X, elementKey.Y - 1);
            if(!Utilities.Simulation.IsPositionOutOfCanvasBounds(belowPosition) &&
               !Utilities.Simulation.IsPositionOccupied(belowPosition))
            {
                if(elementDef.isFalling == false)
                {
                    elementDef.yVelocity += 2f;
                }

                elementDef.isFalling = true;
            }
            else if(Utilities.Simulation.IsPositionOutOfCanvasBounds(belowPosition) ||
               Utilities.Simulation.IsPositionOccupied(belowPosition))
            {
                elementDef.isFalling = false;
            }

            if(newDesiredPosition.X != newAvaiablePosition.X)
            {
                elementDef.xVelocity = 0;
            }

            //Console.WriteLine(elementDef.isFalling.ToString());

            Utilities.Simulation.MoveElement(elementKey, newAvaiablePosition);
        }

        public static void UpdateElementGravity(Point elementKey, SandbxElement elementDef)
        {
            // Apply gravity if falling
            if (elementDef.isFalling)
            {
                elementDef.yVelocity += GameProperties.canvasGravity; // Add a fraction of gravity each frame
            }
            else
            {
                //elementDef.yVelocity = 0;
                elementDef.xVelocity *= elementDef.friction;
            }
        }
    }
}
