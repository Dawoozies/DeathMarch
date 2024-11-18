using UnityEngine;

public class LineOfSight : MonoBehaviour
{
    public Material material;
    public Transform fogPlane;
    void Update()
    {
        Vector3 revealCenter = transform.position;
        fogPlane.position = transform.position;
        //revealCenter.y = 0f;
        material.SetVector("_RevealCenter", revealCenter);
    }
}
