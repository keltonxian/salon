using PureMVC.Patterns.Proxy;

public class LoadingProxy : Proxy
{
    public new static string NAME = "LoadingProxy";

    public LoadingProxy(object data = null) : base(NAME, data)
    {

    }
}
