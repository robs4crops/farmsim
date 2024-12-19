using NModcom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace libFarmSim
{
    public class Sprayer: UpdateableSimObj
    {
        [Input("InfestationLevel")]
        IData InfestationLevel;

        [Input("CurrentSpeed")]
        IData CurrentSpeed;

        [Input("ApplicationRate")]
        IData ApplicationRate = new ConstFloatSimData(200);

        [Output("TankLevel")]
        IData TankLevel = new ConstFloatSimData(0);  // l

        [Output("FlowRate")]
        IData FlowRate = new ConstFloatSimData(0);  // l/s

        double workingWidth;  // m
        double applicationRate_l_m2;  // l/m2
        double tankLevel;

        public override void StartRun()
        {
            workingWidth = 6;

            applicationRate_l_m2 = ApplicationRate.AsFloat / 10000;
            tankLevel = 1000;

            TankLevel.AsFloat = tankLevel;
        }

        public override void HandleEvent(ISimEvent simEvent)
        {
            // depends on timestep, speed, workingwidth and (possibly) infestation level
            double flowRate = applicationRate_l_m2 * workingWidth * CurrentSpeed.AsFloat * InfestationLevel.AsFloat;
            double flowAmount = flowRate * TimeStep * 86400;
            tankLevel -= flowAmount;

            // shouldn't happen -> stop simulation
            if (tankLevel < 0)
                simEvent.SimEnv.RequestStop();

            TankLevel.AsFloat = tankLevel;
            FlowRate.AsFloat = flowRate;
        }
    }
}
