using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

[RequireComponent(typeof(ARCameraManager))]
public class Light1 : MonoBehaviour
{
    private bool _isInfoTextPositionAdjusted;

    private float _lastAppliedIntensity;
    [SerializeField] private ARCameraManager _arCameraManager;
    [SerializeField] private Light _light; //virtual directionla light
    
    [SerializeField] private TMP_Text _cameraInfoText; // debug, if ok, trun off this and camera canvas

    
    [SerializeField] private bool _IntenColor = true;
    [SerializeField] private bool _ifDirection = true;

    
    [SerializeField] private float _intensityMin = 0.05f;
    [SerializeField] private float _intensityMax = 3.5f;

    private Vector3? _targetDirection;
    private Vector3? _lastMainLightDirection;

    [SerializeField] private float _brightnesslow = 0.2f;
    [SerializeField] private float _brightnesshigh = 0.7f;
    private string _lastIntensitySource = "No";
    private string _currentLightingMode = "No";
    private float _lastRawIntensity;
    [SerializeField] private float _Gamma = 1.2f;

    [SerializeField] private float _mainLightLumensLow = 200f;
    [SerializeField] private float _mainLightLumensHigh = 2000f;

    [SerializeField] private float _mainLightColorIntensityScale = 1.5f;
    [Range(0.05f, 2.0f)]
    [SerializeField] private float _Scale = 0.3f;

    
    
    [SerializeField] private float _textPixels = 20f;
    
    [Range(0.1f, 100f)]
    [SerializeField] private float _smoothSpeed = 100f;

    
    
    
    private RectTransform _cameraInfoRect;
    private Vector2 _cameraInfoOriginalAnchoredPos;
    

    private void Awake()
    {
        
        if (_arCameraManager == null) _arCameraManager = GetComponent<ARCameraManager>();
    }

    private void OnEnable()
    {
        AdjustInfoTextPosition(true);

        if (_arCameraManager != null)
        {
            EnsureHdrLightEstimationRequested();
            _arCameraManager.frameReceived += OnCameraFrameReceived;
        }
    }

    private void OnDisable()
    {
        AdjustInfoTextPosition(false);

        if (_arCameraManager != null)
            _arCameraManager.frameReceived -= OnCameraFrameReceived;
    }

