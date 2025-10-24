using UnityEngine;
using UnityEngine.Rendering;

namespace Game.Level
{
    [ExecuteAlways]
    public class MobileLightingSetup : MonoBehaviour
    {
        [Header("Ambient Light")]
        public Color ambientColor = new Color(0.66f, 0.72f, 0.82f);
        public AmbientMode ambientMode = AmbientMode.Flat;

        [Header("Fog")] 
        public bool enableFog = true;
        public Color fogColor = new Color(0.74f, 0.86f, 0.94f);
        public FogMode fogMode = FogMode.Linear;
        public float fogStartDistance = 15f;
        public float fogEndDistance = 90f;

        [Header("Shadows")]
        public LightShadows directionalLightShadows = LightShadows.Hard;
        public float shadowDistance = 55f;

        [Header("Runtime")]
        public bool applyOnEnable = true;

        private void OnEnable()
        {
            if (applyOnEnable)
            {
                Apply();
            }
        }

        private void OnValidate()
        {
            Apply();
        }

        public void Apply()
        {
            RenderSettings.ambientMode = ambientMode;
            RenderSettings.ambientLight = ambientColor;

            RenderSettings.fog = enableFog;
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogStartDistance = fogStartDistance;
            RenderSettings.fogEndDistance = fogEndDistance;

            QualitySettings.shadowDistance = shadowDistance;

            var directionalLight = RenderSettings.sun;
            if (directionalLight != null)
            {
                directionalLight.shadows = directionalLightShadows;
                directionalLight.shadowStrength = 0.65f;
                directionalLight.shadowBias = 0.006f;
                directionalLight.shadowNormalBias = 0.3f;
            }
        }
    }
}
