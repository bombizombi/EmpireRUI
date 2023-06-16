using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using System.Threading;

namespace EmpireRUI;

public class MyInteraction<TInput, TOutput> //: Interaction<TInput, TOutput>
{
    private readonly IList<Func<MyInteractionContext<TInput, TOutput>, IObservable<Unit>>> _handlers;
    private readonly object _sync;
    private readonly IScheduler _handlerScheduler;

    public MyInteraction(IScheduler? handlerScheduler = null)
    {
        _handlers = new List<Func<MyInteractionContext<TInput, TOutput>, IObservable<Unit>>>();
        _sync = new object();
        _handlerScheduler = handlerScheduler ?? CurrentThreadScheduler.Instance;
    }

    public IDisposable RegisterHandler(Action<MyInteractionContext<TInput, TOutput>> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        return RegisterHandler(interaction =>
        {
            handler(interaction);
            //return Observables.Unit;
            //return Observable<Unit>.Default;
            return Observable.Return(Unit.Default);
        });
    }

    public IDisposable RegisterHandler(Func<MyInteractionContext<TInput, TOutput>, Task> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        return RegisterHandler(interaction => handler(interaction).ToObservable());
    }

    public IDisposable RegisterHandler<TDontCare>(
        Func<MyInteractionContext<TInput, TOutput>, IObservable<TDontCare>> handler)
    {
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }

        IObservable<Unit> ContentHandler(MyInteractionContext<TInput, TOutput> context)
        {
            return handler(context).Select(_ => Unit.Default);
        }

        AddHandler(ContentHandler);
        return Disposable.Create(() => RemoveHandler(ContentHandler));
    }

    public virtual IObservable<TOutput> Handle(TInput input)
    {
        var context = new MyInteractionContext<TInput, TOutput>(input);

#if PLAYgroundasdasdf
        // start playg




        var seetype1 = GetHandlers()
           //.Reverse()      //the point of MyInteraction
           .ToObservable()
           .ObserveOn(_handlerScheduler);

        var seetype2 = GetHandlers()
           //.Reverse()      //the point of MyInteraction
           .ToObservable()
           .ObserveOn(_handlerScheduler)
           .Select(handler => Observable.Defer(() => handler(context)));

        var seetype3 = GetHandlers()
           //.Reverse()      //the point of MyInteraction
           .ToObservable()
           .ObserveOn(_handlerScheduler)
           .Select(handler => Observable.Defer(() => handler(context)))
           .Concat();
        var seetype4 = GetHandlers()
           //.Reverse()      //the point of MyInteraction
           .ToObservable()
           .ObserveOn(_handlerScheduler)
           .Select(handler => Observable.Defer(() => handler(context)))
           .Concat()
           .TakeWhile(_ => !context.IsHandled);

        var seetype5 = GetHandlers()
           //.Reverse()      //the point of MyInteraction
           .ToObservable()
           .ObserveOn(_handlerScheduler)
           .Select(handler => Observable.Defer(() => handler(context)))
           .Concat()
           .TakeWhile(_ => !context.IsHandled)
           .IgnoreElements();

        var seetype6 = GetHandlers()
               //.Reverse()      //the point of MyInteraction
               .ToObservable()
               .ObserveOn(_handlerScheduler)
               .Select(handler => Observable.Defer(() => handler(context)))
               .Concat()
               .TakeWhile(_ => !context.IsHandled)
               .IgnoreElements()
               .Select(xxx =>
               {
                   return default(TOutput)!;
               });

        var seetype7 =  GetHandlers()
               //.Reverse()      //the point of MyInteraction
               .ToObservable()
               .ObserveOn(_handlerScheduler)
               .Select(handler => Observable.Defer(() => handler(context)))
               .Concat()
               .TakeWhile(_ => !context.IsHandled)
               .IgnoreElements()
               .Select(xxx => {
                   Debug.WriteLine("Select {xxx} PPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPPP");
                   return default(TOutput)!;
               })
               .Concat(
                       Observable.Defer(
                                        () => context.IsHandled
                                                  ? Observable.Return(context.GetOutput())
                                                  : Observable.Throw<TOutput>(new Exception("not handled:"))));




        // end playground
#endif

        return GetHandlers()
               //.Reverse()      //the point of MyInteraction
               .ToObservable()
               .ObserveOn(_handlerScheduler)
               .Select(handler => Observable.Defer(() => handler(context)))
               .Concat()
               .TakeWhile(_ => !context.IsHandled)
               .IgnoreElements()
               .Select(xxx => {
                   return default(TOutput)!;
                   })
               .Concat(
                       Observable.Defer(
                                        () => context.IsHandled
                                                  ? Observable.Return(context.GetOutput())
                                                  : Observable.Throw<TOutput>(new Exception("not handled:"))));
    }

    protected Func<MyInteractionContext<TInput, TOutput>, IObservable<Unit>>[] GetHandlers()
    {
        lock (_sync)
        {
            return _handlers.ToArray();
        }
    }

    private void AddHandler(Func<MyInteractionContext<TInput, TOutput>, IObservable<Unit>> handler)
    {
        lock (_sync)
        {
            _handlers.Add(handler);
        }
    }

    private void RemoveHandler(Func<MyInteractionContext<TInput, TOutput>, IObservable<Unit>> handler)
    {
        lock (_sync)
        {
            _handlers.Remove(handler);
        }
    }
}






public sealed class MyInteractionContext<TInput, TOutput>
{
    private TOutput _output = default!;
    private int _outputSet;

    public MyInteractionContext(TInput input) => Input = input;

    public TInput Input { get; }

    public bool IsHandled => _outputSet == 1;

    public void SetOutput(TOutput output)
    {
        if (Interlocked.CompareExchange(ref _outputSet, 1, 0) != 0)
        {
            throw new InvalidOperationException("Output has already been set.");
        }

        _output = output;
    }

    public TOutput GetOutput()
    {
        if (_outputSet == 0)
        {
            throw new InvalidOperationException("Output has not been set.");
        }

        return _output;
    }
}
