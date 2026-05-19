using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering;

public abstract class ItemView : MonoBehaviour
{
    [SerializeField] private Transform _itemRoot;
    [SerializeField] private SortingGroup _sortingGroup;
    [SerializeField, SortingLayer] private int _dragSortingLayer;
    [SerializeField] private int _dragSortingOrder;

    private GameObject _prefabObject;
    private int _defaultSortingLayerID;
    private int _defaultSortingOrder;
    private Vector3 _originItemScale;
    private Vector3 _spawnOffset;
    private bool _animLocked;

    public abstract ItemInstance ItemInstance { get; }

    protected virtual void Awake()
    {
        _defaultSortingLayerID = _sortingGroup.sortingLayerID;
        _defaultSortingOrder = _sortingGroup.sortingOrder;
        _originItemScale = _itemRoot.localScale;
    }

    // ======== ... ========

    public void SetSelected(bool isSelected)
    {
        if (isSelected) PlaySelectAnimation();
    }

    public void SetDragging(bool isDragging)
    {
        _sortingGroup.sortingLayerID = isDragging ? _dragSortingLayer : _defaultSortingLayerID;
        _sortingGroup.sortingOrder   = isDragging ? _dragSortingOrder : _defaultSortingOrder;
    }

    // ======== ... ========

    protected void ApplyItem(GameObject prefab, Vector3 spawnOffset)
    {
        _spawnOffset = spawnOffset;

        if (_prefabObject != null)
        {
            Destroy(_prefabObject);
            _prefabObject = null;
        }

        if (prefab == null) return;

        _prefabObject = Instantiate(prefab, _itemRoot);
        _prefabObject.transform.localPosition = _spawnOffset;
        _prefabObject.transform.localRotation = Quaternion.identity;
    }

    protected void ClearItem()
    {
        if (_prefabObject != null)
        {
            Destroy(_prefabObject);
            _prefabObject = null;
        }
    }

    // ======== ... ========

    public void PlayHoverAnimation()
    {
        if (_animLocked) return;

        _itemRoot.DOKill();
        _itemRoot.localScale = _originItemScale;
        _itemRoot.DOPunchScale(_originItemScale * 0.05f, 0.08f, 1, 0f);
    }

    public void PlaySelectAnimation()
    {
        _animLocked = true;

        _itemRoot.DOKill();
        _itemRoot.localScale = _originItemScale;
        _itemRoot.DOPunchScale(_originItemScale * 0.15f, 0.13f, 10, 0.8f).OnComplete(() => _animLocked = false);
    }

    public void PlayDropAnimation()
    {
        _animLocked = true;

        _itemRoot.DOKill();
        _itemRoot.localScale = _originItemScale;
        _itemRoot.DOPunchScale(_originItemScale * 0.35f, 0.45f, 6, 0.3f).OnComplete(() => _animLocked = false);
    }
}
