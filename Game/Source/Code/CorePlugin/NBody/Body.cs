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
        private int _mass;

        public Body()
        {

        }

        public float BoundRadius
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public int Mass
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
    }
}
