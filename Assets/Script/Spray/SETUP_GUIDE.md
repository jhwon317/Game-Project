# 소화기 시스템 설치 가이드

## 📋 개요
인벤토리 없이 심플하게 "들고 분사하는" 소화기 시스템

## 🔧 설치 순서

### 1. 소화기 프리팹 생성

1. **빈 GameObject 생성** → 이름: `Extinguisher`

2. **필수 컴포넌트 추가:**
   - `Rigidbody` (Mass: 7)
   - `Collider` (BoxCollider 권장)
   - `ExtinguisherItem` (새 스크립트)
   - `HeavyObject` (speedModifier = 0.7)

3. **자식 오브젝트 생성** → 이름: `Nozzle`
   - Position: (0, 0.5, 0.3) 예시
   - `SprayEmitter` 컴포넌트 추가
   - `ParticleSystem` (분사 이펙트)

4. **자식 오브젝트 생성** → 이름: `Controller`
   - `ExtinguisherController` 컴포넌트 추가
   - `AudioSource` (sprayLoop 사운드)

5. **연결 (Inspector):**
   ```
   ExtinguisherItem:
   └─ rb: (자동)
   └─ itemCollider: (자동)
   └─ controller: Controller 오브젝트
   
   ExtinguisherController:
   └─ player: Player 오브젝트 (Tag: "Player")
   └─ emitter: Nozzle의 SprayEmitter
   └─ sprayLoop: Controller의 AudioSource
   └─ tankMax: 12
   └─ flowRate: 1
   
   SprayEmitter:
   └─ nozzle: Nozzle Transform
   └─ coneAngleDeg: 24
   └─ effectiveRange: 140
   └─ raysPerFrame: 16 (성능 최적화)
   └─ baseDPS: 110
   └─ sprayVfx: Nozzle의 ParticleSystem
   ```

### 2. Player 태그 설정
- Player GameObject에 Tag: "Player" 설정
- ExtinguisherController가 자동으로 찾음

### 3. UI 추가
1. **빈 GameObject 생성** → 이름: `ExtinguisherUI`
2. `ExtinguisherUI` 컴포넌트 추가
3. 씬 어디든 배치 (DontDestroyOnLoad 필요시 처리)

### 4. 불 오브젝트 확인
- `FireHP` 컴포넌트가 있는지 확인
- 없으면 기존 문서의 FireHP.cs 사용

## 🎮 사용법

### 플레이어 조작
- **E키**: 소화기 줍기/던지기
- **우클릭 (Fire2)**: 소화기 분사 (누르고 있는 동안)
- **이동 속도**: 소화기를 들면 70%로 감소 (HeavyObject.speedModifier)

### 디버그 키 (선택사항)
ExtinguisherController에 다음 메서드 추가 가능:
```csharp
void Update()
{
    // R키로 탱크 리필
    if (Input.GetKeyDown(KeyCode.R))
        Refill();
}
```

## 📊 성능 최적화

### 변경 사항
- **기존**: 56 rays/frame (~933 rays/sec)
- **신규**: 16 rays/frame (~267 rays/sec)
- **데미지 중복 제거**: 같은 불에 여러 ray가 맞아도 한번만 데미지

### 추가 최적화 (필요시)
```csharp
// SprayEmitter.cs에 추가:
[Header("Performance")]
public float rayCastBudgetPerSecond = 300f;
private float _rayDebt = 0f;

public void Spray(Vector3 direction, float deltaTime)
{
    _rayDebt += rayCastBudgetPerSecond * deltaTime;
    int raysThisFrame = Mathf.Min(raysPerFrame, Mathf.FloorToInt(_rayDebt));
    _rayDebt -= raysThisFrame;
    
    // ... 기존 코드 (raysPerFrame 대신 raysThisFrame 사용)
}
```

## 🐛 트러블슈팅

### 1. "분사가 안돼요"
- [ ] PlayerController에 Fire2 입력 코드 추가했나요?
- [ ] ExtinguisherController.enabled = true인가요?
- [ ] tankCurrent > 0인가요?

### 2. "소화기를 못 집어요"
- [ ] ExtinguisherItem에 IInteractable 구현되어 있나요?
- [ ] Collider가 켜져있나요?
- [ ] PlayerInteractor가 감지하고 있나요?

### 3. "불이 안꺼져요"
- [ ] FireHP 컴포넌트가 불 오브젝트에 있나요?
- [ ] LayerMask가 불 레이어를 포함하나요?
- [ ] baseDPS가 너무 낮지 않나요? (110 권장)

### 4. "UI가 안보여요"
- [ ] ExtinguisherUI가 씬에 있나요?
- [ ] Player Tag가 "Player"로 설정되어 있나요?

### 5. "던지면 분사가 계속돼요"
- [ ] ExtinguisherItem.BeThrown()에서 controller.StopSpraying() 호출하나요?
- [ ] controller.enabled = false 하나요?

## 📦 파일 목록

새로 생성된 파일:
- `Assets/Script/Spray/SprayEmitter.cs`
- `Assets/Script/Spray/ExtinguisherController.cs`
- `Assets/Script/Spray/ExtinguisherItem.cs`
- `Assets/Script/Spray/ExtinguisherUI.cs`

수정된 파일:
- `Assets/Script/PlayerController.cs`

백업된 파일:
- `Assets/Script/Spray/ExtinguisherSpray_HPMode.cs.old`

유지된 파일:
- `Assets/Script/Spray/FireHP.cs` (그대로 사용)
- `Assets/Script/HeavyObject.cs` (그대로 사용)

## 🎯 다음 단계 (선택사항)

1. **파티클 효과 개선**
   - 분사 VFX를 더 멋지게
   - 불이 꺼질 때 연기 효과

2. **사운드 추가**
   - 분사 루프 사운드
   - 탱크 고갈 경고음
   - 불 꺼지는 소리

3. **게임플레이 밸런싱**
   - 소화기 무게 조정 (HeavyObject.speedModifier)
   - DPS, 거리 감쇠 조정
   - 탱크 용량 조정

4. **고급 기능**
   - 여러 종류의 소화기 (ABC급 등)
   - 소화기 재충전 스테이션
   - 업적/통계 (몇 개의 불을 껐는지)
