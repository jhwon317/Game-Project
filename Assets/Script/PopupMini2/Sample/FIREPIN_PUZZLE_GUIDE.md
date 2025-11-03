# FirePin 퍼즐 상호작용 시스템 사용 가이드

## 📋 개요

`FirePinPuzzleInteractable`은 플레이어가 E키로 상호작용하면 FirePin 퍼즐을 띄우고, 성공 시 소화기를 보상으로 지급하는 올인원 스크립트입니다.

## 🎯 주요 기능

- ✅ E키 상호작용으로 FirePin 퍼즐 시작
- ✅ 퍼즐 성공 시 자동으로 소화기 지급
- ✅ 즉시 소화기 모드로 진입
- ✅ 제한 시간 모드 지원
- ✅ 일회용/재사용 가능 설정
- ✅ 하이라이트 효과
- ✅ 사운드 피드백
- ✅ 디버그 모드

---

## 🛠️ 설치 방법

### 1단계: 씬 준비

먼저 씬에 필요한 기본 컴포넌트들이 있는지 확인하세요:

```
Hierarchy:
├─ Player (PlayerController, PlayerInteractor)
├─ EventSystem
├─ PopupSessionManager
└─ Canvas (PopupHost 포함)
```

**필수 컴포넌트:**
- `PopupSessionManager` - 팝업 관리
- `PopupHost` - 팝업 UI 표시
- `EventSystem` - UI 이벤트 처리
- `PlayerController` - 플레이어 제어
- `PlayerInteractor` - 상호작용 감지

### 2단계: FirePin 퍼즐 정의 생성

1. Project 창에서 우클릭
2. `Create > PopupMini2 > PuzzleDefinition` 선택
3. 이름을 `FirePinPuzzle`로 변경
4. Inspector에서 설정:

```
FirePinPuzzle (ScriptableObject):
├─ Prefab: (FirePin 퍼즐 프리팹)
├─ Aspect Mode: Fit Contain
├─ Anti Aliasing: 2
├─ Filter Mode: Bilinear
├─ Background Color: (0, 0, 0, 0)
├─ Modal: ✓
├─ Backdrop Closable: ☐
├─ Timeout Sec: 0
└─ Shadows Off: ✓
```

### 3단계: 상호작용 오브젝트 생성

1. Hierarchy에서 빈 GameObject 생성
2. 이름을 `FireExtinguisher_Dispenser`로 변경
3. 위치 설정 (플레이어가 접근할 곳)

### 4단계: 컴포넌트 추가

**FireExtinguisher_Dispenser**에 다음을 추가:

1. **Collider** (Trigger 체크)
   - Box Collider 추가
   - `Is Trigger` 체크
   - Size: (2, 2, 2)

2. **FirePinPuzzleInteractable** 추가
   - Add Component 클릭
   - `FirePinPuzzleInteractable` 검색

3. **시각적 요소** (선택사항)
   - 자식으로 Cube 추가
   - 머티리얼 적용

---

## ⚙️ Inspector 설정

### Popup Puzzle 섹션

```
Session Manager: (PopupSessionManager)
└─ 씬에 있는 PopupSessionManager 드래그

Fire Pin Definition: (FirePinPuzzle)
└─ 2단계에서 만든 ScriptableObject 드래그

Puzzle Args: (비워둠)
└─ JSON으로 추가 인자 전달 (고급 기능)
```

### Extinguisher Reward 섹션

```
Extinguisher Prefab: (소화기 프리팹)
├─ ExtinguisherItem 컴포넌트가 있어야 함
├─ 없으면 기본 소화기 자동 생성
└─ 권장: Prefabs/Extinguisher 사용

Extinguisher Duration: 0
├─ 0 = 무제한
├─ 30 = 30초 후 자동 파괴
└─ 60 = 1분 후 자동 파괴

Auto Destroy Extinguisher: ✓
└─ Q키로 해제 시 소화기 파괴 여부
```

### Interaction Settings 섹션

```
One Time Use: ✓
└─ 한 번만 사용 가능 (보통 체크)

Disable After Use: ✓
└─ 사용 후 오브젝트 비활성화
```

### Visual Feedback 섹션

```
Highlight Material: (머티리얼)
└─ 플레이어가 가까이 가면 적용됨

Target Renderer: (자동 검색)
└─ 하이라이트 적용할 Renderer
```

### Audio Feedback 섹션

