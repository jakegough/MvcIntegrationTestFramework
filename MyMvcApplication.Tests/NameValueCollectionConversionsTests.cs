using System.Collections.Specialized;
using MvcIntegrationTestFramework;
using NUnit.Framework;

namespace MyMvcApplication.Tests
{
    [TestFixture]
    public class When_converting_an_object_with_one_string_property_to_name_value_collection
    {
        private NameValueCollection convertedFromObjectWithString;


        [Test]
        public void Should_have_key_of_name_with_value_hello()
        {
            convertedFromObjectWithString = NameValueCollectionConversions.ConvertFromObject(new { name = "hello" });
            Assert.That(convertedFromObjectWithString["name"], Is.EqualTo("hello"));
        }
    }

    [TestFixture]
    public class When_converting_an_object_has_2_properties_to_name_value_collection
    {
        private NameValueCollection converted;

        [OneTimeSetUp]
        public void _When_converting_an_object_has_2_properties_to_name_value_collection()
        {
            converted = NameValueCollectionConversions.ConvertFromObject(new { name = "hello", age = 30 });
        }

        [Test]
        public void Should_have_2_elements_in_collection()
        {
            Assert.AreEqual(2, converted.Count);
        }

        [Test]
        public void Should_have_key_of_name_and_value_of_hello()
        {
            Assert.AreEqual("hello", converted["name"]);
        }

        [Test]
        public void Should_have_key_of_age_and_value_of_30()
        {
            Assert.AreEqual("30", converted["age"]);
        }
    }

    [TestFixture]
    public class When_converting_an_object_that_has_a_nested_anonymous_object
    {
        private NameValueCollection converted;
       
        [OneTimeSetUp]
        public void _When_converting_an_object_that_has_a_nested_anonymous_object()
        {
            converted = NameValueCollectionConversions.ConvertFromObject(new {Form = new {name = "hello", age = 30}});
        }

        [Test]
        public void Should_have_2_elements()
        {
            Assert.AreEqual(2,converted.Count);
        }

        [Test]
        public void Should_have_key_of_Formdotname_with_value_hello()
        {
            Assert.AreEqual("hello",converted["Form.name"]);
        }

        [Test]
        public void Should_have_key_of_Formdotage_with_value_30()
        {
            Assert.AreEqual("30",converted["Form.age"]);
        }
    }
}
