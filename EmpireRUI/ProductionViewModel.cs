using System.Reactive.Linq;
using System.Windows.Shapes;

namespace EmpireRUI;

public class ProductionViewModel : ReactiveObject, IRoutableViewModel, IDisposable
{
    private ProductionData productionData;
    private IDisposable handlerDisposable;
    public ProductionViewModel(IScreen screen, ProductionData pd, MapViewModel mvm)
    {
        Debug.WriteLine($"creating new ProductionViewModel, observableOK: {observableOK.GetHashCode()}");

        HostScreen = screen;
        //empire = e;
        productionData = pd;
        
        handlerDisposable = mvm.ProductionInteraction.RegisterHandler(async interaction =>
        {
            //Debug.WriteLine($"444 register handler {observableOK}");
            //observableOK.Subscribe(x => Debug.WriteLine($"observableOK: {x}"));
            //Debug.WriteLine($"444b register handler{observableOK.IsDisposed}");

            HostScreen.Router.Navigate.Execute(this);
            Debug.WriteLine($"444c register handler disposed:{observableOK.IsDisposed}");

            bool ok = await observableOK;
            Debug.WriteLine($"444d register handler disposed:{observableOK.IsDisposed}");

            //at System.Reactive.Subjects.AsyncSubject`1.GetResult()
            //at EmpireRUI.ProductionViewModel.<< -ctor > b__1_0 > d.MoveNext() in G:\cs\baci\empire\Empire\EmpireRUI\ProductionViewModel.cs:line 20

            //Debug.WriteLine($"1 observableOK: {ok}");
            if (ok) 
            {
                Debug.WriteLine($"2 setting output: {productionData}");
                interaction.SetOutput(productionData);
            }
            HostScreen.Router.NavigateBack.Execute( Unit.Default);
            //Debug.WriteLine($"444e register handler{observableOK}");

        });

        Debug.WriteLine("RegisterHandler(Func<MyInteractionContext<TInput, TOutput>, Task> handler)zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzz");

    }
    public Subject<bool> observableOK = new();
    public void Dispose()
    {
        UnregisterHandler();
    }

    public void UnregisterHandler()
    {
        handlerDisposable.Dispose();
    }

    public ProductionData Production
    {
        get => productionData;
        set => this.RaiseAndSetIfChanged(ref productionData, value);
    }

    public string? UrlPathSegment => "Production";
    public IScreen HostScreen { get; set; }

    internal void OK()
    {
        Debug.WriteLine($"observableOK before OnNext object id is {observableOK.GetHashCode()}");
        observableOK.OnNext(true);
        observableOK.OnCompleted();
        Debug.WriteLine($"observableOK after OnCopleted object id is {observableOK.GetHashCode()}");
    }
    internal void Cancel()
    {
        Debug.WriteLine($"observableOK before OnNext in Cancel object id is {observableOK.GetHashCode()}");
        observableOK.OnNext(false);
        observableOK.OnCompleted();
    }

}


public class ProductionData
{

    public ProductionEnum production;

}


public enum ProductionEnum
{
    army = 0,
    figher,
    destroyer,
    transport,
    submarine,
    cruiser,
    carrier,
    battleship,
    uninitialized
}

/*
public class My12Interaction<TInput, TOutput> : Interaction<TInput,TOutput>
{
    protected Func<InteractionContext<TInput, TOutput>, IObservable<Unit>>[] GetHandlers()
                   InteractionContext<TInput, TOutput>  IObservable<Unit>
                   param                                returns 
     
    public override IObservable<TOutput> Handle(TInput input)
    {
        var context = new InteractionContext<TInput, TOutput>(input);

        return GetHandlers()
               .Reverse()
               .ToObservable()
               .ObserveOn(_handlerScheduler)
               .Select(handler => Observable.Defer(() => handler(context)))
               .Concat()
               .TakeWhile(_ => !context.IsHandled)
               .IgnoreElements()
               .Select(_ => default(TOutput)!)
               .Concat(
                       Observable.Defer(
                                        () => context.IsHandled
                                                  ? Observable.Return(context.GetOutput())
                                                  : Observable.Throw<TOutput>(new UnhandledInteractionException<TInput, TOutput>(this, input))));
    }

    //222222222

}
*/


