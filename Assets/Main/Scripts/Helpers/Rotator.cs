using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class Rotator : MonoBehaviour
{
    public bool XAxis=false;
    public float XSpeed=1;
    public bool YAxis=false;
    public float YSpeed=1;
    public bool ZAxis=false;
    public float ZSpeed=1;


bool  GlobalStop=true;

    void Start()
    {
        
    }

    void Update()
    {
        if(!GlobalStop)
        transform.Rotate(XAxis?XSpeed:0, YAxis?YSpeed:0, ZAxis?ZSpeed:0, Space.Self);

    }
}
