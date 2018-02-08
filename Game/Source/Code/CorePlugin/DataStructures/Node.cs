using Duality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Output.DataStructures
{
    public class Node
    {
        private Vector2 _position;

        public Node(float x, float y)
        {
            Position = new Vector2(x, y);
        }

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }
    }
}
