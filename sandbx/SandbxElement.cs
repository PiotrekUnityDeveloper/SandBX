using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sandbx
{
    class SandbxElement
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

        // Behaviour
        public ElementType elementType { get; set; }

        // Constructor
        public SandbxElement()
        {
            bounciness = 2.0f;
            friction = 0.5f;
            xVelocity = 0;
            yVelocity = 0;
            isFalling = true; // Default to falling behavior
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