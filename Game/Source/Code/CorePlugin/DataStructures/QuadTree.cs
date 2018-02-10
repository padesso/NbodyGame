using Duality;
using NBody;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataStructures
{
    public class QuadTree
    {
        private Rect _bounds;

        private Body _body;

        private QuadTree _northWest;
        private QuadTree _northEast;
        private QuadTree _southWest;
        private QuadTree _southEast;

        private List<Body> _bodies;

        public QuadTree(float topLeftX, float topLeftY, float width, float height)
        {
            this.Bounds = new Rect(topLeftX, topLeftY, width, height);
            _bodies = new List<Body>();    
        }

        /// <summary>
        /// Create the quads as four quadtrees that are equally subdivided in reference to this quad.
        /// </summary>
        protected virtual void Subdivide()
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

        public bool AddBody(Body body)
        {
            if (Insert(body))
            {
                _bodies.Add(body);
                return true;
            }
            return false;
        }

        public bool RemoveBody(Body body)
        {
            if (Delete(body))
            {
                _bodies.Remove(body);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Add a node to the quadtree
        /// </summary>
        /// <param name="node">A Node to add to the quadtree</param>
        /// <returns>True if the insertion was successful.</returns>
        private bool Insert(Body body)
        {
            // Ignore objects that do not belong in this quad tree
            if (!this.Bounds.Contains(body.Position))
                return false; // object cannot be added to this quad

            //If the quad is not full and it is an external node, fill it
            if (this.Body == null && NorthWest == null)
            {
                this.Body = body;
                return true;
            }

            // Otherwise, subdivide and shift down current position.
            if (NorthWest == null)
            {
                Subdivide();

                //Shift down the current position as this is now an internal node
                if (NorthWest.Insert(this.Body))
                {
                    this.Body = null;
                }
                else if (NorthEast.Insert(this.Body))
                {
                    this.Body = null;
                }
                else if (SouthWest.Insert(this.Body))
                {
                    this.Body = null;
                }
                else if (SouthEast.Insert(this.Body))
                {
                    this.Body = null;
                }
            }

            //Attempt to insert new node
            if (NorthWest.Insert(body))
                return true;

            if (NorthEast.Insert(body))
                return true;

            if (SouthWest.Insert(body))
                return true;

            if (SouthEast.Insert(body))
                return true;
           
            // The point cannot be inserted
            return false;
        }

        /// <summary>
        /// Removes the first node from the quad tree at the position passed.
        /// </summary>
        /// <param name="pos">The node to be removed.</param>
        /// <returns>A true indicates a node was successfully removed.</returns>
        private bool Delete(Body body)
        {
            List<Body> allBodies = this.ToList();

            if (allBodies.Remove(body))
            {
                this.Body = null;
                NorthWest = null;
                NorthEast = null;
                SouthWest = null;
                SouthEast = null;

                foreach (Body tempBody in allBodies)
                {
                    Insert(tempBody);
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Find all points within an axis aligned bounding box.
        /// </summary>
        /// <param name="bounds">An rectangle used to find all points in the quadtree.</param>
        /// <returns>A list of nodes representing all positions within the Rect passed.</returns>
        public List<Body> QueryBounds(Rect bounds)
        {
            List<Body> positionsInBounds = new List<Body>();

            //Get out early if the bounds passed isn't in this quad (or a child quad)
            if (!this.Bounds.Intersects(bounds))
                return positionsInBounds;

            // Terminate here, if there are no children (external node)
            // We only need to check one since the subdivide method instantiates all sub-quads
            if (this.Body != null && NorthWest == null)
            {
                positionsInBounds.Add(this.Body);
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
    
        public double Mass
        {
            get
            {
                if(Body != null)
                {
                    return Body.Mass;
                }

                //TODO: sum mass for all quads below this one
                return 0;
            }
        }

        public Vector2 CenterOfMass
        {
            get
            {
                if(Body != null)
                {
                    return Body.Position;
                }

                //TODO: calc center of mass for all quads below this one
                return Vector2.Zero;
            }
        }

        public List<Body> ToList()
        {
            return this.QueryBounds(this.Bounds);
        }

        public QuadTree NorthWest
        {
            get { return _northWest;  }
            set { _northWest = value; }
        }

        public QuadTree NorthEast
        {
            get { return _northEast; }
            set { _northEast = value; }
        }

        public QuadTree SouthWest
        {
            get { return _southWest; }
            set { _southWest = value; }
        }
        public QuadTree SouthEast
        {
            get { return _southEast; }
            set { _southEast = value; }
        }

        public Body Body
        {
            get { return _body; }
            set { _body = value; }
        }

        public Rect Bounds
        {
            get { return _bounds; }
            set { _bounds = value; }
        }
    }
}
