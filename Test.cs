namespace Syntex;

public interface ITest
{
    public int Property { get; set; }
    public int Method();
}

public class Test : ITest
{
    public virtual int Property { get; set; }
    public virtual int Method()
    {
        throw new NotImplementedException();
    }
}

public class Test<T> : Test
{
    public string? SomeProp { get; set; }
    public override int Property { get; set; }
    public override int Method()
    {
        throw new NotImplementedException();
    }
}
public class Test<T,U> : Test<T>
{
    public string? SomeProp2 { get; set; }
}