    private void Update()
    {
        
        if (_ifDirection && _light != null && _targetDirection.HasValue)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_targetDirection.Value); //direction change, due to camera, it is slow
            _light.transform.rotation = Quaternion.Slerp(_light.transform.rotation, targetRotation, Time.deltaTime * _smoothSpeed);
        }
    }

    private void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        var lightEstimation = args.lightEstimation;
        _currentLightingMode = "HDR";

        if (_light != null)
        {
            
            if (_IntenColor)
            {
                float? brightnessSource = null; // debug, if have source
                string intensitySourceLabel = null;
                if (lightEstimation.mainLightIntensityLumens.HasValue) // mainlight not ambient light
                {
                    float lumens = Mathf.Max(0f, lightEstimation.mainLightIntensityLumens.Value);
                    float t = Mathf.InverseLerp(_mainLightLumensLow, Mathf.Max(_mainLightLumensLow + 0.001f, _mainLightLumensHigh), lumens);
                    t = Mathf.Pow(t, Mathf.Max(0.01f, _Gamma));
                    SetVirtualLightIntensity(Mathf.Lerp(_intensityMin, _intensityMax, t), "IntenLumens");
                }
                else if (lightEstimation.averageMainLightBrightness.HasValue)
                {
                    brightnessSource = lightEstimation.averageMainLightBrightness.Value;
                    intensitySourceLabel = "MainBrightness";
                }
                else if (lightEstimation.mainLightColor.HasValue)
                {
                    Color mainColor = lightEstimation.mainLightColor.Value;
                    float colorIntensity = Mathf.Max(mainColor.r, Mathf.Max(mainColor.g, mainColor.b));
                    float scaled = Mathf.Clamp01(colorIntensity * Mathf.Max(0.01f, _mainLightColorIntensityScale));
                    SetVirtualLightIntensity(Mathf.Lerp(_intensityMin, _intensityMax, scaled), "mainColor ");
                }

                if (brightnessSource.HasValue)
                {
                    float brightness = Mathf.Max(0f, brightnessSource.Value);
                    float t = Mathf.InverseLerp(_brightnesslow, Mathf.Max(_brightnesslow + 0.001f, _brightnesshigh), brightness);
                    t = Mathf.Pow(t, Mathf.Max(0.01f, _Gamma));
                    SetVirtualLightIntensity(Mathf.Lerp(_intensityMin, _intensityMax, t), intensitySourceLabel);
                }

                if (lightEstimation.averageColorTemperature.HasValue) // no value, unless ambient
                {
                    _light.colorTemperature = lightEstimation.averageColorTemperature.Value;
                }

                if (lightEstimation.mainLightColor.HasValue)
                {
                    _light.color = lightEstimation.mainLightColor.Value;
                }
                else if (lightEstimation.colorCorrection.HasValue)
                {
                    _light.color = lightEstimation.colorCorrection.Value;
                }
            }

            
            if (_ifDirection && lightEstimation.mainLightDirection.HasValue)
            {
                _targetDirection = lightEstimation.mainLightDirection.Value; // get camera direction data, slow
                _lastMainLightDirection = lightEstimation.mainLightDirection.Value;
            }
            else if (!_ifDirection)
            {
                _targetDirection = null;
            }
        }

        
        

        //UpdateCameraInfoText(lightEstimation);
    }

    private void EnsureHdrLightEstimationRequested()
    {
        if (_arCameraManager == null)
            return;

        var required =
            LightEstimation.MainLightDirection |
            LightEstimation.MainLightIntensity |
            LightEstimation.AmbientSphericalHarmonics;

        _arCameraManager.requestedLightEstimation |= required;
    }

    private void SetVirtualLightIntensity(float rawIntensity, string source) // need stronger change for effect by hands cover
    {
        if (_light == null)
            return;

        _lastRawIntensity = rawIntensity;
        _lastAppliedIntensity = rawIntensity * Mathf.Max(0f, _Scale);
        _light.intensity = _lastAppliedIntensity;
        _lastIntensitySource = source;
    }

    private void AdjustInfoTextPosition(bool moveUp)
    {
        if (_cameraInfoText == null)
            return;

        if (_cameraInfoRect == null)
            _cameraInfoRect = _cameraInfoText.rectTransform;
        if (_cameraInfoRect == null)
            return;

        if (moveUp && !_isInfoTextPositionAdjusted)
        {
            _cameraInfoOriginalAnchoredPos = _cameraInfoRect.anchoredPosition;
            _cameraInfoRect.anchoredPosition = _cameraInfoOriginalAnchoredPos + Vector2.up * _textPixels;
            _isInfoTextPositionAdjusted = true;
        }
        else if (!moveUp && _isInfoTextPositionAdjusted)
        {
            _cameraInfoRect.anchoredPosition = _cameraInfoOriginalAnchoredPos;
            _isInfoTextPositionAdjusted = false;
        }
    }

    //private void UpdateCameraInfoText(ARLightEstimationData lightEstimation)
    //{
    //    if (_cameraInfoText == null) return;

    //    string brightnessValue = lightEstimation.mainLightIntensityLumens.HasValue
    //        ? lightEstimation.mainLightIntensityLumens.Value.ToString("F1") + " lm"
    //        : lightEstimation.averageMainLightBrightness.HasValue
    //        ? lightEstimation.averageMainLightBrightness.Value.ToString("F4") + " (main)"
    //        : lightEstimation.mainLightColor.HasValue
    //        ? Mathf.Max(lightEstimation.mainLightColor.Value.r, Mathf.Max(lightEstimation.mainLightColor.Value.g, lightEstimation.mainLightColor.Value.b)).ToString("F3") + " (color)": "N/A";

    //    string dirValue;
    //    if (!_ifDirection)
    //    {
    //        dirValue = "Disabled";
    //    }
    //    else if (lightEstimation.mainLightDirection.HasValue)
    //    {
    //        Vector3 dir = lightEstimation.mainLightDirection.Value;
    //        dirValue = "x:" + dir.x.ToString("F3") + ", y:" + dir.y.ToString("F3") + ", z:" + dir.z.ToString("F3");
    //    }
    //    else if (_lastMainLightDirection.HasValue)
    //    {
    //        Vector3 dir = _lastMainLightDirection.Value;
    //        dirValue = "x:" + dir.x.ToString("F3") + ", y:" + dir.y.ToString("F3") + ", z:" + dir.z.ToString("F3") + " (cached)";
    //    }
    //    else
    //    {
    //        dirValue = "N/A";
    //    }

    //    string colorTempValue = lightEstimation.averageColorTemperature.HasValue // color seems no value if use mainlight
    //        ? lightEstimation.averageColorTemperature.Value.ToString("F1")
    //        : "N/A";
    //    string virtualLightIntensity = _light != null ? _light.intensity.ToString("F3") : "N/A";
    //    string virtualLightDir = "N/A";
    //    if (_light != null)
    //    {
    //        Vector3 dir = _light.transform.forward;
    //        virtualLightDir = "x:" + dir.x.ToString("F3") + ", y:" + dir.y.ToString("F3") + ", z:" + dir.z.ToString("F3");
    //    }

    //    _cameraInfoText.text =
    //        "Session State: " + ARSession.state + "\n" +
    //        "Lighting Mode: " + _currentLightingMode + "\n" +
    //        "Intensity/Color: " + (_IntenColor ? "On" : "Off") + "\n" +
    //        "Direction: " + (_ifDirection ? "On" : "Off") + "\n" +
    //        "Main Light Dir: " + dirValue + "\n" +
    //        "Virtual Light Dir: " + virtualLightDir + "\n" +
    //        "Captured Brightness: " + brightnessValue + "\n" +
    //        "Intensity Source: " + _lastIntensitySource + "\n" +
    //        "Mapping Scale: x" + _Scale.ToString("F3") + "\n" +
    //        "Raw Intensity: " + _lastRawIntensity.ToString("F3") + "\n" +
    //        "Virtual Light Intensity: " + virtualLightIntensity + "\n" +
    //        "Applied Intensity: " + _lastAppliedIntensity.ToString("F3") + "\n" +
    //        "Color Temp: " + colorTempValue;
        // canvas to show
        //environment hdr has conflict with ambient model
    //}
}
