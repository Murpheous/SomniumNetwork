using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace SyncedControls.Example
{
    [RequireComponent(typeof(Toggle))]
    public class SyncedToggle : MonoBehaviour
    {
        [SerializeField]
        Toggle toggle;
        [SerializeField]
        Transform syncedTransform;

        public UnityEvent<bool> onValueChanged;

        [Header("Just here to see in inspector")]
        [SerializeField]
        private bool _localState = false;
        [SerializeField]
        private bool _reportedState = false;
        //[SerializeField] 
        private NetworkObject networkObject;
        [SerializeField] bool debug = false;
        public bool isOn 
        { 
            get 
            {
                if (syncedTransform == null)
                    return _localState;
                return syncedTransform.localEulerAngles.y != 0;
            } 
            set 
            {
                if (syncedTransform == null)
                    return;
                NetworkState = value; // Set the Rigidbody's state
            }
        }

        public void SetIsOnWithoutNotify(bool value)
        {
            if (syncedTransform == null)
            {
                _localState = value;
                toggle.SetIsOnWithoutNotify(value);
                return;
            }
            NetworkState = value;
            _reportedState = value;
        }

        private bool NetworkState 
        { 
            get 
            {
                if (syncedTransform == null)
                    return _localState;
                return syncedTransform.localEulerAngles.y != 0;
            }
            set 
            {
                if (syncedTransform == null)
                    return;
                if (debug) 
                    DebugUI.Log(string.Format("{0} NetworkState({1})", gameObject.name, value));
                if (networkObject != null && (networkObject.Runner != null))
                {
                    if (networkObject.StateAuthority != networkObject.Runner.LocalPlayer)
                        networkObject.RequestStateAuthority();
                }
                syncedTransform.localEulerAngles = new Vector3(syncedTransform.localEulerAngles.x, value ? 180 : 0, syncedTransform.localEulerAngles.z);
            }        
        }

        void Awake()
        {   
            if (onValueChanged == null)
                onValueChanged = new UnityEvent<bool>();
            if (toggle == null)
                toggle = GetComponent<Toggle>();
            _localState = toggle.isOn;
            if (syncedTransform == null)
                syncedTransform = GetComponentInChildren<NetworkTransform>().transform;
            if (syncedTransform != null)
                networkObject = syncedTransform.GetComponent<NetworkObject>();
        }

        private void Start()
    
        {
            // Initialize the Rigidbody's state based on the toggle's initial state            
            syncedTransform.localEulerAngles = new Vector3(syncedTransform.localEulerAngles.x, _localState ? 180 : 0, syncedTransform.localEulerAngles.z);
            _reportedState = _localState; // Initialize reported state
            onValueChanged.Invoke(_localState); // Invoke the event with the initial state
        }

        public void onPointerEnter()
        {
            if (networkObject != null && (networkObject.Runner != null))
            {
                if (networkObject.StateAuthority != networkObject.Runner.LocalPlayer)
                    networkObject.RequestStateAuthority();
            }
        }
        private void onToggleValue(bool isOn)
        {
            _localState = isOn;
            if (syncedTransform != null)
            {
                if (NetworkState != isOn)
                    NetworkState = isOn; // Update the Rigidbody's state
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
