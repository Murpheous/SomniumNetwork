
using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SyncedControls.Example
{
    [RequireComponent(typeof(Slider))]
    public class SyncedSlider : MonoBehaviour,ISliderInterface
    {
        [SerializeField]
        private Rigidbody networkedRigibody;
        private Slider mySlider;
        [Header("Smoothing settings")]
        [SerializeField, Range(0f, 5f), Tooltip("Apply damping by setting above zero")]
        private float smoothRate = 4f;
        [SerializeField, Range(0.001f, 0.01f), Tooltip("Smooth threshold")]
        float smoothThreshold = 0.003f;
        [Header("Title and Value Text")]
        [SerializeField]
        private TextMeshProUGUI sliderLabel;
        [SerializeField]
        private TextMeshProUGUI _sliderType;
        [SerializeField]
        private bool hideLabel = false;
        [SerializeField]
        private bool unitsInteger = false;
        [SerializeField]
        private bool displayInteger = false;
        [SerializeField]
        private string _valueSuffix;
        [SerializeField]
        private float _value;

        [SerializeField]
        private float displayScale = 1;

        [SerializeField]
        private float maxValue = 1;
        [SerializeField]
        private float minValue = 0;

        [SerializeField]
        private bool interactable = true;

        [Header("Just to see values in the inspector")]

        [SerializeField] private float _syncedValue;
        [SerializeField] private float _reportedValue;

        [SerializeField] private float _syncedCursorPos;
        [SerializeField] private float _sliderPos;
        //[SerializeField] 
        private NetworkObject networkObject;
        // Event to report synced value changes
        [SerializeField]
        public UnityEvent<float> OnValueChanged;
        // Event to report pointer state
        public UnityEvent<bool> OnPointerMovement;

        // Flag to indicate if the pointer is currently down
        private bool _pointerIsMoving = false;
        public bool PointerIsMoving
        {
            get => _pointerIsMoving;
        }

        public bool Interactable
        {
            get
            {
                return interactable;
            }
            set
            {
                interactable = value;
                if (started)
                    mySlider.interactable = value;
            }
        }

        public float DisplayScale
        {
            get => displayScale;
            set
            {
                displayScale = value;
                UpdateLabel();
            }
        }

        public bool DisplayInteger
        {
            get => displayInteger;
            set
            {
                displayInteger = value;
                UpdateLabel();
            }
        }
        private float SyncedCursorPos
        {
            get => _syncedCursorPos;
            set
            {
                _syncedCursorPos = value;
                SyncedValue = Mathf.Lerp(minValue, maxValue, _syncedCursorPos);
                if (_sliderPos != value)
                {
                    _sliderPos = _syncedCursorPos;
                    mySlider.SetValueWithoutNotify(_syncedCursorPos);
                }
            }
        }

        private float SyncedValue
        {
            get => _syncedValue;
            set
            {
                if (_syncedValue != value)
                {
                    _syncedValue = value;
                    if (smoothRate <= 0 && _reportedValue != _syncedValue)
                        ReportedValue = _syncedValue;
                }
                UpdateLabel();
            }
        }
        private float SliderPos
        {
            get => _sliderPos;
            set
            {
                _sliderPos = value;
                if (started)
                {
                    if (mySlider.value != value)
                        mySlider.SetValueWithoutNotify(value);
                    if (networkObject != null && (networkObject.Runner != null))
                    {
                        if (networkObject.StateAuthority != networkObject.Runner.LocalPlayer)
                            networkObject.RequestStateAuthority();
                    }
                    setSyncedValue(value);
                }
            }
        }
        public void SetValue(float value)
        {
            _value = value;
            _syncedValue = value;
            _reportedValue = value;
            if (!started)
                return;
            float cursorPos = Mathf.Clamp01(Mathf.InverseLerp(minValue, maxValue, value));
            setSyncedValue(cursorPos);
        }

        public void SetLimits(float min, float max)
        {
            minValue = min;
            maxValue = max;
        }
        public string TypeText
        {
            get
            {
                if (_sliderType == null)
                    return "";
                return _sliderType.text;
            }
            set
            {
                if (_sliderType == null)
                    return;
                _sliderType.text = value;
            }
        }
        public string ValueSuffix
        {
            get => _valueSuffix;
            set
            {
                _valueSuffix = value;
                UpdateLabel();
            }
        }
        private void UpdateLabel()
        {
            if (sliderLabel == null || hideLabel)
                return;
            float displayValue = _syncedValue * displayScale;
            if (displayInteger)
                displayValue = Mathf.RoundToInt(displayValue);
            if (unitsInteger || displayInteger)
                sliderLabel.text = string.Format("{0}{1}", (int)displayValue, ValueSuffix);
            else
                sliderLabel.text = string.Format("{0:0.0}{1}", displayValue, ValueSuffix);
        }

        public float MaxValue
        {
            get => maxValue;
            set
            {
                maxValue = value;
            }
        }
        public float MinValue
        {
            get => minValue;
            set
            {
                minValue = value;
            }
        }

        private void setSyncedValue(float value)
        {
            if (rbTransform != null)
            {
                Vector3 localRotation = rbTransform.localRotation.eulerAngles;
                localRotation.y = value * 180f; // Assuming full range of the slider is +- 1
                rbTransform.localEulerAngles = localRotation;
            }
        }

        private void OnSliderValue(float value)
        {

            if (value != SliderPos)
            {
                SliderPos = value;
            }
        }

        public float ReportedValue
        {
            get => _reportedValue;
            private set
            {
                _reportedValue = value;
                OnValueChanged.Invoke(_reportedValue);
            }
        }

        public void onPointer(bool value)
        {
            // Main purpose of pointer on slider is to grab network Authority
            if (value && networkObject != null && networkObject.Runner != null)
            {
                if (networkObject.StateAuthority != networkObject.Runner.LocalPlayer)
                    networkObject.RequestStateAuthority();
            }
        }

        private Transform rbTransform = null;
        private float smthVel = 0;
        private bool isSmoothing = false;
        private void Update()
        {
            if (rbTransform != null)
            {
                if (rbTransform.hasChanged)
                { // Check if the rigidbody has moved from its last synced position
                    rbTransform.hasChanged = false;
                    float pos = rbTransform.localEulerAngles.y / 180f; // Convet +- 180 to +1 to -1 range
                    SyncedCursorPos = Mathf.Clamp01(pos);
                }
            }
            if (smoothRate <= 0f || _reportedValue == SyncedValue)
                return;
            if (Mathf.Abs(_reportedValue - _syncedValue) > smoothThreshold)
            {
                if (!isSmoothing)
                    OnPointerMovement.Invoke(true);
                isSmoothing = true;
                ReportedValue = Mathf.SmoothDamp(_reportedValue, _syncedValue, ref smthVel, 0.1f * smoothRate);
            }
            else
            {
                ReportedValue = _syncedValue;
                if (isSmoothing)
                {
                    isSmoothing = false;
                    OnPointerMovement.Invoke(false);
                }
            }
        }

        private bool started = false;
        public void Awake()
        {
            if (networkedRigibody != null)
            {
                rbTransform = networkedRigibody.transform;
                networkObject = networkedRigibody.GetComponent<NetworkObject>();
            }
            if (OnValueChanged == null)
                OnValueChanged = new UnityEvent<float>();
            if (OnPointerMovement == null)
                OnPointerMovement = new UnityEvent<bool>();
            mySlider = GetComponent<Slider>();
            mySlider.onValueChanged.AddListener(OnSliderValue);
            //setCursorTrack();
            if (sliderLabel != null)
                sliderLabel.gameObject.SetActive(!hideLabel);
            mySlider.interactable = interactable;
            mySlider.minValue = 0f;
            mySlider.maxValue = 1f;
        }
        public void Start()
        {
            float cursorPos = Mathf.Clamp01(Mathf.InverseLerp(minValue, maxValue, _value));
            mySlider.SetValueWithoutNotify(cursorPos);
            if (rbTransform != null)
            {
                setSyncedValue(cursorPos);
                rbTransform.hasChanged = true;
            }
            mySlider.interactable = interactable;
            UpdateLabel();
            started = true;
        }
    }
}
