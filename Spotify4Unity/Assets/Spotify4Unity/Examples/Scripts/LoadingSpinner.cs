using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingSpinner : MonoBehaviour
{
    [Tooltip("How fast should the object rotate")]
    public float Speed = 1f;
    private void Update()
    {
        if (this.gameObject.activeInHierarchy)
        {
            this.transform.Rotate(new Vector3(0, 0, -1 * Speed));
        }
    }
}
