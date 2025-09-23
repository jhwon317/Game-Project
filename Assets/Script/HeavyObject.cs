using UnityEngine;

// 이 스크립트는 '무거운' 속성을 부여하는 역할만 합니다.
public class HeavyObject : MonoBehaviour
{
    [Header("무게 설정")]
    [Tooltip("이 물건을 들었을 때 플레이어 속도가 얼마나 줄어들지 정합니다. (예: 0.8 = 80% 속도)")]
    [Range(0.1f, 1f)]
    public float speedModifier = 0.8f; // 기본값은 80% 속도
}
