﻿Imports Worm.Entities
Imports Worm.Criteria.Values
Imports Worm.Entities.Meta

Namespace Criteria.Core
    Public MustInherit Class TemplateBase
        Implements Worm.Criteria.Core.ITemplate

        Private _oper As FilterOperation

        Public Sub New()

        End Sub

        Public Sub New(ByVal operation As FilterOperation)
            _oper = operation
        End Sub

        Public ReadOnly Property Operation() As FilterOperation Implements ITemplate.Operation
            Get
                Return _oper
            End Get
        End Property

        Public Shared Function OperToStringInternal(ByVal oper As FilterOperation) As String
            Select Case oper
                Case FilterOperation.Equal
                    Return "Equal"
                Case FilterOperation.GreaterEqualThan
                    Return "GreaterEqualThan"
                Case FilterOperation.GreaterThan
                    Return "GreaterThan"
                Case FilterOperation.In
                    Return "In"
                Case FilterOperation.LessEqualThan
                    Return "LessEqualThan"
                Case FilterOperation.NotEqual
                    Return "NotEqual"
                Case FilterOperation.NotIn
                    Return "NotIn"
                Case FilterOperation.Like
                    Return "Like"
                Case FilterOperation.LessThan
                    Return "LessThan"
                Case FilterOperation.Is
                    Return "Is"
                Case FilterOperation.IsNot
                    Return "IsNot"
                Case FilterOperation.Exists
                    Return "Exists"
                Case FilterOperation.NotExists
                    Return "NotExists"
                Case FilterOperation.Between
                    Return "Between"
                Case Else
                    Throw New ObjectMappingException("Operation " & oper & " not supported")
            End Select
        End Function

        Public ReadOnly Property OperToString() As String Implements ITemplate.OperToString
            Get
                Return OperToStringInternal(_oper)
            End Get
        End Property

        Public MustOverride Function GetStaticString() As String Implements ITemplate.GetStaticString

        Public ReadOnly Property OperToStmt(ByVal stmt As StmtGenerator) As String Implements ITemplate.OperToStmt
            Get
                Return stmt.Oper2String(_oper)
            End Get
        End Property
    End Class

    Public Class OrmFilterTemplate
        Inherits TemplateBase
        Implements IOrmFilterTemplate

        Private _os As ObjectSource
        Private _fieldname As String

        'Private _appl As Boolean

        Public Sub New(ByVal t As Type, ByVal propertyAlias As String, ByVal oper As FilterOperation) ', ByVal appl As Boolean)
            MyBase.New(oper)
            _os = New ObjectSource(t)
            _fieldname = propertyAlias
            '_appl = appl
        End Sub

        Public Sub New(ByVal entityName As String, ByVal propertyAlias As String, ByVal oper As FilterOperation) ', ByVal appl As Boolean)
            MyBase.New(oper)
            _os = New ObjectSource(entityName)
            _fieldname = propertyAlias
        End Sub

        Public Sub New(ByVal [alias] As ObjectAlias, ByVal propertyAlias As String, ByVal oper As FilterOperation) ', ByVal appl As Boolean)
            MyBase.New(oper)
            _os = New ObjectSource([alias])
            _fieldname = propertyAlias
        End Sub

        Public Sub New(ByVal os As ObjectSource, ByVal propertyAlias As String, ByVal oper As FilterOperation) ', ByVal appl As Boolean)
            MyBase.New(oper)
            _os = os
            _fieldname = propertyAlias
        End Sub

        Public Overridable Function MakeFilter(ByVal schema As ObjectMappingEngine, ByVal oschema As IObjectSchemaBase, ByVal obj As ICachedEntity) As IEntityFilter 'Implements IOrmFilterTemplate.MakeFilter
            If obj Is Nothing Then
                Throw New ArgumentNullException("obj")
            End If

            Dim lt As Type = _os.AnyType
            If lt Is Nothing Then
                If Not String.IsNullOrEmpty(_os.AnyEntityName) Then
                    lt = schema.GetTypeByEntityName(_os.AnyEntityName)
                Else
                    Throw New InvalidOperationException(String.Format("Type is not specified in filter: {0} {1}", _fieldname, OperToString))
                End If
            End If

            If obj.GetType IsNot lt Then
                Dim o As IKeyEntity = schema.GetJoinObj(oschema, obj, lt)
                If o Is Nothing Then
                    Throw New ArgumentException(String.Format("Template type {0} is not match {1}", lt.ToString, obj.GetType))
                End If
                Return MakeFilter(schema, schema.GetObjectSchema(lt), o)
            Else
                Dim v As Object = obj.GetValueOptimized(Nothing, _fieldname, oschema)
                If _os.Type IsNot Nothing Then
                    Return CreateEntityFilter(_os.Type, _fieldname, New ScalarValue(v), Operation)
                ElseIf Not String.IsNullOrEmpty(_os.EntityName) Then
                    Return CreateEntityFilter(_os.EntityName, _fieldname, New ScalarValue(v), Operation)
                Else
                    Return CreateEntityFilter(_os.ObjectAlias, _fieldname, New ScalarValue(v), Operation)
                End If
            End If
        End Function

        Public ReadOnly Property ObjectSource() As ObjectSource
            Get
                Return _os
            End Get
        End Property

        'Public ReadOnly Property EntityName() As String
        '    Get
        '        Return _en
        '    End Get
        'End Property

        'Public ReadOnly Property Type() As Type
        '    Get
        '        Return _t
        '    End Get
        'End Property

        Public ReadOnly Property PropertyAlias() As String
            Get
                Return _fieldname
            End Get
        End Property

        Public Sub SetType(ByVal oa As ObjectAlias) Implements IOrmFilterTemplate.SetType
            If _os Is Nothing Then
                If oa.Type IsNot Nothing Then
                    _os = New ObjectSource(oa.Type)
                Else
                    _os = New ObjectSource(oa.EntityName)
                End If
            End If
        End Sub

        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Return Equals(TryCast(obj, OrmFilterTemplate))
        End Function

        Public Overloads Function Equals(ByVal obj As OrmFilterTemplate) As Boolean
            If obj Is Nothing Then
                Return False
            End If
            Return _os.Equals(obj._os) AndAlso _fieldname Is obj._fieldname AndAlso Operation = obj.Operation
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return GetStaticString.GetHashCode
        End Function

        Public Overrides Function GetStaticString() As String
            Return _os.ToStaticString & _fieldname & OperToString()
        End Function

        Public Function MakeHash(ByVal schema As ObjectMappingEngine, ByVal oschema As IObjectSchemaBase, ByVal obj As ICachedEntity) As String Implements IOrmFilterTemplate.MakeHash
            If Operation = FilterOperation.Equal Then
                Return MakeFilter(schema, oschema, obj).MakeHash
            Else
                Return EntityFilter.EmptyHash
            End If
        End Function

        Protected Function CreateEntityFilter(ByVal t As Type, ByVal propertyAlias As String, ByVal value As IParamFilterValue, ByVal operation As Worm.Criteria.FilterOperation) As EntityFilter
            Return New EntityFilter(t, propertyAlias, value, operation)
        End Function

        Protected Function CreateEntityFilter(ByVal entityName As String, ByVal propertyAlias As String, ByVal value As IParamFilterValue, ByVal operation As Worm.Criteria.FilterOperation) As EntityFilter
            Return New EntityFilter(entityName, propertyAlias, value, operation)
        End Function

        Protected Function CreateEntityFilter(ByVal oa As ObjectAlias, ByVal propertyAlias As String, ByVal value As IParamFilterValue, ByVal operation As Worm.Criteria.FilterOperation) As EntityFilter
            Return New EntityFilter(oa, propertyAlias, value, operation)
        End Function

        'Public MustOverride ReadOnly Property Operation() As FilterOperation Implements IOrmFilterTemplate.Operation
        'Public MustOverride ReadOnly Property OperToString() As String Implements ITemplate.OperToString
        'Public MustOverride ReadOnly Property OperToStmt() As String Implements ITemplate.OperToStmt
    End Class

    Public Class TableFilterTemplate
        Inherits TemplateBase

        Private _tbl As SourceFragment
        Private _col As String

        Public Sub New(ByVal table As SourceFragment, ByVal column As String, ByVal operation As Worm.Criteria.FilterOperation)
            MyBase.New(operation)
            _tbl = table
            _col = column
        End Sub

        Public Sub New(ByVal table As SourceFragment, ByVal column As String)
            MyBase.New()
            _tbl = table
            _col = column
        End Sub

        Public ReadOnly Property Table() As SourceFragment
            Get
                Return _tbl
            End Get
        End Property

        Public ReadOnly Property Column() As String
            Get
                Return _col
            End Get
        End Property

        Public Overrides Function GetStaticString() As String
            Return _tbl.RawName() & _col & OperToString
        End Function

        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Return Equals(TryCast(obj, TableFilterTemplate))
        End Function

        Public Overloads Function Equals(ByVal obj As TableFilterTemplate) As Boolean
            If obj Is Nothing Then
                Return False
            End If
            Return _tbl Is obj._tbl AndAlso _col Is obj._col AndAlso Operation = obj.Operation
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return GetStaticString.GetHashCode
        End Function

    End Class

End Namespace