
using LoginDomain.Entity;
using System;
using System.Collections.Generic;
using System.Text;

namespace LoginDomain.Service
{
    public interface IUserService
    {
        Users Authenticate (string username, string password);
        IEnumerable<Users> GetAll ();
        Users GetById (int id);
        Users Create (Users user, string password);
        void Update (Users user, string password = null);
        void Delete (int id);
    }
}
