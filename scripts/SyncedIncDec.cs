using Fusion;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SyncedControls.Example
{
    public class SyncedIncDec : MonoBehaviour
    {
        [SerializeField]
        Rigidbody networkedRigidBody;
        [SerializeField] Button IncButton;
        [SerializeField] Button DecButton;

        [SerializeField] int _value = 0;
        [SerializeField] int _minValue = 0;
        [SerializeField] int _maxValue = 10;


        public UnityEvent<int> onValue;

        [Header("Just here to see in inspector")]
        [SerializeField]
        private Transform rbTransform;
        [SerializeField]
        private int _reportedValue = -1;
        [SerializeField]
        private NetworkObject networkObject;

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
            get => _minValue;
            set
            {
                if (value < -179)
                    value = 179;
                _minValue = value;
            }
        }
        public int MaxValue
        {
            get => _maxValue;
            set
            {
                if (value > 179)
                    value = 179;
                _maxValue = value;
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
                if (networkObject != null && (networkObject.Runner != null))
                {
                    if (!networkObject.HasStateAuthority)
                        networkObject.RequestStateAuthority();
                }
                rbTransform.eulerAngles = new Vector3(rbTransform.eulerAngles.x, value, rbTransform.eulerAngles.z);
            }
        }

        public void onPointerEnter()
        {
            if (networkObject != null && (networkObject.Runner != null))
            {
                if (!networkObject.HasStateAuthority)
                    networkObject.RequestStateAuthority();
            }
        }
        public void incValue()
        {
            int incValue = Mathf.Clamp(_value + 1, _minValue, _maxValue);
            if (_reportedValue != incValue)
                rbValue = incValue;
            if (debug)
                DebugUI.Log(string.Format("senting value={0}",incValue));
        }

        public void decValue()
        {
            int decValue = Mathf.Clamp(_value - 1, _minValue, _maxValue);
            if (_reportedValue != decValue)
                rbValue = decValue;
            if (debug)
                DebugUI.Log(string.Format("value={0}", decValue));
        }

        void Awake()
        {
            if (onValue == null)
                onValue = new UnityEvent<int>();
            if (networkedRigidBody == null)
            {
                networkedRigidBody = GetComponentInChildren<Rigidbody>();
            }
            if (networkedRigidBody != null)
            {
                rbTransform = networkedRigidBody.transform;
                networkObject = networkedRigidBody.GetComponent<NetworkObject>();
            }
        }

        public void Start()
        {
            rbValue = _reportedValue;
            started = true;
        }

        void OnEnable()
        {
        }

        void OnDisable()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (rbTransform != null)
            {
                if (rbTransform.hasChanged)
                { // Check if the rigidbody has moved from its last synced position
                    rbTransform.hasChanged = false;
                    int newValue = Mathf.Clamp(Mathf.RoundToInt(rbTransform.eulerAngles.y),_minValue,_maxValue);
                    if (debug)
                    {
                        DebugUI.Log(string.Format("{0} Update: <br>   EulerAngles.y={1} newValue={2}", gameObject.name, rbTransform.eulerAngles.y, newValue));
                    }
                    if (newValue != _value)
                    {
                        if (debug)
                            DebugUI.Log(string.Format("newValue{0} != _value={1}",newValue,_value));
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
