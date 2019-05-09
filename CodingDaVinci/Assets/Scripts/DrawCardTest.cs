using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawCardTest : MonoBehaviour
{
    void Update()
    {
        if(Input.GetKey("d"))
        {
            transform.Rotate(1, 0, 0);
        }
        if(Input.GetKey("a"))
        {
            transform.Rotate(-1, 0, 0);
        }
    }
}
