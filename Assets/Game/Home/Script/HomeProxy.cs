using PureMVC.Patterns.Proxy;

public class HomeProxy : Proxy
{
    public new static string NAME = "HomeProxy";

    public HomeProxy(object data = null) : base(NAME, data)
    {

    }
}
