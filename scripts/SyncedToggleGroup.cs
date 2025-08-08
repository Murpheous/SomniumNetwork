using Fusion;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace SyncedControls.Example
{
    [RequireComponent(typeof(ToggleGroup))]
    public class SyncedToggleGroup : MonoBehaviour
    {
        [SerializeField]
        Transform syncedTransform;
        [SerializeField] ToggleGroup togGroup;
        [SerializeField] Toggle[] toggles;

        public UnityEvent<int> onSelectionChanged;

        [Header("Just here to see in inspector")]
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
                if (syncedTransform == null)
                    return _localState;
                return Mathf.RoundToInt(syncedTransform.eulerAngles.y);
            }
            set
            {
                if (syncedTransform == null)
                    return;
                if (networkObject != null && (networkObject.Runner != null))
                {
                    if (!networkObject.HasStateAuthority)
                        networkObject.RequestStateAuthority();
                }
                syncedTransform.eulerAngles = (new Vector3(0, value, 0));
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
        public void OnValueChanged(bool value)
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
                            DebugUI.Log(string.Format("{0} OnValueChanged:Toggle[{1}]=ON", gameObject.name, i));
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
            if (onSelectionChanged == null)
                onSelectionChanged = new UnityEvent<int>();
            if (togGroup == null)
                togGroup = GetComponent<ToggleGroup>();
            if (syncedTransform == null)
                syncedTransform = GetComponentInChildren<NetworkTransform>().transform;
            if (syncedTransform != null)
            {
                networkObject = syncedTransform.GetComponent<NetworkObject>();
            }
        }

        public void Start()
        {
            numToggles = toggles.Length > 0 ? toggles.Length : 1;
            rbState = _reportedState;
            onSelectionChanged.Invoke(_reportedState); // Invoke the event with the new state
            started = true;
        }

        void OnEnable()
        {
            int nSelected =  0;
            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i] == null)
                {
                    DebugUI.LogError($"{gameObject.name}:Toggle at index {i} is null.");
                    continue;
                }
                toggles[i].group = togGroup;
                if (toggles[i].isOn)
                    nSelected = i; // Set i to selected toggle
                toggles[i].onValueChanged.AddListener(OnValueChanged);
            }
            _localState = nSelected; // Set the local state to the selected toggle index
            _reportedState = nSelected; // Set the reported state to the selected toggle index
        }

        void OnDisable()
        {
            for (int i = 0; i < toggles.Length; i++)
            {
                if (toggles[i] == null)
                {
                    continue;
                }
                toggles[i].onValueChanged.RemoveListener(OnValueChanged);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (syncedTransform != null)
            {
                if (syncedTransform.hasChanged)
                { // Check if the rigidbody has moved from its last synced position
                    syncedTransform.hasChanged = false;
                    int newValue = Mathf.RoundToInt(syncedTransform.eulerAngles.y);
                    if (debug)
                    {
                        DebugUI.Log($"{gameObject.name} Update: <br>   EulerAngles.y={syncedTransform.eulerAngles.y} newValue={newValue}");
                    }
                    if (newValue != _localState)
                    {
                        if (debug)
                            DebugUI.Log($"newValue{newValue} != _localState={_localState}");
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
