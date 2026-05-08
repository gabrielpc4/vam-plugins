using System.Collections.Generic;

namespace DynamicCSharp.Compiler;

internal interface ICompiler
{
	string OutputDirectory { get; set; }

	bool GenerateSymbols { get; set; }

	byte[] AssemblyData { get; }

	byte[] SymbolsData { get; }

	void AddReference(string reference);

	void AddReferences(IEnumerable<string> references);

	ScriptCompilerError[] CompileFiles(string[] source);

	ScriptCompilerError[] CompileSource(string[] source);
}
