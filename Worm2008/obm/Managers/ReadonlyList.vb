﻿Imports System.Collections.Generic
Imports Worm.Criteria.Core

Friend Interface IListEdit
    Inherits IList
    Overloads Sub Add(ByVal o As Orm.IEntity)
    Overloads Sub Remove(ByVal o As Orm.IEntity)
    Overloads Sub Insert(ByVal pos As Integer, ByVal o As Orm.IEntity)
    ReadOnly Property List() As IList
End Interface

Friend Interface ILoadableList
    Inherits IListEdit
    Sub LoadObjects()
End Interface

Public Class ReadOnlyList(Of T As {Orm.IOrmBase})
    Inherits ReadOnlyEntityList(Of T)

    Private _rt As Type

    Public Sub New(ByVal realType As Type)
        MyBase.new()
    End Sub

    Public Sub New()
        MyBase.new()
        _rt = GetType(T)
    End Sub

    Public Sub New(ByVal realType As Type, ByVal col As IEnumerable(Of T))
        MyBase.New(col)
    End Sub

    Public Sub New(ByVal col As IEnumerable(Of T))
        MyBase.New(col)
        _rt = GetType(T)
    End Sub

    Public Sub New(ByVal realType As Type, ByVal list As List(Of T))
        MyBase.New(list)
    End Sub

    Public Sub New(ByVal realType As Type, ByVal list As ReadOnlyList(Of T))
        MyBase.New(list)
    End Sub

    Public Overrides Function LoadObjects() As ReadOnlyEntityList(Of T)
        If _l.Count > 0 Then
            Dim o As T = _l(0)
            Using mc As IGetManager = o.GetMgr()
                Return mc.Manager.LoadObjects(_rt, Me)
            End Using
        Else
            Return Me
        End If
    End Function

    Public Overrides Function LoadObjects(ByVal start As Integer, ByVal length As Integer) As ReadOnlyEntityList(Of T)
        If _l.Count > 0 Then
            Dim o As T = _l(0)
            Using mc As IGetManager = o.GetMgr()
                Return mc.Manager.LoadObjects(_rt, Me, start, length)
            End Using
        Else
            Return Me
        End If
    End Function

    'Public Overrides Function LoadObjects(ByVal fields() As String, ByVal start As Integer, ByVal length As Integer) As ReadOnlyEntityList(Of T)
    '    If _l.Count > 0 Then
    '        Dim o As T = _l(0)
    '        Using mc As IGetManager = o.GetMgr()
    '            Return mc.Manager.LoadObjects(Of T)(Me, fields, start, length)
    '        End Using
    '    Else
    '        Return Me
    '    End If
    'End Function

    Public Function Distinct() As ReadOnlyList(Of T)
        Dim l As New Dictionary(Of T, T)
        For Each o As T In Me
            If Not l.ContainsKey(o) Then
                l.Add(o, o)
            End If
        Next
        Return New ReadOnlyList(Of T)(l.Keys)
    End Function
End Class

