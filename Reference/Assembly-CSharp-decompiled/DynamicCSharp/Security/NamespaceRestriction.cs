using System;
using System.Collections.Generic;
using Mono.Cecil;
using UnityEngine;

namespace DynamicCSharp.Security;

[Serializable]
public sealed class NamespaceRestriction : Restriction
{
	[SerializeField]
	private string namespaceName = string.Empty;

	public string RestrictedNamespace => namespaceName;

	public override string Message => $"The namespace '{namespaceName}' is prohibited and cannot be referenced";

	public override RestrictionMode Mode => DynamicCSharp.Settings.namespaceRestrictionMode;

	public NamespaceRestriction(string restrictedName)
	{
		namespaceName = restrictedName;
	}

	public override bool Verify(ModuleDefinition module)
	{
		if (string.IsNullOrEmpty(namespaceName))
		{
			return true;
		}
		IEnumerable<TypeReference> typeReferences = module.GetTypeReferences();
		foreach (TypeReference item in typeReferences)
		{
			string @namespace = item.Namespace;
			if (string.Compare(namespaceName, @namespace) == 0)
			{
				return false;
			}
		}
		return true;
	}
}
