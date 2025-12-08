using Unity.VisualScripting;
using UnityEngine;


[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    //Referencias
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private Light nightDirectionalLight;
    [SerializeField] private LightingPreset Preset;
    //Variables
    [SerializeField, Range(0, 24)] private float TimeOfDay;
    [SerializeField] private float cicleSpeed = 1;



    private void Update()
    {
        if(Preset == null)
        {
            return;
        }
        if (Application.isPlaying)
        {
            TimeOfDay += Time.deltaTime / (86400 * (1 / cicleSpeed));
            TimeOfDay %= 24; //Clamp de 0 a 24
            UpdateLighting(TimeOfDay /24);
        }
        else
        {
            UpdateLighting(TimeOfDay/24);
        }
    }
    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);
        RenderSettings.fogDensity = Oscillate(timePercent, 0.001f, 0.005f, 1f);
        if(DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);

            DirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0));
            if (nightDirectionalLight != null){
                nightDirectionalLight.transform.localRotation = Quaternion.Euler(new Vector3(((timePercent+0.5f) * 360f) - 90f, 170f, 0));
            }
            }
    }
    private void OnValidate()
    {
        if (DirectionalLight != null)
            return;
        if (RenderSettings.sun != null)
        {
            DirectionalLight.enabled = RenderSettings.sun;
        }
        else
        {
            Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }

            }
        }
    }
    float Oscillate(float time, float minValue, float maxValue, float timeOfPeak)
    {
        float amplitude = (maxValue - minValue) / 2f;
        float midpoint = (maxValue + minValue) / 2f;
        float phase = Mathf.PI / 2f - 2f * Mathf.PI * timeOfPeak;

        return midpoint + amplitude * Mathf.Sin(2f * Mathf.PI * time + phase);
    }

}
