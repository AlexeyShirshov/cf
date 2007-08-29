Imports System.Data.SqlClient
Imports System.Runtime.CompilerServices
Imports CoreFramework.Structures
Imports CoreFramework.Threading
Imports System.Collections.Generic

Namespace Orm

#Region " Filter value "

    Public Interface IFilterValue
        Function GetParam(ByVal schema As OrmSchemaBase, ByVal paramMgr As ICreateParam, ByVal f As IEntityFilter) As String
        Function _ToString() As String
    End Interface

    Public Interface IEvaluableValue
        Inherits IFilterValue
        ReadOnly Property Value() As Object
    End Interface

    Public Class SimpleValue
        Implements IEvaluableValue

        Private _v As Object
        Private _pname As String
        'Private _f As IEntityFilter

        Protected Sub New()
        End Sub

        'Public Sub New(ByVal value As Object)
        '    _v = value
        'End Sub

        Public Sub New(ByVal value As Object)
            _v = value
        End Sub

        Public Function GetParam(ByVal schema As OrmSchemaBase, ByVal paramMgr As ICreateParam, ByVal f As IEntityFilter) As String Implements IFilterValue.GetParam
            Dim v As Object = _v
            If f IsNot Nothing Then
                v = f.PrepareValue(schema, v)
            End If

            'Dim p As String = _pname
            'If String.IsNullOrEmpty(p) Then
            '    p = paramMgr.CreateParam(v)
            '    If paramMgr.NamedParams Then
            '        _pname = p
            '    End If
            'Else
            '    p = paramMgr.AddParam(_pname, v)
            '    _pname = p
            'End If
            'Return p
            _pname = paramMgr.AddParam(_pname, v)
            Return _pname
        End Function

        'Protected Property Value() As Object
        '    Get
        '        Return _v
        '    End Get
        '    Set(ByVal value As Object)
        '        _v = value
        '    End Set
        'End Property

        Public Overridable Function _ToString() As String Implements IFilterValue._ToString
            If _v IsNot Nothing Then
                Return _v.ToString
            End If
            Return String.Empty
        End Function

        Public Overridable ReadOnly Property Value() As Object Implements IEvaluableValue.Value
            Get
                Return _v
            End Get
        End Property
    End Class

    Public Class LiteralValue
        Implements IFilterValue

        Private _pname As String

        Public Sub New(ByVal literal As String)
            _pname = literal
        End Sub

        Public Function GetParam(ByVal schema As OrmSchemaBase, ByVal paramMgr As ICreateParam, ByVal f As IEntityFilter) As String Implements IFilterValue.GetParam
            Return _pname
        End Function

        Public Function _ToString() As String Implements IFilterValue._ToString
            Return _pname
        End Function
    End Class

    Public Class EntityValue
        Inherits SimpleValue

        Private _id As Integer
        Private _t As Type

        Public Sub New(ByVal o As OrmBase)
            MyBase.New()
            If o IsNot Nothing Then
                _id = o.Identifier
                _t = o.GetType
            Else
                _t = GetType(OrmBase)
            End If
        End Sub

        Public Function GetOrmValue(ByVal mgr As OrmManagerBase) As OrmBase
            Return mgr.CreateDBObject(_id, _t)
        End Function

        Public ReadOnly Property OrmType() As Type
            Get
                Return _t
            End Get
        End Property

        Public Overrides ReadOnly Property Value() As Object
            Get
                Return GetOrmValue(OrmManagerBase.CurrentManager)
            End Get
        End Property
    End Class

