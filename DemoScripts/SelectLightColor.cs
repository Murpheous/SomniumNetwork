using UnityEngine;

namespace SyncedControls.Example
{
    [RequireComponent(typeof(Light))]


    public class SelectLightColor : MonoBehaviour
    {
        [SerializeField] private Light _light;
        [SerializeField] private SyncedToggleGroup selector;
        [SerializeField] private int _colorIndex = 0;

        [SerializeField] private Color[] lightColors = new Color[]
        {
            Color.red,
            Color.green,
            Color.blue,
            Color.yellow,
            Color.cyan,
            Color.magenta
        };

        
        private void Awake()
        {
            if (_light == null)
                _light = GetComponent<Light>();
        }

        public int ColorIndex

        {
            get => _colorIndex;
            set
            {
                if (value < 0 || value >= lightColors.Length)
                {
                    DebugUI.LogWarning($"Index {value} is out of range for light colors array.");
                    return;
                }
                _colorIndex = value;
                _light.color = lightColors[_colorIndex];
            }
        }

        void OnEnable()
        {
            if (selector != null)
            {
                ColorIndex = selector.State;
                selector.onSelectionChanged.AddListener((v) => ColorIndex = v);
            }

        }

        void OnDisable()
        {
            if (selector != null)
            {
                selector.onSelectionChanged.RemoveListener((v) => ColorIndex = v);
            }
        }

        void Start()
        {
            ColorIndex = _colorIndex;        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    }
}
