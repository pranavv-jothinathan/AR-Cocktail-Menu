using UnityEngine;
public class LiquidLayerData : MonoBehaviour
{
    public string layerName;
    [TextArea] public string layerDescription;
    public Color titleColor = Color.white;

    public string topLayerName;
    [TextArea] public string topLayerDescription;
    public Color topLayerTitleColor = Color.white;
}