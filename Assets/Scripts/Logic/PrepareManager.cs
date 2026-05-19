using System;
using UnityEngine;
using DG.Tweening;
using JoonyleGameDevKit;
using System.Collections.Generic;

public class PrepareManager
{
    // ========= 영웅 =========

    private SummonManager _summonManager;
    private SquadManager _squadManager;
    private HeroSlotController _summonSlotController;
    private HeroSlotController _squadSlotController;

    //
    private HeroSlotController _lastController;
    private int _lastSlotIdx;
    private HeroView _lastHeroView;

    // drag
    private Vector3 _dragOriginalPos;
    private Vector3 _dragOffset;
    private bool _isDragActive;
    public bool IsDragActive => _isDragActive;
    public bool SuppressHoverPanel { get; set; }
    private HeroView _pendingDropView;

    // selection
    private HeroSlotController _selectedController;
    private int _selectedSlotIdx = -1;
    private Action<HeroInstance> _onHeroSelected;
    private Action _onHeroDeselected;

    // push
    private int _pushPreviewSlot = -1;
    private int _pushPreviewDir;
    private List<(int from, int to)> _pushPreviewChain;

    // sold
    private HeroSellZone _heroSellZone;
    private Action _onHeroSold;

    // ========= 유물 =========

    private RelicManager _relicManager;
    private RelicSlotController _relicSlotController;
    
    // ========= 공통 =========
    
    private InputProvider _inputProvider;

    public PrepareManager(InputProvider inputProvider)
    {
        _inputProvider = inputProvider;
    }

    public void Initialize(
        SummonManager summonManager,
        SquadManager squadManager,
        RelicManager relicManager,
        HeroSlotController summonSlotController,
        HeroSlotController squadSlotController,
        RelicSlotController relicSlotController,
        HeroSellZone heroSellZone,
        Action onHeroSold = null,
        Action<HeroInstance> onHeroSelected = null,
        Action onHeroDeselected = null)
    {
        _summonManager = summonManager;
        _squadManager = squadManager;
        _relicManager = relicManager;
        _summonSlotController = summonSlotController;
        _squadSlotController = squadSlotController;
        _relicSlotController = relicSlotController;
        _heroSellZone = heroSellZone;
        _onHeroSold = onHeroSold;
        _onHeroSelected = onHeroSelected;
        _onHeroDeselected = onHeroDeselected;
    }
    
    public void Tick()
    {
        if (_inputProvider.JustPressed) BeginDrag();
        else if (_inputProvider.IsDragging) UpdateDrag();
        else if (_inputProvider.JustReleased) EndDrag();

        UpdateHover();
    }

    private void BeginDrag()
    {
        var worldPos = _inputProvider.GetWorldPos.ToVector3();
        var controllers = new[] { _summonSlotController, _squadSlotController };

        foreach (var controller in controllers)
        {
            if (controller == null) continue;

            var slotIdx = controller.GetSlotIndexAtWorldPos(worldPos);
            if (slotIdx < 0 || controller.IsSlotEmpty(slotIdx)) continue;

            _lastController = controller;
            _lastSlotIdx = slotIdx;
            _lastHeroView = controller.GetViewAt(slotIdx);
            _dragOriginalPos = _lastHeroView.transform.position;
            _dragOffset = _dragOriginalPos - worldPos;

            break;
        }
    }

