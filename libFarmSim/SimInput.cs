using NetTopologySuite.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libFarmSim
{
    public class SimInput
    {
        public FeatureCollection route;
        public FeatureCollection zones;
        public int TimeStep;
        public double TargetVelocity;
    }
}
