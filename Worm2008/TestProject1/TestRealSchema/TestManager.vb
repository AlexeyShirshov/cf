Imports System
Imports System.Text
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System.Diagnostics
Imports Worm.Database
Imports Worm.Cache
Imports Worm.Entities
Imports Worm.Sorting
Imports Worm.Criteria.Values
Imports Worm.Entities.Meta
Imports Worm.Criteria.Core
Imports Worm.Criteria.Conditions
Imports Worm.Criteria
Imports Worm.Criteria.Joins
Imports Worm.Query

<TestClass()> _
Public Class TestManagerRS
    Implements INewObjectsStore, Worm.ICreateManager

    Private _schemas As New Collections.Hashtable
    Public Function GetSchema(ByVal v As String) As Worm.ObjectMappingEngine
        Dim s As Worm.ObjectMappingEngine = CType(_schemas(v), Worm.ObjectMappingEngine)
        If s Is Nothing Then
            s = New Worm.ObjectMappingEngine(v)
            _schemas.Add(v, s)
        End If
        Return s
    End Function

    Private _cache As CacheBase
    Protected Function GetCache() As CacheBase
        If _c Then
            If _cache Is Nothing Then
                _cache = New ReadonlyCache
            End If
            Return _cache
        Else
            Return New ReadonlyCache
        End If
    End Function

    Private _rwcache As OrmCache
    Protected Function GetRWCache() As OrmCache
        If _c Then
            If _rwcache Is Nothing Then
                _rwcache = New OrmCache
            End If
            Return _rwcache
        Else
            Return New OrmCache
        End If
    End Function

    Public Shared Function CreateManagerSharedFullText(ByVal schema As Worm.ObjectMappingEngine) As OrmReadOnlyDBManager
        Return New OrmReadOnlyDBManager(New ReadonlyCache, schema, New SQLGenerator, My.Settings.FullTextEnabledConn)
    End Function

    Public Shared Function CreateManagerShared(ByVal schema As Worm.ObjectMappingEngine) As OrmReadOnlyDBManager
        Return CreateManagerShared(schema, New ReadonlyCache)
    End Function

    Public Shared Function CreateWriteManagerShared(ByVal schema As Worm.ObjectMappingEngine) As OrmReadOnlyDBManager
        Return CreateWriteManagerShared(schema, New OrmCache)
    End Function

    Public Shared Function CreateManagerShared(ByVal schema As Worm.ObjectMappingEngine, ByVal cache As ReadonlyCache) As OrmReadOnlyDBManager
#If UseUserInstance Then
        Dim path As String = IO.Path.GetFullPath(IO.Path.Combine(IO.Directory.GetCurrentDirectory, "..\..\..\TestProject1\Databases\wormtest.mdf"))
        Return New OrmReadOnlyDBManager(cache, schema, New SQLGenerator, "Data Source=.\sqlexpress;AttachDBFileName='" & path & "';User Instance=true;Integrated security=true;")
#Else
        Return New OrmReadOnlyDBManager(cache, schema, New SQLGenerator, "Server=.\sqlexpress;Integrated security=true;Initial catalog=wormtest")
#End If
    End Function

    Public Shared Function CreateWriteManagerShared(ByVal schema As Worm.ObjectMappingEngine, ByVal cache As OrmCache) As OrmDBManager
#If UseUserInstance Then
        Dim path As String = IO.Path.GetFullPath(IO.Path.Combine(IO.Directory.GetCurrentDirectory, "..\..\..\TestProject1\Databases\wormtest.mdf"))
        Return New OrmDBManager(cache, schema, New SQLGenerator, "Data Source=.\sqlexpress;AttachDBFileName='" & path & "';User Instance=true;Integrated security=true;")
#Else
        Return New OrmDBManager(cache, schema, New SQLGenerator, "Server=.\sqlexpress;Integrated security=true;Initial catalog=wormtest")
#End If
    End Function

    Public Function CreateManager() As Worm.OrmManager Implements Worm.ICreateManager.CreateManager
        Return CreateManagerShared(New Worm.ObjectMappingEngine("1"))
    End Function

    Public Function CreateManager(ByVal schema As Worm.ObjectMappingEngine) As OrmReadOnlyDBManager
#If UseUserInstance Then
        Dim path As String = IO.Path.GetFullPath(IO.Path.Combine(IO.Directory.GetCurrentDirectory, "..\..\..\TestProject1\Databases\wormtest.mdf"))
        Dim mgr As New OrmReadOnlyDBManager(GetCache, schema, New SQLGenerator, "Data Source=.\sqlexpress;AttachDBFileName='" & path & "';User Instance=true;Integrated security=true;")
#Else
        Dim mgr As New OrmReadOnlyDBManager(getCache, schema, New SQLGenerator, "Server=.\sqlexpress;Integrated security=true;Initial catalog=wormtest")
#End If
        mgr.Cache.NewObjectManager = Me
        Return mgr
    End Function

    Public Function CreateWriteManager(ByVal schema As Worm.ObjectMappingEngine) As OrmDBManager
#If UseUserInstance Then
        Dim path As String = IO.Path.GetFullPath(IO.Path.Combine(IO.Directory.GetCurrentDirectory, "..\..\..\TestProject1\Databases\wormtest.mdf"))
        Dim mgr As New OrmDBManager(GetRWCache, schema, New SQLGenerator, "Data Source=.\sqlexpress;AttachDBFileName='" & path & "';User Instance=true;Integrated security=true;")
#Else
        Dim mgr As New OrmDBManager(getrwCache, schema, New SQLGenerator, "Server=.\sqlexpress;Integrated security=true;Initial catalog=wormtest")
