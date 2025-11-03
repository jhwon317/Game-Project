# FirePin 퍼즐 상호작용 - 빠른 시작 가이드 ⚡

## 🚀 30초 만에 설정하기

### 1️⃣ GameObject 생성
```
Hierarchy > 우클릭 > Create Empty
이름: FireExtinguisher_Dispenser
```

### 2️⃣ 컴포넌트 추가
```
Add Component > Box Collider
- Is Trigger: ✓
- Size: (2, 2, 2)

Add Component > FirePinPuzzleInteractable
```

### 3️⃣ Inspector 설정 (필수 3개만!)
```
Session Manager: (씬의 PopupSessionManager 드래그)
Fire Pin Definition: (FirePinPuzzle ScriptableObject 드래그)
Extinguisher Prefab: (소화기 프리팹 드래그)
```

### 4️⃣ 완료! 🎉
```
Play 버튼 → 오브젝트에 다가가기 → E키 → 퍼즐 풀기 → 소화기 GET!
```

---

## 📦 프리셋 설정들

### 🔥 기본 소화기 (무제한)
```
Extinguisher Duration: 0
Auto Destroy: ✓
One Time Use: ✓
```
**결과:** 한 번 받으면 계속 사용 가능, Q키로 해제 시 파괴

---

### ⏱️ 챌린지 모드 (30초)
```
Extinguisher Duration: 30
Auto Destroy: ✓
One Time Use: ✓
```
**결과:** 30초 후 자동으로 사라짐

---

### 🎓 연습 모드 (재사용)
```
Extinguisher Duration: 60
Auto Destroy: ✓
One Time Use: ☐
Disable After Use: ☐
```
**결과:** 계속 퍼즐 풀고 소화기 받을 수 있음

---

## ⚠️ 자주 하는 실수

| 문제 | 원인 | 해결 |
|------|------|------|
| E키가 안 먹힘 | Collider가 Trigger 아님 | `Is Trigger` 체크 |
| 퍼즐이 안 뜸 | PopupSessionManager 없음 | 씬에 추가 |
| 소화기가 안 생김 | 프리팹 미할당 | Inspector 확인 |
| 타이머가 안 작동 | Auto Destroy 체크 해제 | 체크 필수! |

---

## 🎮 조작법

| 키 | 기능 |
|----|------|
| E | 상호작용 (퍼즐 시작) |
| 마우스 | 핀 드래그 |
| 우클릭 | 소화기 분사 |
| Q | 소화기 해제 |
| ESC | 퍼즐 취소 |

---

## 💬 로그 확인

**정상 작동 시:**
```
[FirePinPuzzle] FirePin 퍼즐을 시작합니다...
[FirePinPuzzle] 퍼즐 성공! 소화기를 지급합니다.
[PlayerController] 소화기 모드 진입!
```

**문제 발생 시:**
```
[FirePinPuzzle] PopupSessionManager가 없습니다!
[FirePinPuzzle] PuzzleDefinition이 할당되지 않았습니다!
```

---

## 🎨 Scene 뷰 표시

오브젝트를 선택하면 Gizmos로 상태 확인:

- 🟢 **READY** - 사용 가능
- 🟡 **BUSY** - 퍼즐 진행 중
- ⚫ **USED** - 이미 사용됨
- ⏱️ **30s** - 제한 시간 표시

---

## 📚 더 자세한 정보

`FIREPIN_PUZZLE_GUIDE.md` 참고

---

**Tip:** Context Menu에서 `Reset State`로 테스트 중 상태 초기화 가능! 🔄
