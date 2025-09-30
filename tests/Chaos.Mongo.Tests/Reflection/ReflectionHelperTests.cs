// Copyright (c) 2025 Christian Flessa. All rights reserved.
// This file is licensed under the MIT license. See LICENSE in the project root for more information.
namespace Chaos.Mongo.Tests.Reflection;

using Chaos.Mongo.Reflection;
using FluentAssertions;
using NUnit.Framework;
using System.Reflection;

public class ReflectionHelperTests
{
    [Test]
    public void GetInterfaceImplementations_WhenAbstractClassExists_ExcludesAbstractClass()
    {
        // Arrange
        var interfaceType = typeof(ITestInterface);
        var assemblies = new[] { typeof(ReflectionHelperTests).Assembly };

        // Act
        var result = ReflectionHelper.GetInterfaceImplementations(interfaceType, assemblies);

        // Assert
        result.Should().NotContain(typeof(AbstractImplementation));
    }

    [Test]
    public void GetInterfaceImplementations_WhenAssembliesProvided_ScansOnlyProvidedAssemblies()
    {
        // Arrange
        var interfaceType = typeof(ITestInterface);
        var assemblies = new[] { typeof(ReflectionHelperTests).Assembly };

        // Act
        var result = ReflectionHelper.GetInterfaceImplementations(interfaceType, assemblies);

        // Assert
        result.Should().Contain(typeof(PublicConcreteImplementation));
    }

    [Test]
    public void GetInterfaceImplementations_WhenEmptyAssembliesProvided_ReturnsEmptyEnumerable()
    {
        // Arrange
        var interfaceType = typeof(ITestInterface);
        var assemblies = Enumerable.Empty<Assembly>();

        // Act
        var result = ReflectionHelper.GetInterfaceImplementations(interfaceType, assemblies);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetInterfaceImplementations_WhenImplementationsExist_ReturnsAllConcretePublicClasses()
    {
        // Arrange
        var interfaceType = typeof(ITestInterface);
        var assemblies = new[] { typeof(ReflectionHelperTests).Assembly };

        // Act
        var result = ReflectionHelper.GetInterfaceImplementations(interfaceType, assemblies).ToList();

        // Assert
        result.Should().Contain(typeof(PublicConcreteImplementation));
        result.Should().Contain(typeof(AnotherPublicImplementation));
    }

    [Test]
    public void GetInterfaceImplementations_WhenInterfaceTypeIsNull_ThrowsArgumentNullException()
    {
        // Act
        var act = () => ReflectionHelper.GetInterfaceImplementations(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>()
           .WithParameterName("interfaceType");
    }

    [Test]
    public void GetInterfaceImplementations_WhenNoAssembliesProvided_ScansAllLoadedAssemblies()
    {
        // Arrange
        var interfaceType = typeof(ITestInterface);

        // Act
        var result = ReflectionHelper.GetInterfaceImplementations(interfaceType);

        // Assert
        result.Should().Contain(typeof(PublicConcreteImplementation));
    }

    [Test]
    public void GetInterfaceImplementations_WhenNoImplementationsExist_ReturnsEmptyEnumerable()
    {
        // Arrange
        var interfaceType = typeof(IUnimplementedInterface);
        var assemblies = new[] { typeof(ReflectionHelperTests).Assembly };

        // Act
        var result = ReflectionHelper.GetInterfaceImplementations(interfaceType, assemblies);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public void GetInterfaceImplementations_WhenNonPublicClassExists_ExcludesNonPublicClass()
    {
        // Arrange
        var interfaceType = typeof(ITestInterface);
        var assemblies = new[] { typeof(ReflectionHelperTests).Assembly };

        // Act
        var result = ReflectionHelper.GetInterfaceImplementations(interfaceType, assemblies);

        // Assert
        result.Should().NotContain(typeof(InternalImplementation));
    }

    [Test]
    public void GetInterfaceImplementations_WhenTypeIsNotInterface_ThrowsArgumentException()
    {
        // Arrange
        var nonInterfaceType = typeof(ConcreteClass);

        // Act
        var act = () => ReflectionHelper.GetInterfaceImplementations(nonInterfaceType);

        // Assert
        act.Should().Throw<ArgumentException>()
           .WithParameterName("interfaceType")
           .WithMessage("*must be an interface*");
    }

    public abstract class AbstractImplementation : ITestInterface { }

    public class AnotherPublicImplementation : ITestInterface { }

    public class ConcreteClass { }

    // Test interfaces and classes
    public interface ITestInterface { }

    public interface IUnimplementedInterface { }

    public class PublicConcreteImplementation : ITestInterface { }

    internal class InternalImplementation : ITestInterface { }
}
