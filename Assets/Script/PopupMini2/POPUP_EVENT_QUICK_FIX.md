# 팝업 클릭 안 되는 문제 - 빠른 해결법 ⚡

## 🎯 90% 확률로 이것만 확인!

### 1. RawImage의 Raycast Target 체크 ✓

```
Hierarchy > Viewport (RawImage) 선택
Inspector > RawImage 컴포넌트
└─ Raycast Target: ✓ 체크!
```

### 2. RTUIClickProxyPro 컴포넌트 있는지 확인

```
Viewport GameObject에:
├─ RawImage
├─ CamToRawImage  
└─ RTUIClickProxyPro ← 이게 있어야 함!
```

### 3. CanvasGroup 설정 확인

```
PopupHost (또는 PanelRoot)에:
CanvasGroup:
├─ Interactable: ✓
└─ Block Raycasts: ✓
```

---

## 🔧 진단 도구 사용

**PopupHost에 `PopupEventDebugger` 추가하고 Play!**

자동으로 모든 문제를 찾아서 알려줍니다.

---

## 📞 그래도 안 되면

`POPUP_EVENT_TROUBLESHOOTING.md` 참고!
