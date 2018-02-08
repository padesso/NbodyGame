using Duality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Duality.Drawing;
using Output.DataStructures;

namespace Output.NBody
{
    public class Body : Component, ICmpUpdatable, ICmpRenderer
    {
        private Node _node;
        private float _mass;
        private Vector2 _force;

        public Body(float x, float y, float mass)
        {
            Node = new Node(x, y);
            Mass = mass;
        }

        public void Draw(IDrawDevice device)
        {
            Canvas canvas = new Canvas(device);

            canvas.FillCircle(_node.Position.X, _node.Position.Y, 2);
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

        public void OnUpdate()
        {
            //TODO: integrate movement
        }

        public float BoundRadius
        {
            get
            {
                return 2.0f;
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