```
Audio Source: (자동 검색)
Interact Sound: (클릭음)
Success Sound: (성공음)
Fail Sound: (실패음)
```

---

## 🎮 사용 예시

### 예시 1: 기본 소화기 디스펜서

**시나리오:** 플레이어가 소화기함에 다가가서 E키를 누르면 퍼즐이 뜨고, 성공하면 소화기를 얻음.

**설정:**
```
FirePinPuzzleInteractable:
├─ Session Manager: PopupSessionManager
├─ Fire Pin Definition: FirePinPuzzle
├─ Extinguisher Prefab: Extinguisher (프리팹)
├─ Extinguisher Duration: 0 (무제한)
├─ Auto Destroy: ✓
├─ One Time Use: ✓
└─ Disable After Use: ✓
```

**결과:**
1. E키 → FirePin 퍼즐 시작
2. 핀 뽑기 성공 → 소화기 즉시 장착
3. 우클릭으로 즉시 분사 가능
4. Q키로 해제 가능
5. 디스펜서는 비활성화됨 (재사용 불가)

### 예시 2: 제한 시간 소화기 챌린지

**시나리오:** 퍼즐을 풀면 30초간만 소화기를 사용할 수 있는 챌린지.

**설정:**
```
FirePinPuzzleInteractable:
├─ Extinguisher Duration: 30 (30초)
├─ Auto Destroy: ✓ (필수!)
├─ One Time Use: ✓
└─ Disable After Use: ✓
```

**결과:**
1. E키 → 퍼즐 시작
2. 성공 → 소화기 획득
3. 30초 후 자동으로 소화기 사라짐
4. "⏱ 30s" 표시됨 (Scene 뷰)

### 예시 3: 재사용 가능한 트레이닝 스테이션

**시나리오:** 연습용으로 계속 소화기를 받을 수 있음.

**설정:**
```
FirePinPuzzleInteractable:
├─ Extinguisher Duration: 60 (1분)
├─ One Time Use: ☐ (체크 해제!)
└─ Disable After Use: ☐ (체크 해제!)
```

**결과:**
1. 여러 번 E키로 퍼즐 가능
2. 매번 1분간 소화기 사용 가능
3. 연습에 최적

---

## 🐛 트러블슈팅

### 문제 1: "E키를 눌러도 퍼즐이 안 떠요"

**체크리스트:**
- [ ] PlayerInteractor가 Player에 있나요?
- [ ] Collider의 `Is Trigger`가 체크되어 있나요?
- [ ] PopupSessionManager가 씬에 있나요?
- [ ] Fire Pin Definition이 할당되어 있나요?
- [ ] 플레이어가 Trigger 범위 안에 있나요?

**디버깅:**
```csharp
// Console 로그 확인
[FirePinPuzzle] FireExtinguisher_Dispenser: FirePin 퍼즐을 시작합니다...
```

### 문제 2: "퍼즐은 성공했는데 소화기가 안 생겨요"

**원인:**
- Extinguisher Prefab이 할당되지 않음 → 기본 소화기 생성됨
- 플레이어가 이미 다른 물건을 들고 있음

**해결:**
```csharp
// Console 로그 확인
[FirePinPuzzle] FireExtinguisher_Dispenser: 소화기 지급 완료!
[PlayerController] 소화기 모드 진입!
```

### 문제 3: "제한 시간이 작동 안해요"

**원인:**
- `Auto Destroy Extinguisher`가 체크 해제됨

**해결:**
- `Auto Destroy Extinguisher` 체크 필수!
- Console 로그:
```
[ExtinguisherHelper] Timer expired! (30s)
```

### 문제 4: "이미 소화기를 들고 있는데 또 받을 수 있어요"

**원인:**
- 현재 버전에서는 자동으로 막힘

**로그:**
```
[FirePinPuzzle] FireExtinguisher_Dispenser: 이미 소화기를 들고 있습니다!
```

### 문제 5: "하이라이트가 작동 안해요"

**체크리스트:**
- [ ] Highlight Material이 할당되어 있나요?
- [ ] Target Renderer가 있나요?
- [ ] PlayerInteractor가 SetHighlighted()를 호출하나요?

---

## 🎨 커스터마이징

### 1. 다른 퍼즐 사용

FirePin 대신 다른 퍼즐을 사용하려면:

