using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using Dalamud.Plugin;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;

namespace Replica.Scripting.Host;

public static class ScriptCompiler
{
	private static readonly string[] InjectedUsings = new string[6] { "System", "System.Collections.Generic", "System.Linq", "System.Numerics", "System.Threading.Tasks", "Replica.Scripting.Api" };

	private static ImmutableArray<MetadataReference> _references;

	public static CompileResult Compile(string assemblyName, string source, string displayName)
	{
		List<string> list = new List<string>();
		try
		{
			CSharpParseOptions options = new CSharpParseOptions(LanguageVersion.Latest);
			SyntaxTree tree = CSharpSyntaxTree.ParseText(source, options, displayName);
			CSharpCompilation cSharpCompilation = Build(assemblyName, Prepare(tree, options));
			ImmutableArray<Diagnostic> diagnostics = cSharpCompilation.GetDiagnostics();
			List<UsingDirectiveSyntax> list2 = UnresolvedUsings(diagnostics, cSharpCompilation.SyntaxTrees.First());
			if (list2.Count > 0)
			{
				SyntaxTree tree2 = Prune(cSharpCompilation.SyntaxTrees.First(), list2, options);
				cSharpCompilation = Build(assemblyName, tree2);
				diagnostics = cSharpCompilation.GetDiagnostics();
			}
			List<Diagnostic> list3 = diagnostics.Where((Diagnostic d) => d.Severity == DiagnosticSeverity.Error).ToList();
			if (list3.Count > 0)
			{
				foreach (Diagnostic item in list3.Take(20))
				{
					list.Add(item.ToString());
				}
				return new CompileResult
				{
					Errors = list
				};
			}
			using MemoryStream memoryStream = new MemoryStream();
			EmitResult emitResult = cSharpCompilation.Emit(memoryStream);
			if (!emitResult.Success)
			{
				foreach (Diagnostic item2 in emitResult.Diagnostics.Where((Diagnostic d) => d.Severity == DiagnosticSeverity.Error).Take(20))
				{
					list.Add(item2.ToString());
				}
				return new CompileResult
				{
					Errors = list
				};
			}
			memoryStream.Seek(0L, SeekOrigin.Begin);
			AssemblyLoadContext assemblyLoadContext = AssemblyLoadContext.GetLoadContext(typeof(ScriptCompiler).Assembly) ?? AssemblyLoadContext.Default;
			return new CompileResult
			{
				Assembly = assemblyLoadContext.LoadFromStream(memoryStream)
			};
		}
		catch (Exception ex)
		{
			list.Add(ex.Message);
			return new CompileResult
			{
				Errors = list
			};
		}
	}

