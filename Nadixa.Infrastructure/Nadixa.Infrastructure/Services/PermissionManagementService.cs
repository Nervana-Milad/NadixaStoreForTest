using AutoMapper;
using Nadixa.Application.DTOS.Permission;
using Nadixa.Application.Interfaces;
using Nadixa.Core.Common;
using Nadixa.Core.Entities;
using Nadixa.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Infrastructure.Services
{
    public class PermissionManagementService : IPermissionManagementService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PermissionManagementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<PermissionDto>> GetAllAsync()
        {
            var permissions = await _unitOfWork.Repository<Permission>().GetAllAsync();

            return permissions
                .OrderBy(p => p.Name)
                .Select(p => new PermissionDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Description = p.Description
                })
                .ToList();
        }

        public async Task<PermissionOperationResult> CreateAsync(PermissionCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Code) || string.IsNullOrWhiteSpace(dto.Name))
                return new PermissionOperationResult { Success = false, Message = AppMessages.CodeAndNameRequired };

            var existing = await _unitOfWork.Repository<Permission>()
                .ExistsAsync(p => p.Code == dto.Code);

            if (existing)
                return new PermissionOperationResult { Success = false, Message = AppMessages.PermissionCodeExists };

            await _unitOfWork.Repository<Permission>().AddAsync(new Permission
            {
                Code = dto.Code.Trim(),
                Name = dto.Name.Trim(),
                Description = dto.Description
            });

            await _unitOfWork.CompleteAsync();

            return new PermissionOperationResult { Success = true, Message = AppMessages.PermissionCreated };
        }

        public async Task<PermissionOperationResult> DeleteAsync(int id)
        {
            var permission = await _unitOfWork.Repository<Permission>().GetByIdAsync(id);

            if (permission == null)
                return new PermissionOperationResult { Success = false };

            _unitOfWork.Repository<Permission>().HardDelete(permission);
            await _unitOfWork.CompleteAsync();

            return new PermissionOperationResult { Success = true, Message = AppMessages.PermissionDeleted };

        }

    }
}
