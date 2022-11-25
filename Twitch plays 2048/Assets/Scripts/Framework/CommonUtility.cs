using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommonUtility : MonoBehaviour
{
    public static Color SetAlpha(Color c, float a)
    {
        c.a = a;
        return c;
    }
}
