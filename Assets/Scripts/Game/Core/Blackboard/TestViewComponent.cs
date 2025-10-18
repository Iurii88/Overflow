using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.Blackboard
{
    public class TestViewComponent : AViewComponent
    {
        public BlackboardViewParameter<int> ggg;
        public BlackboardViewParameter<string> sss;
        public BlackboardViewParameter<Vector3> v3;
        public BlackboardViewParameter<System.Numerics.Vector3> v3Numerics;
        public BlackboardViewParameter<List<Vector3>> v3List;
        public BlackboardViewParameter<Vector3[]> v3Array;
    }
}