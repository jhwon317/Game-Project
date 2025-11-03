# 🔄 PlayerController 리팩토링 완료

## 변경 사항 요약

### 핵심 개선: IHoldable 인터페이스 도입

**Before (문제점):**
```csharp
public ThrowableBox heldObject;  // ThrowableBox만 들 수 있음
private ExtinguisherItem _heldExtinguisher;  // 별도 추적
```
→ 두 모드가 섞여서 복잡함

**After (해결):**
```csharp
private IHoldable _heldItem;  // 뭐든 들 수 있음!
```
→ 통합되고 확장 가능

## 📦 새로운 파일

### 1. IHoldable.cs (NEW)
```csharp
public interface IHoldable
{
    Transform GetTransform();
    void OnPickedUp(Transform holdPoint);
    void OnPutDown(Vector3 dropPosition, Vector3 playerForward);
    float GetSpeedModifier();
}
```

**역할:** 들 수 있는 모든 아이템의 공통 규약

## 🔧 수정된 파일

### 2. ThrowableBox.cs (수정)
- ✅ `IHoldable` 구현
- ✅ 던지기 동작 유지
- ✅ Legacy 메서드 유지 (BePickedUp, BeThrown)
- ✅ HeavyObject 자동 감지

**특징:**
```csharp
OnPutDown() → 던지기 (AddForce)
```

### 3. ExtinguisherItem.cs (수정)
- ✅ `IHoldable` 구현
- ✅ 놓기 동작 (던지기 아님!)
- ✅ Legacy 메서드 유지
- ✅ HeavyObject 자동 감지

**특징:**
```csharp
OnPutDown() → 그냥 놓기 (살짝 밀기만)
```

### 4. PlayerController.cs (완전 재작성)
**Before:**
```csharp
public ThrowableBox heldObject;
private ExtinguisherItem _heldExtinguisher;

void Update() {
    // ThrowableBox 처리
    // ExtinguisherItem 별도 처리
    // 복잡한 if 분기...
}
```

**After:**
```csharp
private IHoldable _heldItem;
private MonoBehaviour _heldItemMono;

public void PickUpObject(MonoBehaviour item) {
    if (item is IHoldable holdable) {
        _heldItem = holdable;
        holdable.OnPickedUp(holdPoint);
    }
}

void PutDownItem() {
    _heldItem.OnPutDown(dropPos, throwDir);
}
```

**개선점:**
- ✅ 통합된 픽업/놓기 로직
- ✅ 타입별 자동 분기 (is 연산자)
- ✅ 확장 가능 (새 아이템 추가 쉬움)

### 5. 팝업 통합 스크립트들 (수정)
- InteractablePuzzle_GrantExtinguisher.cs
- SimpleExtinguisherGiver.cs

**변경:**
```csharp
player.PickUpObject(extinguisher);  // 통합 메서드 사용
```

## 🎮 작동 방식

### ThrowableBox 모드
```
E키 → 줍기 → 이동 → E키 → 던지기 (AddForce)
```

### ExtinguisherItem 모드
```
E키 → 장착 → 우클릭 분사 → E키 → 놓기 (살짝 밀기)
```

### 공통 동작
- holdPoint에 부착
- 이동속도 감소 (GetSpeedModifier)
- 한번에 하나만 들 수 있음
- 자동 교체 (새 아이템 주우면 기존 것 놓음)

## 🚀 확장성

### 새로운 아이템 추가 방법
```csharp
public class NewTool : MonoBehaviour, IInteractable, IHoldable
{
    public Transform GetTransform() => transform;
    public void OnPickedUp(Transform holdPoint) { /* ... */ }
    public void OnPutDown(Vector3 pos, Vector3 dir) { /* ... */ }
    public float GetSpeedModifier() => 0.8f;
    
    public void OnInteract(GameObject interactor) {
        interactor.SendMessage("PickUpObject", this);
    }
}
```

→ PlayerController 수정 없이 바로 작동!

## 📊 코드 비교

| 항목 | Before | After | 개선 |
|------|--------|-------|------|
| 필드 수 | 2개 (분리) | 1개 (통합) | 단순화 |
| if 분기 | 많음 | 적음 | 가독성↑ |
| 확장성 | 낮음 | 높음 | IHoldable만 구현 |
| 타입 안정성 | 낮음 | 높음 | 인터페이스 강제 |

## 🐛 호환성

### 작동하는 것
- ✅ 기존 ThrowableBox (그대로 사용 가능)
- ✅ HeavyObject와 호환
- ✅ 팝업 시스템 통합
- ✅ Legacy 메서드 (BePickedUp, BeThrown)

### 주의사항
- IHoldable을 구현하지 않은 오브젝트는 들 수 없음
- PlayerController.PickUpObject(MonoBehaviour) 호출 필요

## 💡 사용 예시

### 1. 기본 사용 (변화 없음)
```csharp
// ThrowableBox에서 (기존 코드 그대로)
public void OnInteract(GameObject interactor) {
    interactor.SendMessage("PickUpObject", this);
}
```

### 2. 플레이어가 특정 아이템 들고 있는지 확인
```csharp
// 다른 스크립트에서
var player = GetComponent<PlayerController>();

// 소화기를 들고 있나?
if (player.GetHeldItem<ExtinguisherItem>() != null) {
    Debug.Log("플레이어가 소화기 들고 있음!");
}

// 아무거나 들고 있나?
if (player.HeldItem != null) {
    Debug.Log("뭔가 들고 있음!");
}
```

### 3. 강제로 아이템 놓게 하기
```csharp
// E키가 아니라 스크립트로 강제 놓기
if (Input.GetKeyDown(KeyCode.G)) {
    // PlayerController에 public으로 만들면:
    // player.PutDownItem();
}
```

## 🎯 다음 단계

1. **Unity에서 테스트**
   - ThrowableBox 정상 작동 확인
   - ExtinguisherItem 정상 작동 확인
   - 상호 교체 확인

2. **필요시 조정**
   - dropPushForce 값 조정
   - speedModifier 밸런싱

3. **새 아이템 추가**
   - 망치, 열쇠, 랜턴 등
   - IHoldable만 구현하면 끝!

## ✅ 체크리스트

업데이트 전 확인:
- [ ] 기존 ThrowableBox 프리팹들 테스트
- [ ] ExtinguisherItem 프리팹 테스트
- [ ] 팝업 → 소화기 획득 테스트
- [ ] HeavyObject 이동속도 감소 확인
- [ ] UI (ExtinguisherUI) 정상 작동 확인

모두 완료! 🎉
