namespace Syntex;

public interface ITest
{
    public int Property { get; set; }
    public int Method();
}

public class Test : ITest
{
    public int Property { get; set; }
    public int Method()
    {
        throw new NotImplementedException();
    }
}

public class Test<T> : Test
{
    public int Property { get; set; }
    public string SomeProp { get; set; }
    public int Method()
    {
        throw new NotImplementedException();
    }
}
public class Test<T,U> : Test<T>
{
    public int Property { get; set; }
    public string SomeProp { get; set; }
    public int Method()
    {
        throw new NotImplementedException();
    }
}
