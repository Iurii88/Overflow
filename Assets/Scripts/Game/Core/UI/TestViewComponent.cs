using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.UI
{
    public class TestViewComponent : AViewComponent
    {
        public BlackboardViewParameter<int> ggg;
        public BlackboardViewParameter<string> sss;
        public BlackboardViewParameter<Vector3> v3;
        public BlackboardViewParameter<System.Numerics.Vector3> v3Numerics;
        public BlackboardViewParameter<List<Vector3>> v3List;
        public BlackboardViewParameter<Vector3[]> v3Array;

        protected override void Subscribe()
        {
            ggg.OnVariableChanged += OnVariableChanged;
            v3.OnVariableChanged += OnVariableChangedV3;
            v3.Value = new Vector3(1, 2, 3);
        }

        private void OnVariableChangedV3(BlackboardVariable<Vector3> varV3)
        {
            Debug.Log($"{varV3.value}");
        }

        private void OnVariableChanged(BlackboardVariable<int> variable)
        {
            
        }

        private void Update()
        {
            //ggg.Value = Time.frameCount % 9;
        }
    }
}