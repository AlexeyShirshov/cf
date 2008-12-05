Imports Worm.Entities
Imports Worm.Entities.Meta
Imports Worm.Cache
Imports Worm.Criteria.Joins

<Entity(GetType(CompositeSchema), "1")> _
Public Class Composite
    Inherits OrmBaseT(Of Composite)
    Implements IOrmEditable(Of Composite)

    Private _m As String
    Private _m2 As String

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(ByVal id As Integer, ByVal cache As CacheBase, ByVal schema As Worm.ObjectMappingEngine)
        MyBase.New(id, cache, schema)
    End Sub

    <Column("Title")> _
    Public Property Message() As String
        Get
            Using SyncHelper(True, "Title")
                Return _m
            End Using
        End Get
        Set(ByVal value As String)
            Using SyncHelper(False, "Title")
                _m = value
            End Using
        End Set
    End Property

    <Column("Title2", Field2DbRelations.ReadOnly)> _
    Public Property Message2() As String
        Get
            Using SyncHelper(True, "Title2")
                Return _m2
            End Using
        End Get
        Set(ByVal value As String)
            Using SyncHelper(False, "Title2")
                _m2 = value
            End Using
        End Set
    End Property

    Public Sub CopyBody2(ByVal from As Composite, ByVal [to] As Composite) Implements IOrmEditable(Of Composite).CopyBody
        With [from]
            [to]._m = ._m
            [to]._m2 = ._m2
        End With
    End Sub
End Class


Public Class CompositeSchema
    Inherits ObjectSchemaBaseImplementation
    Implements IReadonlyObjectSchema

    Private _idx As OrmObjectIndex
    Private _tables() As SourceFragment = {New SourceFragment("dbo.m1"), New SourceFragment("dbo.m2")}

    Public Enum Tables
        First
        Second
    End Enum

    Public Overrides Function GetFieldColumnMap() As Worm.Collections.IndexedCollection(Of String, MapField2Column)
        If _idx Is Nothing Then
            Dim idx As New OrmObjectIndex
            idx.Add(New MapField2Column("ID", "id", GetTables()(Tables.First)))
            idx.Add(New MapField2Column("Title", "msg", GetTables()(Tables.First)))
            idx.Add(New MapField2Column("Title2", "msg", GetTables()(Tables.Second)))
            _idx = idx
        End If
        Return _idx
    End Function

    Public Overrides Function GetTables() As SourceFragment()
        Return _tables
    End Function

    Public Overrides Function GetJoins(ByVal left As SourceFragment, ByVal right As SourceFragment) As Worm.Criteria.Joins.QueryJoin
        If left.Equals(GetTables()(Tables.First)) AndAlso right.Equals(GetTables()(Tables.Second)) Then
            Return New QueryJoin(right, Worm.Criteria.Joins.JoinType.Join, New JoinFilter(right, "id", _objectType, "ID", Worm.Criteria.FilterOperation.Equal))
        End If
        Return MyBase.GetJoins(left, right)
    End Function

    Public Function GetEditableSchema() As IMultiTableWithM2MSchema Implements IReadonlyObjectSchema.GetEditableSchema
        Return New CompositeEditableSchema
    End Function
End Class

Public Class CompositeEditableSchema
    Implements IMultiTableWithM2MSchema

    Private _idx As OrmObjectIndex
    Private _tables() As SourceFragment = {New SourceFragment("dbo.m1")}

    Public Enum Tables
        First
    End Enum

    Public Function GetFieldColumnMap() As Worm.Collections.IndexedCollection(Of String, MapField2Column) Implements IMultiTableWithM2MSchema.GetFieldColumnMap
        If _idx Is Nothing Then
            Dim idx As New OrmObjectIndex
            idx.Add(New MapField2Column("ID", "id", GetTables()(Tables.First)))
            idx.Add(New MapField2Column("Title", "msg", GetTables()(Tables.First)))
            _idx = idx
        End If
        Return _idx
    End Function

    Public Function GetJoins(ByVal left As SourceFragment, ByVal right As SourceFragment) As Worm.Criteria.Joins.QueryJoin Implements IMultiTableWithM2MSchema.GetJoins
        Return Nothing
    End Function

    Public Function GetTables() As SourceFragment() Implements IMultiTableWithM2MSchema.GetTables
        Return _tables
    End Function

    Public Function GetM2MRelations() As Worm.Entities.Meta.M2MRelation() Implements Worm.Entities.Meta.IMultiTableWithM2MSchema.GetM2MRelations
        Return Nothing
    End Function

    Public Function GetSuppressedFields() As String() Implements Worm.Entities.Meta.IObjectSchemaBase.GetSuppressedFields
        Return Nothing
    End Function

    Public Function ChangeValueType(ByVal c As Worm.Entities.Meta.ColumnAttribute, ByVal value As Object, ByRef newvalue As Object) As Boolean Implements Worm.Entities.Meta.IObjectSchemaBase.ChangeValueType
        newvalue = value
        Return False
    End Function

    Public ReadOnly Property Table() As Worm.Entities.Meta.SourceFragment Implements Worm.Entities.Meta.IObjectSchemaBase.Table
        Get
            Return _tables(0)
        End Get
    End Property
End Class