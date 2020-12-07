﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Cosmos.Reference;
using UnityEngine;

namespace Cosmos.Entity
{
    /// <summary>
    /// 实例对象管理器；
    /// 管理例如角色身上的Gadget
    /// </summary>
    [Module]
    internal class EntityManager : Module// , IEntityManager
    { 
        #region Properties
        public int EntityTypeCount { get { return entityTypeObjectDict.Count; } }
        Dictionary<Type, List<IEntityObject>> entityTypeObjectDict;
        Type entityObjectType = typeof(IEntityObject);
        Action<EntityObject> entitySpawnSucceed;
        public event Action<EntityObject> EntitySpawnSucceed
        {
            add { entitySpawnSucceed += value; }
            remove { entitySpawnSucceed -= value; }
        }
        public GameObject EntityRoot { get; private set; }
        /// <summary>
        /// 所有实体列表
        /// </summary>
        public Dictionary<Type, List<EntityObject>> entityDict { get; private set; } 

        IReferencePoolManager referencePoolManager;
        IResourceManager resourceManager;
        #endregion
        #region Methods
        public override void OnInitialization()
        {
            entityTypeObjectDict = new Dictionary<Type, List<IEntityObject>>();
            entityDict = new Dictionary<Type, List<EntityObject>>();
        }
        public override void OnPreparatory()
        {
            referencePoolManager = GameManager.GetModule<IReferencePoolManager>();
            resourceManager = GameManager.GetModule<IResourceManager>();
        }
        public override void OnRefresh()
        {
            if (IsPause)
                return;
            foreach (var entityDict in entityTypeObjectDict)
            {
                foreach (var entity in entityDict.Value)
                {
                    entity.OnRefresh();
                }
            }
        }
        public bool AddEntity<T>(T entity)
            where T : class, IEntityObject, new()
        {
            Type type = typeof(T);
            return AddEntity(type,entity);
        }
        public bool AddEntity(Type type,IEntityObject entity)
        {
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            bool result = false;
            if (!entityTypeObjectDict.ContainsKey(type))
            {
                entityTypeObjectDict.Add(type, new List<IEntityObject>() { entity });
                result = true;
            }
            else
            {
                var set = entityTypeObjectDict[type];
                if (!set.Contains(entity))
                {
                    set.Add(entity);
                    result = true;
                }
            }
            return result;
        }
        /// <summary>
        /// 回收单个实体对象;
        /// 若实体对象存在于缓存中，则移除。若不存在，则不做操作；
        /// </summary>
        /// <param name="entity">实体对象</param>
        public void RecoveryEntity<T>(T entity)
              where T : class, IEntityObject, new()
        {
            Type type = typeof(T);
            if (entityTypeObjectDict.ContainsKey(type))
            {
                var set = entityTypeObjectDict[type];
                if (set.Contains(entity))
                    set.Remove(entity);
            }
            referencePoolManager.Despawn(entity);
        }
        /// <summary>
        /// 回收某一类型的实体对象
        /// </summary>
        /// <param name="type">实体类型</param>
        public void RecoveryEntities(Type type)
        {
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            if (!entityTypeObjectDict.ContainsKey(type))
                throw new ArgumentNullException("EntityManager : can not attach entity, entity type not exist !  Entity Type : " + type.ToString());
            var set = entityTypeObjectDict[type];
            int length = set.Count;
            for (int i = 0; i < length; i++)
            {
                referencePoolManager.Despawns(set[i]);
            }
        }
        public int GetEntityCount<T>()
            where T : class, IEntityObject, new()
        {
            Type type = typeof(T);
            return GetEntityCount(type);
        }
        public int GetEntityCount(Type type)
        {
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            if (!entityTypeObjectDict.ContainsKey(type))
                return -1;
            return entityTypeObjectDict[type].Count;
        }
        public T GetEntity<T>(Predicate<T> predicate)
            where T : class, IEntityObject, new()
        {
            Type type = typeof(T);
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            if (!entityTypeObjectDict.ContainsKey(type))
                throw new ArgumentNullException("EntityManager : can not attach entity, entity type not exist !  Entity Type : " + type.ToString());
            T entity = default;
            var set = entityTypeObjectDict[type];
            int length = set.Count;
            for (int i = 0; i < length; i++)
            {
                if (predicate(set[i] as T))
                    return set[i] as T;
            }
            if (entity == null)
                throw new ArgumentNullException("EntityManager : can not register entity,entityObject is  empty");
            return entity;
        }
        public IEntityObject GetEntity(Type type, Predicate<IEntityObject> predicate)
        {
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            if (!entityTypeObjectDict.ContainsKey(type))
                throw new ArgumentNullException("EntityManager : can not attach entity, entity type not exist !  Entity Type : " + type.ToString());
            IEntityObject entity = default;
            var set = entityTypeObjectDict[type];
            int length = set.Count;
            for (int i = 0; i < length; i++)
            {
                if (predicate(set[i]))
                    return set[i];
            }
            if (entity == null)
                throw new ArgumentNullException("EntityManager : can not register entity,entityObject is  empty");
            return entity;
        }
        public IEntityObject[] GetEntities<T>()
            where T : class, IEntityObject, new()
        {
            Type type = typeof(T);
            return GetEntities(type);
        }
        public IEntityObject[] GetEntities(Type type)
        {
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            if (!entityTypeObjectDict.ContainsKey(type))
                throw new ArgumentNullException("EntityManager : can not attach entity, entity type not exist !  Entity Type : " + type.ToString());
            var set = entityTypeObjectDict[type];
            return set.ToArray();
        }
        public List<IEntityObject> GetEntityList<T>()
            where T : class, IEntityObject, new()
        {
            Type type = typeof(T);
            return GetEntityList(type);
        }
        public List<IEntityObject> GetEntityList(Type type)
        {
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            if (!entityTypeObjectDict.ContainsKey(type))
                throw new ArgumentNullException("EntityManager : can not attach entity, entity type not exist !  Entity Type : " + type.ToString());
            var set = entityTypeObjectDict[type];
            return set;
        }
        public void RegisterEntityType<T>()
            where T : class, IEntityObject, new()
        {
            Type type = typeof(T);
            RegisterEntityType(type);
        }
        public void RegisterEntityType(Type type)
        {
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            if (!entityTypeObjectDict.ContainsKey(type))
                throw new ArgumentException("EntityManager : can not register entity,entityObject already exist Entity Type : " + type.ToString());
            entityTypeObjectDict.Add(type, new List<IEntityObject>());
        }
        public bool DeregisterEntityType<T>()
            where T : class, IEntityObject, new()
        {
            Type type = typeof(T);
            return DeregisterEntityType(type);
        }
        public bool DeregisterEntityType(Type type)
        {
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            if (!entityTypeObjectDict.ContainsKey(type))
                throw new ArgumentNullException("EntityManager : can not attach entity, entity type not exist !  Entity Type : " + type.ToString());
            bool result = false;
            result = entityTypeObjectDict.Remove(type);
            return result;
        }
        public bool ActiveEntity<T>(T entity)
            where T : class, IEntityObject, new()
        {
            Type type = typeof(T);
            return ActiveEntity(type, entity);
        }
        public void ActiveEntity<T>(Predicate<T> predicate)
            where T : class, IEntityObject, new()
        {
            Type type = typeof(T);
            var set = entityTypeObjectDict[type];
            int length = set.Count;
            for (int i = 0; i < length; i++)
            {
                if (predicate(set[i] as T))
                    set[i].OnActive();
            }
        }
        public bool ActiveEntity(Type type, IEntityObject entity)
        {
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            if (!entityTypeObjectDict.ContainsKey(type))
                throw new ArgumentNullException("EntityManager : can not attach entity, entity type not exist !  Entity Type : " + type.ToString());
            bool result = false;
            var set = entityTypeObjectDict[type];
            if (set.Contains(entity))
            {
                entity.OnActive();
                result = true;
            }
            else
                throw new ArgumentNullException("EntityManager : can not register entity, entity is  empty Entity : " + entity.ToString());
            return result;
        }
        public void ActiveEntity(Type type, Predicate<IEntityObject> predicate)
        {
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            if (!entityTypeObjectDict.ContainsKey(type))
                throw new ArgumentNullException("EntityManager : can not attach entity, entity type not exist !  Entity Type : " + type.ToString());
            var set = entityTypeObjectDict[type];
            int length = set.Count;
            for (int i = 0; i < length; i++)
            {
                if (predicate(set[i]))
                    set[i].OnActive();
            }
        }
        public void ActiveEntities<T>()
            where T : class, IEntityObject, new()
        {
            Type type = typeof(T);
            ActiveEntities(type);
        }
        public void ActiveEntities(Type type)
        {
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            var set = entityTypeObjectDict[type];
            int length = set.Count;
            for (int i = 0; i < length; i++)
                set[i].OnActive();
        }
        public void DeactiveEntities<T>()
            where T : class, IEntityObject, new()
        {
            Type type = typeof(T);
            DeactiveEntities(type);
        }
        public void DeactiveEntities(Type type)
        {
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            if (!entityTypeObjectDict.ContainsKey(type))
                throw new ArgumentNullException("EntityManager : can not attach entity, entity type not exist !  Entity Type : " + type.ToString());
            var set = entityTypeObjectDict[type];
            int length = set.Count;
            for (int i = 0; i < length; i++)
                set[i].OnDeactive();
        }
        public bool HasEntity<T>(Predicate<T> predicate)
            where T : class, IEntityObject, new()
        {
            Type type = typeof(T);
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            if (!entityTypeObjectDict.ContainsKey(type))
                return false;
            var set = entityTypeObjectDict[type];
            int length = set.Count;
            for (int i = 0; i < length; i++)
            {
                if (predicate(set[i] as T))
                    return true;
            }
            return false;
        }
        public bool HasEntity(Type type,  Predicate<IEntityObject> predicate)
        {
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            if (!entityTypeObjectDict.ContainsKey(type))
                return false;
            var set = entityTypeObjectDict[type];
            int length = set.Count;
            for (int i = 0; i < length; i++)
            {
                if (predicate(set[i]))
                    return true;
            }
            return false;
        }
        public bool HasEntityType<T>()
            where T : class, IEntityObject, new()
        {
            Type type = typeof(T);
            return HasEntityType(type);
        }
        public bool HasEntityType(Type type)
        {
            if (!entityObjectType.IsAssignableFrom(type))
                throw new TypeAccessException("EntityManager : type  is not sub class of EntityObject ! Type : " + type.ToString());
            if (!entityTypeObjectDict.ContainsKey(type))
                return false;
            return true;
        }

