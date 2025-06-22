using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlendMeshAdjust : MonoBehaviour
{
    public SkinnedMeshRenderer smr;
    public float multiplier = 3;

    void Update()
    {
        var parentScale = transform.parent.transform.localScale;

        this.transform.localScale = new Vector3(5.556f / parentScale.x, 5.556f / parentScale.y, 5.556f / parentScale.y);
        
        smr.SetBlendShapeWeight(0, 800 * parentScale.x * multiplier);
        smr.SetBlendShapeWeight(1, 800 * parentScale.y * multiplier);
        smr.SetBlendShapeWeight(2, 800 * parentScale.z * multiplier);
    }
}
