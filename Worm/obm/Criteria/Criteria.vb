Namespace Orm

    Public Class Criteria
        Private _t As Type

        Public Sub New(ByVal t As Type)
            If t Is Nothing Then
                Throw New ArgumentNullException("t")
            End If

            _t = t
        End Sub

        Public Function Field(ByVal fieldName As String) As CriteriaField
            If String.IsNullOrEmpty(fieldName) Then
                Throw New ArgumentNullException("fieldName")
            End If

            Return New CriteriaField(_t, fieldName)
        End Function

        Public Shared Function Field(ByVal t As Type, ByVal fieldName As String) As CriteriaField
            If t Is Nothing Then
                Throw New ArgumentNullException("t")
            End If

            If String.IsNullOrEmpty(fieldName) Then
                Throw New ArgumentNullException("fieldName")
            End If

            Return New CriteriaField(t, fieldName)
        End Function

        Public Shared Function AutoTypeField(ByVal fieldName As String) As CriteriaField
            If String.IsNullOrEmpty(fieldName) Then
                Throw New ArgumentNullException("fieldName")
            End If

            Return New CriteriaField(Nothing, fieldName)
        End Function
    End Class

    Public Class CriteriaField
        Private _t As Type
        Private _f As String
        Private _con As Orm.Condition.ConditionConstructor
        Private _ct As ConditionOperator

        Protected Friend Sub New(ByVal t As Type, ByVal fieldName As String)
            'If t Is Nothing Then
            '    Throw New ArgumentNullException("t")
            'End If

            'If String.IsNullOrEmpty(fieldName) Then
            '    Throw New ArgumentNullException("fieldName")
            'End If

            _t = t
            _f = fieldName
        End Sub

        Protected Friend Sub New(ByVal t As Type, ByVal fieldName As String, _
            ByVal con As Orm.Condition.ConditionConstructor, ByVal ct As ConditionOperator)
            _t = t
            _f = fieldName
            _con = con
            _ct = ct
        End Sub

        Protected Function GetLink(ByVal fl As Orm.IEntityFilter) As CriteriaLink
            If _con Is Nothing Then
                _con = New Orm.Condition.ConditionConstructor
            End If
            _con.AddFilter(fl, _ct)
            Return New CriteriaLink(_t, _con)
        End Function

        Public Function Eq(ByVal value As Object) As CriteriaLink
            Return GetLink(New EntityFilter(_t, _f, New SimpleValue(value), FilterOperation.Equal))
        End Function

        Public Function NotEq(ByVal value As Object) As CriteriaLink
            Return GetLink(New EntityFilter(_t, _f, New SimpleValue(value), FilterOperation.NotEqual))
        End Function

        Public Function Eq(ByVal value As OrmBase) As CriteriaLink
            Return GetLink(New EntityFilter(_t, _f, New EntityValue(value), FilterOperation.Equal))
        End Function

        Public Function NotEq(ByVal value As OrmBase) As CriteriaLink
            Return GetLink(New EntityFilter(_t, _f, New EntityValue(value), FilterOperation.NotEqual))
        End Function

        Public Function GreaterThanEq(ByVal value As Object) As CriteriaLink
            Return GetLink(New EntityFilter(_t, _f, New SimpleValue(value), FilterOperation.GreaterEqualThan))
        End Function

        Public Function LessThanEq(ByVal value As Object) As CriteriaLink
            Return GetLink(New EntityFilter(_t, _f, New SimpleValue(value), FilterOperation.LessEqualThan))
        End Function

        Public Function GreaterThan(ByVal value As Object) As CriteriaLink
            Return GetLink(New EntityFilter(_t, _f, New SimpleValue(value), FilterOperation.GreaterThan))
        End Function

        Public Function LessThan(ByVal value As Object) As CriteriaLink
            Return GetLink(New EntityFilter(_t, _f, New SimpleValue(value), FilterOperation.LessThan))
        End Function

        Public Function [Like](ByVal value As String) As CriteriaLink
            Return GetLink(New EntityFilter(_t, _f, New SimpleValue(value), FilterOperation.Like))
        End Function

        Public Function Op(ByVal oper As FilterOperation, ByVal value As Object) As CriteriaLink
            Return GetLink(New EntityFilter(_t, _f, New SimpleValue(value), oper))
        End Function
    End Class

    Public Class CriteriaLink
        Private _con As Orm.Condition.ConditionConstructor
        Private _t As Type

        Protected Friend Sub New(ByVal con As Condition.ConditionConstructor)
            _con = con
        End Sub

        Public Sub New()

        End Sub

        Public Sub New(ByVal t As Type)
            _t = t
        End Sub

        Protected Friend Sub New(ByVal t As Type, ByVal con As Orm.Condition.ConditionConstructor)
            _con = con
            _t = t
        End Sub

        Public Function [And](ByVal t As Type, ByVal fieldName As String) As CriteriaField
            If String.IsNullOrEmpty(fieldName) Then
                Throw New ArgumentNullException("fieldName")
            End If

            If t Is Nothing Then
                Throw New ArgumentNullException("t")
            End If

            Return New CriteriaField(t, fieldName, _con, ConditionOperator.And)
        End Function

        Public Function [Or](ByVal t As Type, ByVal fieldName As String) As CriteriaField
            If String.IsNullOrEmpty(fieldName) Then
                Throw New ArgumentNullException("fieldName")
            End If

            If t Is Nothing Then
                Throw New ArgumentNullException("t")
            End If

            Return New CriteriaField(t, fieldName, _con, ConditionOperator.Or)
        End Function

        Public Function [And](ByVal fieldName As String) As CriteriaField
            If String.IsNullOrEmpty(fieldName) Then
                Throw New ArgumentNullException("fieldName")
            End If

            Return New CriteriaField(_t, fieldName, _con, ConditionOperator.And)
        End Function

        Public Function [Or](ByVal fieldName As String) As CriteriaField
            If String.IsNullOrEmpty(fieldName) Then
                Throw New ArgumentNullException("fieldName")
            End If

            Return New CriteriaField(_t, fieldName, _con, ConditionOperator.Or)
        End Function

        Public Function [And](ByVal link As CriteriaLink) As CriteriaLink
            If link IsNot Nothing Then
                _con.AddFilter(link._con.Condition, ConditionOperator.And)
            End If

            Return Me
        End Function

        Public Function [Or](ByVal link As CriteriaLink) As CriteriaLink
            If link IsNot Nothing Then
                _con.AddFilter(link._con.Condition, ConditionOperator.Or)
            End If

            Return Me
        End Function

        Public ReadOnly Property Filter() As IEntityFilter
            Get
                Return Filter(Nothing)
            End Get
        End Property

        Public Overridable ReadOnly Property Filter(ByVal t As Type) As IEntityFilter
            Get
                If _con IsNot Nothing Then
                    Dim ef As IEntityFilter = CType(_con.Condition, IEntityFilter)
                    ef.GetFilterTemplate.SetType(t)
                    Return ef
                Else
                    Return Nothing
                End If
            End Get
        End Property
    End Class

    'Friend Class _CriteriaLink
    '    Inherits CriteriaLink

    '    Private _f As IEntityFilter

    '    Public Sub New(ByVal f As IEntityFilter)
    '        _f = f
    '    End Sub

    '    Public Overrides ReadOnly Property Filter(ByVal t As System.Type) As IEntityFilter
    '        Get
    '            Return _f
    '        End Get
    '    End Property
    'End Class

End Namespace