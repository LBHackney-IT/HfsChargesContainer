using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

public class RepeatAttribute : DataAttribute
{
    private readonly int _count;

    public RepeatAttribute(int count)
    {
        if (count < 1)
        {
            throw new System.ArgumentOutOfRangeException(nameof(count), "Repeat count must be greater than zero.");
        }
        _count = count;
    }

    public override IEnumerable<object[]> GetData(MethodInfo testMethod)
    {
        for (int i = 0; i < _count; i++)
        {
            yield return new object[0];
        }
    }
}
