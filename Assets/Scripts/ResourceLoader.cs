using System;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Models;
using Object = UnityEngine.Object;

public class ResourceMissingException : Exception
{
    public ResourceMissingException(string message) : base(message) {}
}

public static class ResourceLoader
{
    static Dictionary<string, Object> _cache = new Dictionary<string, Object>();

    public static IObservable<Unit> PreloadFieldAsObservable()
    {
        var loadings = new[]
        {
            LoadAsObservable<GameObject>("Prefabs/Player").AsUnitObservable(),
            LoadAsObservable<GameObject>("Prefabs/RedBlock").AsUnitObservable(),
            LoadAsObservable<GameObject>("Prefabs/BlueBlock").AsUnitObservable(),
            LoadAsObservable<GameObject>("Prefabs/GreenBlock").AsUnitObservable(),
            LoadAsObservable<GameObject>("Prefabs/YellowBlock").AsUnitObservable(),
            LoadAsObservable<GameObject>("Prefabs/HardBlock").AsUnitObservable(),
            LoadAsObservable<GameObject>("Prefabs/ImoBlock").AsUnitObservable(),
        };
        return Observable.WhenAll(loadings);
    }

    public static PlayerEntity LoadPlayerEntity()
    {
        var prefab = (GameObject) _cache["Prefabs/Player"];
        var gameObject = Object.Instantiate(prefab);
        return gameObject.GetComponent<PlayerEntity>();
    }

    public static BlockEntity LoadBlockEntity(BlockColor color)
    {
        var prefab = (GameObject) _cache[string.Format("Prefabs/{0}Block", color)];
        var gameObject = Object.Instantiate(prefab);
        return gameObject.GetComponent<BlockEntity>();
    }

    public static IObservable<PlayerEntity> LoadPlayerAsObservable()
    {
        return LoadAsObservable<GameObject>("Prefabs/Player")
            .Select(prefab =>
            {
                var gameObject = Object.Instantiate(prefab);
                return gameObject.GetComponent<PlayerEntity>();
            });
    }

    public static IObservable<BlockEntity> LoadBlockAsObservable(BlockColor color)
    {
        var path = string.Format("Prefabs/{0}Block", color);
        return LoadAsObservable<GameObject>(path)
            .Select(prefab =>
            {
                var gameObject = Object.Instantiate(prefab);
                return gameObject.GetComponent<BlockEntity>();
            });
    }

    public static IObservable<T> LoadAsObservable<T>(string path) where T : Object
    {
        Object asset;
        if (_cache.TryGetValue(path, out asset))
        {
            return Observable.Return((T) asset);
        }

        var loading = Resources.LoadAsync<T>(path);
        return loading.AsObservable()
            .Where(_ => loading.isDone)
            .Select(_ =>
            {
                if (loading.asset == null)
                {
                    throw new ResourceMissingException(string.Format("Missing Resources : {0}", path));
                }
                return (T) loading.asset;
            })
            .DoOnError(error => Debug.LogWarning(error.Message))
            .Do(x => _cache[path] = x);
    }
}
