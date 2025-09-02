using UnityEngine.Events;

namespace SyncedControls.Example
{
    public interface ITweenClient
    {
        public void UpdateTween(float value);
    }

    public interface ISliderInterface
    {
        public UnityEvent<float> OnValueChanged { get; }
        public UnityEvent<bool> OnPointerMovement { get; }
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

