using System;
using System.Collections;
using System.Collections.Generic;

namespace Unosquare.Swan.Test.Mocks
{
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

    public interface ICar
    {
        string Name { get; }
    }

    public class TheOnlyCar : ICar
    {
        public string Name => nameof(TheOnlyCar);
    }

    public class Controller
    {
        public IAnimal Animal { get; set; }

        public ICar Car { get; set; }
    }

    public class StringEnumerable : IEnumerable<string>
    {
        public IEnumerator<string> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    [AttributeUsage(AttributeTargets.All)]
    public class AttributeMock : Attribute
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
}