Public Class ReadOnlyEntityList(Of T As {Orm.ICachedEntity})
    Inherits ReadOnlyObjectList(Of T)
    Implements ILoadableList

    Public Sub New()
        MyBase.new()
    End Sub

    Public Sub New(ByVal col As IEnumerable(Of T))
        MyBase.New(col)
    End Sub

    Public Sub New(ByVal list As List(Of T))
        MyBase.New(list)
    End Sub

    Public Sub New(ByVal list As ReadOnlyEntityList(Of T))
        MyBase.New(list)
    End Sub

    Public Overridable Function LoadObjects() As ReadOnlyEntityList(Of T)
        If _l.Count > 0 Then
            Dim o As T = _l(0)
            Dim l As New List(Of T)
            For Each obj As T In _l
                If Not obj.IsLoaded Then
                    obj.Load()
                End If
                l.Add(obj)
            Next
            Return New ReadOnlyEntityList(Of T)(l)
        Else
            Return Me
        End If
    End Function

    Public Overridable Function LoadObjects(ByVal start As Integer, ByVal length As Integer) As ReadOnlyEntityList(Of T)
        If _l.Count > 0 Then
            Dim o As T = _l(0)
            Dim l As New List(Of T)
            For i As Integer = start To Math.Max(Count, start + length) - 1
                Dim obj As T = _l(i)
                If Not obj.IsLoaded Then
                    obj.Load()
                End If
                If obj.IsLoaded Then
                    l.Add(obj)
                End If
            Next
            Return New ReadOnlyEntityList(Of T)(l)
        Else
            Return Me
        End If
    End Function

    Public Overridable Function LoadObjects(ByVal fields() As String, ByVal start As Integer, ByVal length As Integer) As ReadOnlyEntityList(Of T)
        If _l.Count > 0 Then
            Dim o As T = _l(0)
            Using mc As IGetManager = o.GetMgr()
                Return mc.Manager.LoadObjects(Of T)(Me, fields, start, length)
            End Using
        Else
            Return Me
        End If
    End Function

    Private Sub _LoadObjects() Implements ILoadableList.LoadObjects
        LoadObjects()
    End Sub
End Class

Public Class ReadOnlyObjectList(Of T As {Orm._IEntity})
    Inherits ObjectModel.ReadOnlyCollection(Of T)
    Implements IListEdit

    Protected _l As List(Of T)

    Private ReadOnly Property _List() As IList Implements IListEdit.List
        Get
            Return _l
        End Get
    End Property

    Protected Friend ReadOnly Property List() As IList(Of T)
        Get
            Return _l
        End Get
    End Property

    Public Sub New()
        MyClass.New(New List(Of T))
    End Sub

    Public Sub New(ByVal col As IEnumerable(Of T))
        MyClass.New(New List(Of T)(col))
    End Sub

    Public Sub New(ByVal list As List(Of T))
        MyBase.New(list)
        _l = list
    End Sub

    Public Sub New(ByVal list As ReadOnlyObjectList(Of T))
        MyClass.New(New List(Of T)(list))
    End Sub

    'Public Sub Add(ByVal o As T)
    '    _l.Add(o)
    'End Sub

    'Public Sub AddRange(ByVal col As IEnumerable(Of T))
    '    'For Each o As T In col
    '    '    Add(o)
    '    'Next
    '    _l.AddRange(col)
    'End Sub

    Public Sub Sort(ByVal cs As IComparer(Of T))
        _l.Sort(cs)
    End Sub

    Private Sub _Add(ByVal o As Orm.IEntity) Implements IListEdit.Add
        CType(_l, IList).Add(o)
    End Sub

    Private Overloads Sub Insert(ByVal pos As Integer, ByVal o As Orm.IEntity) Implements IListEdit.Insert
        CType(_l, IList).Insert(pos, o)
    End Sub

    Private Overloads Sub Remove(ByVal o As Orm.IEntity) Implements IListEdit.Remove
        CType(_l, IList).Remove(o)
    End Sub

    Public Function ApplyFilter(ByVal filter As IFilter, ByRef evaluated As Boolean) As ReadOnlyObjectList(Of T)
        If _l.Count > 0 Then
            Dim o As T = _l(0)
            Using mc As IGetManager = o.GetMgr()
                Return mc.Manager.ApplyFilter(Of T)(Me, filter, evaluated)
            End Using
        Else
            Return Me
        End If
    End Function

    Public Function ApplySort(ByVal s As Sorting.Sort) As ICollection(Of T)
        Return OrmManager.ApplySort(Of T)(Me, s)
    End Function

    Public Function GetRange(ByVal index As Integer, ByVal count As Integer) As ReadOnlyObjectList(Of T)
        If _l.Count > 0 Then
            Dim lst As List(Of T) = _l.GetRange(index, count)
            Return New ReadOnlyObjectList(Of T)(lst)
        Else
            Return Me
        End If
    End Function

End Class