namespace Swan.Test.Mocks;

public interface IAnimal
{
    string Name { get; }
}

public class Monkey : IAnimal
{
    public string Name => nameof(Monkey);
}

public class Fish : IAnimal
{
    public string Name => nameof(Fish);

    [AttributeMock("This is an Attribute")]
    public virtual string GetFeeding() => "Worms";
}

[AttributeUsage(AttributeTargets.All)]
public sealed class AttributeMock : Attribute
{
    public AttributeMock(string name)
    {
        Name = name;
    }

    public string Name { get; }
}

public class Clown : Fish
{
    [AttributeMock("This is an Attribute")]
    public string GetName() => "Nemo";
}

public class Shark : Fish
{
    public string GetName() => "Lenny";

    public override string GetFeeding() => "Seals";
}
