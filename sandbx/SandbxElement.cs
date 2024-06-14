using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sandbx
{
    public class SandbxElement
    {
        // Visual Properties
        public Microsoft.Xna.Framework.Color defaultColor { get; set; } // Color assigned to the element, but not used by it. Used for UI colors
        public Microsoft.Xna.Framework.Color baseColor { get; set; } // Base color of the element, if it is not modified or in an non-default state

        // Definition Properties
        public string displayName { get; set; } // Element's display name, used for UI (Example: 'Soiled Rock')
        public string shortName { get; set; } // Element's shortcut name, used for UI (Example: 'SRCK')
        public string name { get; set; } // Element's base name (Example: 'soiled-rock')
        public string idName { get; set; } // Element's indentification name, used to define all element's states (Example: 'soiled-rock-static' / 'soiled-rock-powder)

        // Physics Propeties
        public bool isFalling { get; set; }
        public float xVelocity { get; set; }
        public float yVelocity { get; set; }
        public float friction { get; set; }
        public float bounciness { get; set; }
        public float energyConsumption { get; set; }

        // Behaviour
        public ElementType elementType { get; set; }

        // Constructor
        public SandbxElement()
        {
            bounciness = 2.0f;
            energyConsumption = 0.2f;
            friction = 0.2f;
            xVelocity = 0;
            yVelocity = 0;
            isFalling = true; // Default to falling behavior
        }

        public virtual SandbxElement Clone()
        {
            return new SandbxElement
            {
                baseColor = this.baseColor,
                energyConsumption = this.energyConsumption,
                defaultColor = this.defaultColor,
                idName = this.idName,
                displayName = this.displayName,
                shortName = this.shortName,
                name = this.name,
                elementType = this.elementType,
                xVelocity = this.xVelocity,
                yVelocity = this.yVelocity,
                friction = this.friction,
                bounciness = this.bounciness,
                isFalling = this.isFalling
            };
        }

        public void RecieveVelocity(float xVel, float yVel)
        {
            this.xVelocity += xVel * this.energyConsumption;
            this.yVelocity += yVel * this.energyConsumption;
        }
    }

    public class PowderElement : SandbxElement
    {
        public PowderElement()
        {
            elementType = ElementType.Powder;
            energyConsumption = 0.1f;
            bounciness = 2.0f;
            friction = 0.5f;
            xVelocity = 0;
            yVelocity = 0;
            isFalling = true;
        }

        public override SandbxElement Clone()
        {
            return new PowderElement
            {
                baseColor = this.baseColor,
                defaultColor = this.defaultColor,
                idName = this.idName,
                displayName = this.displayName,
                shortName = this.shortName,
                name = this.name,
                elementType = this.elementType,
                xVelocity = this.xVelocity,
                yVelocity = this.yVelocity,
                friction = this.friction,
                bounciness = this.bounciness,
                isFalling = this.isFalling,
                energyConsumption = this.energyConsumption,
            };
        }
    }

    public class LiquidElement : SandbxElement
    {
        public float fluidFriction { get; set; }
        public float fluidDensity { get; set; }
        public int fluidPouring { get; set; }
        public float fluidViscosity { get; set; }

        public LiquidElement()
        {
            elementType = ElementType.Liquid;
            fluidPouring = 3;
            energyConsumption = 0.1f;
            bounciness = 2.0f;
            friction = 0.5f;
            xVelocity = 0;
            yVelocity = 0;
            isFalling = true;
        }

        public override SandbxElement Clone()
        {
            return new LiquidElement
            {
                baseColor = this.baseColor,
                defaultColor = this.defaultColor,
                idName = this.idName,
                displayName = this.displayName,
                shortName = this.shortName,
                name = this.name,
                elementType = this.elementType,
                xVelocity = this.xVelocity,
                yVelocity = this.yVelocity,
                friction = this.friction,
                bounciness = this.bounciness,
                isFalling = this.isFalling,
                energyConsumption = this.energyConsumption,

                //liquid-specific
                fluidDensity = this.fluidDensity,
                fluidFriction = this.fluidFriction,
                fluidViscosity = this.fluidViscosity,
                fluidPouring = this.fluidPouring,
            };
        }
    }
}

public enum ElementType
{
    Solid,
    Powder,
    Liquid,
    Gas,
}