using Nadixa.Application.DTOS.Permission;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Nadixa.Application.Interfaces
{
    public interface IPermissionManagementService
    {
        Task<List<PermissionDto>> GetAllAsync();
        Task<PermissionOperationResult> CreateAsync(PermissionCreateDto dto);
        Task<PermissionOperationResult> DeleteAsync(int id);
    }
}
