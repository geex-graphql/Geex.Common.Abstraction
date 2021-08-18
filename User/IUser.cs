using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MongoDB.Entities;

namespace Geex.Common.Abstraction.User
{
    public interface IUser : IEntity
    {
        string PhoneNumber { get; set; }
        string UserName { get; set; }
        string Email { get; set; }
    }
}
