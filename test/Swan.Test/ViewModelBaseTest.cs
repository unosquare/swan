using AutoFixture;
using NUnit.Framework;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Swan.Test
{
    [TestFixture]
    public class ViewModelBaseTest
    {
        [Test]
        public void ViewModelBaseInstance()
        {
            var fixture = new Fixture();
            var view = new PersonView();
            var imax = new Random().Next(10);

            for (int i = 0; i < imax; i++)
                view.SetName(fixture.Create<string>());

            Assert.That(view.ChangeCounter, Is.EqualTo(imax));
        }

        [Test]
        public void ViewModelBaseNoChange() {
            var view = new PersonView("Unosquare");
            view.SetName("Unosquare");

            Assert.That(view.ChangeCounter, Is.EqualTo(0));
        }

        [Test]
        public void ViewModelBaseAuxiliar()
        {
            var view = new PersonView();

            // 2 properties will be updated (name and age)
            view.SetProperties("Unosquare", 10);

            Assert.That(view.ChangeCounter, Is.EqualTo(2));
        }

        [Test]
        public async Task ViewModelBaseDeferredNotification()
        {
            var view = new PersonView("", true);
            var actualCount = view.ChangeCounter;

            await view.SetNameAsync("Unosquare");
            Assert.That(view.ChangeCounter, Is.GreaterThan(actualCount));
        }
    }

    internal class PersonView : ViewModelBase
    {
        private string name;
        private int age;

        public int ChangeCounter { get; private set; }

        public PersonView(string initValue = "", bool deferredNotifications = false)
            : base(deferredNotifications)
        {
            name = initValue;
            this.PropertyChanged += Handler;
        }

        public void SetName(string value)
        {
            this.SetProperty(ref name, value);
        }

        public void SetProperties(string name, int age)
        {
            this.name = name;
            this.age = age;
            this.NotifyPropertyChanged(new string[] { "name", "age" });
        }

        public Task SetNameAsync(string value)
        {
            var actulaCount = ChangeCounter;
            this.SetProperty(ref name, value);

            return Task.Run(() => { while (ChangeCounter <= actulaCount) { } });
        }

        private void Handler(object sender, PropertyChangedEventArgs e) {
            ChangeCounter++;
        }
    }
}
