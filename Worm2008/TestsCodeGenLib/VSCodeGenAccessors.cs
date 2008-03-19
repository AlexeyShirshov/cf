﻿// ------------------------------------------------------------------------------
//<autogenerated>
//        This code was generated by Microsoft Visual Studio Team System 2005.
//
//        Changes to this file may cause incorrect behavior and will be lost if
//        the code is regenerated.
//</autogenerated>
//------------------------------------------------------------------------------
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestsCodeGenLib
{
[System.Diagnostics.DebuggerStepThrough()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TestTools.UnitTestGeneration", "1.0.0.0")]
internal class BaseAccessor {
    
    protected Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject m_privateObject;
    
    protected BaseAccessor(object target, Microsoft.VisualStudio.TestTools.UnitTesting.PrivateType type) {
        m_privateObject = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject(target, type);
    }
    
    protected BaseAccessor(Microsoft.VisualStudio.TestTools.UnitTesting.PrivateType type) : 
            this(null, type) {
    }
    
    internal virtual object Target {
        get {
            return m_privateObject.Target;
        }
    }
    
    public override string ToString() {
        return this.Target.ToString();
    }
    
    public override bool Equals(object obj) {
        if (typeof(BaseAccessor).IsInstanceOfType(obj)) {
            obj = ((BaseAccessor)(obj)).Target;
        }
        return this.Target.Equals(obj);
    }
    
    public override int GetHashCode() {
        return this.Target.GetHashCode();
    }
}
[System.Diagnostics.DebuggerStepThrough()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TestTools.UnitTestGeneration", "1.0.0.0")]
internal class WormCodeGenCore_OrmXmlGeneratorAccessor : BaseAccessor {
    
    protected static Microsoft.VisualStudio.TestTools.UnitTesting.PrivateType m_privateType = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateType("Worm.CodeGen.Core", "Worm.CodeGen.Core.OrmXmlGenerator");
    
    internal WormCodeGenCore_OrmXmlGeneratorAccessor(object target) : 
            base(target, m_privateType) {
    }
    
    internal global::System.Xml.XmlDocument _ormXmlDocument {
        get {
            global::System.Xml.XmlDocument ret = ((global::System.Xml.XmlDocument)(m_privateObject.GetField("_ormXmlDocument")));
            return ret;
        }
        set {
            m_privateObject.SetField("_ormXmlDocument", value);
        }
    }

    internal global::Worm.CodeGen.Core.OrmObjectsDef _ormObjectsDef
    {
        get {
            global::Worm.CodeGen.Core.OrmObjectsDef ret = ((global::Worm.CodeGen.Core.OrmObjectsDef)(m_privateObject.GetField("_ormObjectsDef")));
            return ret;
        }
        set {
            m_privateObject.SetField("_ormObjectsDef", value);
        }
    }
    
    internal global::System.Xml.XmlNamespaceManager _nsMgr {
        get {
            global::System.Xml.XmlNamespaceManager ret = ((global::System.Xml.XmlNamespaceManager)(m_privateObject.GetField("_nsMgr")));
            return ret;
        }
        set {
            m_privateObject.SetField("_nsMgr", value);
        }
    }
    
    internal global::System.Xml.XmlNameTable _nametable {
        get {
            global::System.Xml.XmlNameTable ret = ((global::System.Xml.XmlNameTable)(m_privateObject.GetField("_nametable")));
            return ret;
        }
        set {
            m_privateObject.SetField("_nametable", value);
        }
    }
    
    internal static object CreatePrivate(global::Worm.CodeGen.Core.OrmObjectsDef ormObjectsDef) {
        object[] args = new object[] {
                ormObjectsDef};
        Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject priv_obj = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject("Worm.CodeGen.Core", "Worm.CodeGen.Core.OrmXmlGenerator", new System.Type[] {
                    typeof(global::Worm.CodeGen.Core.OrmObjectsDef)}, args);
        return priv_obj.Target;
    }
    
    internal static global::System.Xml.XmlDocument Generate(global::Worm.CodeGen.Core.OrmObjectsDef schema) {
        object[] args = new object[] {
                schema};
        global::System.Xml.XmlDocument ret = ((global::System.Xml.XmlDocument)(m_privateType.InvokeStatic("Generate", new System.Type[] {
                    typeof(global::Worm.CodeGen.Core.OrmObjectsDef)}, args)));
        return ret;
    }
    
    internal void CreateXmlDocument() {
        object[] args = new object[0];
        m_privateObject.Invoke("CreateXmlDocument", new System.Type[0], args);
    }
    
    internal void FillRelations() {
        object[] args = new object[0];
        m_privateObject.Invoke("FillRelations", new System.Type[0], args);
    }
    
    internal void FillEntities() {
        object[] args = new object[0];
        m_privateObject.Invoke("FillEntities", new System.Type[0], args);
    }
    
    internal void FillTypes() {
        object[] args = new object[0];
        m_privateObject.Invoke("FillTypes", new System.Type[0], args);
    }
    
    internal void FillTables() {
        object[] args = new object[0];
        m_privateObject.Invoke("FillTables", new System.Type[0], args);
    }
    
    internal void FillFileDescriptions() {
        object[] args = new object[0];
        m_privateObject.Invoke("FillFileDescriptions", new System.Type[0], args);
    }
}
[System.Diagnostics.DebuggerStepThrough()]
[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.TestTools.UnitTestGeneration", "1.0.0.0")]
internal class Worm_CodeGen_Core_OrmXmlParserAccessor : BaseAccessor {
    
    protected static Microsoft.VisualStudio.TestTools.UnitTesting.PrivateType m_privateType = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateType("Worm.CodeGen.Core", "Worm.CodeGen.Core.OrmXmlParser");
    
    internal Worm_CodeGen_Core_OrmXmlParserAccessor(object target) : 
            base(target, m_privateType) {
    }
    
    internal static string SCHEMA_NAME {
        get {
            string ret = ((string)(m_privateType.GetStaticField("SCHEMA_NAME")));
            return ret;
        }
        set {
            m_privateType.SetStaticField("SCHEMA_NAME", value);
        }
    }
    
    internal global::System.Xml.XmlReader _reader {
        get {
            global::System.Xml.XmlReader ret = ((global::System.Xml.XmlReader)(m_privateObject.GetField("_reader")));
            return ret;
        }
        set {
            m_privateObject.SetField("_reader", value);
        }
    }
    
    internal global::System.Xml.XmlDocument _ormXmlDocument {
        get {
            global::System.Xml.XmlDocument ret = ((global::System.Xml.XmlDocument)(m_privateObject.GetField("_ormXmlDocument")));
            return ret;
        }
        set {
            m_privateObject.SetField("_ormXmlDocument", value);
        }
    }

    internal global::Worm.CodeGen.Core.OrmObjectsDef _ormObjectsDef
    {
        get {
            global::Worm.CodeGen.Core.OrmObjectsDef ret = ((global::Worm.CodeGen.Core.OrmObjectsDef)(m_privateObject.GetField("_ormObjectsDef")));
            return ret;
        }
        set {
            m_privateObject.SetField("_ormObjectsDef", value);
        }
    }
    
    internal global::System.Xml.XmlNamespaceManager _nsMgr {
        get {
            global::System.Xml.XmlNamespaceManager ret = ((global::System.Xml.XmlNamespaceManager)(m_privateObject.GetField("_nsMgr")));
            return ret;
        }
        set {
            m_privateObject.SetField("_nsMgr", value);
        }
    }
    
    internal global::System.Xml.XmlNameTable _nametable {
        get {
            global::System.Xml.XmlNameTable ret = ((global::System.Xml.XmlNameTable)(m_privateObject.GetField("_nametable")));
            return ret;
        }
        set {
            m_privateObject.SetField("_nametable", value);
        }
    }
    
    internal global::System.Xml.XmlResolver _xmlResolver {
        get {
            global::System.Xml.XmlResolver ret = ((global::System.Xml.XmlResolver)(m_privateObject.GetField("_xmlResolver")));
            return ret;
        }
        set {
            m_privateObject.SetField("_xmlResolver", value);
        }
    }
    
    internal global::System.Xml.XmlDocument SourceXmlDocument {
        get {
            global::System.Xml.XmlDocument ret = ((global::System.Xml.XmlDocument)(m_privateObject.GetProperty("SourceXmlDocument")));
            return ret;
        }
        set {
            m_privateObject.SetProperty("SourceXmlDocument", value);
        }
    }

    internal global::Worm.CodeGen.Core.OrmObjectsDef OrmObjectsDef
    {
        get {
            global::Worm.CodeGen.Core.OrmObjectsDef ret = ((global::Worm.CodeGen.Core.OrmObjectsDef)(m_privateObject.GetProperty("OrmObjectsDef")));
            return ret;
        }
    }
    
    internal static object CreatePrivate(global::System.Xml.XmlReader reader) {
        object[] args = new object[] {
                reader};
        Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject priv_obj = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject("Worm.CodeGen.Core", "Worm.CodeGen.Core.OrmXmlParser", new System.Type[] {
                    typeof(global::System.Xml.XmlReader)}, args);
        return priv_obj.Target;
    }
    
    internal static object CreatePrivate(global::System.Xml.XmlReader reader, global::System.Xml.XmlResolver xmlResolver) {
        object[] args = new object[] {
                reader,
                xmlResolver};
        Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject priv_obj = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject("Worm.CodeGen.Core", "Worm.CodeGen.Core.OrmXmlParser", new System.Type[] {
                    typeof(global::System.Xml.XmlReader),
                    typeof(global::System.Xml.XmlResolver)}, args);
        return priv_obj.Target;
    }
    
    internal static object CreatePrivate(global::System.Xml.XmlDocument document) {
        object[] args = new object[] {
                document};
        Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject priv_obj = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject("Worm.CodeGen.Core", "Worm.CodeGen.Core.OrmXmlParser", new System.Type[] {
                    typeof(global::System.Xml.XmlDocument)}, args);
        return priv_obj.Target;
    }
    
    internal static global::Worm.CodeGen.Core.OrmObjectsDef Parse(global::System.Xml.XmlReader reader, global::System.Xml.XmlResolver xmlResolver) {
        object[] args = new object[] {
                reader,
                xmlResolver};
        global::Worm.CodeGen.Core.OrmObjectsDef ret = ((global::Worm.CodeGen.Core.OrmObjectsDef)(m_privateType.InvokeStatic("Parse", new System.Type[] {
                    typeof(global::System.Xml.XmlReader),
                    typeof(global::System.Xml.XmlResolver)}, args)));
        return ret;
    }
    
    internal static global::Worm.CodeGen.Core.OrmObjectsDef LoadXmlDocument(global::System.Xml.XmlDocument document, bool skipValidation) {
        object[] args = new object[] {
                document,
                skipValidation};
        global::Worm.CodeGen.Core.OrmObjectsDef ret = ((global::Worm.CodeGen.Core.OrmObjectsDef)(m_privateType.InvokeStatic("LoadXmlDocument", new System.Type[] {
                    typeof(global::System.Xml.XmlDocument),
                    typeof(bool)}, args)));
        return ret;
    }
    
    internal void FillObjectsDef() {
        object[] args = new object[0];
        m_privateObject.Invoke("FillObjectsDef", new System.Type[0], args);
    }
    
    internal void FillImports() {
        object[] args = new object[0];
        m_privateObject.Invoke("FillImports", new System.Type[0], args);
    }
    
    internal void FillTypes() {
        object[] args = new object[0];
        m_privateObject.Invoke("FillTypes", new System.Type[0], args);
    }
    
    internal void FillEntities() {
        object[] args = new object[0];
        m_privateObject.Invoke("FillEntities", new System.Type[0], args);
    }
    
    internal void FillSuppresedProperties(global::Worm.CodeGen.Core.Descriptors.EntityDescription entity) {
        object[] args = new object[] {
                entity};
        m_privateObject.Invoke("FillSuppresedProperties", new System.Type[] {
                    typeof(global::Worm.CodeGen.Core.Descriptors.EntityDescription)}, args);
    }
    
    internal void FillFileDescriptions() {
        object[] args = new object[0];
        m_privateObject.Invoke("FillFileDescriptions", new System.Type[0], args);
    }
    
    internal void FindEntities() {
        object[] args = new object[0];
        m_privateObject.Invoke("FindEntities", new System.Type[0], args);
    }
    
    internal void FillProperties(global::Worm.CodeGen.Core.Descriptors.EntityDescription entity) {
        object[] args = new object[] {
                entity};
        m_privateObject.Invoke("FillProperties", new System.Type[] {
                    typeof(global::Worm.CodeGen.Core.Descriptors.EntityDescription)}, args);
    }
    
    internal void FillRelations() {
        object[] args = new object[0];
        m_privateObject.Invoke("FillRelations", new System.Type[0], args);
    }
    
    internal void FillTables() {
        object[] args = new object[0];
        m_privateObject.Invoke("FillTables", new System.Type[0], args);
    }
    
    internal void FillEntityTables(global::Worm.CodeGen.Core.Descriptors.EntityDescription entity) {
        object[] args = new object[] {
                entity};
        m_privateObject.Invoke("FillEntityTables", new System.Type[] {
                    typeof(global::Worm.CodeGen.Core.Descriptors.EntityDescription)}, args);
    }
    
    internal void Read() {
        object[] args = new object[0];
        m_privateObject.Invoke("Read", new System.Type[0], args);
    }
    
    internal void xmlReaderSettings_ValidationEventHandler(object sender, global::System.Xml.Schema.ValidationEventArgs e) {
        object[] args = new object[] {
                sender,
                e};
        m_privateObject.Invoke("xmlReaderSettings_ValidationEventHandler", new System.Type[] {
                    typeof(object),
                    typeof(global::System.Xml.Schema.ValidationEventArgs)}, args);
    }
}
}
