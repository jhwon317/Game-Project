using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PopupMini
{
    [Serializable]
    public struct PuzzleResult
    {
        public bool Success;          // true=성공, false=실패/취소
        public string Reason;         // "complete" | "abort:user" | "timeout" | "error:*"
        public object Payload;        // 점수/보상/선택 등 자유
        public static PuzzleResult Ok(object payload = null, string reason = "complete") => new PuzzleResult { Success = true, Reason = reason, Payload = payload };
        public static PuzzleResult Cancel(string reason = "abort:user") => new PuzzleResult { Success = false, Reason = reason };
        public static PuzzleResult Error(string msg) => new PuzzleResult { Success = false, Reason = $"error:{msg}" };
    }

    public struct PopupUIOverride
    {
        public GameObject PanelRoot;        // 교체할 패널 루트(선택)
        public RectTransform ContentRoot;   // 없으면 PanelRoot 하위에서 자동 탐색
        public CamToRawImage Viewport;      // 없으면 PanelRoot 하위에서 자동 탐색
        public CanvasGroup CanvasGroup;     // 없으면 런타임에 자동 추가

        public bool IsValid =>
            PanelRoot || ContentRoot || Viewport || CanvasGroup;

        public static PopupUIOverride FromPanel(GameObject panel)
        {
            if (!panel) return default;
            return new PopupUIOverride
            {
                PanelRoot = panel,
                ContentRoot = panel.GetComponentInChildren<RectTransform>(true),
                Viewport = panel.GetComponentInChildren<CamToRawImage>(true),
                CanvasGroup = panel.GetComponent<CanvasGroup>()
            };
        }
    }

    [Serializable]
    public class PuzzleRequest
    {
        public PuzzleDefinition Definition;
        public object Args;
        public PopupSizeOptions? SizeOverride;   // 선택
        public SessionOptions SessionOverride;  // 선택
        public PopupUIOverride UIOverride;
    }

    public interface IPuzzleController
    {
        event Action<PuzzleResult> Completed;                 // 퍼즐 종료 신호
        void Begin(object args, CancellationToken ct);        // 시작
    }

    public enum AspectMode { Stretch, FitContain, FillCrop }
    public enum PopupSizingMode { PercentAnchors, FixedPixelsUI }

    [Serializable]
    public struct PopupSizeOptions
    {
        public PopupSizingMode Mode;
        public float PercentX;            // 0~1 (화면 가로 비율)
        public float PercentY;            // 0~1 (화면 세로 비율)
        public AspectMode AspectMode;     // FillCrop 권장
        public float Aspect;              // 가로/세로 (정사각=1)
        public Vector2 MinSize;           // px
        public Vector2 MaxSize;           // px
        public bool UseSafeArea;          // iOS 등
        public static PopupSizeOptions DefaultSquare => new PopupSizeOptions
        {
            PercentX = 0.6f,
            PercentY = 0.6f,
            AspectMode = AspectMode.FillCrop,
            Aspect = 1f,
            MinSize = new Vector2(360, 360),
            MaxSize = new Vector2(1200, 1200),
            UseSafeArea = true
        };
        public static PopupSizeOptions Percent(float x, float y, AspectMode view = AspectMode.FitContain)
            => new PopupSizeOptions { Mode = PopupSizingMode.PercentAnchors, PercentX = x, PercentY = y, AspectMode = view };

        public static PopupSizeOptions FixedUI(float w, float h, AspectMode view = AspectMode.FitContain)
            => new PopupSizeOptions { Mode = PopupSizingMode.FixedPixelsUI, MinSize = new Vector2(w, h), MaxSize = new Vector2(w, h), AspectMode = view };
    }

    [Serializable]
    public struct SessionOptions
    {
        public bool Modal;                // Gameplay 입력 차단
        public bool BackdropClosable;     // 바깥 클릭 닫기
        public float? TimeoutSec;         // null=없음
        public static SessionOptions DefaultModal => new SessionOptions { Modal = true, BackdropClosable = false, TimeoutSec = null };
    }
}