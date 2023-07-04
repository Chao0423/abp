﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;

namespace Volo.Abp.Modularity;

public class AbpModuleDescriptor : IAbpModuleDescriptor
{
    public Type Type { get; }

    public Assembly Assembly { get; }
    
    public List<Assembly> AllAssemblies { get; }

    public IAbpModule Instance { get; }

    public bool IsLoadedAsPlugIn { get; }

    public IReadOnlyList<IAbpModuleDescriptor> Dependencies => _dependencies.ToImmutableList();
    private readonly List<IAbpModuleDescriptor> _dependencies;

    public AbpModuleDescriptor(
        [NotNull] Type type,
        [NotNull] IAbpModule instance,
        bool isLoadedAsPlugIn)
    {
        Check.NotNull(type, nameof(type));
        Check.NotNull(instance, nameof(instance));
        AbpModule.CheckAbpModuleType(type);

        if (!type.GetTypeInfo().IsAssignableFrom(instance.GetType()))
        {
            throw new ArgumentException($"Given module instance ({instance.GetType().AssemblyQualifiedName}) is not an instance of given module type: {type.AssemblyQualifiedName}");
        }

        Type = type;
        Assembly = type.Assembly;
        AllAssemblies = CreateAllAssembliesList(type);
        Instance = instance;
        IsLoadedAsPlugIn = isLoadedAsPlugIn;

        _dependencies = new List<IAbpModuleDescriptor>();
    }

    public void AddDependency(IAbpModuleDescriptor descriptor)
    {
        _dependencies.AddIfNotContains(descriptor);
    }

    public override string ToString()
    {
        return $"[AbpModuleDescriptor {Type.FullName}]";
    }
    
    private static List<Assembly> CreateAllAssembliesList(Type moduleType)
    {
        var assemblies = new List<Assembly>();

        var additionalAssemblyDescriptors = moduleType
            .GetCustomAttributes()
            .OfType<IAdditionalModuleAssemblyProvider>();

        foreach (var descriptor in additionalAssemblyDescriptors)
        {
            foreach (var assembly in descriptor.GetAssemblies())
            {
                assemblies.AddIfNotContains(assembly);
            }
        }
        
        assemblies.Add(moduleType.Assembly);

        return assemblies;
    }
}
