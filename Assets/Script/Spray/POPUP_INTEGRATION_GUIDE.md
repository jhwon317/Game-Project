# 팝업-소화기 통합 가이드

## 📋 개요
팝업(퍼즐) 성공 시 소화기를 자동으로 지급하고 즉시 사용 가능하게 만드는 시스템

## 🔧 작동 흐름

```
플레이어가 E키로 상호작용
  ↓
팝업(퍼즐) 시작
  ↓
퍼즐 성공
  ↓
InteractablePuzzle_GrantExtinguisher 실행
  ↓
소화기 생성 or 활성화
  ↓
자동 픽업 (autoPickup = true)
  ↓
PlayerController._heldExtinguisher 설정
  ↓
즉시 우클릭(Fire2)으로 분사 가능!
```

## 🛠️ 구성 요소

### 1. InteractablePuzzle_GrantExtinguisher
팝업 성공 시 소화기를 지급하는 메인 스크립트

**파일 위치:**
`Assets/Script/PopupMini2/Sample/InteractablePuzzle_GrantExtinguisher.cs`

**주요 기능:**
- 팝업 성공 감지
- 소화기 생성 또는 활성화
- 자동 픽업
- 일회성 처리

### 2. SimpleExtinguisherGiver (테스트용)
팝업 없이 바로 소화기를 주는 간단한 스크립트

**파일 위치:**
`Assets/Script/Spray/SimpleExtinguisherGiver.cs`

**용도:**
- 빠른 테스트
- 디버깅
- 특정 상황에서 무조건 소화기 지급

### 3. PlayerController 개선
소화기를 별도로 추적하는 필드 추가

**변경 사항:**
```csharp
// 기존
public ThrowableBox heldObject = null;

// 추가
private ExtinguisherItem _heldExtinguisher = null;
```

## 📦 설치 방법

### 방법 1: 프리팹 생성 방식 (권장)

1. **소화기 프리팹 준비**
   - SETUP_GUIDE.md 참고해서 Extinguisher 프리팹 생성
   - Prefabs 폴더에 저장

2. **상호작용 오브젝트 생성**
   ```
   GameObject 생성: PuzzleTrigger
   ├─ Collider (Trigger 체크)
   ├─ InteractablePuzzle_GrantExtinguisher
   └─ (선택) 시각적 모델
   ```

3. **Inspector 설정**
   ```
   InteractablePuzzle_GrantExtinguisher:
   ├─ Puzzle:
   │  ├─ session: (씬의 PopupSessionManager)
   │  ├─ definition: (퍼즐 정의 ScriptableObject)
   │  └─ jsonArgs: (선택사항)
   │
   ├─ Reward - Extinguisher:
   │  ├─ extinguisherPrefab: Extinguisher 프리팹
   │  ├─ grantMode: SpawnPrefab
   │  ├─ spawnDistance: 2 (플레이어 앞 2m)
   │  └─ autoPickup: ✓ (체크)
   │
   └─ Consume Options:
      ├─ oneTimeUse: ✓
      └─ disableAfterUse: ✓
   ```

### 방법 2: 씬 오브젝트 활성화 방식

1. **소화기를 씬에 배치**
   - 초기에는 비활성화 상태
   - 원하는 위치에 배치

2. **상호작용 오브젝트 설정**
   ```
   InteractablePuzzle_GrantExtinguisher:
   ├─ grantMode: ActivateInScene
   ├─ extinguisherInScene: (씬의 소화기 오브젝트)
   └─ autoPickup: ✓
   ```

### 방법 3: 테스트용 (팝업 없이)

1. **빠른 테스트**
   ```
   GameObject 생성: QuickGiver
   ├─ Collider (Trigger 체크)
   └─ SimpleExtinguisherGiver
      ├─ extinguisherPrefab: Extinguisher 프리팹
      ├─ autoPickup: ✓
      └─ oneTimeUse: ✓
   ```

2. **사용법**
   - E키 누르면 즉시 소화기 획득
   - 팝업 없음 (디버그용)

## 🎮 사용 예시

### 예시 1: 화재 안전 교육 퍼즐

```
시나리오:
1. 플레이어가 소화기 보관함에 다가감
2. E키로 상호작용 → "소화기 사용법" 퍼즐 시작
3. 퍼즐 완료 → 소화기 획득 (자동 픽업)
4. 즉시 우클릭으로 불 끄기 가능
```

**설정:**
- `definition`: FireSafetyPuzzle.asset
- `grantMode`: SpawnPrefab
- `autoPickup`: true

### 예시 2: 스토리 진행 보상

```
시나리오:
1. 보스 클리어
2. 보상 상자 열기 → 직소 퍼즐
3. 퍼즐 완료 → 소화기 등장
4. 직접 E키로 픽업 (선택 가능)
```

**설정:**
- `definition`: JigsawPuzzle.asset
- `grantMode`: ActivateInScene
- `autoPickup`: false (수동 픽업)

### 예시 3: 긴급 상황

