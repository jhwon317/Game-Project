using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace PopupMini
{
    [Serializable]
    public struct PuzzleResult
    {
        public bool Success;          // true=����, false=����/���
        public string Reason;         // "complete" | "abort:user" | "timeout" | "error:*"
        public object Payload;        // ����/����/���� �� ����
        public static PuzzleResult Ok(object payload = null, string reason = "complete") => new PuzzleResult { Success = true, Reason = reason, Payload = payload };
        public static PuzzleResult Cancel(string reason = "abort:user") => new PuzzleResult { Success = false, Reason = reason };
        public static PuzzleResult Error(string msg) => new PuzzleResult { Success = false, Reason = $"error:{msg}" };
    }

    public struct PopupUIOverride
    {
        public GameObject PanelRoot;        // ��ü�� �г� ��Ʈ(����)
        public RectTransform ContentRoot;   // ������ PanelRoot �������� �ڵ� Ž��
        public CamToRawImage Viewport;      // ������ PanelRoot �������� �ڵ� Ž��
        public CanvasGroup CanvasGroup;     // ������ ��Ÿ�ӿ� �ڵ� �߰�

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
        public PopupSizeOptions? SizeOverride;   // ����
        public SessionOptions SessionOverride;  // ����
        public PopupUIOverride UIOverride;
    }

    public interface IPuzzleController
    {
        event Action<PuzzleResult> Completed;                 // ���� ���� ��ȣ
        void Begin(object args, CancellationToken ct);        // ����
    }

    public enum AspectMode { Stretch, FitContain, FillCrop }
    public enum PopupSizingMode { PercentAnchors, FixedPixelsUI }

    [Serializable]
    public struct PopupSizeOptions
    {
        public PopupSizingMode Mode;
        public float PercentX;            // 0~1 (ȭ�� ���� ����)
        public float PercentY;            // 0~1 (ȭ�� ���� ����)
        public AspectMode AspectMode;     // FillCrop ����
        public float Aspect;              // ����/���� (���簢=1)
        public Vector2 MinSize;           // px
        public Vector2 MaxSize;           // px
        public bool UseSafeArea;          // iOS ��
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
        public bool Modal;                // Gameplay �Է� ����
        public bool BackdropClosable;     // �ٱ� Ŭ�� �ݱ�
        public float? TimeoutSec;         // null=����
        public static SessionOptions DefaultModal => new SessionOptions { Modal = true, BackdropClosable = false, TimeoutSec = null };
    }
}