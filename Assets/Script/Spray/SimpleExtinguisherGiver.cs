// 이 파일은 DebugExtinguisherGiver.cs로 대체되었습니다.
// 삭제해도 됩니다.

using UnityEngine;

[System.Obsolete("Use DebugExtinguisherGiver instead")]
public class SimpleExtinguisherGiver : MonoBehaviour
{
    void Awake()
    {
        Debug.LogError("[SimpleExtinguisherGiver] This script is obsolete! Use DebugExtinguisherGiver instead.");
        enabled = false;
    }
}