    private void UpdateDrag()
    {
        if (_lastController == null) return;

        var worldPos = _inputProvider.GetWorldPos.ToVector3();

        if (!_isDragActive)
        {
            var currSlotIdx = _lastController.GetSlotIndexAtWorldPos(worldPos);
            var isStillInSlot = currSlotIdx == _lastSlotIdx;

            if (isStillInSlot) return;

            ClearSelection();

            _isDragActive = true;
            _lastHeroView.SetDragging(true);
            _heroSellZone?.gameObject.SetActive(true);
            _onHeroSelected?.Invoke(_lastHeroView.HeroInstance);
        }

        _lastHeroView.transform.position = worldPos + _dragOffset;

        // 현재 커서 위치가 스쿼드 슬롯 위에 있고 그 슬롯이 차 있으면 밀기 프리뷰 갱신
        var hoveredSquadSlot = _squadSlotController?.GetSlotIndexAtWorldPos(worldPos) ?? -1;
        if (hoveredSquadSlot != -1 && !_squadSlotController.IsSlotEmpty(hoveredSquadSlot))
        {
            // 스쿼드 내부 이동일 때는 드래그 출발 슬롯을 빈 자리로 간주해야 체인 계산이 올바름
            int excludeSrcIdx = _lastController == _squadSlotController ? _lastSlotIdx : -1;
            int pushDir = GetPushDir(hoveredSquadSlot, excludeSrcIdx);

            // 슬롯이나 방향이 바뀐 경우에만 프리뷰 갱신 (매 프레임 재계산 방지)
            if (hoveredSquadSlot != _pushPreviewSlot || pushDir != _pushPreviewDir)
                ApplyPushPreview(hoveredSquadSlot, pushDir, excludeSrcIdx);
        }
        else
        {
            // 슬롯 밖이거나 빈 슬롯 위에 있으면 밀기 프리뷰 불필요
            RevertPushPreview();
        }
    }

    private void EndDrag()
    {
        RevertPushPreview();

        _summonSlotController?.ClearHover();
        _squadSlotController?.ClearHover();

        if (_lastHeroView != null && _isDragActive)
        {
            _lastHeroView.SetDragging(false);

            var worldPos = _inputProvider.GetWorldPos.ToVector3();
            var transferred = _lastController == _summonSlotController
                ? OnSummonHeroDropped(_lastSlotIdx, worldPos)
                : OnSquadHeroDropped(_lastSlotIdx, worldPos);

            var heroView = _lastHeroView;

            if (transferred)
            {
                heroView = _pendingDropView;
                _pendingDropView = null;
                SuppressHoverPanel = true;
            }
            else
            {
                heroView.transform.position = _dragOriginalPos;
            }

            ClearSelection();
            heroView?.PlayDropAnimation();
            _onHeroDeselected?.Invoke();
        }
        else
        {
            UpdateSelection(_lastController, _lastSlotIdx);
        }

        _isDragActive = false;
        _heroSellZone?.gameObject.SetActive(false);

        _lastHeroView = null;
        _lastSlotIdx = -1;
        _lastController = null;
    }

    private void UpdateHover()
    {
        var screenPos = _inputProvider.GetScreenPos;

        if (_isDragActive)
        {
            var srcIdx = (_lastController == _summonSlotController) ? _lastSlotIdx : -1;
            _summonSlotController?.TryHoverAt(screenPos, true, srcIdx);
            srcIdx = (_lastController == _squadSlotController) ? _lastSlotIdx : -1;
            _squadSlotController?.TryHoverAt(screenPos, false, srcIdx);
        }
        else
        {
            _summonSlotController?.TryHoverAt(screenPos, true);
            _squadSlotController?.TryHoverAt(screenPos, true);
        }
    }

    // ========== ... ==========

    /// <summary>
    /// 드래그 중인 영웅의 X 위치를 기준으로 targetSlot에 밀어 넣을 방향을 결정합니다.
    /// 영웅이 슬롯보다 오른쪽에 있으면 +1(오른쪽), 왼쪽에 있으면 -1(왼쪽)을 우선 방향으로 선택합니다.
    /// 우선 방향으로 밀 공간이 없고 반대 방향으로는 가능하면 반대 방향을 반환합니다.
    /// </summary>
    /// <param name="targetSlot">영웅을 삽입할 목표 슬롯 인덱스</param>
    /// <param name="excludeSrc">밀기 체인 계산 시 제외할 슬롯 인덱스 (드래그 출발 슬롯, 기본값 -1 = 제외 없음)</param>
    /// <returns>밀기 방향 (+1: 오른쪽, -1: 왼쪽)</returns>
    private int GetPushDir(int targetSlot, int excludeSrc = -1)
    {
        var heroPosX = _lastHeroView.transform.position.x;
        var squadSlotPosX = _squadSlotController.GetSlotWorldPos(targetSlot).x;
        int preferredDir = heroPosX > squadSlotPosX ? 1 : -1;

        // 진입 방향으로 밀 공간이 없으면 반대 방향으로 폴백
        if (CalcPushChain(targetSlot, preferredDir, excludeSrc) == null &&
            CalcPushChain(targetSlot, (-1) * preferredDir, excludeSrc) != null)
        {
            return (-1) * preferredDir;
        }

        return preferredDir;
    }

