using Duality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Drawing;
using DataStructures;

namespace NBody
{
    public class Body
    {
        private Vector2 _position;
        private Vector2 _velocity;
        private float _mass;
        private Vector2 _acceleration;
        private float _gravity;
        private float _radius;

        public Body(float x, float y, float mass, float gravity, float radius)
        {
            _position = new Vector2(x, y);
            _velocity = Vector2.Zero;
            _mass = mass;
            _acceleration = Vector2.Zero;
            _gravity = gravity;
            _radius = radius;
        }

        public bool IsVisible(IDrawDevice device)
        {
            bool anyGroupFlag =
               (device.VisibilityMask & VisibilityFlag.AllGroups)
               != VisibilityFlag.None;

            bool screenOverlayFlag =
                (device.VisibilityMask & VisibilityFlag.ScreenOverlay)
                != VisibilityFlag.None;

            if (!anyGroupFlag) return false;
            if (screenOverlayFlag) return false;

            return true;
        }

        public Vector2 Position
        {
            get
            {
                return _position;
            }

            set
            {
                _position = value;
            }
        }

        public Vector2 Velocity
        {
            get
            {
                return _velocity;
            }

            set
            {            
                _velocity = value;
            }
        }

        public float Mass
        {
            get
            {
                return _mass;
            }

            set
            {
                _mass = value;
            }
        }

        public Vector2 Acceleration
        {
            get
            {
                return _acceleration;
            }

            set
            {
                _acceleration = value;
            }
        }

        public float Gravity
        {
            get
            {
                return _gravity;
            }

            set
            {
                _gravity = value;
            }
        }

        public float Radius
        {
            get
            {
                return _radius;
            }

            set
            {
                _radius = value;
            }
        } 
    }
}
