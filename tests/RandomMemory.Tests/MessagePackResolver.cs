using MessagePack;
using MessagePack.Resolvers;

namespace RandomMemory.Tests;

[CompositeResolver(typeof(RandomMemoryResolver), typeof(StandardResolver))]
public partial class MessagePackResolver;
