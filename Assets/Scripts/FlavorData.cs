using System.Diagnostics;
using UnityEngine;

public class FlavorData : MonoBehaviour
{
    public string cocktailName;
    [TextArea] public string cocktailDescription;
    [Range(0,1)] public float sour = 0.8f;
    [Range(0,1)] public float sweet = 0.2f;
    [Range(0,1)] public float bitter = 0.8f;
    [Range(0,1)] public float strong = 0.8f;

    public float[] GetValuesArray()
    {
        return new float[] {sour, sweet, bitter, strong };
    }
}