	private static CSharpCompilation Build(string assemblyName, SyntaxTree tree)
	{
		SyntaxTree[] syntaxTrees = new SyntaxTree[1] { tree };
		object references = References();
		AssemblyIdentityComparer assemblyIdentityComparer = DesktopAssemblyIdentityComparer.Default;
		return CSharpCompilation.Create(assemblyName, syntaxTrees, (IEnumerable<MetadataReference>?)references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary, reportSuppressedDiagnostics: false, null, null, null, null, OptimizationLevel.Release, checkOverflow: false, allowUnsafe: true, null, null, default(ImmutableArray<byte>), null, Platform.AnyCpu, ReportDiagnostic.Default, 4, null, concurrentBuild: true, deterministic: false, null, null, null, assemblyIdentityComparer));
	}

	private static SyntaxTree Prepare(SyntaxTree tree, CSharpParseOptions options)
	{
		CompilationUnitSyntax compilationUnitSyntax = (CompilationUnitSyntax)tree.GetRoot();
		HashSet<string> existing = compilationUnitSyntax.Usings.Select((UsingDirectiveSyntax u) => u.Name?.ToString()).ToHashSet();
		string[] array = InjectedUsings.Where((string n) => !existing.Contains(n)).ToArray();
		if (array.Length == 0)
		{
			return tree;
		}
		UsingDirectiveSyntax[] items = SyntaxFactory.ParseCompilationUnit(string.Concat(array.Select((string n) => "using " + n + ";\r\n"))).Usings.ToArray();
		return CSharpSyntaxTree.Create(compilationUnitSyntax.AddUsings(items), options, tree.FilePath);
	}

	private static List<UsingDirectiveSyntax> UnresolvedUsings(ImmutableArray<Diagnostic> diagnostics, SyntaxTree tree)
	{
		List<UsingDirectiveSyntax> list = new List<UsingDirectiveSyntax>();
		SyntaxNode root = tree.GetRoot();
		foreach (Diagnostic item in diagnostics)
		{
			if (item.Severity != DiagnosticSeverity.Error || (item.Id != "CS0246" && item.Id != "CS0234" && item.Id != "CS0400") || item.Location.SourceTree != tree)
			{
				continue;
			}
			UsingDirectiveSyntax usingDirectiveSyntax = root.FindNode(item.Location.SourceSpan, findInsideTrivia: false, getInnermostNodeForTie: true).FirstAncestorOrSelf<UsingDirectiveSyntax>();
			if (usingDirectiveSyntax != null && !list.Contains(usingDirectiveSyntax))
			{
				string text = usingDirectiveSyntax.Name?.ToString();
				if (text == null || !InjectedUsings.Contains(text))
				{
					list.Add(usingDirectiveSyntax);
				}
			}
		}
		return list;
	}

	private static SyntaxTree Prune(SyntaxTree tree, List<UsingDirectiveSyntax> dead, CSharpParseOptions options)
	{
		return CSharpSyntaxTree.Create((CompilationUnitSyntax)tree.GetRoot().RemoveNodes(dead, SyntaxRemoveOptions.KeepNoTrivia), options, tree.FilePath);
	}

	private static ImmutableArray<MetadataReference> References()
	{
		if (!_references.IsDefaultOrEmpty)
		{
			return _references;
		}
		Dictionary<string, string> byName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
		Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
		foreach (Assembly assembly in assemblies)
		{
			if (!assembly.IsDynamic)
			{
				try
				{
					Offer(assembly.Location);
				}
				catch
				{
				}
			}
		}
		Offer(SelfPath());
		foreach (string item in ProbeDirs())
		{
			if (string.IsNullOrEmpty(item) || !Directory.Exists(item))
			{
				continue;
			}
			foreach (string item2 in Directory.EnumerateFiles(item, "*.dll"))
			{
				Offer(item2);
			}
		}
		ImmutableArray<MetadataReference>.Builder builder = ImmutableArray.CreateBuilder<MetadataReference>();
		foreach (string value in byName.Values)
		{
			try
			{
				builder.Add(MetadataReference.CreateFromFile(value));
			}
			catch
			{
			}
		}
		_references = builder.ToImmutable();
		return _references;
		void Offer(string? path)
		{
			if (!string.IsNullOrEmpty(path) && File.Exists(path))
			{
				string text;
				try
				{
					text = AssemblyName.GetAssemblyName(path).Name ?? "";
				}
				catch
				{
					return;
				}
				if (text.Length != 0)
				{
					byName.TryAdd(text, path);
				}
			}
		}
	}

	private static string? SelfPath()
	{
		try
		{
			string text = Plugin.PluginInterface?.AssemblyLocation?.FullName;
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
		}
		catch
		{
		}
		try
		{
			return typeof(ScriptCompiler).Assembly.Location;
		}
		catch
		{
			return null;
		}
	}

	private static IEnumerable<string> ProbeDirs()
	{
		yield return SafeDir(typeof(object).Assembly.Location);
		yield return SafeDir(SelfPath());
		yield return SafeDir(typeof(IDalamudPluginInterface).Assembly.Location);
	}

	private static string SafeDir(string? path)
	{
		if (string.IsNullOrEmpty(path))
		{
			return "";
		}
		try
		{
			return Path.GetDirectoryName(path) ?? "";
		}
		catch
		{
			return "";
		}
	}
}
