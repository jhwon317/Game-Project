# FirePinPuzzleInteractable 패치 노트 v1.1

## 🔧 수정 내역 (2024-11-03)

### 문제점
1. **PlayerController만 지원** - SimplePlayerMove를 사용하는 프로젝트에서 작동 안 함
2. **소화기 프리팹 필수** - 프리팹이 없으면 오류 발생

### 해결 방법

#### 1. 두 가지 플레이어 시스템 모두 지원
```csharp
// PlayerController 또는 SimplePlayerMove 자동 감지
var playerController = interactor.GetComponent<PlayerController>();
var simplePlayerMove = interactor.GetComponent<SimplePlayerMove>();

if (!playerController && !simplePlayerMove)
{
    Debug.LogWarning("플레이어를 찾을 수 없습니다!");
    return;
}
```

#### 2. 보상 지급 방식 개선

**PlayerController 있을 때:**
- ExtinguisherHelper 사용
- 소화기 모드로 진입
- Q키로 해제 가능

**SimplePlayerMove만 있을 때:**
- 소화기를 씬에 직접 생성
- 플레이어 앞 2m 위치에 배치
- 제한 시간 후 자동 파괴 (설정 시)

```csharp
if (playerController)
{
    // ExtinguisherHelper 사용
    ExtinguisherHelper.EnterMode(playerController, extinguisherPrefab, true);
}
else
{
    // 씬에 직접 생성
    GameObject extinguisher = Instantiate(extinguisherPrefab, 
        playerObj.transform.position + playerObj.transform.forward * 2f, 
        Quaternion.identity);
    
    // 제한 시간 처리
    if (extinguisherDuration > 0f)
        Destroy(extinguisher, extinguisherDuration);
}
```

---

## 🎮 사용 방법

### PlayerController 프로젝트
```
설정: 그대로 사용
결과: 
- 퍼즐 성공 → 소화기 자동 장착
- 우클릭으로 분사
- Q키로 해제
```

### SimplePlayerMove 프로젝트
```
설정: 그대로 사용
결과:
- 퍼즐 성공 → 소화기가 플레이어 앞에 생성됨
- E키로 집어서 사용 (별도 구현 필요)
- 제한 시간 후 자동 파괴 (설정 시)
```

---

## 🐛 트러블슈팅

### "PlayerController를 찾을 수 없습니다!"
**원인:** SimplePlayerMove를 사용 중
**해결:** v1.1로 업데이트 완료 - 자동으로 감지됨

### "소화기가 플레이어 손에 안 들려요" (SimplePlayerMove)
**원인:** SimplePlayerMove는 소화기 모드를 지원하지 않음
**해결:** 
1. PlayerController로 마이그레이션 (권장)
2. 또는 소화기가 씬에 생성되므로 E키로 줍는 시스템 별도 구현

---

## 📊 Scene View 표시

Gizmos에서 플레이어 타입 확인 가능:
- **Player: PlayerController** - 소화기 모드 지원
- **Player: SimplePlayerMove** - 씬 생성 모드
- **Player: NONE** - ⚠️ 플레이어 없음!

---

## 💡 권장 사항

### PlayerController 사용을 권장하는 이유:
1. ✅ 소화기 모드 완벽 지원
2. ✅ 자동 장착/해제
3. ✅ Q키로 편리한 해제
4. ✅ HeavyObject 무게 지원
5. ✅ 물리 충돌 자동 처리

### SimplePlayerMove 계속 사용하려면:
1. 소화기가 씬에 생성됨
2. E키로 줍는 시스템 추가 구현 필요
3. 제한 시간은 자동으로 작동

---

## 📝 변경 로그

### v1.1 (2024-11-03)
- ✅ SimplePlayerMove 지원 추가
- ✅ 두 가지 플레이어 시스템 자동 감지
- ✅ SimplePlayerMove용 소화기 씬 생성 로직
- ✅ Gizmos에 플레이어 타입 표시
- ✅ 프리팹 없을 때 기본 소화기 생성

### v1.0 (2024-11-03)
- 초기 릴리즈
- PlayerController만 지원

---

**업데이트 일자:** 2024-11-03  
**호환성:** PlayerController + SimplePlayerMove 모두 지원
