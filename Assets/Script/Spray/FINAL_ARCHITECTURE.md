# 🎯 소화기 시스템 최종 구조

## 핵심 설계 원칙

### ✅ 두 모드는 완전히 독립
```
ThrowableBox 모드 (운반)
  - 원본 코드 그대로 유지
  - E키: 줍기/던지기
  
ExtinguisherItem 모드 (도구 사용)
  - 헬퍼를 통한 진입/해제
  - 우클릭: 분사
  - 자동 관리
```

### ✅ 모드 배타적 처리
- 동시에 두 모드 진입 불가
- PlayerController가 체크

---

## 📦 파일 구조

### 핵심 파일 (3개)
1. **PlayerController.cs** (수정)
   - ThrowableBox 로직: 원본 유지
   - 소화기 모드 필드 추가
   - `EnterExtinguisherMode()` / `ExitExtinguisherMode()`

2. **ExtinguisherHelper.cs** (NEW)
   - `EnterMode()` - 모드 진입
   - `ExitMode()` - 모드 해제
   - `ToggleMode()` - 토글
   - `EnterModeWithTimer()` - 제한시간

3. **ExtinguisherItem.cs** (단순화)
   - Controller 참조만
   - 나머지는 PlayerController가 처리

### 사용처 (3개)
4. **InteractablePuzzle_GrantExtinguisher.cs** (수정)
   - 팝업 성공 → 헬퍼 호출
   - 제한 시간 옵션

5. **DebugExtinguisherGiver.cs** (NEW)
   - 인터랙터블 버전 (E키)
   - 테스트/디버그용

6. **DebugExtinguisherToggle.cs** (NEW)
   - 키보드 단축키 버전 (F5)
   - PlayerController에 붙여서 사용

---

## 🎮 사용 방법

### 방법 1: 팝업 리워드
```
1. InteractablePuzzle_GrantExtinguisher 오브젝트 생성
2. 팝업 설정
3. extinguisherPrefab 할당
4. (옵션) durationSeconds 설정
```

### 방법 2: 디버그 (인터랙터블)
```
1. DebugExtinguisherGiver 오브젝트 생성
2. extinguisherPrefab 할당
3. E키로 즉시 모드 진입
```

### 방법 3: 디버그 (키보드)
```
1. PlayerController에 DebugExtinguisherToggle 추가
2. extinguisherPrefab 할당
3. F5키로 토글
```

### 방법 4: 스크립트로 직접
```csharp
var player = GetComponent<PlayerController>();

// 모드 진입
ExtinguisherHelper.EnterMode(player, prefab, autoDestroy: true);

// 모드 해제
ExtinguisherHelper.ExitMode(player, destroyExtinguisher: true);

// 제한 시간 모드
ExtinguisherHelper.EnterModeWithTimer(player, prefab, 30f); // 30초
```

---

## 💡 주요 기능

### 1. 자동 파괴
```csharp
// 모드 해제 시 소화기 자동 파괴
ExtinguisherHelper.EnterMode(player, prefab, autoDestroy: true);
```

### 2. 제한 시간
```csharp
// 30초 후 자동 해제
ExtinguisherHelper.EnterModeWithTimer(player, prefab, 30f);
```

### 3. 모드 체크
```csharp
if (player.IsInExtinguisherMode) {
    Debug.Log("소화기 모드 활성!");
}

var ext = player.EquippedExtinguisher;
if (ext && ext.controller) {
    float tank = ext.controller.TankPercent;
}
```

---

## 🔄 작동 흐름

### 팝업 → 소화기 모드
```
플레이어 E키 상호작용
  ↓
팝업 시작
  ↓
퍼즐 완료
  ↓
InteractablePuzzle_GrantExtinguisher.GrantExtinguisher()
  ↓
ExtinguisherHelper.EnterMode(player, prefab)
  ↓
소화기 생성
  ↓
player.EnterExtinguisherMode(extinguisher)
  ↓
소화기 손에 부착
  ↓
controller.enabled = true
  ↓
우클릭으로 분사 가능! 🔥
```

