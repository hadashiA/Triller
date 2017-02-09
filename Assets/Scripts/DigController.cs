using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using Models;
using Services;
using UniRx.Triggers;

public class DigController : MonoBehaviour
{
    [SerializeField]
    DigSettings _settings;

    [SerializeField]
    Transform _fieldTransform;

    [SerializeField]
    Text _meterText;

    [SerializeField]
    Image _hpGauge;

    [SerializeField]
    DebugPanel _debugPanel;

    readonly Dictionary<int, BlockEntity> _blockEntities = new Dictionary<int, BlockEntity>();
    readonly Subject<FieldCoord> _digSubject = new Subject<FieldCoord>();
    readonly Subject<Unit> _gameoverSubject = new Subject<Unit>();
    PlayerEntity _playerEntity;

    void Awake()
    {
        _debugPanel.gameObject.SetActive(false);
    }

    void Start()
    {
        ResourceLoader.PreloadFieldAsObservable().Subscribe(_ => GameStart());

        var digging = _digSubject.AsObservable()
            .Select(coord => Dig.Field.Dig(coord))
            .Publish()
            .RefCount();

        digging
            .SelectMany(d => d.Blocks.ToObservable())
            .SelectMany(b => GetBlockEntity(b).DisapperAsObservable().Select(_ => b))
            .Subscribe(RemoveBlockEntity)
            .AddTo(this);

        digging
            .Select(d => d.Unbalances)
            .Do(unbalances =>
            {
                foreach (var u in unbalances)
                {
                    var block = Dig.Field.GetBlock(u);
                    GetBlockEntity(block).Shake();
                }
            })
            .SelectMany(unbalances =>
            {
                return Observable.Timer(TimeSpan.FromSeconds(_settings.ShakeDuration))
                    .Select(_ => unbalances);
            })
            .Subscribe(unbalances =>
            {
                foreach (var u in unbalances)
                {
                    if (!Dig.Field.IsBlock(u)) continue;
                    var block = Dig.Field.GetBlock(u);
                    Dig.Field.SetFalling(u, true);
                    GetBlockEntity(block).Fall();
                }
            })
            .AddTo(this);
    }

    void Update()
    {
        var field = Dig.Field;
        if (field == null) return;

        for (var row = field.Rows - 1; row >= 0; row--)
        {
            if (Dig.Player.Coord.Row == row && _playerEntity != null)
            {
                _playerEntity.Tick();
            }
            for (var col = 0; col < field.Cols; col++)
            {
                var coord = new FieldCoord(col, row);
                if (field.IsBlock(coord))
                {
                    var block = field.GetBlock(coord);
                    var entity = GetBlockEntity(block);
                    entity.Tick();
                }
            }
        }
        Dig.Player.Damage(_settings.DamageSpeed * Time.deltaTime);
        _debugPanel.DrawDebugGrid(_fieldTransform);
    }

    void GameStart()
    {
        Dig.Player = new Player(_settings);
        Dig.Field = FieldGenerator.Generate(_settings);

        CreateField();
        CreatePlayer();
    }

    void CreateField()
    {
        foreach (Transform child in _fieldTransform)
        {
            Destroy(child.gameObject);
        }
        foreach (var coord in Dig.Field)
        {
            if (Dig.Field.IsBlock(coord)) CreateBlock(coord);
        }
    }

    void CreateBlock(FieldCoord entryCoord)
    {
        var block = Dig.Field.GetBlock(entryCoord);
        var blockEntity = ResourceLoader.LoadBlockEntity(block.Color);
        var t = blockEntity.transform;
        t.SetParent(_fieldTransform);
        t.localPosition = Dig.Field.GetLocalPosition(entryCoord);
        _blockEntities[block.Id] = blockEntity;

        if (block.Color == BlockColor.Imo)
            blockEntity.Imo();

        blockEntity.OnContactAsObservable()
            .Subscribe(c => Dig.Field.SetFalling(c, false))
            .AddTo(this);

        blockEntity.OnDownAsObservable()
            .Do(down => Dig.Field.Move(down.Up, down))
            .Select(c => Dig.Field.TryStick(c))
            .Where(stickings => stickings.Any())
            .Do(stickings =>
            {
                foreach (var s in stickings)
                {
                    var b = Dig.Field.GetBlock(s);
                    var entity = GetBlockEntity(b);
                    entity.Blink();
                }
            })
            .SelectMany(stickings =>
            {
                return Observable.Timer(TimeSpan.FromSeconds(_settings.BlinkDuration))
                    .Select(_ => stickings);
            })
            .Subscribe(stickings =>
            {
                foreach (var s in stickings)
                {
                    if (Dig.Field.IsBlock(s))
                    {
                        _digSubject.OnNext(s);
                        return;
                    }
                }
            })
            .AddTo(blockEntity);
    }

    void CreatePlayer()
    {
        _playerEntity = ResourceLoader.LoadPlayerEntity();

        var t = _playerEntity.transform;
        t.SetParent(_fieldTransform);
        t.localPosition = Dig.Field.GetLocalPosition(Dig.Player.Coord);

        _playerEntity.UpdateAsObservable()
            .Subscribe(_ =>
            {
                var pos = t.localPosition;
                _fieldTransform.position = new Vector3( 0f, Mathf.Max(-pos.y - 7.5f, 0f));
                _meterText.text = string.Format("{0:F2}", pos.y);
                _debugPanel.SetPosition(pos);
            })
            .AddTo(_playerEntity);

        _playerEntity.OnChangeDirectionAsObservable()
            .Subscribe(direction => _debugPanel.SetDirection(direction))
            .AddTo(_playerEntity);

        _playerEntity.OnChangeCoordAsObservable()
            .Subscribe(coord =>
            {
                Dig.Player.Coord = coord;
                if (Dig.Field.IsItem(coord))
                {
                    var block = Dig.Field.GetBlock(coord);
                    RemoveBlockEntity(block);
                    Dig.Field.Remove(coord);
                    Dig.Player.Hp += 20f;
                }
                _debugPanel.SetCoord(coord);
            })
            .AddTo(_playerEntity);

        _playerEntity.OnDigAsObservable()
            .Subscribe(coord => _digSubject.OnNext(coord))
            .AddTo(_playerEntity);

        this.LateUpdateAsObservable()
            .Do(_ => _hpGauge.fillAmount = Dig.Player.Hp / 100f)
            .Where(_ => Dig.Player.Hp <= 0f)
            .First()
            .Do(_ => _playerEntity.Die())
            .Delay(TimeSpan.FromSeconds(5))
            .Subscribe(_ => GameStart())
            .AddTo(_playerEntity);

        this.LateUpdateAsObservable()
            .Where(_ => !Dig.Field.CanMove(Dig.Player.Coord))
            .First()
            .Do(_ => _playerEntity.Die())
            .Delay(TimeSpan.FromSeconds(5))
            .Subscribe(_ => GameStart())
            .AddTo(_playerEntity);

        _meterText.gameObject.SetActive(true);
    }

    void RemoveBlockEntity(Block block)
    {
        Destroy(GetBlockEntity(block).gameObject);
        _blockEntities.Remove(block.Id);
    }

    BlockEntity GetBlockEntity(Block block)
    {
        return _blockEntities[block.Id];
    }
}
