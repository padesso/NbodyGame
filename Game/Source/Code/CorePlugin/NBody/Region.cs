using Duality;
using DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBody
{
    public class Region
    {
        private QuadTree _quadTree;

        public Region(QuadTree quadTree)
        {
            QuadTree = quadTree;
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

        public MassDistribution Distribution()
        {
            List<Node> quadNodes = _quadTree.QueryBounds(_quadTree.Bounds);

            MassDistribution tempDistribution = new MassDistribution();

            if(quadNodes.Count == 1)
            {
                //TODO: update to use Body object
                //tempDistribution.Mass = quadNodes[0].Mass;
                //tempDistribution.CenterOfMass = quadNodes[0].Position;
            }
            foreach(Node node in quadNodes)
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