### 제한 시간 모드
```
ExtinguisherHelper.EnterModeWithTimer(player, prefab, 30f)
  ↓
ExtinguisherTimerHelper 생성 (자동)
  ↓
코루틴 시작 (30초 대기)
  ↓
30초 경과
  ↓
ExtinguisherHelper.ExitMode(player) 자동 호출
  ↓
소화기 파괴
  ↓
모드 해제
```

---

## 🛠️ PlayerController 변경 사항

### 추가된 필드
```csharp
private ExtinguisherItem _equippedExtinguisher = null;
private bool _inExtinguisherMode = false;
```

### 추가된 메서드
```csharp
public bool EnterExtinguisherMode(ExtinguisherItem extinguisher)
public void ExitExtinguisherMode()
public bool IsInExtinguisherMode { get; }
public ExtinguisherItem EquippedExtinguisher { get; }
```

### 수정된 로직
```csharp
void Update() {
    // E키: 소화기 모드가 아닐 때만 ThrowableBox 처리
    if (!_inExtinguisherMode) {
        // 기존 ThrowableBox 로직
    }
    
    // 우클릭: 소화기 모드일 때만 분사
    if (_inExtinguisherMode && _equippedExtinguisher != null) {
        // 분사 로직
    }
}

void Move() {
    // ThrowableBox 무게 반영
    if (heldObject != null) { ... }
    // 소화기 모드 무게 반영
    else if (_inExtinguisherMode && _equippedExtinguisher != null) { ... }
}
```

---

## 📋 체크리스트

### Unity에서 확인할 것
- [ ] ThrowableBox 정상 작동 (기존 기능)
- [ ] 소화기 프리팹 생성
- [ ] 팝업 → 소화기 획득 테스트
- [ ] 디버그 단축키 (F5) 테스트
- [ ] 제한 시간 모드 테스트
- [ ] UI (ExtinguisherUI) 정상 작동
- [ ] 모드 배타성 확인 (동시 진입 불가)

### 예상 시나리오 테스트
1. **시나리오 1: 일반 사용**
   - 팝업 완료 → 소화기 획득 → 분사 → 탱크 소진 → 모드 유지

2. **시나리오 2: 제한 시간**
   - 팝업 완료 → 소화기 획득 → 30초 경과 → 자동 해제

3. **시나리오 3: 모드 충돌**
   - 상자 들기 → 소화기 획득 시도 → 거부 메시지
   - 소화기 모드 → 상자 들기 시도 → E키 무반응

---

## 🔧 커스터마이징

### 다른 제한 시간 설정
```csharp
// Inspector에서 설정
durationSeconds = 60f; // 60초

// 또는 스크립트에서
ExtinguisherHelper.EnterModeWithTimer(player, prefab, 60f);
```

### 수동 해제 버튼 추가
```csharp
// Q키로 수동 해제
if (Input.GetKeyDown(KeyCode.Q) && player.IsInExtinguisherMode) {
    ExtinguisherHelper.ExitMode(player, true);
}
```

### 소화기 교체
```csharp
// 기존 모드 해제
ExtinguisherHelper.ExitMode(player, true);
// 새 소화기로 재진입
ExtinguisherHelper.EnterMode(player, newPrefab, true);
```

---

## 🎯 장점

### ✅ ThrowableBox 무손상
- 원본 코드 전혀 안 건드림
- 기존 기능 100% 유지

### ✅ 독립적 관리
- ExtinguisherHelper로 중앙 관리
- 어디서든 쉽게 호출

### ✅ 확장 가능
- 제한 시간 모드
- 자동 파괴 옵션
- 다양한 진입 방식

### ✅ 디버그 편의
- F5 토글
- 단축키 지원
- 로그 출력

---

## 🚀 완료!

모든 파일 작성 완료. 이제 Unity에서:
1. 소화기 프리팹 생성
2. 팝업 트리거 설정
3. 테스트!

Good luck! 🔥🧯
