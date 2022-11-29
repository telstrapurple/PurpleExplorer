﻿using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;

namespace PurpleExplorer.Services;

public class NewtonsoftJsonSuspensionDriver : ISuspensionDriver
{
    private readonly string _file;
    private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
    {
        TypeNameHandling = TypeNameHandling.All,
        Formatting = Formatting.Indented
    };

    public NewtonsoftJsonSuspensionDriver(string file) => _file = file;

    public IObservable<Unit> InvalidateState()
    {
        if (File.Exists(_file))
            File.Delete(_file);
        return Observable.Return(Unit.Default);
    }

    public IObservable<object> LoadState()
    {
        var lines = File.ReadAllText(_file);
        var state = JsonConvert.DeserializeObject<object>(lines, _settings);
        return Observable.Return(state);
    }

    public IObservable<Unit> SaveState(object state)
    {
        var lines = JsonConvert.SerializeObject(state, _settings);
        File.WriteAllText(_file, lines);
        return Observable.Return(Unit.Default);
    }
}