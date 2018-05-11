using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace Hedra.Engine.StructureSystem.VillageSystem
{
    interface ISchemeGenerator
    {
        IEnumerator BuildBlacksmith(object[] Params);
        IEnumerator BuildSingleHouse(object[] Params);
        IEnumerator BuildFarms(object[] Params);
        IEnumerator BuildCenter(object[] Params);
        IEnumerator BuildMarket(object[] Params);
        IEnumerator GenerateWindmill(object[] Params);
        IEnumerator GenerateStable(object[] Params);
    }
}
