namespace NRTools
{
    using System;

    [AttributeUsage(AttributeTargets.Method)]
    public class Setup : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class Test : Attribute { }

    [AttributeUsage(AttributeTargets.Method)]
    public class Teardown : Attribute { }

}