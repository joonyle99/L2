using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class InGameManager : MonoBehaviour
{
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private UIController _uiController;
    private GameStateController<InGameState> _gameStateController;
    private InputProvider _inputProvider;
    private GoldSystem _goldSystem;

    [Space]

    [Header("1- Prepare Phase")]
    [SerializeField] private Canvas _prepareCanvas;
    [SerializeField] private GameObject _prepareStage;
    [SerializeField] private HeroDatabase _heroDatabase;
    [SerializeField] private RelicDatabase _relicDatabase;
    [SerializeField] private SummonTable _summonTable;
    [SerializeField] private SummonConfig _summonConfig;
    [SerializeField] private SquadConfig _squadConfig;
    [SerializeField] private RelicConfig _relicConfig;
    [SerializeField] private HeroSlotController _summonSlotController;
    [SerializeField] private HeroSlotController _squadSlotController;
    [SerializeField] private RelicSlotController _relicSlotController;
    [SerializeField] private SummonTrail _summonTrailPrefab;
    [SerializeField] private HeroSellZone _heroSellZone;
    [SerializeField] private int _startGoldAmount = 20;
    [SerializeField] private float _prepareDuration = 30f;
    private PrepareManager _prepareManager;
    private SummonManager _summonManager;
    private SquadManager _squadManager;
    private RelicManager _relicManager;
    private PrepareTimer _prepareTimer;

    [Space]
    
    [Header("2 - Battle Phase")]
    [SerializeField] private Canvas _battleCanvas;
    [SerializeField] private GameObject _battleStage;
    [SerializeField] private RoundTable _roundTable;
    private BattleManager _battleManager;
    private RoundManager _roundManager;

    private void Start()
    {
        Initialize();
    }

    private void OnDestroy()
    {
        _gameStateController.OnStateChanged -= OnStateChanged;
        _gameStateController.OnStateChanged -= _prepareTimer.OnStateChanged;
        _gameStateController.OnStateChanged -= _cameraController.OnStateChanged;
        _gameStateController.OnStateChanged -= _uiController.OnStateChanged;
        if (SoundManager.Instance != null) _gameStateController.OnStateChanged -= SoundManager.Instance.OnStateChanged;
    }

    private void Update()
    {
        var deltaTime = Time.deltaTime;
        
        _inputProvider?.Tick();
        _prepareManager?.Tick();
        _prepareTimer?.Tick(deltaTime);
    }

    private void Initialize()
    {
        _gameStateController = new GameStateController<InGameState>();
        _cameraController.Initialize(null);
        _inputProvider = new InputProvider();
        _goldSystem = new GoldSystem(_startGoldAmount);
        _summonManager = new SummonManager(_heroDatabase, _summonConfig, _summonTable);
        _squadManager = new SquadManager(_squadConfig);
        _relicManager = new RelicManager(_relicDatabase, _relicConfig);
        _prepareManager = new PrepareManager(_inputProvider);
        _prepareTimer = new PrepareTimer(_prepareDuration);
        _summonManager.Initialize(_goldSystem.TrySpend, _uiController.SetSummonCostText);
        _squadManager.Initialize();
        var infoPanel = _uiController.InfoPanel;
        _summonSlotController?.Initialize(
            _summonManager,
            _summonTrailPrefab != null ? MakeSummonTrailEffect : null,
            (hero, worldPos) => { if (!_prepareManager.IsDragActive && !_prepareManager.SuppressHoverPanel) infoPanel.ShowHover(hero, worldPos); },
            () => { _prepareManager.SuppressHoverPanel = false; infoPanel.HideHover(); });
        _squadSlotController?.Initialize(
            _squadManager,
            null,
            (hero, worldPos) => { if (!_prepareManager.IsDragActive && !_prepareManager.SuppressHoverPanel) infoPanel.ShowHover(hero, worldPos); },
            () => { _prepareManager.SuppressHoverPanel = false; infoPanel.HideHover(); });
        _relicSlotController.Initialize(_relicManager);
        _prepareManager.Initialize(_summonManager, _squadManager, _relicManager, _summonSlotController, _squadSlotController, _relicSlotController, _heroSellZone, () => _goldSystem.AddGold(_squadConfig.sellPrice), infoPanel.ShowPinned, infoPanel.Unpin);
        _prepareTimer.Initialize(_uiController.SetTimerText, Battle);
        _battleManager = new BattleManager();
        _roundManager = new RoundManager(_roundTable);
        _battleManager.Initialize();
        _roundManager.Initialize(_ => _summonManager.ResetCost());
        _uiController.Initialize(() => _summonManager.TrySummon(0), Battle);
        _goldSystem.Initialize(_uiController.SetGoldText);
        if (SoundManager.Instance != null) _gameStateController.OnStateChanged += SoundManager.Instance.OnStateChanged;
        _gameStateController.OnStateChanged += _uiController.OnStateChanged;
        _gameStateController.OnStateChanged += _cameraController.OnStateChanged;
        _gameStateController.OnStateChanged += _prepareTimer.OnStateChanged;
        _gameStateController.OnStateChanged += OnStateChanged;
        _gameStateController.ChangeState(InGameState.Prepare);
    }

    // ========== 상태 전환 ==========

    public void Prepare() => _gameStateController.ChangeState(InGameState.Prepare);
    public void Battle() => _gameStateController.ChangeState(InGameState.Battle);
    public void Result() => _gameStateController.ChangeState(InGameState.Result);
    public void MatchEnd() => _gameStateController.ChangeState(InGameState.MatchEnd);

    private void OnStateChanged(InGameState prev, InGameState curr)
    {
        _prepareCanvas?.gameObject.SetActive(curr == InGameState.Prepare);
        _prepareStage?.SetActive(curr == InGameState.Prepare);
        _battleCanvas?.gameObject.SetActive(curr == InGameState.Battle);
        _battleStage?.SetActive(curr == InGameState.Battle);
    }

    // ========== 연출 ==========

    private bool MakeSummonTrailEffect(HeroInstance heroInstance, Vector3 endWorldPos, Action onComplete)
    {
        var trail = Instantiate(_summonTrailPrefab);
        var startWorldPos = _uiController.GetSummonButtonWorldPos();
        trail.Launch(startWorldPos, endWorldPos, onComplete);
        return true;
    }
}
