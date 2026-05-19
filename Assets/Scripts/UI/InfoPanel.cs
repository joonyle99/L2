using TMPro;
using UnityEngine;

public class InfoPanel : UIPanel
{
    [SerializeField] private RectTransform _panel;
    [SerializeField] private Camera _uiCamera;

    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _gradeText;
    [SerializeField] private TextMeshProUGUI _triggerText;
    [SerializeField] private TextMeshProUGUI _effectText;
    [SerializeField] private TextMeshProUGUI _subText;

    [SerializeField] private Vector2 _pinnedAnchoredPos;
    [SerializeField] private Vector2 _hoverOffset;

    private Canvas _canvas;
    private Camera _mainCamera;

    private bool _isPinned;

    public void Initialize()
    {
        _canvas = GetComponent<Canvas>();
        _mainCamera = Camera.main;
    }

    // ========== ... ==========

    public void ShowHover(HeroInstance hero, Vector3 worldPos)
    {
        if (_isPinned) return;
        Populate(hero);
        SetHoveredPos(worldPos);
        Show();
    }

    public void HideHover()
    {
        if (_isPinned) return;
        Hide();
    }

    public void ShowPinned(HeroInstance hero)
    {
        _isPinned = true;
        SetPinnedPos();
        Populate(hero);
        Show();
    }

    public void Unpin()
    {
        _isPinned = false;
        Hide();
    }

    // ========== ... ==========

    // TODO: 이후 화면 어디에 Info Panel을 배치하면 좋을지 고민해보기
    private void SetHoveredPos(Vector3 worldPos)
    {
        var screenPos = _mainCamera.WorldToScreenPoint(worldPos);
        var canvasRectTrans = (RectTransform)_canvas.transform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRectTrans, screenPos, _uiCamera, out var basePoint);

        var canvasHalf = canvasRectTrans.rect.size * 0.5f;
        var panelHalf  = _panel.rect.size * 0.5f;
        var offset     = _hoverOffset;
        var tentative  = basePoint + offset;

        if (tentative.x - panelHalf.x < -canvasHalf.x || tentative.x + panelHalf.x > canvasHalf.x)
            offset.x = -offset.x;
        if (tentative.y - panelHalf.y < -canvasHalf.y || tentative.y + panelHalf.y > canvasHalf.y)
            offset.y = -offset.y;

        _panel.anchoredPosition = basePoint + offset;
    }

    private void SetPinnedPos()
    {
        _panel.anchoredPosition = _pinnedAnchoredPos;
    }

    // ========== ... ==========

    private void Populate(HeroInstance hero)
    {
        var data = hero.Data;
        _nameText.text = data.Name;
        _gradeText.text = data.Grade.ToDisplayText();
        _triggerText.text = data.Ability.Trigger.ToDisplayText();
        _effectText.text = data.Ability.Effect.ToDisplayText(data.Ability.Target, data.Ability.Value);
        _subText.text = $"가보자 가보자";
    }

    // ========== ... ==========

    private void OnDrawGizmos()
    {
        RectTransform parentRect = _panel.parent as RectTransform;
        if (parentRect == null) return;

        Vector2 panelSize = _panel.rect.size;
        Vector2 pivot = _panel.pivot;
        float sphereSize = panelSize.x * 0.05f;

        // === Pinned (cyan) ===
        DrawPanelGizmo(_pinnedAnchoredPos, parentRect, panelSize, pivot, Color.cyan, sphereSize);

        // === Hover (yellow) ===
        DrawPanelGizmo(_panel.anchoredPosition + _hoverOffset, parentRect, panelSize, pivot, Color.yellow, sphereSize);

        // 오프셋 연결선
        Vector3 pinnedWorld = parentRect.TransformPoint(_pinnedAnchoredPos);
        Vector3 hoverWorld = parentRect.TransformPoint(_pinnedAnchoredPos + _hoverOffset);
        Gizmos.color = Color.white;
        Gizmos.DrawLine(pinnedWorld, hoverWorld);
    }

    private void DrawPanelGizmo(Vector2 anchoredPos, RectTransform parent, Vector2 size, Vector2 pivot, Color color, float sphereSize)
    {
        Gizmos.color = color;

        Vector2 bl = anchoredPos - size * pivot;
        Vector2 tr = bl + size;

        Vector3 c0 = parent.TransformPoint(new Vector3(bl.x, bl.y, 0));
        Vector3 c1 = parent.TransformPoint(new Vector3(bl.x, tr.y, 0));
        Vector3 c2 = parent.TransformPoint(new Vector3(tr.x, tr.y, 0));
        Vector3 c3 = parent.TransformPoint(new Vector3(tr.x, bl.y, 0));

        Gizmos.DrawLine(c0, c1);
        Gizmos.DrawLine(c1, c2);
        Gizmos.DrawLine(c2, c3);
        Gizmos.DrawLine(c3, c0);
    }
}
