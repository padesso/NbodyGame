using Duality;
using DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBody
{
    public struct Region
    {
        private QuadTree _quadTree;
        private List<Body> _bodies;

        public Region(float topLeftX, float topLeftY, float width, float height)
        {
            _bodies = new List<Body>();
            _quadTree = new QuadTree(topLeftX, topLeftY, width, height);
        }

        public QuadTree QuadTree
        {
            get
            {
                return _quadTree;
            }

            private set
            {
                _quadTree = value;
            }
        }

        public List<Body> Bodies
        {
            get
            {
                return _bodies;
            }

            set
            {
                _bodies = value;
            }
        }

        public bool AddBody(Body body)
        {
            if (_quadTree.Insert(body.Node))
            {
                Bodies.Add(body);
                return true;
            }
            return false;
        }

        public bool RemoveBody(Body body)
        {
            if (_quadTree.Delete(body.Node))
            {
                Bodies.Remove(body);
                return true;
            }
            return false;
        }

        public MassDistribution Distribution()
        {
            MassDistribution tempDistribution = new MassDistribution();

            if(Bodies.Count == 1)
            {
                tempDistribution.Mass = Bodies[0].Mass;
                tempDistribution.CenterOfMass = Bodies[0].Node.Position;
            }
            foreach(Body node in Bodies)
            {

            }

            return tempDistribution;
        }

        /*
         * if number of particles in this node equals 1
         {
            CenterOfMass = Particle.Position
            Mass = Particle.Mass  
          }
          else
          {
            // Compute the center of mass based on the masses of 
            // all child quadrants and the center of mass as the 
            // center of mass of the child quadrants weightes with 
            // their mass
            for all child quadrants that have particles in them
            {
              Quadrant.ComputeMassDistribution
              Mass += Quadrant.Mass
              CenterOfMass +=  Quadrant.Mass * Quadrant.CenterOfMass
            }
            CenterOfMass /= Mass
          }
          */
    }
}
