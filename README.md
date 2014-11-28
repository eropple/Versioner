# Versioner #
**Versioner** is a utility library to provide the notion of _versions_ and _dependencies_
for .NET applications and to implement common operations atop them.

Versioner is implemented as a Portable Class Library and should be usable in any .NET
context.

## Installation ##
Versioner <a href="https://www.nuget.org/packages/EdCanHack.Versioner/1.0.0" target="_blank">is
on NuGet</a>. To install, run the following command in the Package Manager Console:

```
PM> Install-Package EdCanHack.Versioner
```

## Usage ##
Your consuming application needs only to implement `IVersioned` on all versionable objects and
`IDepending` on all objects that require versioned dependencies; in use cases that create
a dependency graph, these objects will often be the same ones.

Common operations on `IVersioned` and `IDepending` objects can be found in the `Resolver`
class.

## Future Work ##
I don't have any immediate future plans for Versioner. I'm interested in other folks' use cases,
though, so feel free to file an issue or (preferably, of course) send a pull request.