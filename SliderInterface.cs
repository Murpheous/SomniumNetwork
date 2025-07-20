using UnityEngine;
using UnityEngine.Events;

namespace QuantumScatter
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
        public bool PointerIsDown { get; }

        public bool Interactable { get; set; }

        public float DisplayScale { get; set; }

        public void SetValue(float value);

        public void SetLimits(float min, float max);

        public float MaxValue { get; set; }
        public float MinValue { get; set; }

        public string TitleText { get; set; }
        public string SliderUnit { get; set; }

        // Event Subscription
        public void subscribeValue(UnityAction<float> call);
        public void subscribePointer(UnityAction<bool> call);

        // Event Unsubscription
        public void unsubscribeValue(UnityAction<float> call);
        public void unsubscribePointer(UnityAction<bool> call);
    }
}
