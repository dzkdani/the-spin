using NUnit.Framework;
using System;
using System.Reflection;

public class ServiceLocatorTests
{
    private sealed class TestService
    {
    }

    [SetUp]
    public void SetUp()
    {
        InvokeServiceLocator("Unregister", new TestService());
    }

    [TearDown]
    public void TearDown()
    {
        TestService service = (TestService)InvokeServiceLocator("Get");

        if (service != null)
            InvokeServiceLocator("Unregister", service);
    }

    [Test]
    public void UnregisterDoesNotRemoveReplacementService()
    {
        TestService first = new TestService();
        TestService replacement = new TestService();

        Assert.IsTrue((bool)InvokeServiceLocator("TryRegister", first));
        Assert.IsFalse((bool)InvokeServiceLocator("TryRegister", replacement));

        InvokeServiceLocator("Register", replacement);
        InvokeServiceLocator("Unregister", first);

        Assert.AreSame(replacement, InvokeServiceLocator("Get"));
    }

    private static object InvokeServiceLocator(string methodName, params object[] arguments)
    {
        Type serviceLocator = Type.GetType("ServiceLocator, Assembly-CSharp");
        Type serviceType = typeof(TestService);
        MethodInfo method = serviceLocator.GetMethod(methodName)
            .MakeGenericMethod(serviceType);

        return method.Invoke(null, arguments);
    }
}
