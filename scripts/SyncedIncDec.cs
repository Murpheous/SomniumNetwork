using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if FUSION2
using Fusion;
#endif

namespace SyncedControls.Example
{
    public class SyncedIncDec : MonoBehaviour
    {
        [SerializeField]
        Rigidbody syncedRigidBody;
        [SerializeField] Button IncButton;
        [SerializeField] Button DecButton;

        [SerializeField] int _value = 0;
        [SerializeField] int minValue = 0;
        [SerializeField] int maxValue = 10;

        [SerializeField]
        private UnityEvent<int> onValue;

        public UnityEvent<int> OnValue
        {
            get => onValue;
        }

        [Header("Just here to see in inspector")]
        [SerializeField]
        private Transform rbTransform;
        [SerializeField]
        private int _reportedValue = -1;
#if FUSION2
        [SerializeField]
        private NetworkObject networkObject;
        private void RequestNetAuthority()
        {
            if (networkObject != null && (networkObject.Runner != null))
            {
                if (!networkObject.HasStateAuthority)
                    networkObject.RequestStateAuthority();
            }
        }

        private void checkNetworkObjects()
        {
            if (networkObject == null)
                networkObject = GetComponentInChildren<NetworkObject>();
            if (syncedRigidBody != null)
            {
                rbTransform = syncedRigidBody.transform;
                if (networkObject == null)
                    networkObject = syncedRigidBody.GetComponent<NetworkObject>();
            }
            if (networkObject == null)
            {
                DebugUI.LogWarning($"{gameObject.name}: No NetworkObject found");
            }
        }
#else
        // No Fusion
        private void RequestNetAuthority()
        {
        }
        private void checkNetworkObjects()
        {
            if (syncedRigidBody != null)
            {
                if (rbTransform == null)
                    rbTransform = syncedRigidBody.transform;
            }
        }

#endif
        [SerializeField]
        private bool debug = true;
        private bool started = false;

        public int Value
        {
            get => _reportedValue;
            set
            {
                if (!started)
                {
                    _value = value;
                    _reportedValue = value;
                }
            }
        }

        public int MinValue
        {
            get => minValue;
            set
            {
                if (value < -179)
                    value = 179;
                minValue = value;
            }
        }
        public int MaxValue
        {
            get => maxValue;
            set
            {
                if (value > 179)
                    value = 179;
                maxValue = value;
            }
        }


        private int rbValue
        {
            get
            {
                if (rbTransform == null)
                    return _value;
                return Mathf.RoundToInt(rbTransform.eulerAngles.y);
            }
            set
            {
                if (rbTransform == null)
                    return;
                rbTransform.eulerAngles = new Vector3(rbTransform.eulerAngles.x, value, rbTransform.eulerAngles.z);
            }
        }

        public void onPointerEnter()
        {
            RequestNetAuthority();
        }

        public void incValue()
        {
            int incValue = Mathf.Clamp(_value + 1, minValue, maxValue);
            if (_reportedValue != incValue)
            {
                rbValue = incValue;
                if (debug)
                    DebugUI.Log($"{gameObject.name}:IncDec + to {incValue}");
            }
        }

        public void decValue()
        {
            int decValue = Mathf.Clamp(_value - 1, minValue, maxValue);
            if (_reportedValue != decValue)
            {
                rbValue = decValue;
                if (debug)
                    DebugUI.Log($"{gameObject.name}:IncDec - to {decValue}");
            }
        }
        void Awake()
        {
            if (onValue == null)
                onValue = new UnityEvent<int>();
            if (syncedRigidBody == null)
            {
                syncedRigidBody = GetComponentInChildren<Rigidbody>();
            }
            checkNetworkObjects();
        }

        public void Start()
        {
            rbValue = _reportedValue;
            started = true;
        }

        void OnEnable()
        {
            if (IncButton != null)
                IncButton.onClick.AddListener(incValue);
            if (DecButton != null)
                DecButton.onClick.AddListener(decValue);
        }

        void OnDisable()
        {
            if (IncButton != null)
                IncButton.onClick.RemoveListener(incValue);
            if (DecButton != null)
                DecButton.onClick.RemoveListener(decValue);
        }

        // Update is called once per frame
        void Update()
        {
            if (rbTransform != null)
            {
                if (rbTransform.hasChanged)
                { // Check if the rigidbody has moved from its last synced position
                    rbTransform.hasChanged = false;
                    int newValue = Mathf.Clamp(Mathf.RoundToInt(rbTransform.eulerAngles.y), minValue, maxValue);
                    if (debug)
                        DebugUI.Log($"{gameObject.name} Update: <br>   EulerAngles.y={rbTransform.eulerAngles.y} newValue={newValue}");
                    if (newValue != _value)
                    {
                        if (debug)
                            DebugUI.Log($"newValue{newValue} != _value={_value}");
                        _value = newValue; // Update the local state
                    }
                    if (_reportedValue != newValue)
                    {
                        onValue.Invoke(newValue); // Invoke the event with the new state
                        _reportedValue = newValue; // Update the reported state
                    }
                }
            }
        }
    }
}
