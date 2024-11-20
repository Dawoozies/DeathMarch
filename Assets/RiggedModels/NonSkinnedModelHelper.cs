using UnityEngine;

public class NonSkinnedModelHelper : MonoBehaviour
{
    public Transform ArmatureToFindBoneIn;
    [ContextMenu("SetUp")]
    void SetUp()
    {
        string boneName = gameObject.name;
        boneName = boneName.Remove(0,2);
        Debug.Log($"Substring = {boneName}");
        Transform[] allBones = ArmatureToFindBoneIn.GetComponentsInChildren<Transform>();
        Transform foundBone = null;
        foreach(var bone in allBones)
        {
            if(bone.gameObject.name.Contains(boneName))
            {
                foundBone = bone;
                break;
            }
        }
        transform.SetParent(foundBone, true);
        DestroyImmediate(this);
    }
}
