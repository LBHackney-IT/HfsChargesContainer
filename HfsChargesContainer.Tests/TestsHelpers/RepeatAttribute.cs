using System.Collections.Generic;
using System.Reflection;
using Xunit.Sdk;

/// 'Repeat' test attribute that is used to debug testss flake
/// Usage: Mark your test method as [Theory] and add [Repeat(100)] below it.
/// Then add a 'int _' input parameter to you test method signature.
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
        for (int i = 1; i <= _count; i++)
        {
            yield return new object[] { i };
        }
    }
}
