using UnityEngine;

public class LineOfSight : MonoBehaviour
{
    public Material material;
    void Update()
    {
        Vector3 revealCenter = transform.position;
        revealCenter.y = 0f;
        material.SetVector("_RevealCenter", revealCenter);
    }
}
