Janga - A Fluent Validation Library
===================================

Introduction
------------
Janga is a Validation library that uses a fluent interface, thus allowing you to write
a validation similar to this:

```csharp
bool passed = employee.Enforce()
			.When("Age", Compares.IsGreaterThan, 45)
			.When("Department", Compares.In, deptList)
			.IsValid();
if(passed)
{
    SomeProcess();
}