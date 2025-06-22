using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBack : MonoBehaviour
{
    private Transform child;
    // Start is called before the first frame update
    void Start()
    {
        child = this.transform.GetChild(0);
    }

    // Update is called once per frame
    void Update()
    {
        child.transform.localPosition = new Vector3(0, 0, -this.transform.localPosition.z);
    }
}