    /// <summary>
    /// targetSlot부터 pushDir 방향으로 연속된 영웅들을 한 칸씩 밀었을 때의 이동 경로(체인)를 계산합니다.
    /// 체인 도중 배열 경계를 벗어나면 밀기가 불가능하므로 null을 반환합니다.
    /// 빈 슬롯 또는 excludeSrc를 만나면 거기서 체인이 끝납니다 (공간 확보 성공).
    /// </summary>
    /// <param name="targetSlot">밀기를 시작할 슬롯 인덱스</param>
    /// <param name="pushDir">밀기 방향 (+1: 오른쪽, -1: 왼쪽)</param>
    /// <param name="excludeSrc">드래그 출발 슬롯처럼 빈 자리로 취급할 슬롯 인덱스 (기본값 -1 = 제외 없음)</param>
    /// <returns>밀기 경로 리스트 (from→to 쌍). 경계 초과로 불가능하면 null</returns>
    private List<(int from, int to)> CalcPushChain(int targetSlot, int pushDir, int excludeSrc = -1)
    {
        var chain = new List<(int, int)>();
        var curr = targetSlot;

        while (curr >= 0 && curr < _squadSlotController.SlotCount)
        {
            if (curr == excludeSrc || _squadSlotController.IsSlotEmpty(curr)) break;
            var next = curr + pushDir;
            if (next < 0 || next >= _squadSlotController.SlotCount) return null; // 경계 초과 → 불가
            chain.Add((curr, next));
            curr = next;
        }

        return chain;
    }

    /// <summary>
    /// 드래그 중 영웅들이 밀릴 위치를 시각적으로 미리 보여줍니다.
    /// 기존 프리뷰가 있으면 먼저 되돌린 뒤, CalcPushChain으로 계산한 체인대로 HeroView를 이동시킵니다.
    /// 밀기가 불가능한 경우(chain == null)에는 프리뷰를 적용하지 않습니다.
    /// </summary>
    /// <param name="targetSlot">드롭 목표 슬롯 인덱스</param>
    /// <param name="pushDir">밀기 방향 (+1: 오른쪽, -1: 왼쪽)</param>
    /// <param name="excludeSrc">드래그 출발 슬롯처럼 빈 자리로 취급할 슬롯 인덱스 (기본값 -1 = 제외 없음)</param>
    private void ApplyPushPreview(int targetSlot, int pushDir, int excludeSrc = -1)
    {
        RevertPushPreview();

        var chain = CalcPushChain(targetSlot, pushDir, excludeSrc);
        if (chain == null) return;

        _pushPreviewSlot = targetSlot;
        _pushPreviewDir = pushDir;
        _pushPreviewChain = chain;

        foreach (var (from, to) in chain)
        {
            var fromHeroView = _squadSlotController.GetViewAt(from);
            fromHeroView.transform.DOKill();
            fromHeroView.transform.DOMove(_squadSlotController.GetItemWorldPos(to), 0.12f).SetEase(Ease.OutCubic);
        }
    }

    /// <summary>
    /// ApplyPushPreview로 옮긴 HeroView들을 원래 슬롯 위치로 되돌립니다.
    /// 프리뷰가 없으면 아무 작업도 하지 않습니다.
    /// </summary>
    private void RevertPushPreview()
    {
        if (_pushPreviewChain == null) return;

        foreach (var (from, to) in _pushPreviewChain)
        {
            var fromHeroView = _squadSlotController.GetViewAt(from);
            fromHeroView.transform.DOKill();
            fromHeroView.transform.DOMove(_squadSlotController.GetItemWorldPos(from), 0.12f).SetEase(Ease.OutCubic);
        }

        _pushPreviewChain = null;
        _pushPreviewSlot = -1;
    }

    // ========== ... ==========

