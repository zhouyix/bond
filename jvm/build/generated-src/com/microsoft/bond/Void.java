
//------------------------------------------------------------------------------
// This code was generated by a tool.
//
//   Tool : Bond Compiler 0.8.0.0
//   Input filename:  ../idl/bond/core/bond.bond
//   Output filename: Void.java
//
// Changes to this file may cause incorrect behavior and will be lost when
// the code is regenerated.
// <auto-generated />
//------------------------------------------------------------------------------

package com.microsoft.bond;


@javax.annotation.Generated("gbc")
public class Void implements com.microsoft.bond.BondSerializable {

    public static final com.microsoft.bond.SchemaDef SCHEMA = new com.microsoft.bond.SchemaDef();
    public static final com.microsoft.bond.StructDef STRUCT_DEF = new com.microsoft.bond.StructDef();
    
    private static boolean schemaInitialized = false;

    public static synchronized void initSchema() {
        if (schemaInitialized) { return; }

        SCHEMA.root.id = com.microsoft.bond.BondDataType.BT_STRUCT;
        SCHEMA.root.struct_def = 0;
        SCHEMA.root.element = null;
        SCHEMA.root.key = null;
        SCHEMA.root.bonded_type = false;

        STRUCT_DEF.metadata.name = "Void";
        STRUCT_DEF.metadata.qualified_name = "com.microsoft.bond.Void";
        STRUCT_DEF.metadata.modifier = com.microsoft.bond.Modifier.Optional;
        
        // TODO: .base_def
        SCHEMA.structs.add(0, STRUCT_DEF);

        

        schemaInitialized = true;
    }

    public static com.microsoft.bond.SchemaDef getSchema() {
        initSchema();
        return SCHEMA;
    }

    public static com.microsoft.bond.StructDef getStructDef() {
        initSchema();
        return STRUCT_DEF;
    }
    

    private final com.microsoft.bond.protocol.TaggedProtocolReader.ReadFieldResult __readFieldResult = new com.microsoft.bond.protocol.TaggedProtocolReader.ReadFieldResult();
    private final com.microsoft.bond.protocol.TaggedProtocolReader.ReadContainerResult __readContainerResult = new com.microsoft.bond.protocol.TaggedProtocolReader.ReadContainerResult();

    

    @Override
    public void serialize(com.microsoft.bond.protocol.ProtocolWriter writer) throws java.io.IOException {
        initSchema();

        writer.writeStructBegin(SCHEMA.structs.get(0).metadata);
        
        writer.writeStructEnd();
    }

    @Override
    public void deserialize(com.microsoft.bond.protocol.TaggedProtocolReader reader) throws java.io.IOException {
        initSchema();

        reader.readStructBegin();
        
        reader.readStructEnd();
    }

    @Override
    public void marshal(com.microsoft.bond.protocol.ProtocolWriter writer) throws java.io.IOException {
        writer.writeVersion();
        serialize(writer);
    }
}