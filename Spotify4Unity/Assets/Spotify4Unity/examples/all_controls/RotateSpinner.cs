using UnityEngine;

/// <summary>
/// Simple script to rotate an object around
/// </summary>
public class RotateSpinner : MonoBehaviour
{
    [Tooltip("How fast should the object rotate")]
    public float Speed = 1f;
    private void Update()
    {
        if(this.gameObject.activeInHierarchy)
        {
            this.transform.Rotate(new Vector3(0, 0, 1 * Speed));
        }
    }
}
