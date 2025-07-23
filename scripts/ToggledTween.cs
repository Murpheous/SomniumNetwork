
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A Toggled Tween component that alterantes the direction of an animation 
/// Provides a simple way to reverse direction of an animation based on a toggle state.
/// </summary >
namespace SyncedControls.Example
{
    public class ToggledTween : MonoBehaviour
    {
        [Tooltip("Initial State"), SerializeField]
        private bool _toggleState = false;
        [Header("Client Behaviours")]
        [SerializeField] public UnityEvent <float> onValueChanged;

        [SerializeField, Tooltip("Animation Curve")]
        AnimationCurve tweenCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
        [SerializeField] float easingTime = 0.75f;

        [Header("Internal Behaviours - serialzed to see in inspector")]
        [SerializeField, Tooltip("Animation curve lookup")]
        private float _animationLookup = 0;
        private bool _isPlaying = false;
        public bool IsPlaying => _isPlaying;

        private bool _previousToggleState = false;
        public bool ToggleState
        {
            get => _toggleState;
            set
            {
                _isPlaying |= value != _previousToggleState;
                _toggleState = value;
                _previousToggleState = value;
            }
        }

        public void setState(bool state)
        {
            ToggleState = state;
            _animationLookup = state ? 1 : 0;
            _isPlaying = true;
        }

        public float TweenValue => tweenCurve.Evaluate(_animationLookup);

        private void Update()
        {
            if (!_isPlaying)
                return;
            _animationLookup = Mathf.Clamp01(_animationLookup + (Time.deltaTime * (_toggleState ? easingTime : -easingTime)));
            _isPlaying = _animationLookup > 0 && _animationLookup < 1;
            onValueChanged.Invoke(tweenCurve.Evaluate(_animationLookup));
        }
        private void Awake()
        {
            if (onValueChanged == null)
                onValueChanged = new UnityEvent<float>();
            _animationLookup = _toggleState ? 1f : 0f;
            _previousToggleState = _toggleState;
            _isPlaying = false;
        }
        private void Start()
        {
            _animationLookup = _toggleState ? 1f : 0f;
            _isPlaying = true; // Abruptly signal the tweened value to the toggle state at start
        }
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (tweenCurve == null)
                tweenCurve = AnimationCurve.EaseInOut(0.0f, 0.0f, 1.0f, 1.0f);
            ToggleState = _toggleState; // Trigger the state change to update the tween time
        }
#endif
    }
}