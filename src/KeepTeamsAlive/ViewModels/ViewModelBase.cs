using System;
using System.ComponentModel;
using Reactive.Bindings.Disposables;

namespace KeepTeamsAlive.ViewModels;

public class ViewModelBase : INotifyPropertyChanged, IDisposable
{
    protected CompositeDisposable Disposables = new();

    public event PropertyChangedEventHandler PropertyChanged;

    public void Dispose()
    {
        Disposables.Dispose();
    }
}