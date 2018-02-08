using Duality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures
{
    public class QuadTree
    {
        private Rect bounds;

        private Node position;

        private QuadTree northWest;
        private QuadTree northEast;
        private QuadTree southWest;
        private QuadTree southEast;

        public QuadTree(float topLeftX, float topLeftY, float width, float height)
        {
            this.Bounds = new Rect(topLeftX, topLeftY, width, height);
        }

        /// <summary>
        /// Create the quads as four equal regions in reference to this quad.
        /// </summary>
        private void Subdivide()
        {
            NorthWest = new QuadTree(
                    Bounds.X, 
                    Bounds.Y,
                    Bounds.W / 2.0f, 
                    Bounds.H / 2.0f);

            NorthEast = new QuadTree(
                    Bounds.CenterX, 
                    Bounds.Y,
                    Bounds.W / 2.0f, 
                    Bounds.H / 2.0f);

            SouthWest = new QuadTree(
                    Bounds.X, 
                    Bounds.CenterY,
                    Bounds.W / 2.0f, 
                    Bounds.H / 2.0f);

            SouthEast = new QuadTree(
                    Bounds.CenterX, 
                    Bounds.CenterY,
                    Bounds.W / 2.0f, 
                    Bounds.H / 2.0f);
        }

        /// <summary>
        /// Add a position to the quadtree
        /// </summary>
        /// <param name="position">A Vector to add to the quadtree<./param>
        /// <returns>True if the insertion was successful.</returns>
        public bool Insert(Node pos)
        {
            // Ignore objects that do not belong in this quad tree
            if (!this.Bounds.Contains(pos.Position))
                return false; // object cannot be added to this quad

            //If the quad is not full and it is an external node, fill it
            if (this.Position == null && NorthWest == null)
            {
                this.Position = pos;
                return true;
            }

            // Otherwise, subdivide and shift down current position.
            if (NorthWest == null)
            {
                Subdivide();

                //Shift down the current position as this is now an internal node
                if (NorthWest.Insert(this.Position))
                {
                    this.Position = null;
                }
                else if (NorthEast.Insert(this.Position))
                {
                    this.Position = null;
                }
                else if (SouthWest.Insert(this.Position))
                {
                    this.Position = null;
                }
                else if (SouthEast.Insert(this.Position))
                {
                    this.Position = null;
                }
            }

            //Attempt to insert new node
            if (NorthWest.Insert(pos))
                return true;

            if (NorthEast.Insert(pos))
                return true;

            if (SouthWest.Insert(pos))
                return true;

            if (SouthEast.Insert(pos))
                return true;
           
            // The point cannot be inserted
            return false;
        }

        /// <summary>
        /// Removes the first position from the quad tree at the position passed.
        /// </summary>
        /// <param name="pos">The position to be removed.</param>
        /// <returns>A true indicates a position was successfully removed.</returns>
        public bool Delete(Node pos)
        {
            //TODO: Optimize this...
            List<Node> allPositions = this.ToList();

            if (allPositions.Remove(pos))
            {
                Position = null;
                NorthWest = null;
                NorthEast = null;
                SouthWest = null;
                SouthEast = null;

                foreach (Node position in allPositions)
                {
                    Insert(position);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Find all points within an axis aligned bounding box.
        /// </summary>
        /// <param name="bounds">An axis aligned bounding box to find all points in the quadtree.</param>
        /// <returns>A list of Vectors representing all positions within the AABB passed.</returns>
        public List<Node> QueryBounds(Rect bounds)
        {
            List<Node> positionsInBounds = new List<Node>();

            //Get out early if the bounds passed isn't in this quad (or a child quad)
            if (!this.Bounds.Intersects(bounds))
                return positionsInBounds;

            // Terminate here, if there are no children (external node)
            // We only need to check one since the subdivide method instantiates all sub-quads
            if (this.Position != null && NorthWest == null)
            {
                positionsInBounds.Add(this.Position);
                return positionsInBounds;
            }

            // Otherwise, add the positions from the children
            if (NorthWest != null)
                positionsInBounds.AddRange(NorthWest.QueryBounds(bounds));

            if (NorthEast != null)
                positionsInBounds.AddRange(NorthEast.QueryBounds(bounds));

            if (SouthWest != null)
                positionsInBounds.AddRange(SouthWest.QueryBounds(bounds));

            if (SouthEast != null)
                positionsInBounds.AddRange(SouthEast.QueryBounds(bounds));

            return positionsInBounds;
        }

        public List<Node> ToList()
        {
            return this.QueryBounds(this.Bounds);
        }

        public QuadTree NorthWest
        {
            get { return northWest;  }
            set { northWest = value; }
        }

        public QuadTree NorthEast
        {
            get { return northEast; }
            set { northEast = value; }
        }

        public QuadTree SouthWest
        {
            get { return southWest; }
            set { southWest = value; }
        }
        public QuadTree SouthEast
        {
            get { return southEast; }
            set { southEast = value; }
        }

        public Node Position
        {
            get { return position; }
            set { position = value; }
        }

        public Rect Bounds
        {
            get { return bounds; }
            set { bounds = value; }
        }
    }
}
