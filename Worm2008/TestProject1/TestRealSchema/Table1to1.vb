Imports Worm.Entities
Imports Worm.Cache
Imports Worm.Entities.Meta

<Entity(GetType(Tables1to1.TablesImplementation), "1")> _
Public Class Tables1to1
    Inherits OrmBaseT(Of Tables1to1)
    Implements IOrmEditable(Of Tables1to1), IOptimizedValues

    Private _table1 As Table1
    Private _table1back As Table1
    Private _k As String

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(ByVal id As Integer, ByVal cache As OrmCache, ByVal schema As Worm.ObjectMappingEngine)
        MyBase.New(id, cache, schema)
    End Sub

    'Protected Overrides Sub CopyBody(ByVal from As Worm.Orm.OrmBase, ByVal [to] As Worm.Orm.OrmBase)
    '    CopyRelation(CType(from, Tables1to3), CType([to], Tables1to3))
    'End Sub

    Protected Sub CopyRelation(ByVal [from] As Tables1to1, ByVal [to] As Tables1to1) Implements IOrmEditable(Of Tables1to1).CopyBody
        With [from]
            [to]._table1 = ._table1
            [to]._table1back = ._table1back
            [to]._k = ._k
        End With
    End Sub

    'Public Overloads Overrides Function CreateSortComparer(ByVal sort As String, ByVal sortType As Worm.Orm.SortType) As System.Collections.IComparer
    '    Throw New NotSupportedException
    'End Function

    'Public Overloads Overrides Function CreateSortComparer(Of T As {New, Worm.Orm.OrmBase})(ByVal sort As String, ByVal sortType As Worm.Orm.SortType) As System.Collections.Generic.IComparer(Of T)
    '    Throw New NotSupportedException
    'End Function

    'Protected Overrides Function GetNew() As Worm.Orm.OrmBase
    '    Return New Tables1to3(Identifier, OrmCache, OrmSchema)
    'End Function

    Public Overridable Sub SetValue( _
        ByVal fieldName As String, ByVal oschema As IEntitySchema, ByVal value As Object) Implements IOptimizedValues.SetValueOptimized
        Select Case fieldName
            Case "K"
                K = CStr(value)
            Case "Table1"
                Table1 = CType(value, TestProject1.Table1)
            Case "Table1Back"
                Table1Back = CType(value, TestProject1.Table1)
            Case Else
                SetValueReflection(fieldName, value, oschema)
                'Throw New NotSupportedException(fieldName)
                'MyBase.SetValue(pi, fieldName, oschema, value)
        End Select
    End Sub

    Public Function GetValueOptimized(ByVal propertyAlias As String, ByVal schema As Worm.Entities.Meta.IEntitySchema) As Object Implements Worm.Entities.IOptimizedValues.GetValueOptimized
        Select Case propertyAlias
            Case "K"
                Return _k
            Case "Table1"
                Return _table1
            Case "Table1Back"
                Return _table1back
            Case "ID"
                Return Identifier
            Case Else
                Throw New NotSupportedException(propertyAlias)
                'MyBase.SetValue(pi, fieldName, oschema, value)
        End Select
    End Function

    <EntityPropertyAttribute(PropertyAlias:="K")> _
    Public Property K() As String
        Get
            Using SyncHelper(True, "K")
                Return _k
            End Using
        End Get
        Set(ByVal value As String)
            Using SyncHelper(False, "K")
                _k = value
            End Using
        End Set
    End Property

    <EntityPropertyAttribute(PropertyAlias:="Table1")> _
    Public Property Table1() As Table1
        Get
            Using SyncHelper(True, "Table1")
                Return _table1
            End Using
        End Get
        Set(ByVal value As Table1)
            Using SyncHelper(False, "Table1")
                _table1 = value
            End Using
        End Set
    End Property

    <EntityPropertyAttribute(PropertyAlias:="Table1Back")> _
    Public Property Table1Back() As Table1
        Get
            Using SyncHelper(True, "Table1")
                Return _table1back
            End Using
        End Get
        Set(ByVal value As Table1)
            Using SyncHelper(False, "Table1")
                _table1back = value
            End Using
        End Set
    End Property

    Public Class TablesImplementation
        Inherits ObjectSchemaBaseImplementation
        Implements IRelation

        Private _idx As OrmObjectIndex
        Public Shared _tables() As SourceFragment = {New SourceFragment("dbo.Table1to1")}

        Public Enum Tables
            Main
        End Enum

        Public Overrides Function GetFieldColumnMap() As Worm.Collections.IndexedCollection(Of String, MapField2Column)
            If _idx Is Nothing Then
                Dim idx As New OrmObjectIndex
                idx.Add(New MapField2Column("ID", "id", GetTables()(Tables.Main)))
                idx.Add(New MapField2Column("K", "k", GetTables()(Tables.Main)))
                idx.Add(New MapField2Column("Table1", "table1", GetTables()(Tables.Main)))
                idx.Add(New MapField2Column("Table1Back", "table1_back", GetTables()(Tables.Main)))
                _idx = idx
            End If
            Return _idx
        End Function

        Public Overrides Function GetTables() As SourceFragment()
            Return _tables
        End Function

        Public Function GetFirstType() As IRelation.RelationDesc Implements IRelation.GetFirstType
            Return New IRelation.RelationDesc("Table1", GetType(Table1), False)
        End Function

        Public Function GetSecondType() As IRelation.RelationDesc Implements IRelation.GetSecondType
            Return New IRelation.RelationDesc("Table1Back", GetType(Table1), True)
        End Function

    End Class
End Class