1. 다른 PuzzleDefinition 생성
2. `Fire Pin Definition`에 할당
3. 퍼즐 프리팹에 IPuzzleController 구현 필요

### 2. 조건부 보상

퍼즐 점수에 따라 다른 소화기 지급:

```csharp
// FirePinPuzzleInteractable.cs 수정
private bool GrantExtinguisher(PlayerController player)
{
    // TODO: result.Payload에서 점수 파싱
    // 점수에 따라 다른 프리팹 사용
    
    var prefab = score > 80 ? advancedExtinguisher : basicExtinguisher;
    return ExtinguisherHelper.EnterMode(player, prefab, true);
}
```

### 3. UI 표시

퍼즐 진행 중 UI 표시:

```csharp
// 퍼즐 시작 전
UIManager.ShowMessage("소화기 안전핀을 뽑으세요!");

// 퍼즐 성공 후
UIManager.ShowMessage("소화기 획득!");
```

### 4. 애니메이션 추가

퍼즐 성공 시 애니메이션:

```csharp
private bool GrantExtinguisher(PlayerController player)
{
    // 애니메이션 재생
    var animator = GetComponent<Animator>();
    if (animator) animator.SetTrigger("OpenDoor");
    
    // 딜레이 후 소화기 지급
    await Task.Delay(1000);
    
    return ExtinguisherHelper.EnterMode(player, extinguisherPrefab, true);
}
```

---

## 💡 고급 기능

### Context Menu

Scene 뷰에서 오브젝트 우클릭:

```
FirePinPuzzleInteractable
└─ Reset State (상태 초기화)
```

**용도:**
- 테스트 중 상태 리셋
- One Time Use 재설정

### Gizmos 시각화

Scene 뷰에서 오브젝트 선택 시:

- **초록색 구체** - 사용 가능 (READY)
- **노란색 구체** - 진행 중 (BUSY)
- **회색 구체** - 사용됨 (USED)
- **⏱ 30s** - 제한 시간 표시

### Debug 로그

Console에서 진행 상황 추적:

```
[FirePinPuzzle] FireExtinguisher_Dispenser: FirePin 퍼즐을 시작합니다...
[FirePinPuzzle] FireExtinguisher_Dispenser: 퍼즐 성공! 소화기를 지급합니다.
[FirePinPuzzle] FireExtinguisher_Dispenser: 무제한 모드
[ExtinguisherHelper] Entered mode! (AutoDestroy: True)
[PlayerController] 소화기 모드 진입!
[FirePinPuzzle] FireExtinguisher_Dispenser: 소화기 지급 완료!
```

---

## 📝 체크리스트

### 씬 설정
- [ ] PopupSessionManager가 씬에 있음
- [ ] PopupHost가 Canvas에 있음
- [ ] EventSystem이 있음
- [ ] Player에 PlayerController + PlayerInteractor

### 프리팹 설정
- [ ] FirePin 퍼즐 프리팹 준비
- [ ] 소화기 프리팹 준비 (ExtinguisherItem 컴포넌트)

### ScriptableObject 설정
- [ ] PuzzleDefinition 생성 (FirePinPuzzle)
- [ ] Prefab 할당
- [ ] 설정 완료

### GameObject 설정
- [ ] Collider 추가 (Is Trigger 체크)
- [ ] FirePinPuzzleInteractable 추가
- [ ] Inspector 필드 모두 할당

### 테스트
- [ ] E키로 퍼즐 시작됨
- [ ] 퍼즐 성공 시 소화기 획득
- [ ] 우클릭으로 분사 가능
- [ ] Q키로 해제 가능
- [ ] 제한 시간 작동 (설정한 경우)

---

## 🔗 관련 파일

- `FirePinPuzzleInteractable.cs` - 메인 스크립트
- `ExtinguisherHelper.cs` - 소화기 모드 헬퍼
- `PlayerController.cs` - 플레이어 제어
- `PopupSessionManager.cs` - 팝업 관리
- `IInteractable.cs` - 상호작용 인터페이스

---

## 📚 참고 문서

- `BUG_FIX_REPORT.md` - 소화기 버그 수정
- `POPUP_CLICK_BUG_FIX.md` - 팝업 클릭 버그 수정
- `POPUP_INTEGRATION_GUIDE.md` - 팝업 통합 가이드

---

**작성일:** 2024-11-03  
**작성자:** AI Assistant  
**버전:** 1.0
