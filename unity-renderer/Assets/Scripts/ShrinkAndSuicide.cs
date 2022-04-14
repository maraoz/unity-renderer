using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShrinkAndSuicide : MonoBehaviour
{
    public float shrinkFactor = 0.9f;

    void Start()
    {
        Destroy(gameObject, 2);
    }
    void Update()
    {
        transform.localScale *= shrinkFactor;
    }
}