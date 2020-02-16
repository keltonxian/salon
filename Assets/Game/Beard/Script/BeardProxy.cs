using PureMVC.Patterns.Proxy;

public class BeardProxy : Proxy
{
    public new static string NAME = "BeardProxy";

    public BeardProxy(object data = null) : base(NAME, data)
    {

    }
}
