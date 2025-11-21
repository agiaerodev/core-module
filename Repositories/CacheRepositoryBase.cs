using Core.Factory;
using Core.Interfaces;
using Hangfire.Common;
using Idata.Entities.Core;
using Ihelpers.Helpers;
using Ihelpers.Interfaces;
using Microsoft.AspNetCore.Server.IIS.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.IO.Pipelines;

namespace Core.Repositories
{
    public class CacheRepositoryBase<TEntity> : IRepositoryBase<TEntity> where TEntity : EntityBase
    {
        private readonly IRepositoryBase<TEntity> _repositoryBase;
        private readonly ICacheBase _cacheBase;
        private readonly Type _entityType;

        public CacheRepositoryBase(IRepositoryBase<TEntity> repositoryBase, ICacheBase cacheBase)
        {
            _repositoryBase = repositoryBase;
            _cacheBase = cacheBase;
            _entityType = typeof(TEntity);
        }

        public CacheRepositoryBase(RepositoryFactory<TEntity> dependenciesContainer, IRepositoryBase<TEntity> repositoryBase)
        {
            this._cacheBase = dependenciesContainer._cache;
            this._repositoryBase = repositoryBase; 
        }


        public void BeforeUpdate(ref TEntity? common, ref UrlRequestBase? requestBase, ref BodyRequestBase? bodyRequestBase)
        {
            _repositoryBase.BeforeUpdate(ref common, ref requestBase, ref bodyRequestBase);
        }
        public async Task<TEntity?> Create(UrlRequestBase? requestBase, BodyRequestBase? bodyRequestBase)
        {
            await _cacheBase.Clear(new List<string>() { _entityType?.FullName });
            return await _repositoryBase.Create(requestBase, bodyRequestBase);
        }

        public async Task CreateExport(UrlRequestBase? requestBase, BodyRequestBase? bodyRequestBase)
        {
            await _repositoryBase.CreateExport(requestBase, bodyRequestBase);
        }

        public void CustomFilters(ref IQueryable<TEntity> query, ref UrlRequestBase? requestBase)
        {
            _repositoryBase.CustomFilters(ref query, ref requestBase);
        }

        public async Task<TEntity?> DeleteBy(UrlRequestBase? requestBase, dynamic modelToRemove = null)
        {
            await _cacheBase.Clear(new List<string>() { _entityType?.FullName });
            return await _repositoryBase.DeleteBy(requestBase, modelToRemove);
        }

        public async Task<TEntity?> GetItem(UrlRequestBase? requestBase)
        {

            IQueryable<TEntity> query = await _repositoryBase.GetOrCreateQueryShow(requestBase);

            var queryString = query.ToQueryString();

            var cacheKey = $"{_entityType.FullName}_{queryString}_{requestBase.GetSetting("timezone") ?? requestBase.getCurrentContextUser()?.timezone ?? ""}";

            var result = await _cacheBase.GetValue<TEntity>(cacheKey);

            if(result == null)
            {
                var repoResult = await _repositoryBase.GetItem(requestBase);

                await _cacheBase.Remember(cacheKey, repoResult, new List<string>() { _entityType?.FullName });

                return repoResult;
            }

            return result;
            
        }

        public async Task<List<TEntity?>> GetItemsBy(UrlRequestBase? requestBase)
        {
            var queryString = (await _repositoryBase.GetOrCreateQueryIndex(requestBase)).ToQueryString();

            var cacheKey = $"{_entityType.FullName}_{queryString}_{requestBase.page}_{requestBase.take}_{requestBase.getCurrentContextUser()?.timezone ?? ""}";

            var result = await _cacheBase.GetValue<List<TEntity?>>(cacheKey);

            if (result == null)
            {
                var repoResult = await _repositoryBase.GetItemsBy(requestBase);

                await _cacheBase.Remember(cacheKey, repoResult, new List<string>() { _entityType?.FullName });

                return repoResult;
            }

            return result;
            
        }

        public async Task<IQueryable<TEntity?>> GetOrCreateQueryShow(UrlRequestBase? requestBase)
        {
            return await _repositoryBase.GetOrCreateQueryShow(requestBase);
        }

        public async Task<IQueryable<TEntity?>> GetOrCreateQueryIndex(UrlRequestBase? requestBase)
        {
            return await _repositoryBase.GetOrCreateQueryIndex(requestBase);
        }


        public async Task Initialize(dynamic wichContext)
        {
            await _repositoryBase.Initialize(wichContext);
        }

        public async Task Initialize(dynamic wichContext, dynamic wichUser)
        {
            await _repositoryBase.Initialize(wichContext, wichUser);

        }

        public async Task LogAction(string message, UrlRequestBase? requestBase, LogType logType = LogType.Information)
        {
            await _repositoryBase.LogAction(message, requestBase, logType); 
        }

        public async Task<TEntity?> RestoreBy(UrlRequestBase? requestBase)
        {
            await _cacheBase.Clear(new List<string>() { _entityType?.FullName });
            return await _repositoryBase.RestoreBy(requestBase);
        }

        public async Task SyncRelations(object? input, dynamic relations, dynamic dataContext)
        {
            await _repositoryBase.SyncRelations(input, relations, dataContext);
        }

        public async Task<TEntity?> UpdateBy(UrlRequestBase? requestBase, BodyRequestBase? bodyRequestBase)
        {
            await _cacheBase.Clear(new List<string>() { _entityType?.FullName });
            return await _repositoryBase.UpdateBy(requestBase, bodyRequestBase);
        }

        public async Task<TEntity?> UpdateOrdering(UrlRequestBase? requestBase, BodyRequestBase? bodyRequestBase)
        {
            await _cacheBase.Clear(new List<string>() { _entityType?.FullName });
            return await _repositoryBase.UpdateOrdering(requestBase, bodyRequestBase);
        }

        public Task<List<dynamic>> GetDynamicPropertiesList(UrlRequestBase urlRequestBase, List<(string, string)> keys)
        {
            throw new NotImplementedException();
        }

        public void NullifyFinalQuery()
        {
            throw new NotImplementedException();
        }
    }
}
