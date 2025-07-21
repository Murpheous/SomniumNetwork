using UnityEngine;
using UnityEngine.Events;

namespace SyncedControls.Example
{
    public class floatEvent : UnityEvent<float> { }
    public class toggleEvent : UnityEvent<bool> { }
    public class integerEvent : UnityEvent<int> { }

    public interface ITweenClient
    {
        public void UpdateTween(float value);
    }

    public interface ISliderInterface 
    {
        public bool PointerIsMoving { get; }

        public bool Interactable { get; set; }

        public float DisplayScale { get; set; }

        public void SetValue(float value);

        public void SetLimits(float min, float max);

        public float MaxValue { get; set; }
        public float MinValue { get; set; }

        public string TypeText { get; set; }
        public string ValueSuffix { get; set; }

    }
}
