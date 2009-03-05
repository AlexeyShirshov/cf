﻿Imports System.Collections.Generic
Imports System.ComponentModel
Imports Worm.Entities.Meta

Namespace Entities

    <Serializable()> _
    <DefaultProperty("Item")> _
    Public Class AnonymousEntity
        Inherits Entity
        Implements IOptimizedValues

        Private _props As New Dictionary(Of String, Object)

        Public Overridable Overloads Function GetValue( _
            ByVal propertyAlias As String, ByVal oschema As Meta.IEntitySchema) As Object Implements IOptimizedValues.GetValueOptimized
            Return _props(propertyAlias)
        End Function

        Public Overridable Sub SetValue( _
            ByVal propertyAlias As String, ByVal oschema As Meta.IEntitySchema, ByVal value As Object) Implements IOptimizedValues.SetValueOptimized
            _props(propertyAlias) = value
        End Sub

        Default Public ReadOnly Property Item(ByVal field As String) As Object
            Get
                Return GetValue(field, Nothing)
            End Get
        End Property
    End Class

    <Serializable()> _
    Public Class AnonymousCachedEntity
        Inherits AnonymousEntity
        Implements _ICachedEntity

        Private _pk() As String
        Private _hasPK As Boolean

        Public Event Added(ByVal sender As ICachedEntity, ByVal args As System.EventArgs) Implements ICachedEntity.Added
        Public Event Deleted(ByVal sender As ICachedEntity, ByVal args As System.EventArgs) Implements ICachedEntity.Deleted
        Public Event OriginalCopyRemoved(ByVal sender As ICachedEntity) Implements ICachedEntity.OriginalCopyRemoved
        Public Event Saved(ByVal sender As ICachedEntity, ByVal args As ObjectSavedArgs) Implements ICachedEntity.Saved
        Public Event Updated(ByVal sender As ICachedEntity, ByVal args As System.EventArgs) Implements ICachedEntity.Updated

        Public Function CheckIsAllLoaded(ByVal schema As ObjectMappingEngine, ByVal loadedColumns As Integer) As Boolean Implements _ICachedEntity.CheckIsAllLoaded

        End Function

        Public Function ForseUpdate(ByVal c As Meta.EntityPropertyAttribute) As Boolean Implements _ICachedEntity.ForseUpdate

        End Function

        Public Overloads Sub Init(ByVal pk() As Meta.PKDesc, ByVal cache As Cache.CacheBase, ByVal schema As ObjectMappingEngine) Implements _ICachedEntity.Init

        End Sub

        Public ReadOnly Property IsPKLoaded() As Boolean Implements _ICachedEntity.IsPKLoaded
            Get
                Return _hasPK
            End Get
        End Property

        Public Sub PKLoaded(ByVal pkCount As Integer) Implements _ICachedEntity.PKLoaded
            If _pk Is Nothing Then
                Throw New OrmObjectException("PK is not loaded")
            End If
            _hasPK = True
        End Sub

        Public Sub RaiseCopyRemoved() Implements _ICachedEntity.RaiseCopyRemoved

        End Sub

        Public Sub RaiseSaved(ByVal sa As OrmManager.SaveAction) Implements _ICachedEntity.RaiseSaved

        End Sub

        Public Function Save(ByVal mc As OrmManager) As Boolean Implements _ICachedEntity.Save

        End Function

        Public Sub SetLoaded(ByVal value As Boolean) Implements _ICachedEntity.SetLoaded

        End Sub

        Public Function SetLoaded(ByVal fieldName As String, ByVal loaded As Boolean, ByVal check As Boolean, ByVal schema As ObjectMappingEngine) As Boolean Implements _ICachedEntity.SetLoaded

        End Function

        Public Function SetLoaded(ByVal c As EntityPropertyAttribute, ByVal loaded As Boolean, ByVal check As Boolean, ByVal schema As ObjectMappingEngine) As Boolean Implements _ICachedEntity.SetLoaded

        End Function

        Public ReadOnly Property UpdateCtx() As UpdateCtx Implements _ICachedEntity.UpdateCtx
            Get

            End Get
        End Property

        Public Function AcceptChanges(ByVal updateCache As Boolean, ByVal setState As Boolean) As ICachedEntity Implements ICachedEntity.AcceptChanges

        End Function

        Public ReadOnly Property ChangeDescription() As String Implements ICachedEntity.ChangeDescription
            Get

            End Get
        End Property

        Public Sub CreateCopyForSaveNewEntry(ByVal mgr As OrmManager, ByVal pk() As Meta.PKDesc) Implements _ICachedEntity.CreateCopyForSaveNewEntry

        End Sub

        Public Function GetPKValues() As Meta.PKDesc() Implements ICachedEntity.GetPKValues
            Dim schema As Worm.ObjectMappingEngine = MappingEngine
            Dim oschema As IEntitySchema = schema.GetEntitySchema(Me.GetType)
            Dim l As New List(Of PKDesc)
            For Each pk As String In _pk
                l.Add(New PKDesc(pk, schema.GetPropertyValue(Me, pk, oschema)))
            Next
            Return l.ToArray
        End Function

        Public ReadOnly Property HasChanges() As Boolean Implements ICachedEntity.HasChanges
            Get

            End Get
        End Property

        Public ReadOnly Property Key() As Integer Implements ICachedEntity.Key
            Get
                Dim schema As Worm.ObjectMappingEngine = MappingEngine
                Dim oschema As IEntitySchema = schema.GetEntitySchema(Me.GetType)
                Dim k As Integer
                For Each pk As String In _pk
                    k = k Xor schema.GetPropertyValue(Me, pk, oschema).GetHashCode
                Next
                Return k
            End Get
        End Property

        Public Overloads Sub Load(ByVal propertyAlias As String) Implements ICachedEntity.Load

        End Sub

        Public ReadOnly Property OriginalCopy() As ICachedEntity Implements ICachedEntity.OriginalCopy
            Get

            End Get
        End Property

        Public Sub RejectChanges() Implements ICachedEntity.RejectChanges

        End Sub

        Public Sub RejectRelationChanges(ByVal mgr As OrmManager) Implements ICachedEntity.RejectRelationChanges

        End Sub

        Public Sub RemoveFromCache(ByVal cache As Cache.CacheBase) Implements ICachedEntity.RemoveOriginalCopy

        End Sub

        Public Function SaveChanges(ByVal AcceptChanges As Boolean) As Boolean Implements ICachedEntity.SaveChanges

        End Function

        Public Sub UpdateCache(ByVal mgr As OrmManager, ByVal oldObj As ICachedEntity) Implements _ICachedEntity.UpdateCache

        End Sub

        'Public Sub SetSpecificSchema(ByVal mpe As ObjectMappingEngine) Implements _ICachedEntity.SetSpecificSchema

        'End Sub

        Public Function BeginAlter() As System.IDisposable Implements ICachedEntity.BeginAlter

        End Function

        Public Function BeginEdit() As System.IDisposable Implements ICachedEntity.BeginEdit

        End Function

        Public Sub CheckEditOrThrow() Implements ICachedEntity.CheckEditOrThrow

        End Sub

        Public Function Delete(ByVal mgr As OrmManager) As Boolean Implements ICachedEntity.Delete

        End Function

        Public Overloads Sub RejectChanges1(ByVal mgr As OrmManager) Implements _ICachedEntity.RejectChanges

        End Sub

        'Public Overloads ReadOnly Property OriginalCopy1(ByVal cache As Cache.CacheBase) As ICachedEntity Implements _ICachedEntity.OriginalCopy
        '    Get

        '    End Get
        'End Property

        Public Overloads Sub Load(ByVal mgr As OrmManager, Optional ByVal propertyAlias As String = Nothing) Implements _ICachedEntity.Load

        End Sub
    End Class
End Namespace