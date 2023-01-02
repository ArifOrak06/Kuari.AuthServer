using AutoMapper.Internal.Mappers;
using Kuari.AuthServer.Core.DTOs;
using Kuari.AuthServer.Core.Repositories;
using Kuari.AuthServer.Core.Services;
using Kuari.AuthServer.Core.UnitOfWork;
using Kuari.AuthServer.Service.Mappings.AutoMapper;
using Kuari.AuthServer.SharedLibrary.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Kuari.AuthServer.Service.Services
{
    public class Service<TEntity, TDto> : IService<TEntity, TDto> where TEntity : class where TDto : class
    {
        private readonly IUnitOfWork _unitOfWork;
        private IGenericRepository<TEntity> _genericRepository;
        public Service(IUnitOfWork unitOfWork, IGenericRepository<TEntity> genericRepository)
        {
            _unitOfWork = unitOfWork; //saveChangesAsync, SaveChanges
            _genericRepository = genericRepository;  // DataBase Operations

        }

        public async Task<Response<TDto>> AddAsync(TDto dto)
        {
            var newEntity = ObjectMapper.Mapper.Map<TEntity>(dto);
            await _genericRepository.AddAsync(newEntity);
            await _unitOfWork.CommitAsync();

            // geriye bir dto dönmemiz gerektiği için entitymizi tekrar dto'ya mapleyelim.

            var newDto = ObjectMapper.Mapper.Map<TDto>(newEntity);
            return Response<TDto>.Success(newDto, 200);
        }

        public async Task<Response<IEnumerable<TDto>>> GetAllAsync()
        {
            var newEntities = await _genericRepository.GetAllAsync();
            var newDtos = ObjectMapper.Mapper.Map<IEnumerable<TDto>>(newEntities);
            return Response<IEnumerable<TDto>>.Success(newDtos,200);
        }

        public async Task<Response<TDto>> GetByIdAsync(int id)
        {
            var product = await _genericRepository.GetByIdAsync(id);
            if (product == null)
            {
                return Response<TDto>.Fail("İlgili ID'ye sahip ürün bulunamamıştır.", 404,true);
            }

            var productDto = ObjectMapper.Mapper.Map<TDto>(product);
            return Response<TDto>.Success(productDto, 200);
        }

        public async Task<Response<NoDataDto>> RemoveAsync(int id)
        {
            // Öncelikle bu parametre olarak gelen id'ye sahip ürün var mı onu kontrol edelim.
            var isExistEntity = await _genericRepository.GetByIdAsync(id);
            if (isExistEntity == null)
            {
                return Response<NoDataDto>.Fail("ilgili Id'ye sahip ürün bulunmamaktadır.", 404,true);
            }

            _genericRepository.Remove(isExistEntity);
            await _unitOfWork.CommitAsync();
            // 204 StatusCode = ResponseBody'sinde hiç bir data olmayacak.
            return Response<NoDataDto>.Success(204);
        }

        public async Task<Response<NoDataDto>> UpdateAsync(TDto dto, int id)
        {
            var isExistEntity = await _genericRepository.GetByIdAsync(id);
            if (isExistEntity == null)
            {
                return Response<NoDataDto>.Fail("ilgili id'ye sahip ürün bulunamamıştır", 404, true);
            }
            // yukarıda id parametresi ile bulunan product veritabanındaki product olduğu için biz bize parametre olarak gelen 
            // dto'yu entity'e mapleyip akabinde maplenen entity'i id ile gelen entitye update edeceğiz.
            var updateEntity = ObjectMapper.Mapper.Map<TEntity>(dto);
            _genericRepository.Update(updateEntity);
            await _unitOfWork.CommitAsync();
            return Response<NoDataDto>.Success(204);

        }

        public async Task<Response<IEnumerable<TDto>>> Where(Expression<Func<TEntity, bool>> predicate)
        {
            var list = _genericRepository.Where(predicate);
            // gerçek bir projenizde burada Iquerable nesne döneceği için henüz memory'e yansıyan dataları çekilmiş bir nesne listesi olamyacak,
            // bu nedenle ister sıralama ister filtreleme isterseniz sayfalama pagination operasyonlarını yaptıktan sonra yani business kodlarınızı yazdıktan 
            // sonra ne dönmek isterseniz ona göre son şeklini dataya verip veritabanını en son aşamada meşgul edecek olan toListASync() kodunu çalıştırır ve 
            // değişikliğinizi yapabilirsiniz.
            return Response<IEnumerable<TDto>>.Success(ObjectMapper.Mapper.Map<IEnumerable<TDto>>(await list.ToListAsync()),200);
        }
    }
}