#End If
        mgr.Cache.NewObjectManager = Me
        Return mgr
    End Function

    Private _l As Boolean
    Public Property WithLoad() As Boolean
        Get
            Return _l
        End Get
        Set(ByVal value As Boolean)
            _l = value
        End Set
    End Property

    Private _c As Boolean
    Public Property SharedCache() As Boolean
        Get
            Return _c
        End Get
        Set(ByVal value As Boolean)
            _c = value
        End Set
    End Property

    <TestMethod()> _
    Public Sub TestSave()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim t2 As Table2 = mgr.Find(Of Table2)(1)

            t2.Tbl = mgr.Find(Of Table1)(2)

            mgr.BeginTransaction()
            Try
                t2.SaveChanges(True)

            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod(), ExpectedException(GetType(Worm.OrmManagerException))> _
    Public Sub TestSaveConcurrency()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim t As Table3 = mgr.Find(Of Table3)(1)
            Assert.IsNotNull(t)
            t.Code = t.Code + CByte(10)

            Assert.IsTrue(t.InternalProperties.IsLoaded)
            Assert.IsTrue(t.InternalProperties.IsPropertyLoaded("Ref"))

            Dim t2 As Table3 = Nothing
            Dim prev As Byte = 0
            Try
                Using mgr2 As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
                    t2 = mgr2.Find(Of Table3)(1)
                    prev = t2.Code
                    t2.Code = t.Code + CByte(10)
                    mgr2.SaveChanges(t2, True)
                End Using

                mgr.SaveChanges(t, True)
            Finally
                Using mgr2 As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
                    t2.Code = prev
                    mgr2.SaveChanges(t2, True)
                End Using
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestValidateCache()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim t2 As Table2 = mgr.Find(Of Table2)(1)
            Dim tt As IList(Of Table2) = CType(mgr.Find(Of Table2)(New Ctor(GetType(Table2)).prop("Table1").eq(New Table1(1, mgr.Cache, mgr.MappingEngine)), Nothing, WithLoad), Global.System.Collections.Generic.IList(Of Global.TestProject1.Table2))
            Assert.AreEqual(2, tt.Count)

            t2.Tbl = mgr.Find(Of Table1)(2)

            mgr.BeginTransaction()
            Try
                t2.SaveChanges(True)

                tt = CType(mgr.Find(Of Table2)(New Ctor(GetType(Table2)).prop("Table1").eq(New Table1(1, mgr.Cache, mgr.MappingEngine)), Nothing, WithLoad), Global.System.Collections.Generic.IList(Of Global.TestProject1.Table2))
                Assert.AreEqual(1, tt.Count)

            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestValidateCache2()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            'Dim t1 As Table1 = New Table1(1, mgr.Cache, mgr.ObjectSchema)
            Dim t1 As Table1 = mgr.GetOrmBaseFromCacheOrCreate(Of Table1)(1)

            Dim tt As IList(Of Table2) = CType(mgr.Find(Of Table2)(New Ctor(GetType(Table2)).prop("Table1").eq(t1), Nothing, WithLoad), Global.System.Collections.Generic.IList(Of Global.TestProject1.Table2))
            Assert.AreEqual(2, tt.Count)

            Dim t2 As New Table2(-100, mgr.Cache, mgr.MappingEngine)
            t2.Tbl = t1

            mgr.BeginTransaction()
            Try
                t2.SaveChanges(True)

                tt = CType(mgr.Find(Of Table2)(New Ctor(GetType(Table2)).prop("Table1").eq(t1), Nothing, WithLoad), Global.System.Collections.Generic.IList(Of Global.TestProject1.Table2))
                Assert.AreEqual(3, tt.Count)

            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestValidateCache3()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim t1 As Table1 = New Table1(1, mgr.Cache, mgr.MappingEngine)
            Dim tt As IList(Of Table2) = CType(mgr.Find(Of Table2)(New Ctor(GetType(Table2)).prop("Table1").eq(t1), Nothing, WithLoad), Global.System.Collections.Generic.IList(Of Global.TestProject1.Table2))
            Assert.AreEqual(2, tt.Count)

            Dim t2 As New Table2(-100, mgr.Cache, mgr.MappingEngine)
            t2.Tbl = New Table1(2, mgr.Cache, mgr.MappingEngine)

            mgr.BeginTransaction()
            Try
                t2.SaveChanges(True)

                tt = CType(mgr.Find(Of Table2)(New Ctor(GetType(Table2)).prop("Table1").eq(t1), Nothing, WithLoad), Global.System.Collections.Generic.IList(Of Global.TestProject1.Table2))
                Assert.AreEqual(2, tt.Count)

            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestFindField()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim c As ICollection(Of Table1) = mgr.Find(Of Table1)(New Ctor(GetType(Table1)).prop("Code").eq(2), Nothing, WithLoad)

            Assert.AreEqual(1, c.Count)
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestXmlField()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim t As Table3 = mgr.Find(Of Table3)(2)

            Assert.AreEqual("root", t.Xml.DocumentElement.Name)
            Dim attr As System.Xml.XmlAttribute = t.Xml.CreateAttribute("first")
            attr.Value = "hi!"
            t.Xml.DocumentElement.Attributes.Append(attr)

            t.SaveChanges(True)
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestAddBlob()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim t2 As New Table2(-100, mgr.Cache, mgr.MappingEngine)

            mgr.BeginTransaction()

            Try
                t2.SaveChanges(True)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestLoadGUID()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim t As Table4 = mgr.Find(Of Table4)(1)

            Assert.AreEqual(False, t.Col)

            Assert.AreEqual(New Guid("7c78c40a-fd96-44fe-861f-0f87b8d04bd5"), t.GUID)

            Dim cc As ICollection(Of Table4) = mgr.Find(Of Table4)(New Ctor(GetType(Table4)).prop("Col").eq(False), Nothing, True)

            Assert.AreEqual(1, cc.Count)
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestAddWithPK()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim t As New Table4(4, mgr.Cache, mgr.MappingEngine)
            Dim g As Guid = t.GUID
            mgr.BeginTransaction()

            Try
                t.SaveChanges(True)

                Assert.AreNotEqual(g, t.GUID)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestAddWithPK2()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("2"))
            Dim t As New Table4(4, mgr.Cache, mgr.MappingEngine)


            mgr.BeginTransaction()

            Try
                t.SaveChanges(True)

            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestSwitchCache()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Using New Worm.OrmManager.CacheListBehavior(mgr, False)
                Dim c2 As ICollection(Of Table1) = mgr.Find(Of Table1)(New Ctor(GetType(Table1)).prop("Code").eq(2), Nothing, WithLoad)

                Assert.AreEqual(1, c2.Count)
            End Using

            Dim c As ICollection(Of Table1) = mgr.Find(Of Table1)(New Ctor(GetType(Table1)).prop("Code").eq(2), Nothing, WithLoad)

            Assert.AreEqual(1, c.Count)

            c = mgr.Find(Of Table1)(New Ctor(GetType(Table1)).prop("Code").eq(2), Nothing, WithLoad)

            Assert.AreEqual(1, c.Count)
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestSwitchCache2()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim n As Date = Now
            For i As Integer = 0 To 100000
                Dim need As Boolean = Now.Subtract(n).TotalSeconds < 1
                If Not need Then
                    n = Now
                End If

                Using New Worm.OrmManager.CacheListBehavior(mgr, need)
                    Dim c2 As ICollection(Of Table1) = mgr.Find(Of Table1)(New Ctor(GetType(Table1)).prop("Code").eq(2), Nothing, WithLoad)

                    Assert.AreEqual(1, c2.Count)
                End Using
            Next

        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestDeleteFromCache()
        Dim schema As Worm.ObjectMappingEngine = GetSchema("1")

        Using mgr As OrmReadOnlyDBManager = CreateManager(schema)

            Dim t1 As Table1 = mgr.Find(Of Table1)(1)

            Dim t3 As Worm.ReadOnlyList(Of Table2) = mgr.Find(Of Table2)(New Ctor(GetType(Table2)).prop("Table1").eq(t1), Nothing, WithLoad)
            mgr.LoadObjects(t3)
            Assert.AreEqual(2, t3.Count)

            For Each t2 As Table2 In t3
                Assert.IsTrue(t2.InternalProperties.IsLoaded)
            Next

            mgr.BeginTransaction()
            Try

                mgr.RemoveObjectFromCache(t1)

                t3 = mgr.Find(Of Table2)(New Ctor(GetType(Table2)).prop("Table1").eq(t1), Nothing, WithLoad)

                Assert.AreEqual(2, t3.Count)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestComplexM2M()
        Dim schema As Worm.ObjectMappingEngine = GetSchema("1")

        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(schema)
            Dim t1 As Table1 = mgr.Find(Of Table1)(1)
            Dim t3 As Table33 = mgr.Find(Of Table33)(1)
            Dim c As ICollection(Of Table33) = t1.M2M.Find(Of Table33)(Nothing, Nothing, WithLoad)

            Assert.AreEqual(3, c.Count)

            Dim c2 As ICollection(Of Table1) = t3.M2M.Find(Of Table1)(Nothing, SCtor.prop(GetType(Table1), "Enum").asc, WithLoad)

            Assert.AreEqual(1, c2.Count)

            Dim r1 As New Tables1to3(-100, mgr.Cache, mgr.MappingEngine)
            r1.Title = "913nv"
            r1.Table1 = mgr.Find(Of Table1)(2)
            r1.Table3 = t3
            mgr.BeginTransaction()
            Try
                r1.SaveChanges(True)

                c = t1.M2M.Find(Of Table33)(Nothing, Nothing, WithLoad)

                Assert.AreEqual(3, c.Count)

                c2 = t3.M2M.Find(Of Table1)(Nothing, SCtor.prop(GetType(Table1), "Enum").asc, WithLoad)

                Assert.AreEqual(2, c2.Count)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestComplexM2M2()
        Dim schema As Worm.ObjectMappingEngine = GetSchema("1")

        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(schema)
            Dim t1 As Table1 = mgr.Find(Of Table1)(1)
            Dim t3 As Table33 = mgr.Find(Of Table33)(1)
            Dim c As ICollection(Of Table33) = t1.M2M.Find(Of Table33)(Nothing, Nothing, WithLoad)

            Assert.AreEqual(3, c.Count)

            Dim c2 As ICollection(Of Table1) = t3.M2M.Find(Of Table1)(Nothing, Nothing, WithLoad)

            Assert.AreEqual(1, c2.Count)

            Dim r1 As Tables1to3 = mgr.Find(Of Tables1to3)(1)
            r1.Delete()
            mgr.BeginTransaction()
            Try
                r1.SaveChanges(True)

                c = t1.M2M.Find(Of Table33)(Nothing, Nothing, WithLoad)

                Assert.AreEqual(2, c.Count)

                c2 = t3.M2M.Find(Of Table1)(Nothing, Nothing, WithLoad)

                Assert.AreEqual(0, c2.Count)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestComplexM2M3()
        Dim schema As Worm.ObjectMappingEngine = GetSchema("1")

        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(schema)
            Dim t1 As Table1 = mgr.Find(Of Table1)(1)
            Dim t3 As Table33 = mgr.Find(Of Table33)(2)
            Dim t As Type = schema.GetTypeByEntityName("Table3")
            Dim f As PredicateLink = CType(New Ctor(t).prop("Code").eq(2), PredicateLink)
            Dim c As ICollection(Of Table33) = t1.M2M.Find(Of Table33)(f, _
                Nothing, WithLoad)

            Assert.AreEqual(2, c.Count)

            Dim r1 As New Tables1to3(-100, mgr.Cache, mgr.MappingEngine)
            r1.Title = "913nv"
            r1.Table1 = t1
            r1.Table3 = t3
            mgr.BeginTransaction()
            Try
                r1.SaveChanges(True)

                Dim c2 As ICollection(Of Table33) = t1.M2M.Find(Of Table33)(f, Nothing, WithLoad)

                Assert.AreEqual(3, c2.Count)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod(), ExpectedException(GetType(InvalidOperationException))> _
    Public Sub TestComplexM2M4()
        Dim schema As Worm.ObjectMappingEngine = GetSchema("1")

        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(schema)
            Dim t1 As Table1 = mgr.Find(Of Table1)(1)
            Dim t3 As Table33 = mgr.Find(Of Table33)(1)
            Dim c As ICollection(Of Table33) = t1.M2M.Find(Of Table33)(Nothing, Nothing, WithLoad)

            Assert.AreEqual(3, c.Count)

            t1.M2M.Add(t3)

            mgr.BeginTransaction()
            Try
                t1.SaveChanges(True)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestComplexM2M5()
        Dim schema As Worm.ObjectMappingEngine = GetSchema("1")

        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(schema)
            Dim t1 As Table1 = mgr.Find(Of Table1)(1)
            Dim t3 As Table33 = mgr.Find(Of Table33)(1)
            Dim c As ICollection(Of Table33) = t1.M2M.Find(Of Table33)(Nothing, Nothing, True)

            Assert.AreEqual(3, c.Count)

            For Each o As Table33 In c
                Assert.IsTrue(o.InternalProperties.IsLoaded)
            Next

            Dim r1 As New Tables1to3(-100, mgr.Cache, mgr.MappingEngine)
            r1.Title = "913nv"
            r1.Table1 = t1
            r1.Table3 = t3
            mgr.BeginTransaction()
            Try
                r1.SaveChanges(False)

                Assert.AreNotEqual(-100, r1.Identifier)

                Dim c2 As ICollection(Of Table33) = t1.M2M.Find(Of Table33)(Nothing, Nothing, WithLoad)
                Assert.AreEqual(4, c2.Count)

                r1.RejectChanges()

                c2 = t1.M2M.Find(Of Table33)(Nothing, Nothing, WithLoad)
                Assert.AreEqual(3, c2.Count)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestLoadObjects()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim tt1 As Table1 = mgr.Find(Of Table1)(1)
            Dim tt2 As Table1 = mgr.Find(Of Table1)(2)

            Dim t1s As ICollection(Of Table1) = mgr.ConvertIds2Objects(Of Table1)(New Object() {1, 2}, False)
            Dim t10s As ICollection(Of Table10) = mgr.LoadObjects(Of Table10)("Table1", Nothing, CType(t1s, Collections.ICollection))

            Assert.AreEqual(3, t10s.Count)

            Dim t1 As ICollection(Of Table10) = mgr.Find(Of Table10)(New Ctor(GetType(Table10)).prop("Table1").eq(tt1), Nothing, WithLoad)
            Assert.AreEqual(2, t1.Count)

            Dim t2 As ICollection(Of Table10) = mgr.Find(Of Table10)(New Ctor(GetType(Table10)).prop("Table1").eq(tt2), Nothing, WithLoad)
            Assert.AreEqual(1, t2.Count)

            t10s = mgr.LoadObjects(Of Table10)("Table1", Nothing, CType(t1s, Collections.ICollection))
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestLoadObjects2()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim tt1 As Table1 = mgr.Find(Of Table1)(1)
            Dim tt2 As Table1 = mgr.Find(Of Table1)(2)

            Dim t1 As ICollection(Of Table10) = mgr.Find(Of Table10)(New Ctor(GetType(Table10)).prop("Table1").eq(tt1), Nothing, WithLoad)
            Assert.AreEqual(2, t1.Count)

            Dim t1s As ICollection(Of Table1) = mgr.ConvertIds2Objects(Of Table1)(New Object() {1, 2}, False)
            Dim t10s As ICollection(Of Table10) = mgr.LoadObjects(Of Table10)("Table1", Nothing, CType(t1s, Collections.ICollection))
            Assert.AreEqual(3, t10s.Count)

            Dim t2 As ICollection(Of Table10) = mgr.Find(Of Table10)(New Ctor(GetType(Table10)).prop("Table1").eq(tt2), Nothing, WithLoad)
            Assert.AreEqual(1, t2.Count)

        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestLoadObjects3()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim tt1 As Table1 = mgr.Find(Of Table1)(1)
            Dim tt2 As Table1 = mgr.Find(Of Table1)(2)

            Dim t1s As ICollection(Of Table1) = mgr.ConvertIds2Objects(Of Table1)(New Object() {1, 2}, False)
            Dim t10s As ICollection(Of Table10) = mgr.LoadObjects(Of Table10)("Table1", New Ctor(GetType(Table10)).prop("ID").eq(1), CType(t1s, Collections.ICollection))
            Assert.AreEqual(1, t10s.Count)

        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestLoadObjects4()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim tt1 As Table1 = mgr.CreateOrmBase(Of Table1)(1)
            Dim tt2 As Table1 = mgr.CreateOrmBase(Of Table1)(1)

            mgr.LoadObjects(New Worm.ReadOnlyList(Of Table1)(New List(Of Table1)(New Table1() {tt1, tt2})))
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestLoadObjects5()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim tt1 As Table2 = mgr.CreateOrmBase(Of Table2)(1)

            Dim t As ICollection(Of Table2) = mgr.LoadObjects(Of Table2)( _
                New Worm.ReadOnlyList(Of Table2)(New List(Of Table2)(New Table2() {tt1})), New String() {"Table1"}, 0, 1)

            Assert.AreEqual(1, t.Count)

            Assert.AreEqual(1, CType(t, IList(Of Table2))(0).Tbl.Identifier)
            Assert.IsTrue(CType(t, IList(Of Table2))(0).Tbl.InternalProperties.IsLoaded)
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestLoadObjectsM2M()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim t1s As ICollection(Of Table1) = mgr.ConvertIds2Objects(Of Table1)(New Object() {1, 2}, False)

            mgr.LoadObjects(Of Table33)(mgr.MappingEngine.GetM2MRelation(GetType(Table1), GetType(Table33), True), Nothing, CType(t1s, Collections.ICollection), Nothing)

            Dim tt1 As Table1 = mgr.Find(Of Table1)(1)

            tt1.M2M.Find(Of Table33)(Nothing, Nothing, False)
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestM2MFilterValidation()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim tt1 As Table1 = mgr.Find(Of Table1)(1)
            Dim t As Type = mgr.MappingEngine.GetTypeByEntityName("Table3")
            'Dim con As New Orm.OrmCondition.OrmConditionConstructor
            'con.AddFilter(New Orm.OrmFilter(t, "Code", New TypeWrap(Of Object)(2), Orm.FilterOperation.Equal))
            Dim c As ICollection(Of Table33) = tt1.M2M.Find(Of Table33)(New Ctor(t).prop("Code").eq(2), Nothing, WithLoad)

            Assert.AreEqual(2, c.Count)
            mgr.BeginTransaction()
            Try
                Dim tt2 As Table33 = mgr.Find(Of Table33)(1)
                Assert.AreEqual(Of Byte)(1, tt2.Code)
                tt2.Code = 2
                tt2.SaveChanges(True)

                c = tt1.M2M.Find(Of Table33)(New Ctor(t).prop("Code").eq(2), Nothing, WithLoad)

                Assert.AreEqual(3, c.Count)
            Finally
                mgr.Rollback()

            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestM2MSorting()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim tt1 As Table1 = mgr.Find(Of Table1)(1)
            Dim t As Type = mgr.MappingEngine.GetTypeByEntityName("Table3")
            'Dim con As New Orm.OrmCondition.OrmConditionConstructor
            'con.AddFilter(New Orm.OrmFilter(t, "Code", New TypeWrap(Of Object)(2), Orm.FilterOperation.Equal))
            Dim s As Sort = SCtor.prop(GetType(Table33), "Code").desc
            Dim c As Worm.ReadOnlyList(Of Table33) = tt1.M2M.Find(Of Table33)(Nothing, s, WithLoad)
            Assert.AreEqual(3, c.Count)
            'Assert.AreEqual(Of Byte)(2, c(0).Code)
            'Assert.AreEqual(Of Byte)(1, c(1).Code)

            mgr.BeginTransaction()
            Try
                Using st As New ModificationsTracker(mgr)
                    Dim tt2 As Table33 = New Table33(-100, mgr.Cache, mgr.MappingEngine)
                    st.Add(tt2)
                    tt2.RefObject = tt1
                    tt2.Code = 3

                    Dim t3 As New Tables1to3(-101, mgr.Cache, mgr.MappingEngine)
                    st.Add(t3)

                    t3.Table1 = tt1
                    t3.Table3 = tt2
                    t3.Title = "sdfpsdfm"
                    st.AcceptModifications()
                End Using

                c = tt1.M2M.Find(Of Table33)(Nothing, s, WithLoad)
                Assert.AreEqual(4, c.Count)
                Assert.AreEqual(Of Byte)(3, c(0).Code)
                Assert.AreEqual(Of Byte)(2, c(1).Code)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestFuncs()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("2"))
            Dim t1 As Table1 = mgr.Find(Of Table1)(1)
            Assert.IsNotNull(t1)
        End Using

        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("3"))
            Dim t1 As Table1 = mgr.Find(Of Table1)(2)
            Assert.IsNotNull(t1)

            t1 = mgr.Find(Of Table1)(1)
            Assert.IsNull(t1)
        End Using
    End Sub

    <TestMethod(), ExpectedException(GetType(Data.SqlClient.SqlException))> _
    Public Sub TestMultipleDelete()
        Using mgr As OrmDBManager = CreateWriteManager(GetSchema("1"))
            Dim f As New EntityFilter(GetType(Table3), "Code", New ScalarValue(1), Worm.Criteria.FilterOperation.LessEqualThan)
            mgr.BeginTransaction()

            Try
                Dim i As Integer = mgr.Delete(f)
                Assert.AreEqual(1, i)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestDeleteNotLoaded()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim t As Table1 = mgr.GetOrmBaseFromCacheOrCreate(Of Table1)(1)
            Assert.AreEqual(ObjectState.NotLoaded, t.InternalProperties.ObjectState)
            t.Delete()
            Assert.AreEqual(ObjectState.Deleted, t.InternalProperties.ObjectState)
        End Using
    End Sub

    <TestMethod(), ExpectedException(GetType(Data.SqlClient.SqlException))> _
    Public Sub TestSimpleObjects()
        Using mgr As OrmDBManager = CreateWriteManager(GetSchema("1"))
            Dim s1 As SimpleObj = mgr.Find(Of SimpleObj)(1)

            Assert.AreEqual("first", s1.Title)

            mgr.BeginTransaction()
            Try
                s1.Delete()
                s1.SaveChanges(True)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestSimpleObjects2()
        Using mgr As OrmDBManager = CreateWriteManager(GetSchema("1"))
            Dim s1 As SimpleObj2 = mgr.Find(Of SimpleObj2)(2)

            Assert.AreEqual("second", s1.Title)

            mgr.BeginTransaction()
            Try
                s1 = New SimpleObj2
                s1.Title = "555"
                s1.SaveChanges(True)
            Finally
                Assert.IsTrue(s1.ID > 0)
                Assert.AreEqual("555", s1.Title)
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestPager()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim cc As ICollection(Of Table1) = mgr.FindTop(Of Table1)(10, Nothing, Nothing, True)
            Assert.AreEqual(3, cc.Count)

            Using New Worm.OrmManager.PagerSwitcher(mgr, 0, 1)
                cc = mgr.FindTop(Of Table1)(10, Nothing, Nothing, True)
                Assert.AreEqual(1, cc.Count)
            End Using
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestExecResults()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim cc As ICollection(Of Table1) = mgr.FindTop(Of Table1)(10, Nothing, Nothing, True)

            Assert.AreEqual(3, mgr.GetLastExecitionResult.RowCount)
            Assert.IsFalse(mgr.GetLastExecitionResult.CacheHit)

            System.Diagnostics.Trace.WriteLine(mgr.GetLastExecitionResult.ExecutionTime.ToString)
            System.Diagnostics.Trace.WriteLine(mgr.GetLastExecitionResult.FetchTime.ToString)

            Dim t As Table1 = mgr.Find(Of Table1)(1)
            t.Load()

            Assert.AreEqual(1, mgr.Cache.GetLoadTime(GetType(Table1)).First)

            System.Diagnostics.Trace.WriteLine(mgr.Cache.GetLoadTime(GetType(Table1)).Second.ToString)
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestCompositeDelete()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim e As Composite = mgr.Find(Of Composite)(1)
            Assert.AreEqual(1, e.ID)
            Assert.AreEqual("������", e.Message)
            Assert.AreEqual("hi", e.Message2)

            e.Delete()
            mgr.BeginTransaction()
            Try
                e.SaveChanges(True)

                Assert.IsFalse(mgr.IsInCachePrecise(e))

                e = mgr.Find(Of Composite)(1)

                Assert.IsNull(e)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestCompositeUpdate()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim e As Composite = mgr.Find(Of Composite)(1)
            Assert.AreEqual(1, e.ID)
            Assert.AreEqual("������", e.Message)
            Assert.AreEqual("hi", e.Message2)

            e.Message2 = "adfgopmi"

            mgr.BeginTransaction()
            Try
                e.SaveChanges(True)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestCompositeInsert()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim e As New Composite(1, mgr.Cache, mgr.MappingEngine)
            e.Message = "don"
            e.Message2 = "dionsd"
            mgr.BeginTransaction()
            Try
                e.SaveChanges(True)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestEntityM2M()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim t As Tables1to1 = mgr.Find(Of Tables1to1)(1)
            Dim t1 As Table1 = mgr.Find(Of Table1)(1)
            Dim t1back As Table1 = mgr.Find(Of Table1)(2)

            Assert.AreEqual(1, t.Identifier)

            Assert.AreEqual(t1, t.Table1)
            Assert.AreEqual(t1back, t.Table1Back)

            Assert.AreEqual(1, t1.M2M.Find(Of Table1)(Nothing, Nothing, True, False).Count)

            Assert.AreEqual(2, t1.M2M.Find(Of Table1)(Nothing, Nothing, False, False).Count)
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestFilter()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim c As ICollection(Of Table2) = mgr.Find(Of Table2)(Ctor.prop(GetType(Table2), "Money").eq(1), Nothing, False)
            Dim c2 As ICollection(Of Table2) = mgr.Find(Of Table2)(Ctor.prop(GetType(Table2), "Money").eq(2), Nothing, False)

            Assert.AreEqual(1, c.Count)
            Assert.AreEqual(1, c2.Count)

            mgr.BeginTransaction()
            Try
                Dim t As Table2 = mgr.Find(Of Table2)(1)
                Assert.AreEqual(1D, t.Money)

                t.Money = 2
                t.SaveChanges(True)

                c = mgr.Find(Of Table2)(Ctor.prop(GetType(Table2), "Money").eq(1), Nothing, False)
                c2 = mgr.Find(Of Table2)(Ctor.prop(GetType(Table2), "Money").eq(2), Nothing, False)

                Assert.AreEqual(0, c.Count)
                Assert.AreEqual(2, c2.Count)

            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestSort()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim tt() As Table10 = New Table10() {mgr.Find(Of Table10)(2), mgr.Find(Of Table10)(1), mgr.Find(Of Table10)(3)}
            Dim c As ICollection(Of Table10) = Worm.OrmManager.ApplySort(tt, SCtor.prop(GetType(Table10), "Table1"))
            Assert.AreEqual(1, GetList(Of Table10)(c)(0).Identifier)
            Assert.AreEqual(2, GetList(Of Table10)(c)(1).Identifier)
            Assert.AreEqual(3, GetList(Of Table10)(c)(2).Identifier)

            c = Worm.OrmManager.ApplySort(tt, SCtor.prop(GetType(Table10), "Table1").desc)
            Assert.AreEqual(3, GetList(Of Table10)(c)(0).Identifier)
            'Assert.AreEqual(2, GetList(Of Table10)(c)(1).Identifier)
            'Assert.AreEqual(1, GetList(Of Table10)(c)(2).Identifier)

            c = Worm.OrmManager.ApplySort(tt, SCtor.prop(GetType(Table10), "Table1").next_prop("ID"))
            Assert.AreEqual(1, GetList(Of Table10)(c)(0).Identifier)
            Assert.AreEqual(2, GetList(Of Table10)(c)(1).Identifier)
            Assert.AreEqual(3, GetList(Of Table10)(c)(2).Identifier)

            c = Worm.OrmManager.ApplySort(tt, SCtor.prop(GetType(Table10), "Table1").next_prop("ID").desc)
            Assert.AreEqual(2, GetList(Of Table10)(c)(0).Identifier)
            Assert.AreEqual(1, GetList(Of Table10)(c)(1).Identifier)
            Assert.AreEqual(3, GetList(Of Table10)(c)(2).Identifier)
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestSortEx()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim tt() As Table10 = New Table10() {mgr.Find(Of Table10)(2), mgr.Find(Of Table10)(1), mgr.Find(Of Table10)(3)}
            Dim c As ICollection(Of Table10) = Worm.OrmManager.ApplySort(tt, SCtor.prop(GetType(Table1), "Title"))
            Assert.AreEqual(1, GetList(Of Table10)(c)(0).Identifier)
            Assert.AreEqual(2, GetList(Of Table10)(c)(1).Identifier)
            Assert.AreEqual(3, GetList(Of Table10)(c)(2).Identifier)

            Dim c2 As System.Collections.ICollection = Worm.OrmManager.ApplySortT(tt, SCtor.prop(GetType(Table1), "Title"))
            Dim l2 As System.Collections.IList = CType(c2, Collections.IList)
            Assert.AreEqual(1, CType(l2(0), Table10).Identifier)
            Assert.AreEqual(2, CType(l2(1), Table10).Identifier)
            Assert.AreEqual(3, CType(l2(2), Table10).Identifier)
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestIsNull()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim t As Worm.ReadOnlyList(Of Table3) = mgr.Find(Of Table3)(Ctor.prop(GetType(Table3), "XML").is_null, Nothing, False)

            Assert.AreEqual(1, t.Count)

            Dim t2 As ICollection(Of Table2) = mgr.Find(Of Table2)(Ctor.prop(GetType(Table2), "Table1").is_null, Nothing, False)

            Assert.AreEqual(0, t2.Count)

            Dim r As Boolean
            Dim f As Worm.Criteria.Core.IFilter = Ctor.prop(GetType(Table3), "XML").is_null.Filter()
            Assert.AreEqual(1, mgr.ApplyFilter(t, f, r).Count)

        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestIn()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim t As Worm.ReadOnlyObjectList(Of Table1) = mgr.Find(Of Table1)(Ctor.prop(GetType(Table1), "EnumStr").[in]( _
                New String() {"first", "sec"}), Nothing, False)

            Assert.AreEqual(3, t.Count)

            Dim r As Boolean
            t = mgr.ApplyFilter(t, CType(New Ctor(GetType(Table1)).prop("Code").[in]( _
                New Integer() {45, 8923}).Filter, Worm.Criteria.Core.IEntityFilter), r)

            Assert.AreEqual(2, t.Count)
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestNotIn()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim t As ICollection(Of Table1) = mgr.Find(Of Table1)(Ctor.prop(GetType(Table1), "EnumStr").not_in( _
                New String() {"sec"}), Nothing, False)

            Assert.AreEqual(1, t.Count)

            t = mgr.Find(Of Table1)(Ctor.prop(GetType(Table1), "EnumStr").not_in( _
                New String() {}), Nothing, False)

            Assert.AreNotEqual(1, t.Count)
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestInSubQuery()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim t As ICollection(Of Table2) = mgr.Find(Of Table2)(Ctor.prop(GetType(Table2), "Table1").[in]( _
                GetType(Table1)), Nothing, False)

            Assert.AreEqual(2, t.Count)

            t = mgr.Find(Of Table2)(Ctor.prop(GetType(Table2), "Money").[in]( _
                GetType(Table1), "Code"), Nothing, False)

            Assert.AreEqual(1, t.Count)
            Assert.AreEqual(2D, GetList(t)(0).Money)

            t = mgr.Find(Of Table2)(Ctor.prop(GetType(Table2), "Money").[in]( _
                GetType(Table1), "Code"), Nothing, False)

            Assert.AreEqual(1, t.Count)

            mgr.BeginTransaction()
            Try
                Dim t2 As New Table2(1934, mgr.Cache, mgr.MappingEngine)
                t2.Tbl = mgr.Find(Of Table1)(1)
                t2.Money = 2
                t2.SaveChanges(True)

                t = mgr.Find(Of Table2)(Ctor.prop(GetType(Table2), "Money").[in]( _
                    GetType(Table1), "Code"), Nothing, False)

                Assert.AreEqual(2, t.Count)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestExistSubQuery()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim tt2 As Type = GetType(Table2)

            Dim t As ICollection(Of Table2) = mgr.Find(Of Table2)(New Ctor(tt2).prop("Table1").exists( _
                GetType(Table1)), Nothing, False)

            Assert.AreEqual(2, t.Count)

            t = mgr.Find(Of Table2)(New Ctor(tt2).prop("Table1").not_exists( _
                GetType(Table1)), Nothing, False)

            Assert.AreEqual(0, t.Count)

            Dim c As New Condition.ConditionConstructor
            c.AddFilter(New JoinFilter(tt2, "Table1", GetType(Table1), "ID", Worm.Criteria.FilterOperation.Equal))
            c.AddFilter(New EntityFilter(GetType(Table1), "Code", New ScalarValue(45), Worm.Criteria.FilterOperation.Equal))
            Dim f As Worm.Criteria.Core.IFilter = CType(c.Condition, Worm.Criteria.Core.IFilter)

            t = mgr.Find(Of Table2)(Ctor.not_exists( _
                GetType(Table1), f), Nothing, False)

            Assert.AreEqual(2, t.Count)

            t = mgr.Find(Of Table2)(Ctor.not_exists( _
                GetType(Table1), f), Nothing, False)

            Assert.AreEqual(2, t.Count)

            mgr.BeginTransaction()
            Try
                Dim t2 As New Table2(1934, mgr.Cache, mgr.MappingEngine)
                t2.Tbl = mgr.Find(Of Table1)(1)
                t2.SaveChanges(True)

                t = mgr.Find(Of Table2)(Ctor.prop(GetType(Table2), "Table1").not_exists( _
                    GetType(Table1), f), Nothing, False)

                Assert.AreEqual(3, t.Count)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestBetween()
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim c As ICollection(Of Table1) = mgr.Find(Of Table1)(Ctor.prop(GetType(Table1), "Code").between(2, 45), SCtor.prop(GetType(Table1), "Code"), False)

            Assert.AreEqual(2, c.Count)

            Assert.AreEqual(2, GetList(c)(0).Code)
            Assert.AreEqual(45, GetList(c)(1).Code)

            mgr.BeginTransaction()
            Try
                GetList(c)(0).Code = 100
                GetList(c)(0).SaveChanges(True)

                c = mgr.Find(Of Table1)(Ctor.prop(GetType(Table1), "Code").between(2, 45), SCtor.prop(GetType(Table1), "Code"), False)

                Assert.AreEqual(1, c.Count)

                Dim t As New Table1(GetIdentity, mgr.Cache, mgr.MappingEngine)
                t.Code = 30
                t.CreatedAt = Now
                t.SaveChanges(True)

                c = mgr.Find(Of Table1)(Ctor.prop(GetType(Table1), "Code").between(2, 45), SCtor.prop(GetType(Table1), "Code"), False)

                Assert.AreEqual(2, c.Count)
                Assert.AreEqual(30, GetList(c)(0).Code)
                Assert.AreEqual(45, GetList(c)(1).Code)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestCustomFilter()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim c As ICollection(Of Table1) = mgr.Find(Of Table1)( _
                Ctor.custom("power({0},2)", New FieldReference(GetType(Table1), "Code")).greater_than(1000), _
                SCtor.prop(GetType(Table1), "Code"), False)

            Assert.AreEqual(2, c.Count)

            Assert.AreEqual(3, GetList(c)(0).Identifier)
            Assert.AreEqual(2, GetList(c)(1).Identifier)

            c = mgr.Find(Of Table1)(CType(Ctor.prop(GetType(Table1), "Enum").eq(2), PredicateLink). _
                [and]("power({0},2)", New FieldReference(GetType(Table1), "Code")).greater_than(1000), Nothing, True)

            Assert.AreEqual(1, c.Count)
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestSaveNew()
        'OrmReadOnlyDBManager.StmtSource.Listeners.Add(New Diagnostics.DefaultTraceListener)
        OrmReadOnlyDBManager.StmtSource.Listeners(0).TraceOutputOptions = TraceOptions.None
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim t1 As Table1 = New Table1(-345, mgr.Cache, mgr.MappingEngine)
            Dim t2 As Table2 = mgr.Find(Of Table2)(1)
            t2.Tbl = t1

            Assert.AreEqual(ObjectState.Modified, t2.InternalProperties.ObjectState)
            mgr.BeginTransaction()
            Try
                Dim b As Boolean = t2.SaveChanges(True)
                Assert.AreEqual(ObjectState.Modified, t2.InternalProperties.ObjectState)
                Assert.IsTrue(b)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestSaveNewSmart()
        'OrmReadOnlyDBManager.StmtSource.Listeners.Add(New Diagnostics.DefaultTraceListener)
        OrmReadOnlyDBManager.StmtSource.Listeners(0).TraceOutputOptions = TraceOptions.None
        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            Dim t1 As Table1 = New Table1(-345, mgr.Cache, mgr.MappingEngine)
            t1.CreatedAt = Now
            Dim t2 As Table2 = mgr.Find(Of Table2)(1)
            t2.Tbl = t1

            Assert.AreEqual(ObjectState.Modified, t2.InternalProperties.ObjectState)
            mgr.BeginTransaction()
            Try
                Dim b As Boolean = t2.SaveChanges(True)
                Assert.AreEqual(ObjectState.Created, t1.InternalProperties.ObjectState)
                Assert.AreEqual(ObjectState.Modified, t2.InternalProperties.ObjectState)
                Assert.IsTrue(b)

                Using st As New ModificationsTracker(mgr)
                    st.Add(t2)
                    st.Add(t1)
                    st.AcceptModifications()
                End Using

                Assert.AreEqual(ObjectState.None, t2.InternalProperties.ObjectState)
                Assert.AreEqual(ObjectState.None, t1.InternalProperties.ObjectState)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> _
    Public Sub TestInh()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim l As Table1_x = mgr.Find(Of Table1_x)(1)
            l.Load()

            Dim r As Worm.ReadOnlyList(Of Table1_x) = mgr.FindDistinct(Of Table1_x)(Ctor.prop(GetType(Table1_x), "ID").eq(1), SCtor.prop(GetType(Table1_x), "Title"), True)
            Assert.AreEqual(1, r.Count)

        End Using
    End Sub

    <TestMethod()> Public Sub TestRawObject()
        Dim t As Table1 = CType(Activator.CreateInstance(GetType(Table1)), Table1)

        t.Name = "kasdfn"
        t.CreatedAt = Now
        t.Identifier = -100

        Assert.AreEqual(ObjectState.Created, t.InternalProperties.ObjectState)

        Using mgr As OrmReadOnlyDBManager = CreateWriteManager(GetSchema("1"))
            mgr.BeginTransaction()
            Try
                Using st As New ModificationsTracker(mgr)
                    st.Add(t)

                    st.AcceptModifications()
                End Using

                Assert.AreEqual(ObjectState.None, t.InternalProperties.ObjectState)
            Finally
                mgr.Rollback()
            End Try
        End Using
    End Sub

    <TestMethod()> Public Sub TestRawCommand()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Dim l As New List(Of Table1)
            Using cmd As New System.Data.SqlClient.SqlCommand("select id,code from table1 where id = 1")
                Dim s As List(Of SelectExpression) = FCtor.column(Nothing, "id", "ID", Field2DbRelations.PK).Add_custom("Code", "code").GetAllProperties

                mgr.QueryObjects(Of Table1)(cmd, l, s, _
                    Nothing, SelectExpression.GetMapping(s))

                Assert.AreEqual(1, l.Count)
                Assert.AreEqual(1, l(0).ID)
            End Using
        End Using
    End Sub

    <TestMethod()> Public Sub TestExecuteScalar()
        Using mgr As OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
            Using cmd As New System.Data.SqlClient.SqlCommand("select max(id) from table1")
                Dim obj As Object = mgr.ExecuteScalar(cmd)
                Assert.AreEqual(3, obj)
            End Using
        End Using
    End Sub

    '<TestMethod()> _
    'Public Sub TestSortAny()
    '    Using mgr As Orm.OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
    '        Dim c As ICollection(Of Table1) = mgr.Find(Of Table1)( _
    '            Orm.Criteria.AutoTypeField("Code").Between(2, 45), _
    '            Orm.Sorting.Field("Code"), False)

    '        c = mgr.Find(Of Table1)(Orm.Criteria.AutoTypeField("Code").Between(2, 45), Orm.Sorting.Any, False)
    '    End Using

    '    Using mgr As Orm.OrmReadOnlyDBManager = CreateManager(GetSchema("1"))
    '        Dim c As ICollection(Of Table1) = mgr.Find(Of Table1)(Orm.Criteria.AutoTypeField("Code").Between(2, 45), Orm.Sorting.Any, False)
    '    End Using
    'End Sub

    Private Function GetList(Of T As {KeyEntity})(ByVal col As ICollection(Of T)) As IList(Of T)
        Return CType(col, Global.System.Collections.Generic.IList(Of T))
    End Function

    Private _id As Integer = -100
    Private _new_objects As New Dictionary(Of Integer, KeyEntity)

    Public Sub AddNew(ByVal obj As _ICachedEntity) Implements INewObjectsStore.AddNew
        If obj Is Nothing Then
            Throw New ArgumentNullException("obj")
        End If

        _new_objects.Add(CInt(CType(obj, IKeyEntity).Identifier), CType(obj, KeyEntity))
    End Sub

    Public Function GetIdentity(ByVal type As Type) As PKDesc() Implements INewObjectsStore.GetPKForNewObject
        Dim i As Integer = _id
        _id += -1
        Return New PKDesc() {New PKDesc("id", _id)}
    End Function

    Public Function GetIdentity() As Integer
        Return CInt(GetIdentity(Nothing)(0).Value)
    End Function

    Public Function GetNew(ByVal t As System.Type, ByVal id() As Meta.PKDesc) As _ICachedEntity Implements INewObjectsStore.GetNew
        Dim o As KeyEntity = Nothing
        _new_objects.TryGetValue(CInt(id(0).Value), o)
        Return o
    End Function

    Public Sub RemoveNew(ByVal t As System.Type, ByVal id() As Meta.PKDesc) Implements INewObjectsStore.RemoveNew
        _new_objects.Remove(CInt(id(0).Value))
    End Sub

    Public Sub RemoveNew(ByVal obj As _ICachedEntity) Implements INewObjectsStore.RemoveNew
        If obj Is Nothing Then
            Throw New ArgumentNullException("obj")
        End If

        _new_objects.Remove(CInt(CType(obj, IKeyEntity).Identifier))
    End Sub
End Class
