using System;
using UnityEngine;
using UniRx;
using Models;
using UniRx.Triggers;

[RequireComponent(typeof(Animator))]
public class PlayerEntity : MonoBehaviour
{
    [SerializeField]
    Sprite _downSprite;

    [SerializeField]
    Sprite _upSprite;

    [SerializeField]
    Sprite _leftSprite;

    [SerializeField]
    Sprite _crySprite;

    readonly IReactiveProperty<Direction> _direction = new ReactiveProperty<Direction>(Direction.Down);
    readonly Subject<FieldCoord> _digSubject = new Subject<FieldCoord>();

    Animator _animator;
    SpriteRenderer _renderer;
    Vector3 _lastTouchPosition;
    float _lastDigTime;
    float _preferredX;
    bool _dead;

    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
        _lastDigTime = Time.time;
        _preferredX = Dig.Field.GetLocalPosition(Dig.Player.Coord).x;

        _direction.AsObservable()
            .Subscribe(d =>
            {
                switch (d)
                {
                    case Direction.Down:
                        _renderer.sprite = _downSprite;
                        _renderer.flipX = false;
                        break;
                    case Direction.Up:
                        _renderer.sprite = _upSprite;
                        _renderer.flipX = false;
                        break;
                    case Direction.Left:
                        _renderer.sprite = _leftSprite;
                        _renderer.flipX = false;
                        break;
                    case Direction.Right:
                        _renderer.sprite = _leftSprite;
                        _renderer.flipX = true;
                        break;
                }
            })
            .AddTo(this);
    }

    void Update()
    {
        if (_dead)
        {
            _renderer.sprite = _crySprite;
            var t = transform;
            var tori = t.Find("tori_dead");
            tori.gameObject.SetActive(true);
            tori.Translate( 0f, Time.deltaTime * 0.25f, 0f);

            var toriSprite = tori.GetComponent<SpriteRenderer>();
            toriSprite.color = new Color(
                toriSprite.color.r,
                toriSprite.color.g,
                toriSprite.color.b,
                Mathf.Max(0f, toriSprite.color.a - Time.deltaTime * 0.25f));
        }
    }

    public void Tick()
    {
        var t = transform;
        var walkDistance = Dig.Player.WalkSpeed * Time.deltaTime;

        if (!_dead)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _lastTouchPosition = Input.mousePosition;
            }
            else if (Input.GetMouseButton(0))
            {
                var diff = Input.mousePosition - _lastTouchPosition;
                if (diff.magnitude < 0.1f)
                {
                    TryDig();
                }
                else
                {
                    if (Mathf.Abs(diff.x) > Mathf.Abs((diff.y)))
                    {
                        if (diff.x < 0f)
                        {
                            _direction.Value = Direction.Left;
                            var leftEdge = t.localPosition + Field.LeftOffset - new Vector3(walkDistance, 0f);
                            var leftCoord = Dig.Field.GetCoord(leftEdge);
                            if (leftEdge.x < _preferredX && Dig.Field.CanMove(leftCoord))
                            {
                                _preferredX = Dig.Field.GetLocalPosition(leftCoord).x;
                            }
                        }
                        else
                        {
                            _direction.Value = Direction.Right;
                            var rightEdge = t.localPosition + Field.RightOffset + new Vector3(walkDistance, 0f);
                            var rightCoord = Dig.Field.GetCoord(rightEdge);
                            if (rightEdge.x > _preferredX && Dig.Field.CanMove(rightCoord))
                            {
                                _preferredX = Dig.Field.GetLocalPosition(rightCoord).x;
                            }
                        }
                    }
                    else
                    {
                        _direction.Value = diff.y > 0f ? Direction.Up : Direction.Down;
                    }
                }
            }
        }

        var x = t.localPosition.x;
        if (Math.Abs(_preferredX - x) < walkDistance)
        {
            t.localPosition = new Vector3(_preferredX, t.localPosition.y);
        }
        else if (x < _preferredX)
        {
            t.localPosition = new Vector3(Mathf.Min(x + walkDistance, _preferredX), t.localPosition.y);
        }
        else
        {
            t.localPosition = new Vector3(Math.Max(x - walkDistance, _preferredX), t.localPosition.y);
        }

        // Fall
        var newPosition = t.localPosition + new Vector3(0f, -Dig.Field.FallSpeed * Time.deltaTime);
        var bottomEdge = newPosition + Field.DownOffset;
        var bottomCoord = Dig.Field.GetCoord(bottomEdge);
        if (Dig.Field.CanFall(bottomCoord, true))
        {
            t.localPosition = newPosition;
        }
        else
        {
            var fixedCoord = Dig.Field.GetCoord(t.localPosition);
            var preferredY = Dig.Field.GetLocalPosition(fixedCoord).y;
            t.localPosition = new Vector3(t.localPosition.x, preferredY);
        }
    }

    public IObservable<Direction> OnChangeDirectionAsObservable()
    {
        return _direction.AsObservable().DistinctUntilChanged();
    }

    public IObservable<FieldCoord> OnChangeCoordAsObservable()
    {
        var t = transform;
        return this.UpdateAsObservable()
            .Select(_ => Dig.Field.GetCoord(t.localPosition))
            .DistinctUntilChanged();
    }

    public IObservable<FieldCoord> OnDigAsObservable()
    {
        return _digSubject.AsObservable();
    }

    public void Die()
    {
        _dead = true;
    }

    void TryDig()
    {
        if (Time.time - _lastDigTime < Dig.Player.DigInterval) return;
        var digCoord = Dig.Field.GetCoord(transform.localPosition).Shift(_direction.Value);
        _digSubject.OnNext(digCoord);
        _lastDigTime = Time.time;
    }
}