```
시나리오:
1. 화재 발생!
2. 비상 버튼 클릭
3. 즉시 소화기 획득 (팝업 없음)
```

**사용:**
- SimpleExtinguisherGiver 사용

## 🐛 트러블슈팅

### 문제 1: "팝업 성공했는데 소화기가 안 생겨요"

**체크리스트:**
- [ ] `extinguisherPrefab`이 할당되어 있나요?
- [ ] 프리팹에 `ExtinguisherItem` 컴포넌트가 있나요?
- [ ] Console에 에러 메시지가 있나요?
- [ ] `grantMode`가 올바른가요?

**디버깅:**
```csharp
// InteractablePuzzle_GrantExtinguisher.cs의 GrantExtinguisher() 메서드에
// 로그 추가하여 확인
Debug.Log($"[DEBUG] Grant mode: {grantMode}");
Debug.Log($"[DEBUG] Prefab: {extinguisherPrefab}");
Debug.Log($"[DEBUG] Player: {player}");
```

### 문제 2: "소화기는 생겼는데 자동 픽업이 안돼요"

**원인:**
- `autoPickup = false`로 설정됨
- PlayerController를 찾지 못함

**해결:**
```csharp
// Player GameObject에 Tag "Player" 설정 확인
// PlayerController 컴포넌트가 있는지 확인
```

### 문제 3: "소화기를 들었는데 분사가 안돼요"

**체크리스트:**
- [ ] `_heldExtinguisher`가 null이 아닌가요?
- [ ] `ExtinguisherController.enabled = true`인가요?
- [ ] `tankCurrent > 0`인가요?
- [ ] Fire2 버튼이 올바르게 설정되어 있나요?

**디버깅:**
```csharp
// PlayerController.Update()에 로그 추가
if (_heldExtinguisher != null)
{
    Debug.Log($"[DEBUG] Held extinguisher, tank: {_heldExtinguisher.controller?.tankCurrent}");
}
```

### 문제 4: "기존에 들고 있던 물건이 있으면 소화기를 못 받아요"

**해결됨!**
- PlayerController가 자동으로 기존 물건을 내려놓고 소화기 픽업
- `PickUpObject()` 메서드에서 자동 처리

### 문제 5: "소화기를 던질 수가 없어요"

**원인:**
- E키로 던지기 시도
- `_heldExtinguisher`가 제대로 처리되지 않음

**확인:**
```csharp
// PlayerController.ThrowObject() 로그
Debug.Log($"[DEBUG] Throwing - heldObject: {heldObject != null}, extinguisher: {_heldExtinguisher != null}");
```

## 🎯 고급 활용

### 1. 조건부 보상

팝업 결과(점수)에 따라 다른 소화기 지급:

```csharp
// InteractablePuzzle_GrantExtinguisher 수정
[Header("Conditional Rewards")]
public GameObject basicExtinguisher;
public GameObject advancedExtinguisher;
public float scoreThreshold = 80f;

bool GrantExtinguisher(GameObject interactor)
{
    // Payload 파싱해서 점수 확인
    float score = ParseScore(lastResult.Payload);
    
    var prefab = score >= scoreThreshold 
        ? advancedExtinguisher 
        : basicExtinguisher;
        
    // 생성 로직...
}
```

### 2. 지연 지급

퍼즐 완료 후 애니메이션 재생 후 지급:

```csharp
if (result.Success)
{
    await Task.Delay(2000); // 2초 대기
    GrantExtinguisher(interactor);
}
```

### 3. 여러 보상 조합

소화기 + 다른 아이템:

```csharp
if (result.Success)
{
    GrantExtinguisher(interactor);
    GrantHealthPack(interactor);
    // ...
}
```

## 📝 참고 사항

### PlayerController 변경 사항 요약

**추가된 필드:**
- `private ExtinguisherItem _heldExtinguisher`

**수정된 메서드:**
- `Update()` - 소화기 분사 입력 처리
- `Move()` - 소화기 무게 반영
- `PickUpObject(ThrowableBox)` - 자동 교체
- `PickUpObject(ExtinguisherItem)` - 새 오버로드
- `ThrowObject()` - 소화기 던지기 지원

### 호환성

**작동하는 것:**
- ✅ ThrowableBox와 ExtinguisherItem 병행 사용
- ✅ HeavyObject로 이동속도 감소
- ✅ 기존 상호작용 시스템과 통합
- ✅ 팝업 시스템과 통합

**작동하지 않는 것:**
- ❌ 인벤토리 시스템 (제거됨)
- ❌ RewardGrantHelper (더 이상 사용 안 함)

## 🚀 다음 단계

1. **소화기 프리팹 완성**
   - 모델, 파티클, 사운드 추가

2. **퍼즐 디자인**
   - 소화기 사용법 교육 퍼즐
   - 화재 안전 퀴즈 등

3. **레벨 디자인**
   - 소화기 획득 지점 배치
   - 불 배치
   - 난이도 조절

4. **밸런싱**
   - 탱크 용량
   - 무게
   - DPS
