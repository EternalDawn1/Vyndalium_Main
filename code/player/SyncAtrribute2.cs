namespace GeneralGame;

[AttributeUsage( AttributeTargets.Property )]
[CodeGenerator( CodeGeneratorFlags.WrapPropertySet | CodeGeneratorFlags.Instance, "__sync_SetValue", 0 )]
[CodeGenerator( CodeGeneratorFlags.WrapPropertyGet | CodeGeneratorFlags.Instance, "__sync_GetValue", 0 )]
[Description( "Automatically synchronize a property of a networked object from the owner to other clients." )]
[SourceLocation( "Scene\\Networking\\Sync.cs", 6 )]
public class SyncAttribute2 : Attribute
{
    //
    // Summary:
    //     Query this value for changes rather than counting on set being called. This is
    //     appropriate if the value returned by its getter can change without calling its
    //     setter.
    [Description( "Query this value for changes rather than counting on set being called. This is appropriate if the value returned by its getter can change without calling its setter." )]
   
    public bool Query { get; set; }
}