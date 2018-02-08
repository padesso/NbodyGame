using Duality;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBody
{
    public class MassDistribution
    {
        private float _mass;
        private Vector2 _centerOfMass;

        public MassDistribution()
        {

        }

        public MassDistribution(float mass, Vector2 centerOfMass)
        {
            Mass = mass;
            CenterOfMass = centerOfMass;
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

        public Vector2 CenterOfMass
        {
            get
            {
                return _centerOfMass;
            }

            set
            {
                _centerOfMass = value;
            }
        }
    }
}
