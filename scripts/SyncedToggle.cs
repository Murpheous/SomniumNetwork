using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
#if NO_FUSION_SYNC
#else
using Fusion;
#endif

namespace SyncedControls.Example
{
    [RequireComponent(typeof(Toggle))]
    public class SyncedToggle : MonoBehaviour
    {
        [SerializeField]
        Toggle toggle;
        [SerializeField]
        Transform syncedTransform;
        [SerializeField]
        private UnityEvent<bool> onValueChanged;
        public UnityEvent<bool> OnValueChanged
        {
            get => onValueChanged;
        }

        [Header("Just here to see in inspector")]
        [SerializeField]
        private bool _localState = false;
        [SerializeField]
        private bool _reportedState = false;
#if NO_FUSION_SYNC
        // No Fusion
        private void RequestNetAuthority()
        {
        }
        private void checkNetworkObjects()
        {
            if (syncedTransform == null)
            {
                DebugUI.Log($"{gameObject.name} syncedTransform not set");
                Rigidbody rb = GetComponentInChildren<Rigidbody>();
                if (rb != null)
                    syncedTransform = rb.transform;
                else
                    DebugUI.Log($"{gameObject.name} no rigidbody");
            }
        }
#else
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

            if (syncedTransform == null)
            {
                DebugUI.Log($"{gameObject.name} syncedTransform not set");
                Rigidbody rb = GetComponentInChildren<Rigidbody>();
                if (rb != null)
                    syncedTransform = rb.transform;
                else
                    DebugUI.Log($"{gameObject.name} no rigidbody");
            }

            if (syncedTransform != null)
            {
                if (networkObject == null)
                    networkObject = syncedTransform.GetComponent<NetworkObject>();
            }
            if (networkObject == null)
            {
                DebugUI.LogWarning($"{gameObject.name}: No NetworkObject found");
            }
        }
#endif
        [SerializeField] bool debug = false;
        public bool isOn
        {
            get => NetworkState;
            set
            {
                NetworkState = value; // Set the Rigidbody's state
            }
        }

        public void SetIsOnWithoutNotify(bool value)
        {
            NetworkState = value;
            _reportedState = value;
        }

        private bool NetworkState
        {
            get
            {
                if (syncedTransform == null)
                    return toggle.isOn;
                return syncedTransform.localEulerAngles.y != 0;
            }
            set
            {
                if (syncedTransform == null)
                {
                    _localState = value;
                    toggle.SetIsOnWithoutNotify(value);
                    return;
                }
                if (debug)
                    DebugUI.Log(string.Format("{0} NetworkState({1})", gameObject.name, value));
                RequestNetAuthority();
                syncedTransform.localEulerAngles = new Vector3(0, value ? 180 : 0, 0);
            }
        }

        private void OnValidate()
        {
            if (toggle == null)
                toggle = GetComponent<Toggle>();
        }
        void Awake()
        {
            checkNetworkObjects();
            if (onValueChanged == null)
                onValueChanged = new UnityEvent<bool>();
            if (toggle == null)
                toggle = GetComponent<Toggle>();
            _localState = toggle.isOn;
            if (syncedTransform != null)
            {
                syncedTransform.localEulerAngles = new Vector3(0, _localState ? 180 : 0, 0);
            }
        }

        public void onPointerEnter()
        {
            RequestNetAuthority();
        }
        private void onToggleValue(bool isOn)
        {
            _localState = isOn;
            if (syncedTransform != null)
            {
                if (NetworkState != isOn)
                {
                    NetworkState = isOn; // Update the Rigidbody's state
                }
            }
            else
            {
                onValueChanged.Invoke(isOn);
            }
        }
        void OnEnable()
        {
            toggle.onValueChanged.AddListener(onToggleValue);
        }

        void OnDisable()
        {
            toggle.onValueChanged.RemoveListener(onToggleValue);
        }

        // Update is called once per frame
        void Update()
        {
            if (syncedTransform != null)
            {
                if (syncedTransform.hasChanged)
                { // Check if the rigidbody has moved from its last synced position
                    syncedTransform.hasChanged = false;
                    bool newValue = Mathf.Abs(syncedTransform.localEulerAngles.y) > 90;
                    if (newValue != toggle.isOn)
                    {
                        toggle.SetIsOnWithoutNotify(newValue); // Update the toggle state without invoking the event
                        _localState = newValue; // Update the local state
                    }
                    if (_reportedState != newValue)
                    {
                        onValueChanged.Invoke(newValue); // Invoke the event with the new state
                        _reportedState = newValue; // Update the reported state
                    }
                }
            }
        }
    }
}
