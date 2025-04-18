using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructionTimer : MonoBehaviour
{
    public float destructionTimer = 1f;

    // Update is called once per frame
    void Update()
    {
        destructionTimer -= Time.deltaTime;

        if (destructionTimer <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void SetTimer(float time)
    {
        destructionTimer = time;
    }
}
