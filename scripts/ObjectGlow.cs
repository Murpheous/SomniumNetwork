using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SyncedControls.Example
{
    [RequireComponent(typeof(MeshRenderer))]
    public class ObjectGlow : MonoBehaviour
    {

        [SerializeField]
        private Color glowColor = Color.yellow; // Default glow color
        [SerializeField] 
        private PhotonSyncedSlider glowSlider;
        [SerializeField]
        private bool debug = false; // Enable or disable debug logging
        private bool hasEmission = false;
        [SerializeField,Range(0f,1f)]
        private float glowIntensity = 1.0f; // Default intensity
        private MeshRenderer meshRenderer;
        private Material glowMaterial;


        public Color GlowColor
        {
            get { return glowColor; }
            set
            {
                glowColor = value;
                UpdateColor();
            }
        }

        public float GlowIntensity
        {
            get { return glowIntensity; }
            set
            {
                glowIntensity = Mathf.Clamp01(value);
                UpdateColor();
            }
        }
        private void UpdateColor()
        {
            //if (debug) // Debugging output - can spam the console
            //    DebugUI.Log($"{gameObject.name} UpdateColor: Intensity={glowIntensity}");
            if (glowMaterial == null)
                return;
            Color emissionColor = glowColor * glowIntensity;
            if (hasEmission )
            {
                glowMaterial.SetColor("_EmissionColor", emissionColor);
                //DynamicGI.SetEmissive(objectRenderer, emissionColor);
            }
            else
            {
                glowMaterial.SetColor("_Color", glowColor);
            }

        }
        void Awake()
        {
            if (glowSlider != null)
            {
                glowSlider.SetLimits(0f, 1f);
                glowSlider.SetValue(glowIntensity);
                glowSlider.Interactable = true;
                glowSlider.DisplayScale = 100f;
                glowSlider.TypeText = gameObject.name + "\nGlow";
                glowSlider.ValueSuffix = "%";
            }
            meshRenderer = GetComponent<MeshRenderer>();
            // Ensure the object has a Renderer component
            if (meshRenderer == null)
            {
                DebugUI.LogError("ObjectGlow requires a Renderer component.");
                return;
            }
            glowMaterial = meshRenderer.material;
            if (glowMaterial == null)
            {
                DebugUI.LogError("ObjectGlow requires a material to be assigned.");
                return;
            }
            if (glowMaterial.HasProperty("_EmissionColor"))
            {
                DebugUI.Log($"ObjectGlow: {gameObject.name} supports emission.");
                glowMaterial.EnableKeyword("_EMISSION");
                hasEmission = true;
            }
            else
            {
                DebugUI.LogWarning("The assigned material does not support emission.");
                hasEmission = false;
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            UpdateColor();
        }
    }
}
