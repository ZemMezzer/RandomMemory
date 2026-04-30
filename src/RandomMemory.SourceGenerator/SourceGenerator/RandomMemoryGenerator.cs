using System.Diagnostics;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RandomMemory.GeneratorCore;
using RandomMemory.Utility;
using DatabaseBuilderTemplate = RandomMemory.GeneratorCore.DatabaseBuilderTemplate;
using ImmutableBuilderTemplate = RandomMemory.GeneratorCore.ImmutableBuilderTemplate;
using MemoryDatabaseTemplate = RandomMemory.GeneratorCore.MemoryDatabaseTemplate;
using MessagePackResolverTemplate = RandomMemory.GeneratorCore.MessagePackResolverTemplate;
using TableTemplate = RandomMemory.GeneratorCore.TableTemplate;

namespace RandomMemory.SourceGenerator;

[Generator(LanguageNames.CSharp)]
public partial class RandomMemoryGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(RandomMemoryGeneratorOptions.EmitAttribute);

        var namespaceProvider = context.AnalyzerConfigOptionsProvider.Select((x, _) =>
            {
                x.GlobalOptions.TryGetValue("build_property.RootNamespace", out var defaultNamespace);
                return defaultNamespace;
            })
            .WithTrackingName("RandomMemory.AnalyzerConfig");

        var generatorOptions = context.CompilationProvider.Select((compilation, _) =>
            {
                foreach (var attr in compilation.Assembly.GetAttributes())
                {
                    if (attr.AttributeClass?.Name == "RandomMemoryGeneratorOptionsAttribute")
                    {
                        return RandomMemoryGeneratorOptions.FromAttribute(attr);
                    }
                }

                return default;
            })
            .WithTrackingName("RandomMemory.CompilationProvider");

        var memoryTables = context.SyntaxProvider.ForAttributeWithMetadataName("RandomMemory.MemoryTableAttribute",
            (node, token) => true,
            (ctx, token) => ctx)
            .WithTrackingName("RandomMemory.SyntaxProvider.0_ForAttributeWithMetadataName")
            .Collect()
            .Select((xs, _) =>
            {
                var list = new List<GenerationContext>();
                var reporter = new DiagnosticReporter();
                foreach (var ctx in xs)
                {
                    var memoryTableAttr = ctx.Attributes[0]; // AllowMultiple=false
                    var classDecl = ctx.TargetNode as TypeDeclarationSyntax; // class or record
                    var context = CodeGenerator.CreateGenerationContext(classDecl!, memoryTableAttr, reporter);
                    if (context != null)
                    {
                        list.Add(context);
                    }
                }
                list.Sort((a, b) => string.Compare(a.ClassName, b.ClassName, StringComparison.Ordinal));
                return (reporter, new EquatableArray<GenerationContext>(list.ToArray()));
            })
            .WithTrackingName("RandomMemory.SyntaxProvider.1_CollectAndSelect");

        var allCombined = memoryTables
            .Combine(namespaceProvider)
            .Combine(generatorOptions)
            .WithTrackingName("RandomMemory.SyntaxProvider.2_AllCombined");

        context.RegisterSourceOutput(allCombined, EmitMemoryTable);
    }

    void EmitMemoryTable(SourceProductionContext context, (((DiagnosticReporter, EquatableArray<GenerationContext>), string?), RandomMemoryGeneratorOptions) value)
    {
        var (((diagnostic, memoryTables), defaultNamespace), generatorOptions) = value;
        diagnostic.ReportToContext(context);
        if (memoryTables.Length == 0)
        {
            return;
        }

        var usingNamespace = generatorOptions.Namespace ?? defaultNamespace ?? "RandomMemory";
        var prefixClassName = generatorOptions.PrefixClassName ?? "";
        var throwIfKeyNotFound = !generatorOptions.IsReturnNullIfKeyNotFound; // becareful, reverse!

        var usingStrings = string.Join(Environment.NewLine, memoryTables.SelectMany(x => x.UsingStrings).Distinct().OrderBy(x => x, StringComparer.Ordinal));

        var builderTemplate = new DatabaseBuilderTemplate();
        var databaseTemplate = new MemoryDatabaseTemplate();
        var immutableBuilderTemplate = new ImmutableBuilderTemplate();
        var resolverTemplate = new MessagePackResolverTemplate();
        builderTemplate.Namespace = databaseTemplate.Namespace = immutableBuilderTemplate.Namespace = resolverTemplate.Namespace = usingNamespace;
        builderTemplate.PrefixClassName = databaseTemplate.PrefixClassName = immutableBuilderTemplate.PrefixClassName = resolverTemplate.PrefixClassName = prefixClassName;
        builderTemplate.Using = databaseTemplate.Using = immutableBuilderTemplate.Using = resolverTemplate.Using = (usingStrings + Environment.NewLine + ("using " + usingNamespace + ".Tables;"));
        builderTemplate.GenerationContexts = databaseTemplate.GenerationContexts = immutableBuilderTemplate.GenerationContexts = resolverTemplate.GenerationContexts = memoryTables.ToArray();

        Log(AddSource(context, builderTemplate.ClassName, builderTemplate.TransformText()));
        Log(AddSource(context, immutableBuilderTemplate.ClassName, immutableBuilderTemplate.TransformText()));
        Log(AddSource(context, databaseTemplate.ClassName, databaseTemplate.TransformText()));
        Log(AddSource(context, resolverTemplate.ClassName, resolverTemplate.TransformText()));

        foreach (var generationContext in memoryTables)
        {
            var template = new TableTemplate()
            {
                Namespace = usingNamespace,
                GenerationContext = generationContext,
                Using = string.Join(Environment.NewLine, generationContext.UsingStrings),
                ThrowKeyIfNotFound = throwIfKeyNotFound
            };

            Log(AddSource(context, generationContext.ClassName + "Table", template.TransformText()));
        }
    }

    static void Log(string msg) => Trace.WriteLine(msg);

    static string AddSource(SourceProductionContext context, string fileName, string content)
    {
        var contentString = NormalizeNewLines(content);
        context.AddSource($"RandomMemory.{fileName}.g.cs", contentString);
        return $"Generate {fileName}.";

        static string NormalizeNewLines(string content)
        {
            // The T4 generated code may be text with mixed line ending types. (CR + CRLF)
            // We need to normalize the line ending type in each Operating Systems. (e.g. Windows=CRLF, Linux/macOS=LF)
            return content.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
        }
    }
}
