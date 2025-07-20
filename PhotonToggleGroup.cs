using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace QuantumScatter
{
    [RequireComponent(typeof(ToggleGroup))]
    public class PhotonToggleGroup : MonoBehaviour
    {
        [SerializeField]
        Rigidbody networkedRigidBody;
        [SerializeField] ToggleGroup togGroup;
        [SerializeField] Toggle[] toggles;

        public integerEvent onSelectionChanged;

        [Header("Just here to see in inspector")]
        [SerializeField]
        private Transform rbTransform;
        [SerializeField]
        private int _localState = -1;
        [SerializeField]
        private int _reportedState = -1;
        [SerializeField]
        private NetworkObject networkObject;
        private int numToggles = 0;
        [SerializeField]
        private bool debug = false;
        private bool started = false;
        public int State 
        {
            get => _reportedState;
            set
            {
                if (!started)
                {
                    _localState = value;
                    _reportedState = value;
                }
            }
        }

        private int rbState
        {
            get
            {
                if (rbTransform == null)
                    return _localState;
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
        public void onValueChanged(bool value)
        {
            if (toggles == null || toggles.Length == 0)
            {
                Debug.LogError("Toggles array is empty or not assigned.");
                return;
            }
            if (!togGroup.allowSwitchOff && !value)
                return; // If toggles cannot be switched off, ignore this event if value is false
            int activeToggle = -1;
            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i] != null)
                {
                    if (value && toggles[i].isOn)
                    {
                        activeToggle = i;
                        if (debug)
                            DebugUI.Log(string.Format("{0} onValueChanged:Toggle[{1}]=ON", gameObject.name, i));
                        break;
                    }
                }
            }
            if (debug)
                DebugUI.Log(string.Format("activeToggle={0}",activeToggle));
            if (activeToggle != _localState)
            {
                _localState = activeToggle;
                rbState = activeToggle;
            }
        }

        void Awake()
        {
            togGroup = GetComponent<ToggleGroup>();
            onSelectionChanged = new integerEvent();
            if (togGroup == null)
                togGroup = GetComponent<ToggleGroup>();
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
            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i] == null)
                {
                    Debug.LogError("Toggle at index " + i + " is null. Please assign all toggles in the inspector.");
                    continue;
                }
                toggles[i].group = togGroup;
                toggles[i].SetIsOnWithoutNotify(i == _reportedState);
                toggles[i].onValueChanged.AddListener(onValueChanged);
            }
            numToggles = toggles.Length > 0 ? toggles.Length : 1;
            rbState = _reportedState;
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
                    int newValue = Mathf.RoundToInt(rbTransform.eulerAngles.y);
                    if (debug)
                    {
                        DebugUI.Log(string.Format("{0} Update: <br>   EulerAngles.y={1} newValue={2}", gameObject.name, rbTransform.eulerAngles.y, newValue));
                    }
                    if (newValue != _localState)
                    {
                        if (debug)
                            DebugUI.Log(string.Format("newValue{0} != _localState={1}",newValue,_localState));
                        if (_localState >= 0 && _localState < numToggles && toggles[_localState] != null)
                            toggles[_localState].SetIsOnWithoutNotify(false); // Update the previous toggle state without invoking the event
                        if (newValue >= 0 && newValue < numToggles && toggles[newValue]!=null)
                            toggles[newValue].SetIsOnWithoutNotify(true); // Update the toggle state without invoking the event
                        _localState = newValue; // Update the local state
                    }
                    if (_reportedState != newValue && newValue < numToggles)
                    {
                        onSelectionChanged.Invoke(newValue); // Invoke the event with the new state
                        _reportedState = newValue; // Update the reported state
                    }
                }
            }
        }
    }
}
