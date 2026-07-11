using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class IconLayoutPartOverride
{
    public string partName;
    public Vector3 localPosition;
    public Vector3 localEulerAngles;
}

[Serializable]
public class IconLayoutOverrideSet
{
    public string key;
    public Vector3 stageLocalEuler;
    public IconLayoutPartOverride[] parts = Array.Empty<IconLayoutPartOverride>();
}

[Serializable]
public class IconLayoutOverrideFile
{
    public IconLayoutOverrideSet[] sets = Array.Empty<IconLayoutOverrideSet>();
}
