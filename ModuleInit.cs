using System.Management.Automation;
using System.Reflection;
using PwshPrompt.Consts;

namespace PwshPrompt;

/// <summary>
/// Registers and removes custom type accelerators on module import/removal.
/// </summary>
public sealed class ModuleInit : IModuleAssemblyInitializer, IModuleAssemblyCleanup
{
	private static readonly Dictionary<string, Type> _accelerators = new()
	{
		["Colors"] = typeof(Colors),
	};

	private static readonly MethodInfo? _addMethod;
	private static readonly MethodInfo? _removeMethod;

	static ModuleInit()
	{
		var type = typeof(PSObject).Assembly
			.GetType("System.Management.Automation.TypeAccelerators");
		_addMethod = type?.GetMethod("Add", BindingFlags.Public | BindingFlags.Static);
		_removeMethod = type?.GetMethod("Remove", BindingFlags.Public | BindingFlags.Static);
	}

	/// <inheritdoc />
	public void OnImport()
	{
		foreach (var (name, type) in _accelerators)
			_addMethod?.Invoke(null, new object[] { name, type });
	}

	/// <inheritdoc />
	public void OnRemove(PSModuleInfo module)
	{
		foreach (var name in _accelerators.Keys)
			_removeMethod?.Invoke(null, new object[] { name });
	}
}