        public Coroutine CreateEntity(Type type, string entityName, Action<float> loadingAction, Action<EntityObject> loadDoneAction)
        {
            var attribute = type.GetCustomAttribute<EntityAssetAttribute>();
            if (attribute != null)
            {
                if (entityDict.ContainsKey(type))
                {
                    //if (attribute.UseObjectPool&& ObjectPools[type].Count > 0)
                    //{
                    //    EntityObject entityObject = SpawnEntity(type, ObjectPools[type].Dequeue(), entityName == "<None>" ? type.Name : entityName);

                    //    loadingAction?.Invoke(1);
                    //    loadDoneAction?.Invoke(entityObject);
                    //    entitySpawnSucceed?.Invoke(entityObject);
                    //    return null;
                    //}
                    //else
                    //{
                    //    if (_defineEntities.ContainsKey(type.FullName) && _defineEntities[type.FullName] != null)
                    //    {
                    //        EntityObject entityObject = SpawnEntity(type, Main.Clone(_defineEntities[type.FullName], _entitiesGroup[type].transform), entityName == "<None>" ? type.Name : entityName);
                    //        loadingAction?.Invoke(1);
                    //        loadDoneAction?.Invoke(entityObject);
                    //        entitySpawnSucceed?.Invoke(entityObject);
                    //        return null;
                    //    }
                    //    else
                    //    {
                    //        var assetInfo = new AssetInfo(attribute.AssetBundleName, attribute.AssetPath, attribute.ResourcePath);
                    //       return resourceManager.LoadPrefabAsync(assetInfo, (obj) => 
                    //        {
                    //            EntityObject entityObject = SpawnEntity(type, obj, entityName == "<None>" ? type.Name : entityName);
                    //            loadDoneAction?.Invoke(entityObject);
                    //            entitySpawnSucceed?.Invoke(entityObject);
                    //        }, loadingAction);
                    //    }
                    //}
                }
                else
                {
                    throw new ArgumentNullException($"EntityManager-->创建实体失败：实体对象 :{type.Name }并未存在！");
                }
            }
            return null;
        }
        //生成实体
        private EntityObject SpawnEntity(Type type, object entity, string entityName)
        {
            EntityObject entityObject =referencePoolManager.Spawn(type) as EntityObject;
            entityDict[type].Add(entityObject);
            entityObject.SetEntity( entity);
            entityObject.OnInitialization();
            entityObject.OnActive();
            return entityObject;
        }
        #endregion
    }
}