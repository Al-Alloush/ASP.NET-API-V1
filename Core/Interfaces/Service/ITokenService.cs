using Core.Models.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Interfaces.Service
{
    public interface ITokenService
    {
        Task<string> CreateToken(AppUser user);
    }
}