    private void UpdateSelection(HeroSlotController controller, int slotIdx)
    {
        // 빈 곳 클릭 → 선택 해제
        if (controller == null)
        {
            ClearSelection();
            return;
        }

        // 같은 영웅 재클릭 → 토글 해제
        if (controller == _selectedController && slotIdx == _selectedSlotIdx)
        {
            ClearSelection();
            return;
        }


        // 이전 선택 해제
        if (_selectedController != null)
        {
            _selectedController.SetPinnedIdx(-1);
            var selectedHeroView = _selectedController.GetViewAt(_selectedSlotIdx);
            selectedHeroView?.SetSelected(false);
        }

        _selectedController = controller;
        _selectedSlotIdx = slotIdx;

        var currHeroView = controller.GetViewAt(slotIdx);
        controller.SetPinnedIdx(slotIdx);
        currHeroView?.SetSelected(true);
        _heroSellZone?.gameObject.SetActive(true);
        _onHeroSelected?.Invoke(currHeroView?.HeroInstance);
    }

    private void ClearSelection()
    {
        if (_selectedController == null) return;

        _selectedController.SetPinnedIdx(-1);
        var selectedHeroView = _selectedController.GetViewAt(_selectedSlotIdx);
        selectedHeroView?.SetSelected(false);

        _selectedController = null;
        _selectedSlotIdx = -1;
        _heroSellZone?.gameObject.SetActive(false);
        _onHeroDeselected?.Invoke();
    }

    // ========== ... ==========

    private bool TrySellHero(int srcIdx, Vector3 worldPos, bool fromSummon)
    {
        if (_heroSellZone == null || !_heroSellZone.ContainsWorldPos(worldPos)) return false;

        var heroInstance = fromSummon
            ? _summonManager.TakeFromBench(srcIdx)
            : _squadManager.TakeFromBench(srcIdx);
        if (heroInstance == null) return false;

        _onHeroSold?.Invoke();
        return true;
    }

    private bool OnSummonHeroDropped(int srcIdx, Vector3 worldPos)
    {
        // 1. 영웅을 판매하는 경우
        if (TrySellHero(srcIdx, worldPos, fromSummon: true)) return true;

        // 2. ...
        if (_squadSlotController != null)
        {
            // 스쿼드 슬롯 내 드랍 위치 (슬롯 인덱스) 반환
            var dstIdx = _squadSlotController.GetSlotIndexAtWorldPos(worldPos);
            if (dstIdx == -1) return false;

            // 먼저 배치 가능한지 체크
            var pushDir = GetPushDir(dstIdx);
            var canPlace = _squadSlotController.IsSlotEmpty(dstIdx) || _squadManager.CanInsertHero(dstIdx, pushDir);
            if (!canPlace) return false;

            // 배치 가능 확인 후 소환 슬롯에서 영웅을 가져감
            var heroInstance = _summonManager.TakeFromBench(srcIdx);
            if (heroInstance == null) return false;

            if (_squadSlotController.IsSlotEmpty(dstIdx))
            {
                var added = _squadManager.AddToBench(heroInstance, dstIdx);
                if (added) _pendingDropView = _squadSlotController.GetViewAt(dstIdx);
                return added;
            }

            _squadManager.InsertHero(heroInstance, dstIdx, pushDir);
            _pendingDropView = _squadSlotController.GetViewAt(dstIdx);
            return true;
        }

        return false;
    }

    private bool OnSquadHeroDropped(int srcIdx, Vector3 worldPos)
    {
        // 1. 영웅을 판매하는 경우
        if (TrySellHero(srcIdx, worldPos, fromSummon: false)) return true;

        // 2. 스쿼드 내 영웅을 이동시키는 경우 (빈 슬롯이면 이동, 차 있으면 밀어내기)
        if (_squadSlotController != null)
        {
            var dstIdx = _squadSlotController.GetSlotIndexAtWorldPos(worldPos);
            if (dstIdx == -1) return false;

            if (_squadSlotController.IsSlotEmpty(dstIdx))
            {
                var moved = _squadManager.MoveHero(srcIdx, dstIdx);
                if (moved) _pendingDropView = _squadSlotController.GetViewAt(dstIdx);
                return moved;
            }

            var pushDir = GetPushDir(dstIdx, srcIdx);
            var pushed = _squadManager.MoveHeroWithPush(srcIdx, dstIdx, pushDir);
            if (pushed) _pendingDropView = _squadSlotController.GetViewAt(dstIdx);
            return pushed;
        }

        return false;
    }
}
