using System;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Models;

public class BlockEntity : MonoBehaviour
{
    const float FadeDuration = 0.2f;

    enum State
    {
        Fixed, Falling, Shaking, Blinking
    }

    [SerializeField]
    Sprite _imoSprite;

    [SerializeField]
    Sprite _imoStandSprite;

    readonly Subject<FieldCoord> _downSubject = new Subject<FieldCoord>();
    readonly Subject<FieldCoord> _contactSubject = new Subject<FieldCoord>();

    State _state = State.Fixed;
    Renderer _renderer;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
    }

    public void Imo()
    {
        var r = new System.Random();
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(_ =>
            {
                GetComponent<SpriteRenderer>().sprite = r.Next(2) == 0 ? _imoSprite : _imoStandSprite;
            })
            .AddTo(this);
    }

    public void Tick()
    {
        var t = transform;
        var lastCoord = Dig.Field.GetCoord(t.localPosition);
        switch (_state)
        {
            case State.Shaking:
                t.Translate(Mathf.Sin(Time.time * 50f) / 30f, 0f, 0f);
                break;

            case State.Falling:
                var newPosition = t.localPosition + new Vector3(0f, -Dig.Field.FallSpeed * Time.deltaTime);
                var bottomEdge = newPosition + Field.DownOffset;
                var bottomCoord = Dig.Field.GetCoord(bottomEdge);

                if (Dig.Field.AllowFallWithGroup(lastCoord))
                {
                    t.localPosition = newPosition;
                    var coord = Dig.Field.GetCoord(t.localPosition);
                    if (coord != lastCoord) _downSubject.OnNext(coord);
                }
                else
                {
                    Fix();
                    _contactSubject.OnNext(lastCoord);
                }
                break;

            case State.Blinking:
                _renderer.material.SetFloat("_Blink", Mathf.Sin(Time.time * 1000.0f));
                break;
        }
    }

    public IObservable<FieldCoord> OnDownAsObservable()
    {
        return _downSubject.AsObservable();
    }

    public IObservable<FieldCoord> OnContactAsObservable()
    {
        return _contactSubject.AsObservable();
    }

    public IObservable<FieldCoord> DisapperAsObservable()
    {
        return this.UpdateAsObservable()
            .Scan(0f, (elapsed, _) => elapsed + Time.deltaTime)
            .Select(elapsed =>
            {
                var fade = Mathf.Clamp01(elapsed / FadeDuration);
                _renderer.material.SetFloat("_Fade", fade);
                return fade;
            })
            .Where(fade => fade >= 1f)
            .First()
            .Select(_ => Dig.Field.GetCoord(transform.localPosition));
    }

    public void Shake()
    {
        _state = State.Shaking;
    }

    public void Blink()
    {
        Fix();
        _state = State.Blinking;
    }

    public void Fall()
    {
        Fix();
        _state = State.Falling;
    }

    void Fix()
    {
        var t = transform;
        var fixedCoord = Dig.Field.GetCoord(t.localPosition);
        var preferred = Dig.Field.GetLocalPosition(fixedCoord);
        t.localPosition = new Vector3(preferred.x, preferred.y);
        _state = State.Fixed;
    }
}