#End Region

    Public Interface IOrmFilterTemplate
        Function MakeFilter(ByVal schema As OrmSchemaBase, ByVal obj As OrmBase) As IEntityFilter
        Sub SetType(ByVal t As Type)
    End Interface

    Public Interface IFilter
        Function MakeSQLStmt(ByVal schema As OrmSchemaBase, ByVal tableAliases As IDictionary(Of OrmTable, String), ByVal pname As ICreateParam) As String
        Function GetAllFilters() As ICollection(Of IFilter)
        Function Equals(ByVal f As IFilter) As Boolean
        Function ReplaceFilter(ByVal replacement As IFilter, ByVal replacer As IFilter) As IFilter
        Function ToString() As String
    End Interface

    Public Interface ITemplateFilter
        Inherits IFilter
        Function GetStaticString() As String
        Function ReplaceByTemplate(ByVal replacement As ITemplateFilter, ByVal replacer As ITemplateFilter) As ITemplateFilter
        ReadOnly Property Template() As TemplateBase
        Function MakeSingleStmt(ByVal schema As DbSchema, ByVal pname As ICreateParam) As Pair(Of String)
    End Interface

    Public Interface IValuableFilter
        ReadOnly Property Value() As IFilterValue
    End Interface

    Public Interface IEntityFilter
        Inherits ITemplateFilter

        Enum EvalResult
            Found
            NotFound
            Unknown
        End Enum

        Function Eval(ByVal schema As OrmSchemaBase, ByVal obj As OrmBase, ByVal oschema As IOrmObjectSchemaBase) As EvalResult
        Function GetFilterTemplate() As IOrmFilterTemplate
        Function PrepareValue(ByVal schema As OrmSchemaBase, ByVal v As Object) As Object
    End Interface

    Public MustInherit Class TemplateBase

        Private _oper As FilterOperation

        Public Sub New()

        End Sub

        Public Sub New(ByVal operation As FilterOperation)
            _oper = operation
        End Sub

        Public ReadOnly Property Operation() As FilterOperation
            Get
                Return _oper
            End Get
        End Property

        Protected Friend ReadOnly Property OperToString() As String
            Get
                Select Case _oper
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
                    Case Else
                        Throw New DBSchemaException("Operation " & _oper & " not supported")
                End Select
            End Get
        End Property

        Protected Friend Function Oper2String() As String
            Return Oper2String(_oper)
        End Function

        Protected Friend Shared Function Oper2String(ByVal oper As FilterOperation) As String
            Select Case oper
                Case FilterOperation.Equal
                    Return " = "
                Case FilterOperation.GreaterEqualThan
                    Return " >= "
                Case FilterOperation.GreaterThan
                    Return " > "
                Case FilterOperation.In
                    Return " in "
                Case FilterOperation.NotEqual
                    Return " <> "
                Case FilterOperation.NotIn
                    Return " not in "
                Case FilterOperation.LessEqualThan
                    Return " <= "
                Case FilterOperation.Like
                    Return " like "
                Case FilterOperation.LessThan
                    Return " < "
                Case Else
                    Throw New DBSchemaException("invalid opration " & oper.ToString)
            End Select
        End Function

        Public MustOverride Function GetStaticString() As String

        Public Overrides Function ToString() As String
            Return GetStaticString()
        End Function

    End Class

    Public Class TableFilterTemplate
        Inherits TemplateBase

        Private _tbl As OrmTable
        Private _col As String

        Public Sub New(ByVal table As OrmTable, ByVal column As String, ByVal operation As FilterOperation)
            MyBase.New(operation)
            _tbl = table
            _col = column
        End Sub

        Public Sub New(ByVal table As OrmTable, ByVal column As String)
            MyBase.New()
            _tbl = table
            _col = column
        End Sub

        Public ReadOnly Property Table() As OrmTable
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
            Return _tbl.TableName() & _col & Oper2String()
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

    Public MustInherit Class FilterBase
        Implements IFilter, IValuableFilter

        Private _v As IFilterValue

        Public Sub New(ByVal value As IFilterValue)
            If value Is Nothing Then
                Throw New ArgumentNullException("value")
            End If
            _v = value
        End Sub

        Protected MustOverride Function _ToString() As String Implements IFilter.ToString
        Public MustOverride Function MakeSQLStmt(ByVal schema As OrmSchemaBase, ByVal tableAliases As System.Collections.Generic.IDictionary(Of OrmTable, String), ByVal pname As ICreateParam) As String Implements IFilter.MakeSQLStmt
        Public MustOverride Function GetAllFilters() As System.Collections.Generic.ICollection(Of IFilter) Implements IFilter.GetAllFilters

        Public Overrides Function ToString() As String
            Return _ToString()
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return _ToString.GetHashCode()
        End Function

        Public Overrides Function Equals(ByVal f As Object) As Boolean
            Return Equals(TryCast(f, FilterBase))
        End Function

        Public Overloads Function Equals(ByVal f As FilterBase) As Boolean
            If f IsNot Nothing Then
                Return _ToString.Equals(f.ToString)
            Else
                Return False
            End If
        End Function

        Public ReadOnly Property Value() As IFilterValue Implements IValuableFilter.Value
            Get
                Return _v
            End Get
        End Property

        'Protected Sub SetValue(ByVal v As IFilterValue)
        '    _v = v
        'End Sub

        Protected Function GetParam(ByVal schema As OrmSchemaBase, ByVal pmgr As ICreateParam) As String
            If _v Is Nothing Then
                'Return pmgr.CreateParam(Nothing)
                Throw New InvalidOperationException("Param is null")
            End If
            Return _v.GetParam(schema, pmgr, TryCast(Me, IEntityFilter))
        End Function

        Private Function Equals1(ByVal f As IFilter) As Boolean Implements IFilter.Equals
            Return Equals(TryCast(f, FilterBase))
        End Function

        Public Function ReplaceFilter(ByVal replacement As IFilter, ByVal replacer As IFilter) As IFilter Implements IFilter.ReplaceFilter
            Return ReplaceFilter(TryCast(replacement, FilterBase), TryCast(replacer, FilterBase))
        End Function

        Public Function ReplaceFilter(ByVal replacement As FilterBase, ByVal replacer As FilterBase) As FilterBase
            If Equals(replacement) Then
                Return replacer
            End If
            Return Nothing
        End Function
    End Class

    Public MustInherit Class TemplatedFilterBase
        Inherits FilterBase
        Implements ITemplateFilter

        Private _templ As TemplateBase

        Public Sub New(ByVal v As IFilterValue, ByVal template As TemplateBase)
            MyBase.New(v)
            _templ = template
        End Sub

        Public Function GetStaticString() As String Implements ITemplateFilter.GetStaticString
            Return _templ.GetStaticString
        End Function

        Public Function Replacetemplate(ByVal replacement As ITemplateFilter, ByVal replacer As ITemplateFilter) As ITemplateFilter Implements ITemplateFilter.ReplaceByTemplate
            If Not _templ.Equals(replacement.Template) Then
                Return Nothing
            End If
            Return replacer
        End Function

        Public ReadOnly Property Template() As TemplateBase Implements ITemplateFilter.Template
            Get
                Return _templ
            End Get
        End Property

        Public MustOverride Function MakeSingleStmt(ByVal schema As DbSchema, ByVal pname As ICreateParam) As CoreFramework.Structures.Pair(Of String) Implements ITemplateFilter.MakeSingleStmt
    End Class

    Public Class TableFilter
        Inherits TemplatedFilterBase

        'Private _templ As TableFilterTemplate

        Public Sub New(ByVal table As OrmTable, ByVal column As String, ByVal value As IFilterValue, ByVal operation As FilterOperation)
            MyBase.New(value, New TableFilterTemplate(table, column, operation))
            '_templ = New TableFilterTemplate(table, column, operation)
        End Sub

        Protected Overrides Function _ToString() As String
            'Return _templ.Table.TableName & _templ.Column & Value._ToString & _templ.OperToString
            Return Value._ToString & GetStaticString()
        End Function

        'Public Overrides Function GetStaticString() As String
        '    Return _templ.Table.TableName() & _templ.Column & _templ.Oper2String
        'End Function

        Public Shadows ReadOnly Property Template() As TableFilterTemplate
            Get
                Return CType(MyBase.Template, TableFilterTemplate)
            End Get
        End Property

        Public Overrides Function MakeSQLStmt(ByVal schema As OrmSchemaBase, ByVal tableAliases As System.Collections.Generic.IDictionary(Of OrmTable, String), ByVal pname As ICreateParam) As String
            Dim map As New MapField2Column(String.Empty, Template.Column, Template.Table)
            Dim [alias] As String = String.Empty

            If tableAliases IsNot Nothing Then
                [alias] = tableAliases(map._tableName) & "."
            End If

            Return [alias] & map._columnName & Template.Oper2String & GetParam(schema, pname)
        End Function

        Public Overrides Function GetAllFilters() As System.Collections.Generic.ICollection(Of IFilter)
            Return New TableFilter() {Me}
        End Function

        Public Overrides Function MakeSingleStmt(ByVal schema As DbSchema, ByVal pname As ICreateParam) As CoreFramework.Structures.Pair(Of String)
            If schema Is Nothing Then
                Throw New ArgumentNullException("schema")
            End If

            If pname Is Nothing Then
                Throw New ArgumentNullException("pname")
            End If

            Dim prname As String = Value.GetParam(schema, pname, Nothing)

            Return New Pair(Of String)(Template.Column, prname)
        End Function
    End Class

    Public Class OrmFilterTemplate
        Inherits TemplateBase
        Implements IOrmFilterTemplate

        Private _t As Type
        Private _fieldname As String
        'Private _appl As Boolean

        Public Sub New(ByVal t As Type, ByVal fieldName As String, ByVal oper As FilterOperation) ', ByVal appl As Boolean)
            MyBase.New(oper)
            _t = t
            _fieldname = fieldName
            '_appl = appl
        End Sub

        Public Function MakeFilter(ByVal schema As OrmSchemaBase, ByVal obj As OrmBase) As IEntityFilter Implements IOrmFilterTemplate.MakeFilter
            If obj Is Nothing Then
                Throw New ArgumentNullException("obj")
            End If

            If obj.GetType IsNot _t Then
                Throw New ArgumentException(String.Format("Template type {0} is not match {1}", _t.ToString, obj.GetType))
            End If

            Dim v As Object = schema.GetFieldValue(obj, _fieldname)

            Return New EntityFilter(_t, _fieldname, New SimpleValue(v), Operation)
        End Function

        Public ReadOnly Property Type() As Type
            Get
                Return _t
            End Get
        End Property

        Public ReadOnly Property FieldName() As String
            Get
                Return _fieldname
            End Get
        End Property

        Public Sub SetType(ByVal t As System.Type) Implements IOrmFilterTemplate.SetType
            If _t Is Nothing Then
                _t = t
            End If
        End Sub

        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Return Equals(TryCast(obj, OrmFilterTemplate))
        End Function

        Public Overloads Function Equals(ByVal obj As OrmFilterTemplate) As Boolean
            If obj Is Nothing Then
                Return False
            End If
            Return _t Is obj._t AndAlso _fieldname Is obj._fieldname AndAlso Operation = obj.Operation
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return GetStaticString.GetHashCode
        End Function

        Public Overrides Function GetStaticString() As String
            Return _t.ToString & _fieldname & Oper2String()
        End Function
    End Class

    Public Class EntityFilter
        Inherits TemplatedFilterBase
        Implements IEntityFilter

        'Private _templ As OrmFilterTemplate
        Private _str As String

        Public Sub New(ByVal t As Type, ByVal fieldName As String, ByVal value As IFilterValue, ByVal operation As FilterOperation)
            MyBase.New(value, New OrmFilterTemplate(t, fieldName, operation))
            _str = value._ToString & Template.GetStaticString
        End Sub

        Protected Overrides Function _ToString() As String
            Return _str
        End Function

        'Public Overrides Function GetStaticString() As String
        '    Return _templ.Type.ToString & _templ.FieldName & _templ.Oper2String
        'End Function

        Public Shadows ReadOnly Property Template() As OrmFilterTemplate
            Get
                Return CType(MyBase.Template, OrmFilterTemplate)
            End Get
        End Property

        Public Overrides Function MakeSQLStmt(ByVal schema As OrmSchemaBase, ByVal tableAliases As System.Collections.Generic.IDictionary(Of OrmTable, String), ByVal pname As ICreateParam) As String
            Dim map As MapField2Column = schema.GetObjectSchema(Template.Type).GetFieldColumnMap()(Template.FieldName)
            Dim [alias] As String = String.Empty

            If tableAliases IsNot Nothing Then
                [alias] = tableAliases(map._tableName) & "."
            End If

            Return [alias] & map._columnName & Template.Oper2String & GetParam(schema, pname)
        End Function

        Public Function Eval(ByVal schema As OrmSchemaBase, ByVal obj As OrmBase, ByVal oschema As IOrmObjectSchemaBase) As IEntityFilter.EvalResult Implements IEntityFilter.Eval
            Dim evval As IEvaluableValue = TryCast(Value, IEvaluableValue)
            If evval IsNot Nothing Then
                If schema Is Nothing Then
                    Throw New ArgumentNullException("schema")
                End If

                If obj Is Nothing Then
                    Throw New ArgumentNullException("obj")
                End If

                Dim t As Type = obj.GetType
                If Template.Type Is t Then
                    Dim r As IEntityFilter.EvalResult = IEntityFilter.EvalResult.NotFound
                    Dim v As Object = obj.GetValue(Template.FieldName, oschema) 'schema.GetFieldValue(obj, _fieldname)
                    If v IsNot Nothing Then
                        Dim tt As Type = v.GetType
                        Dim orm As OrmBase = TryCast(v, OrmBase)
                        Dim b As Boolean = orm IsNot Nothing
                        If b Then
                            Dim ov As EntityValue = TryCast(Value, EntityValue)
                            If ov Is Nothing Then
                                Throw New InvalidOperationException(String.Format("Field {0} is Entity but param is not", Template.FieldName))
                            End If
                            If tt IsNot ov.OrmType Then
                                Throw New InvalidOperationException(String.Format("Field {0} is type of {1} but param is type of {2}", Template.FieldName, tt.ToString, ov.OrmType.ToString))
                            End If
                        End If

                        Dim val As Object = evval.Value
                        Select Case Template.Operation
                            Case FilterOperation.Equal
                                If Equals(v, val) Then
                                    r = IEntityFilter.EvalResult.Found
                                End If
                            Case FilterOperation.GreaterEqualThan
                                Dim i As Integer = CType(v, IComparable).CompareTo(val)
                                If i >= 0 Then
                                    r = IEntityFilter.EvalResult.Found
                                End If
                            Case FilterOperation.GreaterThan
                                Dim i As Integer = CType(v, IComparable).CompareTo(val)
                                If i > 0 Then
                                    r = IEntityFilter.EvalResult.Found
                                End If
                            Case FilterOperation.LessEqualThan
                                Dim i As Integer = CType(v, IComparable).CompareTo(val)
                                If i <= 0 Then
                                    r = IEntityFilter.EvalResult.Found
                                End If
                            Case FilterOperation.LessThan
                                Dim i As Integer = CType(v, IComparable).CompareTo(val)
                                If i < 0 Then
                                    r = IEntityFilter.EvalResult.Found
                                End If
                            Case FilterOperation.NotEqual
                                If Not Equals(v, val) Then
                                    r = IEntityFilter.EvalResult.Found
                                End If
                            Case Else
                                r = IEntityFilter.EvalResult.Unknown
                        End Select
                    Else
                        If Value Is Nothing Then
                            r = IEntityFilter.EvalResult.Found
                        End If
                    End If

                    Return r
                End If
            End If

            Return IEntityFilter.EvalResult.Unknown
        End Function

        Public Overrides Function GetAllFilters() As System.Collections.Generic.ICollection(Of IFilter)
            Return New EntityFilter() {Me}
        End Function

        Public Function GetFilterTemplate() As IOrmFilterTemplate Implements IEntityFilter.GetFilterTemplate
            If TryCast(Value, IEvaluableValue) IsNot Nothing Then
                Return Template
            End If
            Return Nothing
        End Function

        Public Function PrepareValue(ByVal schema As OrmSchemaBase, ByVal v As Object) As Object Implements IEntityFilter.PrepareValue
            Return schema.ChangeValueType(Template.Type, schema.GetColumnByFieldName(Template.Type, Template.FieldName), v)
        End Function

        'Public Overrides Function Equals(ByVal obj As Object) As Boolean
        '    Return Equals(TryCast(obj, EntityFilter))
        'End Function

        'Public Overloads Function Equals(ByVal obj As EntityFilter) As Boolean
        '    If obj Is Nothing Then
        '        Return False
        '    End If
        '    Return _str = obj._str
        'End Function

        'Public Overrides Function GetHashCode() As Integer
        '    Return _str.GetHashCode
        'End Function

        Public Overrides Function MakeSingleStmt(ByVal schema As DbSchema, ByVal pname As ICreateParam) As Pair(Of String)
            If schema Is Nothing Then
                Throw New ArgumentNullException("schema")
            End If

            If pname Is Nothing Then
                Throw New ArgumentNullException("pname")
            End If

            Dim prname As String = Value.GetParam(schema, pname, Me)


            Dim map As MapField2Column = schema.GetObjectSchema(Template.Type).GetFieldColumnMap()(Template.FieldName)
            Dim v As IValuableFilter = TryCast(Value, IValuableFilter)
            If v IsNot Nothing AndAlso v.Value Is DBNull.Value Then
                If schema.GetFieldTypeByName(Template.Type, Template.FieldName) Is GetType(Byte()) Then
                    pname.GetParameter(prname).DbType = System.Data.DbType.Binary
                ElseIf schema.GetFieldTypeByName(Template.Type, Template.FieldName) Is GetType(Decimal) Then
                    pname.GetParameter(prname).DbType = System.Data.DbType.Decimal
                End If
            End If

            Return New Pair(Of String)(map._columnName, prname)
        End Function
    End Class

    Public Class JoinFilter
        Implements IFilter

        Friend _e1 As Pair(Of Type, String)
        Friend _t1 As Pair(Of OrmTable, String)

        Friend _e2 As Pair(Of Type, String)
        Friend _t2 As Pair(Of OrmTable, String)

        Friend _oper As FilterOperation

        Public Sub New(ByVal t As Type, ByVal fieldName As String, ByVal t2 As Type, ByVal fieldName2 As String, ByVal operation As FilterOperation)
            _e1 = New Pair(Of Type, String)(t, fieldName)
            _e2 = New Pair(Of Type, String)(t2, fieldName2)
            _oper = operation
        End Sub

        Public Sub New(ByVal table As OrmTable, ByVal column As String, ByVal t2 As Type, ByVal FieldName2 As String, ByVal operation As FilterOperation)
            _t1 = New Pair(Of OrmTable, String)(table, column)
            _e2 = New Pair(Of Type, String)(t2, FieldName2)
            _oper = operation
        End Sub

        Public Sub New(ByVal table As OrmTable, ByVal column As String, ByVal table2 As OrmTable, ByVal column2 As String, ByVal operation As FilterOperation)
            _t1 = New Pair(Of OrmTable, String)(table, column)
            _t2 = New Pair(Of OrmTable, String)(table2, column2)
            _oper = operation
        End Sub

        'Public Function GetStaticString() As String Implements IFilter.GetStaticString
        '    Throw New NotSupportedException
        'End Function

        Public Function MakeSQLStmt(ByVal schema As OrmSchemaBase, ByVal tableAliases As System.Collections.Generic.IDictionary(Of OrmTable, String), ByVal pname As ICreateParam) As String Implements IFilter.MakeSQLStmt
            Dim map As MapField2Column = Nothing
            If _e1 IsNot Nothing Then
                map = schema.GetObjectSchema(_e1.First).GetFieldColumnMap(_e1.Second)
            Else
                map = New MapField2Column(Nothing, _t1.Second, _t1.First)
            End If

            Dim map2 As MapField2Column = Nothing
            If _e2 IsNot Nothing Then
                map2 = schema.GetObjectSchema(_e2.First).GetFieldColumnMap(_e2.Second)
            Else
                map2 = New MapField2Column(Nothing, _t2.Second, _t2.First)
            End If

            Dim [alias] As String = String.Empty

            If tableAliases IsNot Nothing Then
                [alias] = tableAliases(map._tableName) & "."
            End If

            Dim alias2 As String = String.Empty
            If map2._tableName IsNot Nothing AndAlso tableAliases IsNot Nothing AndAlso tableAliases.ContainsKey(map2._tableName) Then
                alias2 = tableAliases(map2._tableName) & "."
            End If

            Return [alias] & map._columnName & TemplateBase.Oper2String(_oper) & alias2 & map2._columnName
        End Function

        Public Function ReplaceFilter(ByVal replacement As IFilter, ByVal replacer As IFilter) As IFilter Implements IFilter.ReplaceFilter
            If Not Equals(replacement) Then
                Return Nothing
            End If
            Return replacer
        End Function

        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Return Equals(TryCast(obj, JoinFilter))
        End Function

        Public Overloads Function Equals(ByVal obj As JoinFilter) As Boolean
            If obj Is Nothing Then
                Return False
            End If
            'Return ( _
            '    (Equals(_e1, obj._e1) AndAlso Equals(_e2, obj._e2)) OrElse (Equals(_e1, obj._e2) AndAlso Equals(_e2, obj._e1)) _
            '    ) OrElse ( _
            '    (Equals(_t1, obj._t1) AndAlso Equals(_t2, obj._t2)) OrElse (Equals(_t1, obj._t2) AndAlso Equals(_t2, obj._t1)) _
            '    )
            Dim v1 As Object = _e1
            Dim ve1 As Object = obj._e1
            If _e1 Is Nothing Then
                v1 = _t1
                ve1 = obj._t1
            End If

            Dim v2 As Object = _e2
            Dim ve2 As Object = obj._e2
            If v2 Is Nothing Then
                v2 = _t2
                ve2 = obj._e2
            End If

            Dim b As Boolean = (Equals(v1, ve1) AndAlso Equals(v2, ve2)) _
                OrElse (Equals(v1, ve2) AndAlso Equals(v2, ve1))
        End Function

        Protected Shared Function ChangeEntityJoinToValue(ByVal source As IFilter, ByVal t As Type, ByVal field As String, ByVal value As IFilterValue) As IFilter
            For Each _fl As IFilter In source.GetAllFilters()
                Dim fl As JoinFilter = TryCast(_fl, JoinFilter)
                If fl IsNot Nothing Then
                    Dim f As IFilter = Nothing
                    If fl._e1 IsNot Nothing AndAlso fl._e1.First Is t AndAlso fl._e1.Second = field Then
                        If fl._e2 IsNot Nothing Then
                            f = New EntityFilter(fl._e2.First, fl._e2.Second, value, fl._oper)
                        Else
                            f = New TableFilter(fl._t2.First, fl._t2.Second, value, fl._oper)
                        End If
                    ElseIf fl._e2 IsNot Nothing AndAlso fl._e2.First Is t AndAlso fl._e2.Second = field Then
                        If fl._e1 IsNot Nothing Then
                            f = New EntityFilter(fl._e1.First, fl._e1.Second, value, fl._oper)
                        Else
                            f = New TableFilter(fl._t1.First, fl._t1.Second, value, fl._oper)
                        End If
                    End If

                    If f IsNot Nothing Then
                        Return source.ReplaceFilter(fl, f)
                    End If
                End If
            Next
            Return Nothing
        End Function

        Public Shared Function ChangeEntityJoinToLiteral(ByVal source As IFilter, ByVal t As Type, ByVal field As String, ByVal literal As String) As IFilter
            Return ChangeEntityJoinToValue(source, t, field, New LiteralValue(literal))
        End Function

        Public Shared Function ChangeEntityJoinToParam(ByVal source As IFilter, ByVal t As Type, ByVal field As String, ByVal value As TypeWrap(Of Object)) As IFilter
            Return ChangeEntityJoinToValue(source, t, field, New SimpleValue(value))
        End Function

        Public Function GetAllFilters() As System.Collections.Generic.ICollection(Of IFilter) Implements IFilter.GetAllFilters
            Return New JoinFilter() {Me}
        End Function

        Private Function Equals1(ByVal f As IFilter) As Boolean Implements IFilter.Equals
            Equals(TryCast(f, JoinFilter))
        End Function

        Private Function _ToString() As String Implements IFilter.ToString
            Dim sb As New StringBuilder
            If _e1 IsNot Nothing Then
                sb.Append(_e1.First.ToString).Append(_e1.Second).Append(" - ")
            Else
                sb.Append(_t1.First.TableName).Append(_t1.Second).Append(" - ")
            End If
            If _e2 IsNot Nothing Then
                sb.Append(_e2.First.ToString).Append(_e2.Second).Append(" - ")
            Else
                sb.Append(_t2.First.TableName).Append(_t2.Second).Append(" - ")
            End If
            Return sb.ToString
        End Function

        Public Overrides Function ToString() As String
            Return _ToString()
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return _ToString.GetHashCode
        End Function
    End Class

    'Public Class OrmFilter
    '    Implements IOrmFilter

    '    Private _tbl As OrmTable
    '    Private _value As Object
    '    Private _pname As String
    '    Private _param_value As Boolean
    '    Private _ormt As Type
    '    Private _ft As FilterTemplate

    '    Public Sub New(ByVal t As Type, ByVal FieldName As String, ByVal value As TypeWrap(Of Object), ByVal operation As FilterOperation)
    '        Debug.Assert(Not GetType(OrmBase).IsAssignableFrom(value.GetType))

    '        '_t = t
    '        '_fieldname = FieldName
    '        '_oper = operation
    '        _ft = New FilterTemplate(t, FieldName, operation, True)
    '        If value IsNot Nothing Then
    '            _value = value.Value
    '        End If
    '        _param_value = False
    '    End Sub

    '    Public Sub New(ByVal t As Type, ByVal FieldName As String, ByVal literal As String, ByVal operation As FilterOperation)
    '        '_t = t
    '        '_fieldname = FieldName
    '        '_oper = operation
    '        _ft = New FilterTemplate(t, FieldName, operation, False)
    '        _value = literal
    '        _param_value = True
    '    End Sub

    '    Public Sub New(ByVal t As Type, ByVal FieldName As String, ByVal entity As OrmBase, ByVal operation As FilterOperation)
    '        '_t = t
    '        '_fieldname = FieldName
    '        '_oper = operation
    '        _ft = New FilterTemplate(t, FieldName, operation, True)
    '        If entity IsNot Nothing Then
    '            _value = entity.Identifier
    '            _ormt = entity.GetType
    '        Else
    '            _ormt = GetType(OrmBase)
    '        End If
    '        _param_value = False
    '    End Sub

    '    Public Sub New(ByVal t As Type, ByVal FieldName As String, ByVal t2 As Type, ByVal FieldName2 As String, ByVal operation As FilterOperation)
    '        '_t = t
    '        '_fieldname = FieldName
    '        '_oper = operation
    '        _ft = New FilterTemplate(t, FieldName, operation)
    '        _value = New Pair(Of Type, String)(t2, FieldName2)
    '        _param_value = False
    '    End Sub

    '    Public Sub New(ByVal table As OrmTable, ByVal column As String, ByVal value As TypeWrap(Of Object), ByVal operation As FilterOperation)
    '        '_t = Nothing
    '        '_fieldname = column
    '        '_oper = operation
    '        _ft = New FilterTemplate(Nothing, column, operation)
    '        _tbl = table
    '        If value IsNot Nothing Then
    '            _value = value.Value
    '        End If
    '        _param_value = False
    '    End Sub

    '    Public Sub New(ByVal table As OrmTable, ByVal column As String, ByVal literal As String, ByVal operation As FilterOperation)
    '        '_t = Nothing
    '        '_fieldname = column
    '        '_oper = operation
    '        _ft = New FilterTemplate(Nothing, column, operation)
    '        _tbl = table
    '        _value = literal
    '        _param_value = True
    '    End Sub

    '    Public Sub New(ByVal table As OrmTable, ByVal column As String, ByVal entity As OrmBase, ByVal operation As FilterOperation)
    '        '_t = Nothing
    '        '_fieldname = column
    '        '_oper = operation
    '        _ft = New FilterTemplate(Nothing, column, operation)
    '        _tbl = table
    '        If entity IsNot Nothing Then
    '            _value = entity.Identifier
    '            _ormt = entity.GetType
    '        Else
    '            _ormt = GetType(OrmBase)
    '        End If
    '        _param_value = False
    '    End Sub

    '    Public Sub New(ByVal table As OrmTable, ByVal column As String, ByVal t2 As Type, ByVal FieldName2 As String, ByVal operation As FilterOperation)
    '        _tbl = table
    '        '_fieldname = column
    '        '_oper = operation
    '        _ft = New FilterTemplate(Nothing, column, operation)
    '        _value = New Pair(Of Type, String)(t2, FieldName2)
    '        _param_value = False
    '    End Sub

    '    Public Sub New(ByVal table As OrmTable, ByVal column As String, ByVal table2 As OrmTable, ByVal column2 As String, ByVal operation As FilterOperation)
    '        _tbl = table
    '        '_fieldname = column
    '        '_oper = operation
    '        _ft = New FilterTemplate(Nothing, column, operation)
    '        _value = New Pair(Of OrmTable, String)(table2, column2)
    '        _param_value = False
    '    End Sub

    '    Protected ReadOnly Property _t() As Type
    '        Get
    '            Return _ft.Type
    '        End Get
    '    End Property

    '    Protected ReadOnly Property _fieldName() As String
    '        Get
    '            Return _ft.FieldName
    '        End Get
    '    End Property

    '    Public ReadOnly Property IsParamOrm() As Boolean
    '        Get
    '            Return _ormt IsNot Nothing
    '        End Get
    '    End Property

    '    Public ReadOnly Property IsValueType() As Boolean
    '        Get
    '            If _value Is Nothing Then Return False
    '            Return GetType(Pair(Of Type, String)) Is Value.GetType
    '        End Get
    '    End Property

    '    Public ReadOnly Property IsValueTable() As Boolean
    '        Get
    '            If _value Is Nothing Then Return False
    '            Return GetType(Pair(Of OrmTable, String)) Is Value.GetType
    '        End Get
    '    End Property

    '    Public ReadOnly Property IsValueComplex() As Boolean
    '        Get
    '            Return IsValueTable OrElse IsValueType
    '        End Get
    '    End Property

    '    Public ReadOnly Property IsValueLiteral() As Boolean
    '        Get
    '            Return _param_value
    '        End Get
    '    End Property

    '    Public ReadOnly Property Type() As Type
    '        Get
    '            Return _ft.Type
    '        End Get
    '    End Property

    '    Public ReadOnly Property FieldName() As String
    '        Get
    '            Return _ft.FieldName
    '        End Get
    '    End Property

    '    Public ReadOnly Property Operation() As FilterOperation
    '        Get
    '            Return _ft.Operation
    '        End Get
    '    End Property

    '    Public ReadOnly Property Value() As Object
    '        Get
    '            Return _value
    '        End Get
    '    End Property

    '    Public ReadOnly Property ValueOfType() As Pair(Of Type, String)
    '        Get
    '            Return CType(Value, Pair(Of Global.System.Type, String))
    '        End Get
    '    End Property

    '    Public ReadOnly Property ValueOfTable() As Pair(Of OrmTable, String)
    '        Get
    '            Return CType(Value, Pair(Of OrmTable, String))
    '        End Get
    '    End Property

    '    Public ReadOnly Property ValueOrm(ByVal mgr As OrmManagerBase) As OrmBase
    '        Get
    '            If Not IsParamOrm Then
    '                Throw New InvalidOperationException("Value is not Orm")
    '            End If
    '            Return mgr.CreateDBObject(CInt(_value), _ormt)
    '        End Get
    '    End Property

    '    Public Function GetRealTable(ByVal schema As OrmSchemaBase) As OrmTable
    '        Dim s As OrmTable = _tbl
    '        If _t IsNot Nothing Then
    '            s = schema.GetObjectSchema(_t).GetFieldColumnMap()(_fieldName)._tableName
    '        End If
    '        Return s
    '    End Function

    '    Public ReadOnly Property Table() As OrmTable
    '        Get
    '            Return _tbl
    '        End Get
    '    End Property

    '    Public Overrides Function ToString() As String
    '        Dim s As String = Nothing
    '        If _tbl IsNot Nothing Then
    '            s = _tbl.TableName
    '        ElseIf _t IsNot Nothing Then
    '            s = _t.Name
    '        Else
    '            Throw New InvalidOperationException("Table name must be specified")
    '        End If
    '        Dim v As String = _value.ToString
    '        'If Me.IsParamOrm Then
    '        '    v = CType(_value, OrmBase).Identifier.ToString
    '        'End If
    '        Return s & _fieldName & v & _ft.operToString
    '    End Function

    '    Public Overrides Function GetHashCode() As Integer
    '        Return ToString.GetHashCode()
    '    End Function

    '    Public Overrides Function Equals(ByVal obj As Object) As Boolean
    '        Dim o As OrmFilter = TryCast(obj, OrmFilter)
    '        Return Equals(o)
    '    End Function

    '    Public Overloads Function Equals(ByVal obj As OrmFilter) As Boolean
    '        If obj IsNot Nothing Then
    '            Return ToString.Equals(obj.ToString)
    '        Else
    '            Return False
    '        End If
    '    End Function

    '    Public Function MakeSQLStmt(ByVal schema As OrmSchemaBase, ByVal tableAliases As IDictionary(Of OrmTable, String), ByVal pname As ICreateParam) As String Implements IOrmFilter.MakeSQLStmt

    '        If schema Is Nothing Then
    '            Throw New ArgumentNullException("schema")
    '        End If

    '        Dim map As MapField2Column = Nothing
    '        If _t IsNot Nothing Then
    '            map = schema.GetObjectSchema(_t).GetFieldColumnMap()(_fieldName)
    '        Else
    '            map = New MapField2Column(String.Empty, _fieldName, _tbl)
    '        End If

    '        If IsValueComplex Then
    '            If tableAliases Is Nothing Then
    '                Throw New ArgumentNullException("tableAliases")
    '            End If

    '            Dim map2 As MapField2Column = Nothing
    '            If IsValueType Then
    '                Dim v As Pair(Of Type, String) = CType(Value, Pair(Of Global.System.Type, String))
    '                map2 = schema.GetObjectSchema(v.First).GetFieldColumnMap()(v.Second)
    '            Else
    '                Dim v As Pair(Of OrmTable, String) = CType(Value, Pair(Of OrmTable, String))
    '                map2 = New MapField2Column(String.Empty, v.Second, v.First)
    '            End If

    '            Dim [alias] As String = String.Empty

    '            If tableAliases IsNot Nothing Then
    '                [alias] = tableAliases(map._tableName) & "."
    '            End If

    '            Dim alias2 As String = String.Empty
    '            If map2._tableName IsNot Nothing AndAlso tableAliases IsNot Nothing AndAlso tableAliases.ContainsKey(map2._tableName) Then
    '                alias2 = tableAliases(map2._tableName) & "."
    '            End If

    '            Return [alias] & map._columnName & _ft.Oper2String() & alias2 & map2._columnName
    '        Else
    '            If IsValueLiteral Then
    '                Dim [alias] As String = String.Empty

    '                If tableAliases IsNot Nothing Then
    '                    [alias] = tableAliases(map._tableName) & "."
    '                End If

    '                Return [alias] & map._columnName & _ft.Oper2String() & CStr(_value)
    '            Else
    '                If pname Is Nothing Then
    '                    Throw New ArgumentNullException("pname")
    '                End If

    '                Dim [alias] As String = String.Empty

    '                If tableAliases IsNot Nothing Then
    '                    [alias] = tableAliases(map._tableName) & "."
    '                End If

    '                Dim o As Object = Value
    '                If Not IsParamOrm Then
    '                    o = ChangeValue(schema, Value)
    '                End If

    '                If String.IsNullOrEmpty(_pname) OrElse Not pname.NamedParams Then
    '                    _pname = pname.CreateParam(o)
    '                Else
    '                    _pname = pname.AddParam(_pname, o)
    '                End If

    '                Return [alias] & map._columnName & _ft.Oper2String() & _pname
    '            End If
    '        End If
    '    End Function

    '    Protected Function ChangeValue(ByVal schema As OrmSchemaBase, ByVal v As Object) As Object
    '        If _t IsNot Nothing Then
    '            Return schema.ChangeValueType(_t, schema.GetColumnByFieldName(_t, _fieldName), v)
    '        End If
    '        Return v
    '    End Function

    '    Public Function MakeSingleStmt(ByVal schema As DbSchema, ByVal pname As ICreateParam) As Pair(Of String)
    '        If schema Is Nothing Then
    '            Throw New ArgumentNullException("schema")
    '        End If

    '        If pname Is Nothing Then
    '            Throw New ArgumentNullException("pname")
    '        End If

    '        Dim prname As String = String.Empty
    '        If IsValueLiteral Then
    '            prname = CStr(_value)
    '        Else
    '            Dim o As Object = Value
    '            If Not IsParamOrm Then
    '                o = ChangeValue(schema, Value)
    '            End If

    '            If String.IsNullOrEmpty(_pname) OrElse Not pname.NamedParams Then
    '                prname = pname.CreateParam(o)
    '            Else
    '                prname = _pname
    '            End If
    '        End If

    '        If _t IsNot Nothing Then
    '            Dim map As MapField2Column = schema.GetObjectSchema(_t).GetFieldColumnMap()(_fieldName)
    '            If _value Is DBNull.Value Then
    '                If schema.GetFieldTypeByName(_t, _fieldName) Is GetType(Byte()) Then
    '                    pname.GetParameter(prname).DbType = System.Data.DbType.Binary
    '                ElseIf schema.GetFieldTypeByName(_t, _fieldName) Is GetType(Decimal) Then
    '                    pname.GetParameter(prname).DbType = System.Data.DbType.Decimal
    '                End If
    '            End If
    '            Return New Pair(Of String)(map._columnName, prname)
    '        Else
    '            Return New Pair(Of String)(_fieldName, prname)
    '        End If
    '    End Function

    '    Public Function GetAllFilters() As System.Collections.Generic.ICollection(Of OrmFilter) Implements IOrmFilter.GetAllFilters
    '        'Dim l As New List(Of OrmFilter)
    '        'l.Add(Me)
    '        'Return l
    '        Return New OrmFilter() {Me}
    '    End Function

    '    Public Function ReplaceFilter(ByVal replacement As IOrmFilter, ByVal replacer As IOrmFilter) As IOrmFilter Implements IOrmFilter.ReplaceFilter
    '        If Not Equals(replacement) Then
    '            Return Nothing 'Throw New ArgumentException("invalid filter", "replacement")
    '        End If
    '        Return replacer
    '    End Function

    '    Public Function GetStaticString() As String Implements IOrmFilter.GetStaticString
    '        Dim tbl As String = String.Empty
    '        If _t IsNot Nothing Then
    '            tbl = _t.Name
    '        ElseIf _tbl IsNot Nothing Then
    '            tbl = _tbl.TableName
    '        Else
    '            Throw New InvalidOperationException("Table name must be specified")
    '        End If
    '        Return tbl & _fieldName & _ft.Oper2String()
    '    End Function

    '    Public Function Eval(ByVal schema As OrmSchemaBase, ByVal obj As OrmBase, ByVal oschema As IOrmObjectSchemaBase) As IOrmFilter.EvalResult Implements IOrmFilter.Eval
    '        If schema Is Nothing Then
    '            Throw New ArgumentNullException("schema")
    '        End If

    '        If obj Is Nothing Then
    '            Throw New ArgumentNullException("o")
    '        End If

    '        Dim r As IOrmFilter.EvalResult = IOrmFilter.EvalResult.NotFound

    '        If _t IsNot Nothing AndAlso Not (IsValueLiteral Or IsValueComplex) Then
    '            Dim t As Type = obj.GetType
    '            If _t Is t Then
    '                Dim v As Object = obj.GetValue(_fieldName, oschema) 'schema.GetFieldValue(obj, _fieldname)
    '                If v IsNot Nothing Then
    '                    Dim tt As Type = v.GetType
    '                    Dim orm As OrmBase = TryCast(v, OrmBase)
    '                    Dim b As Boolean = orm IsNot Nothing
    '                    If b Then
    '                        If Not IsParamOrm Then
    '                            Return IOrmFilter.EvalResult.NotFound
    '                        End If
    '                        If tt IsNot _ormt Then
    '                            Return IOrmFilter.EvalResult.NotFound
    '                        End If
    '                    End If

    '                    Select Case _ft.Operation
    '                        Case FilterOperation.Equal
    '                            If Equals(v, _value) Then
    '                                r = IOrmFilter.EvalResult.Found
    '                            End If
    '                        Case FilterOperation.GreaterEqualThan
    '                            Dim i As Integer = CType(v, IComparable).CompareTo(_value)
    '                            If i >= 0 Then
    '                                r = IOrmFilter.EvalResult.Found
    '                            End If
    '                        Case FilterOperation.GreaterThan
    '                            Dim i As Integer = CType(v, IComparable).CompareTo(_value)
    '                            If i > 0 Then
    '                                r = IOrmFilter.EvalResult.Found
    '                            End If
    '                        Case FilterOperation.LessEqualThan
    '                            Dim i As Integer = CType(v, IComparable).CompareTo(_value)
    '                            If i <= 0 Then
    '                                r = IOrmFilter.EvalResult.Found
    '                            End If
    '                        Case FilterOperation.LessThan
    '                            Dim i As Integer = CType(v, IComparable).CompareTo(_value)
    '                            If i < 0 Then
    '                                r = IOrmFilter.EvalResult.Found
    '                            End If
    '                        Case FilterOperation.NotEqual
    '                            If Not Equals(v, _value) Then
    '                                r = IOrmFilter.EvalResult.Found
    '                            End If
    '                        Case Else
    '                            r = IOrmFilter.EvalResult.Unknown
    '                    End Select
    '                Else
    '                    If _value Is Nothing Then
    '                        r = IOrmFilter.EvalResult.Found
    '                    End If
    '                End If
    '            End If
    '        End If
    '        Return r
    '    End Function

    '    Public Shared Sub ChangeValueToLiteral(ByRef join As OrmJoin, ByVal t As Type, ByVal field As String, ByVal table As OrmTable, ByVal literal As String)
    '        Dim f As IOrmFilter = Nothing
    '        For Each fl As OrmFilter In join.Condition.GetAllFilters()
    '            If fl.IsValueComplex AndAlso fl.Type Is t AndAlso fl.FieldName = field Then
    '                If fl.IsValueTable Then
    '                    f = New OrmFilter(table, literal, fl.ValueOfTable.First, fl.ValueOfTable.Second, fl.Operation)
    '                Else
    '                    f = New OrmFilter(table, literal, fl.ValueOfType.First, fl.ValueOfType.Second, fl.Operation)
    '                End If
    '            End If
    '            If f Is Nothing Then
    '                If fl.IsValueType AndAlso fl.ValueOfType.First Is t AndAlso fl.ValueOfType.Second = field Then
    '                    If fl.Table Is Nothing Then
    '                        f = New OrmFilter(table, literal, fl.Type, fl.FieldName, fl.Operation)
    '                    Else
    '                        f = New OrmFilter(table, literal, fl.Table, fl.FieldName, fl.Operation)
    '                    End If
    '                End If
    '            End If

    '            If f IsNot Nothing Then
    '                join.ReplaceFilter(fl, f)
    '            End If
    '        Next
    '    End Sub

    '    Public Shared Function ChangeValueToLiteral(ByVal source As IOrmFilter, ByVal t As Type, ByVal field As String, ByVal literal As String) As IOrmFilter
    '        Dim f As IOrmFilter = Nothing
    '        For Each fl As OrmFilter In source.GetAllFilters()
    '            If fl.IsValueComplex AndAlso fl.Type Is t AndAlso fl.FieldName = field Then
    '                If fl.IsValueTable Then
    '                    f = New OrmFilter(fl.ValueOfTable.First, fl.ValueOfTable.Second, literal, fl.Operation)
    '                Else
    '                    f = New OrmFilter(fl.ValueOfType.First, fl.ValueOfType.Second, literal, fl.Operation)
    '                End If
    '            End If
    '            If f Is Nothing Then
    '                If fl.IsValueType AndAlso fl.ValueOfType.First Is t AndAlso fl.ValueOfType.Second = field Then
    '                    If fl.Table Is Nothing Then
    '                        f = New OrmFilter(fl.Type, fl.FieldName, literal, fl.Operation)
    '                    Else
    '                        f = New OrmFilter(fl.Table, fl.FieldName, literal, fl.Operation)
    '                    End If
    '                End If
    '            End If

    '            If f IsNot Nothing Then
    '                Return source.ReplaceFilter(fl, f)
    '            End If
    '        Next
    '        Return Nothing
    '    End Function

    '    Public Shared Function ChangeValueToParam(ByVal source As IOrmFilter, ByVal t As Type, ByVal field As String, ByVal value As Object) As IOrmFilter
    '        Dim f As IOrmFilter = Nothing
    '        For Each fl As OrmFilter In source.GetAllFilters()
    '            If fl.IsValueComplex AndAlso fl.Type Is t AndAlso fl.FieldName = field Then
    '                If fl.IsValueTable Then
    '                    f = New OrmFilter(fl.ValueOfTable.First, fl.ValueOfTable.Second, New TypeWrap(Of Object)(value), fl.Operation)
    '                Else
    '                    f = New OrmFilter(fl.ValueOfType.First, fl.ValueOfType.Second, New TypeWrap(Of Object)(value), fl.Operation)
    '                End If
    '            End If
    '            If f Is Nothing Then
    '                If fl.IsValueType AndAlso fl.ValueOfType.First Is t AndAlso fl.ValueOfType.Second = field Then
    '                    If fl.Table Is Nothing Then
    '                        f = New OrmFilter(fl.Type, fl.FieldName, New TypeWrap(Of Object)(value), fl.Operation)
    '                    Else
    '                        f = New OrmFilter(fl.Table, fl.FieldName, New TypeWrap(Of Object)(value), fl.Operation)
    '                    End If
    '                End If
    '            End If

    '            If f IsNot Nothing Then
    '                Return source.ReplaceFilter(fl, f)
    '            End If
    '        Next
    '        Return Nothing
    '    End Function
    'End Class ' OrmFilter

    Public Class Condition
        Implements ITemplateFilter

        Public Class ConditionConstructor
            Private _cond As Condition

            Public Function AddFilter(ByVal f As IFilter, ByVal [operator] As ConditionOperator) As ConditionConstructor
                Return AddFilter(f, [operator], True)
            End Function

            Protected Function AddFilter(ByVal f As IFilter, ByVal [operator] As ConditionOperator, ByVal useOper As Boolean) As ConditionConstructor
                If _cond Is Nothing AndAlso f IsNot Nothing Then
                    If TypeOf (f) Is EntityFilter Then
                        _cond = New EntityCondition(CType(f, IEntityFilter), Nothing, [operator])
                    Else
                        _cond = New Condition(f, Nothing, [operator])
                    End If
                ElseIf _cond IsNot Nothing AndAlso _cond._right Is Nothing AndAlso f IsNot Nothing Then
                    If Not TypeOf (f) Is EntityFilter AndAlso TypeOf (_cond) Is EntityCondition Then
                        _cond = New Condition(_cond, f, [operator])
                    Else
                        _cond._right = f
                        If useOper Then
                            _cond._oper = [operator]
                        End If
                    End If
                ElseIf f IsNot Nothing Then
                    If TypeOf (f) Is EntityFilter AndAlso TypeOf (_cond) Is EntityCondition Then
                        _cond = New EntityCondition(CType(_cond, IEntityFilter), CType(f, IEntityFilter), [operator])
                    Else
                        _cond = New Condition(_cond, f, [operator])
                    End If
                End If

                Return Me
            End Function

            Public Function AddFilter(ByVal f As IFilter) As ConditionConstructor
                Return AddFilter(f, ConditionOperator.And, False)
            End Function

            Public ReadOnly Property Condition() As IFilter
                Get
                    If _cond Is Nothing Then
                        Return Nothing
                    ElseIf _cond._right Is Nothing Then
                        Return _cond._left
                    Else
                        Return _cond
                    End If
                End Get
            End Property

            Public ReadOnly Property IsEmpty() As Boolean
                Get
                    Return _cond Is Nothing
                End Get
            End Property
        End Class

        Protected _left As IFilter
        Protected _right As IFilter
        Protected _oper As ConditionOperator

        Public Sub New(ByVal left As IFilter, ByVal right As IFilter, ByVal [operator] As ConditionOperator)
            _left = left
            _right = right
            _oper = [operator]
        End Sub

        Public Function GetAllFilters() As System.Collections.Generic.ICollection(Of IFilter) Implements IFilter.GetAllFilters
            Dim res As ICollection(Of IFilter) = _left.GetAllFilters

            If _right IsNot Nothing Then
                Dim l As New List(Of IFilter)
                l.AddRange(res)
                l.AddRange(_right.GetAllFilters)
                res = l
            End If

            Return res
        End Function

        Public Function GetStaticString() As String Implements ITemplateFilter.GetStaticString
            Dim r As String = String.Empty
            If _right IsNot Nothing Then
                Dim rt As ITemplateFilter = CType(_right, ITemplateFilter)
                r = rt.GetStaticString
            End If

            Dim lt As ITemplateFilter = CType(_left, ITemplateFilter)
            Return lt.GetStaticString & Condition2String() & r
        End Function

        Public Function MakeSQLStmt(ByVal schema As OrmSchemaBase, ByVal tableAliases As IDictionary(Of OrmTable, String), ByVal pname As ICreateParam) As String Implements IFilter.MakeSQLStmt
            If _right Is Nothing Then
                Return _left.MakeSQLStmt(schema, tableAliases, pname)
            End If

            Return "(" & _left.MakeSQLStmt(schema, tableAliases, pname) & Condition2String() & _right.MakeSQLStmt(schema, tableAliases, pname) & ")"
        End Function

        Private Function _ReplaceTemplate(ByVal replacement As ITemplateFilter, ByVal replacer As ITemplateFilter) As ITemplateFilter Implements ITemplateFilter.ReplaceByTemplate
            Return ReplaceCondition(replacement, replacer)
        End Function

        Public Overridable Function ReplaceCondition(ByVal replacement As ITemplateFilter, ByVal replacer As ITemplateFilter) As Condition
            If replacement.Template.Equals(CType(_left, ITemplateFilter).Template) Then
                Return New Condition(replacer, _right, _oper)
            ElseIf replacement.Template.Equals(CType(_right, ITemplateFilter).Template) Then
                Return New Condition(_left, replacer, _oper)
            Else
                Dim r As ITemplateFilter = CType(_left, ITemplateFilter).ReplaceByTemplate(replacement, replacer)

                If r IsNot Nothing Then
                    Return New Condition(r, _right, _oper)
                Else
                    r = CType(_right, ITemplateFilter).ReplaceByTemplate(replacement, replacer)

                    If r IsNot Nothing Then
                        Return New Condition(_left, r, _oper)
                    End If
                End If
            End If

            Return Nothing
        End Function

        Public Overrides Function ToString() As String
            Return _ToString()
        End Function

        Public Overrides Function GetHashCode() As Integer
            Return _ToString.GetHashCode
        End Function

        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Return Equals(TryCast(obj, Condition))
        End Function

        Public Overloads Function Equals(ByVal con As Condition) As Boolean
            If con Is Nothing Then
                Return False
            End If

            Dim r As Boolean = True

            If _right IsNot Nothing Then
                r = _right.Equals(con._right)
            End If

            Return _left.Equals(con._left) AndAlso r AndAlso _oper = con._oper
        End Function

        Private Function Condition2String() As String
            If _oper = ConditionOperator.And Then
                Return " and "
            Else
                Return " or "
            End If
        End Function

        Private Function Equals1(ByVal f As IFilter) As Boolean Implements IFilter.Equals
            Return Equals(TryCast(f, Condition))
        End Function

        Public Overridable ReadOnly Property Template() As TemplateBase Implements ITemplateFilter.Template
            Get
                Throw New NotImplementedException
            End Get
        End Property

        Public Function ReplaceFilter(ByVal replacement As IFilter, ByVal replacer As IFilter) As IFilter Implements IFilter.ReplaceFilter
            If replacement.Equals(_left) Then
                Return New Condition(replacer, _right, _oper)
            ElseIf replacement.Equals(_right) Then
                Return New Condition(_left, replacer, _oper)
            Else
                Dim r As IFilter = _left.ReplaceFilter(replacement, replacer)

                If r IsNot Nothing Then
                    Return New Condition(r, _right, _oper)
                Else
                    r = _right.ReplaceFilter(replacement, replacer)

                    If r IsNot Nothing Then
                        Return New Condition(_left, r, _oper)
                    End If
                End If
            End If

            Return Nothing
        End Function

        Public Function MakeSingleStmt(ByVal schema As DbSchema, ByVal pname As ICreateParam) As CoreFramework.Structures.Pair(Of String) Implements ITemplateFilter.MakeSingleStmt
            Throw New NotSupportedException
        End Function

        Private Function _ToString() As String Implements IFilter.ToString
            Dim r As String = String.Empty
            If _right IsNot Nothing Then
                r = _right.ToString()
            End If

            Return _left.ToString() & Condition2String() & r
        End Function
    End Class

    Friend Class EntityCondition
        Inherits Condition
        Implements IEntityFilter

        Class ConditionTemplate
            Inherits TemplateBase
            Implements IOrmFilterTemplate

            Private _con As EntityCondition

            Public Sub New(ByVal con As EntityCondition)
                _con = con
            End Sub

            Public Function MakeFilter(ByVal schema As OrmSchemaBase, ByVal obj As OrmBase) As IEntityFilter Implements IOrmFilterTemplate.MakeFilter

            End Function

            Public Sub SetType(ByVal t As System.Type) Implements IOrmFilterTemplate.SetType

            End Sub

            Public Overrides Function GetStaticString() As String

            End Function
        End Class

        Public Sub New(ByVal left As IEntityFilter, ByVal right As IEntityFilter, ByVal [operator] As ConditionOperator)
            MyBase.New(left, right, [operator])
        End Sub

        Protected ReadOnly Property Left() As IEntityFilter
            Get
                Return CType(_left, IEntityFilter)
            End Get
        End Property

        Protected ReadOnly Property Right() As IEntityFilter
            Get
                Return CType(_right, IEntityFilter)
            End Get
        End Property

        Public Function Eval(ByVal schema As OrmSchemaBase, ByVal obj As OrmBase, ByVal oschema As IOrmObjectSchemaBase) As IEntityFilter.EvalResult Implements IEntityFilter.Eval
            If schema Is Nothing Then
                Throw New ArgumentNullException("schema")
            End If

            If obj Is Nothing Then
                Throw New ArgumentNullException("obj")
            End If

            Dim b As IEntityFilter.EvalResult = Left.Eval(schema, obj, oschema)
            If _right IsNot Nothing Then
                If _oper = ConditionOperator.And Then
                    If b = IEntityFilter.EvalResult.Found Then
                        b = Right.Eval(schema, obj, oschema)
                    End If
                ElseIf _oper = ConditionOperator.Or Then
                    If b <> IEntityFilter.EvalResult.Unknown Then
                        Dim r As IEntityFilter.EvalResult = Right.Eval(schema, obj, oschema)
                        If r <> IEntityFilter.EvalResult.Unknown Then
                            If b <> IEntityFilter.EvalResult.Found Then
                                b = r
                            End If
                        Else
                            b = r
                        End If
                    End If
                End If
            End If
            Return b
        End Function

        Public Function GetFilterTemplate() As IOrmFilterTemplate Implements IEntityFilter.GetFilterTemplate
            Return CType(Template, IOrmFilterTemplate)
        End Function

        Public Function PrepareValue(ByVal schema As OrmSchemaBase, ByVal v As Object) As Object Implements IEntityFilter.PrepareValue
            Throw New NotSupportedException
        End Function

        Public Overrides Function ReplaceCondition(ByVal replacement As ITemplateFilter, ByVal replacer As ITemplateFilter) As Condition
            If replacement.Equals(_left) Then
                If TypeOf (replacer) Is EntityFilter Then
                    Return New EntityCondition(CType(replacer, IEntityFilter), CType(_right, IEntityFilter), _oper)
                Else
                    Return New Condition(replacer, _right, _oper)
                End If
            ElseIf replacement.Equals(_right) Then
                If TypeOf (replacer) Is EntityFilter Then
                    Return New EntityCondition(CType(_left, IEntityFilter), CType(replacer, IEntityFilter), _oper)
                Else
                    Return New Condition(_left, replacer, _oper)
                End If
            Else
                Dim r As IFilter = _left.ReplaceFilter(replacement, replacer)

                If r IsNot Nothing Then
                    Return New EntityCondition(CType(r, IEntityFilter), CType(_right, IEntityFilter), _oper)
                Else
                    r = _right.ReplaceFilter(replacement, replacer)

                    If r IsNot Nothing Then
                        Return New EntityCondition(CType(_left, IEntityFilter), CType(r, IEntityFilter), _oper)
                    End If
                End If
            End If

            Return Nothing
        End Function

        Public Overrides ReadOnly Property Template() As TemplateBase
            Get
                Return New ConditionTemplate(Me)
            End Get
        End Property
    End Class

    'Public Class OrmCondition
    '    Implements IOrmFilter

    '    Private _left As IOrmFilter
    '    Private _right As IOrmFilter
    '    Private _oper As ConditionOperator

    '    Public Sub New(ByVal left As IOrmFilter, ByVal right As IOrmFilter, ByVal [operator] As ConditionOperator)
    '        _left = left
    '        _right = right
    '        _oper = [operator]
    '    End Sub

    '    Public Overrides Function ToString() As String

    '        Dim r As String = String.Empty
    '        If _right IsNot Nothing Then
    '            r = CObj(_right).ToString()
    '        End If

    '        Return CObj(_left).ToString() & Condition2String() & r
    '    End Function

    '    Public Overrides Function GetHashCode() As Integer
    '        Return ToString.GetHashCode
    '    End Function

    '    Public Overrides Function Equals(ByVal obj As Object) As Boolean
    '        If obj.GetType IsNot GetType(OrmCondition) Then Return False
    '        Return Equals(CType(obj, OrmCondition))
    '    End Function

    '    Public Overloads Function Equals(ByVal con As OrmCondition) As Boolean
    '        If con Is Nothing Then
    '            Return False
    '        End If

    '        Dim r As Boolean = True

    '        If _right IsNot Nothing Then
    '            r = CObj(_right).Equals(con._right)
    '        End If

    '        Return CObj(_left).Equals(con._left) AndAlso r AndAlso _oper = con._oper
    '    End Function

    '    Public Function MakeSQLStmt(ByVal schema As OrmSchemaBase, ByVal tableAliases As IDictionary(Of OrmTable, String), ByVal pname As ICreateParam) As String Implements IOrmFilter.MakeSQLStmt

    '        If _right Is Nothing Then
    '            Return _left.MakeSQLStmt(schema, tableAliases, pname)
    '        End If

    '        Return "(" & _left.MakeSQLStmt(schema, tableAliases, pname) & Condition2String() & _right.MakeSQLStmt(schema, tableAliases, pname) & ")"
    '    End Function

    '    Private Function Condition2String() As String
    '        If _oper = ConditionOperator.And Then
    '            Return " and "
    '        Else
    '            Return " or "
    '        End If
    '    End Function

    '    'Public ReadOnly Property Left() As IOrmFilter
    '    '    Get
    '    '        Return _left
    '    '    End Get
    '    'End Property

    '    'Public ReadOnly Property Right() As IOrmFilter
    '    '    Get
    '    '        Return _right
    '    '    End Get
    '    'End Property

    '    Public Function GetAllFilters() As System.Collections.Generic.ICollection(Of OrmFilter) Implements IOrmFilter.GetAllFilters

    '        Dim res As ICollection(Of OrmFilter) = _left.GetAllFilters

    '        If _right IsNot Nothing Then
    '            Dim l As New List(Of OrmFilter)
    '            l.AddRange(res)
    '            l.AddRange(_right.GetAllFilters)
    '            res = l
    '        End If

    '        Return res
    '    End Function

    '    Private Function _ReplaceFilter(ByVal replacement As IOrmFilter, ByVal replacer As IOrmFilter) As IOrmFilter Implements IOrmFilter.ReplaceFilter
    '        Return ReplaceFilter(replacement, replacer)
    '    End Function

    '    Public Function ReplaceFilter(ByVal replacement As IOrmFilter, ByVal replacer As IOrmFilter) As OrmCondition

    '        If replacement.GetStaticString.Equals(_left.GetStaticString) Then
    '            Return New OrmCondition(replacer, _right, _oper)
    '        ElseIf replacement.GetStaticString.Equals(_right.GetStaticString) Then
    '            Return New OrmCondition(_left, replacer, _oper)
    '        Else
    '            Dim r As IOrmFilter = _left.ReplaceFilter(replacement, replacer)

    '            If r IsNot Nothing Then
    '                Return New OrmCondition(r, _right, _oper)
    '            Else
    '                r = _right.ReplaceFilter(replacement, replacer)

    '                If r IsNot Nothing Then
    '                    Return New OrmCondition(_left, r, _oper)
    '                End If
    '            End If
    '        End If

    '        Return Nothing
    '    End Function

    '    Public Class OrmConditionConstructor
    '        Private _cond As OrmCondition

    '        Public Function AddFilter(ByVal f As IOrmFilter, ByVal [operator] As ConditionOperator) As OrmConditionConstructor
    '            If _cond Is Nothing AndAlso f IsNot Nothing Then
    '                _cond = New OrmCondition(f, Nothing, [operator])
    '            ElseIf _cond IsNot Nothing AndAlso _cond._right Is Nothing AndAlso f IsNot Nothing Then
    '                _cond._right = f
    '                _cond._oper = [operator]
    '            ElseIf f IsNot Nothing Then
    '                _cond = New OrmCondition(_cond, f, [operator])
    '            End If

    '            Return Me
    '        End Function

    '        Public Function AddFilter(ByVal f As IOrmFilter) As OrmConditionConstructor
    '            If _cond Is Nothing AndAlso f IsNot Nothing Then
    '                _cond = New OrmCondition(f, Nothing, ConditionOperator.And)
    '            ElseIf _cond IsNot Nothing AndAlso _cond._right Is Nothing AndAlso f IsNot Nothing Then
    '                _cond._right = f
    '            ElseIf f IsNot Nothing Then
    '                _cond = New OrmCondition(_cond, f, ConditionOperator.And)
    '            End If

    '            Return Me
    '        End Function

    '        Public ReadOnly Property Condition() As IOrmFilter
    '            Get
    '                If _cond Is Nothing Then
    '                    Return Nothing
    '                ElseIf _cond._right Is Nothing Then
    '                    Return _cond._left
    '                Else
    '                    Return _cond
    '                End If
    '            End Get
    '        End Property

    '        Public ReadOnly Property IsEmpty() As Boolean
    '            Get
    '                Return _cond Is Nothing
    '            End Get
    '        End Property
    '    End Class

    '    Public Function GetStaticString() As String Implements IOrmFilter.GetStaticString
    '        Dim r As String = String.Empty
    '        If _right IsNot Nothing Then
    '            r = _right.GetStaticString
    '        End If

    '        Return _left.GetStaticString & Condition2String() & r
    '    End Function

    '    Public Function Eval(ByVal schema As OrmSchemaBase, ByVal obj As OrmBase, ByVal oschema As IOrmObjectSchemaBase) As IOrmFilter.EvalResult Implements IOrmFilter.Eval
    '        If schema Is Nothing Then
    '            Throw New ArgumentNullException("schema")
    '        End If

    '        If obj Is Nothing Then
    '            Throw New ArgumentNullException("obj")
    '        End If

    '        Dim b As IOrmFilter.EvalResult = _left.Eval(schema, obj, oschema)
    '        If _right IsNot Nothing Then
    '            If _oper = ConditionOperator.And Then
    '                If b = IOrmFilter.EvalResult.Found Then
    '                    b = _right.Eval(schema, obj, oschema)
    '                End If
    '            ElseIf _oper = ConditionOperator.Or Then
    '                If b <> IOrmFilter.EvalResult.Unknown Then
    '                    Dim r As IOrmFilter.EvalResult = _right.Eval(schema, obj, oschema)
    '                    If r <> IOrmFilter.EvalResult.Unknown Then
    '                        If b <> IOrmFilter.EvalResult.Found Then
    '                            b = r
    '                        End If
    '                    Else
    '                        b = r
    '                    End If
    '                End If
    '            End If
    '        End If
    '        Return b
    '    End Function

    'End Class ' OrmCondition

    Public Structure OrmJoin
        Private _table As OrmTable
        Private _joinType As JoinType
        Private _condition As IFilter

        Public Sub New(ByVal Table As OrmTable, ByVal joinType As JoinType, ByVal condition As IFilter)
            _table = Table
            _joinType = JoinType
            _condition = condition
        End Sub

        Public Function MakeSQLStmt(ByVal schema As DbSchema, ByVal tableAliases As IDictionary(Of OrmTable, String), ByVal pname As ICreateParam) As String
            If IsEmpty Then
                Throw New InvalidOperationException("Object must be created")
            End If

            If _condition Is Nothing Then
                Throw New InvalidOperationException("Join condition must be specified")
            End If

            If tableAliases Is Nothing Then
                Throw New ArgumentNullException("tableAliases")
            End If

            'Dim table As String = _table
            'Dim sch as IOrmObjectSchema = schema.GetObjectSchema(
            Return JoinTypeString() & _table.TableName & " " & tableAliases(_table) & " on " & _condition.MakeSQLStmt(schema, tableAliases, pname)
        End Function

        Public ReadOnly Property IsEmpty() As Boolean
            Get
                Return _table Is Nothing
            End Get
        End Property

        Private Function JoinTypeString() As String
            Select Case _joinType
                Case JoinType.Join
                    Return " join "
                Case JoinType.LeftOuterJoin
                    Return " left join "
                Case JoinType.RightOuterJoin
                    Return " right join "
                Case JoinType.FullJoin
                    Return " full join "
                Case JoinType.CrossJoin
                    Return " cross join "
                Case Else
                    Throw New DBSchemaException("invalid join type " & _joinType.ToString)
            End Select
        End Function

        Public ReadOnly Property Condition() As IFilter
            Get
                Return _condition
            End Get
        End Property

        Public Sub ReplaceFilter(ByVal replacement As IFilter, ByVal replacer As IFilter)
            _condition = _condition.ReplaceFilter(replacement, replacer)
        End Sub

        'Public Function GetStaticString() As String
        '    Return _table.TableName & JoinTypeString() & _condition.GetStaticString
        'End Function

        Public Overrides Function ToString() As String
            Return _table.TableName & JoinTypeString() & _condition.ToString
        End Function

        Public ReadOnly Property Table() As OrmTable
            Get
                Return _table
            End Get
        End Property

        Public Sub InjectJoinFilter(ByVal t As Type, ByVal field As String, ByVal table As OrmTable, ByVal column As String)
            For Each _fl As IFilter In _condition.GetAllFilters()
                Dim f As JoinFilter = Nothing
                Dim fl As JoinFilter = TryCast(_fl, JoinFilter)
                If fl._e1 IsNot Nothing AndAlso fl._e1.First Is t AndAlso fl._e1.Second = field Then
                    If fl._e2 IsNot Nothing Then
                        f = New JoinFilter(table, column, fl._e2.First, fl._e2.Second, fl._oper)
                    Else
                        f = New JoinFilter(table, column, fl._t2.First, fl._t2.Second, fl._oper)
                    End If
                End If
                If f Is Nothing Then
                    If fl._e2 IsNot Nothing AndAlso fl._e2.First Is t AndAlso fl._e2.Second = field Then
                        If fl._e1 IsNot Nothing Then
                            f = New JoinFilter(table, column, fl._e1.First, fl._e1.Second, fl._oper)
                        Else
                            f = New JoinFilter(table, column, fl._t1.First, fl._t1.Second, fl._oper)
                        End If
                    End If
                End If

                If f IsNot Nothing Then
                    ReplaceFilter(fl, f)
                    Exit For
                End If
            Next
        End Sub

    End Structure

End Namespace