#nullable disable

namespace RandomMemory.GeneratorCore
{
    public interface ITemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public string PrefixClassName { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
        public string ClassName { get; }
    }

    public partial class DatabaseBuilderTemplate : ITemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public string PrefixClassName { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }

        public string ClassName => PrefixClassName + "DatabaseBuilder";
    }

    public partial class MemoryDatabaseTemplate : ITemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public string PrefixClassName { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
        public string ClassName => PrefixClassName + "MemoryDatabase";
    }

    public partial class MetaMemoryDatabaseTemplate : ITemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public string PrefixClassName { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
        public string ClassName => PrefixClassName + "MetaMemoryDatabase";
    }

    public partial class TransactionTemplate : ITemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public string PrefixClassName { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
        public string ClassName => PrefixClassName + "Transaction";
    }

    public partial class MessagePackResolverTemplate : ITemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public string PrefixClassName { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
        public string ClassName => PrefixClassName + "RandomMemoryResolver";
    }

    public partial class DatabaseSessionTemplate : ITemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public string PrefixClassName { get; set; }
        public GenerationContext[] GenerationContexts { get; set; }
        public string ClassName => PrefixClassName + "DatabaseSession";
    }

    public partial class TableTemplate
    {
        public string Namespace { get; set; }
        public string Using { get; set; }
        public string PrefixClassName { get; set; }
        public GenerationContext GenerationContext { get; set; }

        public bool ThrowKeyIfNotFound { get; set; }
    }
}
