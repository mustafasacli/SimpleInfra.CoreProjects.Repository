namespace SimpleInfra.Business.Core
{
    using SimpleFileLogging;
    using SimpleFileLogging.Enums;
    using SimpleFileLogging.Interfaces;
    using SimpleInfra.Common.Response;
    using SimpleInfra.Data;
    using SimpleInfra.Dto.Core;
    using SimpleInfra.Entity.Core;
    using SimpleInfra.IoC;
    using SimpleInfra.Mapping;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Base Simple Business class.
    /// </summary>
    /// <typeparam name="TDto"></typeparam>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TRepository"></typeparam>
    public abstract class BaseSimpleBusiness<TDto, TEntity, TRepository> : IDisposable
            where TDto : BaseSimpleDto, new()
            where TEntity : BaseSimpleEntity, new()
        where TRepository : SimpleBaseDataRepository<TEntity>, new()
    {
        private TRepository repository = null;

        /// <summary>
        /// Base Repository
        /// </summary>
        protected TRepository Repository
        {
            get
            {
                if (repository == null)
                    repository = CreateRepository();

                return repository;
            }
        }

        /// <summary>
        /// Creates new repository.
        /// </summary>
        /// <returns>TRepository instance</returns>
        protected TRepository CreateRepository()
        {
            return new TRepository();
        }

        /// <summary>
        /// protected constructor
        /// </summary>
        protected BaseSimpleBusiness()
        {
            this.Logger = SimpleFileLogger.Instance;
            this.Logger.LogDateFormatType = SimpleLogDateFormats.Hour;
            this.repository = this.CreateRepository();
        }

        /// <summary>
        /// Gets Logger instance.
        /// </summary>
        public ISimpleLogger Logger
        { get; protected set; }

        /// <summary>
        /// Gets Instance of class.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        protected T GetInstance<T>() where T : class
        {
            return SimpleIoC.Instance.GetInstance<T>();
        }

        /// <summary>
        /// Insert record internally.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="autoSave"></param>
        /// <returns></returns>
        protected virtual SimpleResponse<TDto> InsertInternal(TDto dto, bool autoSave = true)
        {
            var response = new SimpleResponse<TDto>();
            var ent = MapReverse(dto);

            this.Repository.Add(ent);

            if (autoSave)
                response.ResponseCode = this.Repository.SaveChanges();

            var result = Map(ent);
            response.Data = result;

            return response;
        }

        /// <summary>
        /// Insert record internally.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="autoSave"></param>
        /// <returns></returns>
        protected virtual SimpleResponse InsertInternalDontReturnDto(TDto dto, bool autoSave = true)
        {
            var response = new SimpleResponse();
            var ent = MapReverse(dto);

            this.Repository.Add(ent);

            if (autoSave)
                response.ResponseCode = this.Repository.SaveChanges();

            //var result = Map(ent);
            //response.Data = result;

            return response;
        }

        /// <summary>
        /// Inserts records internally.
        /// </summary>
        /// <param name="dtoList"></param>
        /// <param name="autoSave"></param>
        /// <returns></returns>
        protected virtual SimpleResponse<List<TDto>> InsertRangeInternal(List<TDto> dtoList, bool autoSave = true)
        {
            var response = new SimpleResponse<List<TDto>>();
            if (dtoList == null || dtoList.Count < 1)
                return response;

            var entList = MapListReverse(dtoList);
            this.Repository.AddRange(entList);

            if (autoSave)
                response.ResponseCode = this.Repository.SaveChanges();

            var result = MapList(entList);
            response.Data = result;

            return response;
        }

        /// <summary>
        /// Update record internally.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="autoSave"></param>
        /// <returns></returns>
        protected virtual SimpleResponse<TDto> UpdateInternal(TDto dto, bool autoSave = true)
        {
            var response = new SimpleResponse<TDto>();
            var ent = MapReverse(dto);

            this.Repository.Update(ent);

            if (autoSave)
                response.ResponseCode = this.Repository.SaveChanges();

            var result = Map(ent);
            response.Data = result;

            return response;
        }

        /// <summary>
        /// Updates records internally.
        /// </summary>
        /// <param name="dtoList"></param>
        /// <param name="autoSave"></param>
        /// <returns></returns>
        protected virtual SimpleResponse<List<TDto>> UpdateRangeInternal(List<TDto> dtoList, bool autoSave = true)
        {
            var response = new SimpleResponse<List<TDto>>();

            if (dtoList == null || dtoList.Count < 1)
                return response;

            var entList = MapListReverse(dtoList) ?? new List<TEntity>();
            this.Repository.UpdateRange(entList);
            //entList.ForEach(
            //    q =>
            //    {
            //        this.Repository.Update(q);
            //    });

            if (autoSave)
                response.ResponseCode = this.Repository.SaveChanges();

            var result = MapList(entList);
            response.Data = result;

            return response;
        }

        /// <summary>
        /// Delete record internally.
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="autoSave"></param>
        /// <returns></returns>
        protected virtual SimpleResponse<TDto> DeleteInternal(TDto dto, bool autoSave = true)
        {
            var response = new SimpleResponse<TDto>();
            var ent = MapReverse(dto);

            this.Repository.Delete(ent);

            if (autoSave)
                response.ResponseCode = this.Repository.SaveChanges();


            response.Data = dto;
            return response;
        }

        /// <summary>
        /// Deletes records internally.
        /// </summary>
        /// <param name="dtoList"></param>
        /// <param name="autoSave"></param>
        /// <returns></returns>
        protected virtual SimpleResponse<List<TDto>> DeleteRangeInternal(List<TDto> dtoList, bool autoSave = true)
        {
            var response = new SimpleResponse<List<TDto>>();

            if (dtoList == null || dtoList.Count < 1)
                return response;

            var entList = MapListReverse(dtoList);

            this.Repository.DeleteRange(entList.ToArray());
            ////entList.ForEach(
            ////    q =>
            ////    {
            ////        this.Repository.Delete(q);
            ////    });

            if (autoSave)
                response.ResponseCode = this.Repository.SaveChanges();

            response.Data = dtoList;
            return response;
        }

        /// <summary>
        /// Reads all records as internally.
        /// </summary>
        /// <returns></returns>
        protected SimpleResponse<List<TDto>> ReadAllInternal()
        {
            List<TDto> dtos;
            List<TEntity> entities;
            entities = this.Repository
                .GetAll(asNoTracking: true)
                .ToList() ?? new List<TEntity>();

            dtos = MapList(entities) ?? new List<TDto>();

            var response = new SimpleResponse<List<TDto>> { Data = dtos };

            response.ResponseCode = dtos.Count;
            response.RCode = response.ResponseCode.ToString();

            return response;
        }

        /// <summary>
        /// Return single entity for given predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="asNoTracking"></param>
        /// <returns>returns entity instance if predicate condition true.</returns>
        protected TEntity ReadSingle(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = false)
        {
            var entity = this.Repository.Single(predicate, asNoTracking: asNoTracking);
            return entity;
        }

        /// <summary>
        /// Return single dto for given predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>returns dto instance if predicate condition true.</returns>
        protected TDto ReadDtoSingle(Expression<Func<TEntity, bool>> predicate)
        {
            var entity = ReadSingle(predicate, asNoTracking: true);
            var dto = Map(entity);
            return dto;
        }

        /// <summary>
        /// Return first entity for given predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="asNoTracking"></param>
        /// <returns>returns first entity instance if predicate condition true.</returns>
        protected TEntity ReadFirst(Expression<Func<TEntity, bool>> predicate, bool asNoTracking = false)
        {
            var entity = this.Repository.FirstOrDefault(predicate, asNoTracking: asNoTracking);
            return entity;
        }

        /// <summary>
        /// Return first dto for given predicate.
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns>returns first dto instance if predicate condition true.</returns>
        protected TDto ReadDtoFirst(Expression<Func<TEntity, bool>> predicate)
        {
            var entity = ReadFirst(predicate, asNoTracking: true);
            var dto = Map(entity);
            return dto;
        }

        /// <summary>
        /// Maps entity to data transfer object.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected TDto Map(TEntity entity)
        {
            var dto = SimpleMapper.Map<TEntity, TDto>(entity);
            return dto;
        }

        /// <summary>
        /// Maps data transfer object to entity.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        protected TEntity MapReverse(TDto dto)
        {
            var entity = SimpleMapper.Map<TDto, TEntity>(dto);
            return entity;
        }

        /// <summary>
        /// Maps entity to data transfer object.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dto"></param>
        protected void MapTo(TEntity entity, TDto dto)
        {
            SimpleMapper.MapTo(entity, dto);
        }
        //
        /// <summary>
        /// Maps data transfer object to entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="dto"></param>
        protected void MapToReverse(TDto dto, TEntity entity)
        {
            SimpleMapper.MapTo(dto, entity);
        }

        /// <summary>
        /// Maps entity list to data transfer object list.
        /// </summary>
        /// <param name="entityList"></param>
        /// <returns></returns>
        protected List<TDto> MapList(List<TEntity> entityList)
        {
            var dtos = SimpleMapper.MapList<TEntity, TDto>(entityList);
            return dtos;
        }

        /// <summary>
        /// Maps data transfer object list to entity list.
        /// </summary>
        /// <param name="entityList"></param>
        /// <returns></returns>
        protected List<TEntity> MapListReverse(List<TDto> entityList)
        {
            var entities = SimpleMapper.MapList<TDto, TEntity>(entityList);
            return entities;
        }

        /// <summary>
        /// Saves changes.
        /// </summary>
        /// <returns></returns>
        protected int SaveChanges()
        {
            var result = this.Repository.SaveChanges();
            return result;
        }


        #region IDisposable Members

        private bool disposed = false;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing && repository != null)
                {
                    repository.Dispose();
                }
            }

            this.disposed = true;
        }

        /// <summary>
        /// Disposes object.
        /// </summary>
        public virtual void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion IDisposable Members
    }
}
