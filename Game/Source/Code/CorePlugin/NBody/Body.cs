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
    public struct Body
    {
        private Node _node;
        private float _mass;
        private Vector2 _force;

        public Body(float x, float y, float mass)
        {
            _node = new Node(x, y);
            _mass = mass;
            _force = Vector2.One;
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

        public Node Node
        {
            get
            {
                return _node;
            }

            set
            {
                _node = value;
            }
        }

        public Vector2 Force
        {
            get
            {
                return _force;
            }

            set
            {
                _force = value;
            }
        }
    }
}
