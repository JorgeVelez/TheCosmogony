using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrintVersion : MonoBehaviour
{
    void Start()
    {
        gameObject.GetComponent<Text>().text="V" + Application.version;
    }

    void Update()
    {
        
    }
}
