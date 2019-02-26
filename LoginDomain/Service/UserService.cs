using System;
using System.Collections.Generic;
using System.Linq;
using LoginDomain.Entity;
using LoginDomain.Helpers;
using LoginDomain.Repository;

namespace LoginDomain.Service
{
    public class UserService : IUserService
    {
        private DataContext _context;
        //private IRepository _repository;

        public UserService (DataContext context)
        {
            //_repository = repository;
            _context = context;
        }

        public Users Authenticate (string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var user = _context.Users.SingleOrDefault(x => x.UserName == username);

            if (user == null)
                return null;

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }

        private static bool VerifyPasswordHash (string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if(string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace","password");
            if (storedHash.Length != 64) throw new ArgumentException("Too small!! Expected length is 64 bytes", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Too small!! Expected length is 128 bytes", "passwordSalt");

            using(var hmac=new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }

        public Users Create (Users user, string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new AppException("Password required");

            if (_context.Users.Any(x => x.UserName == user.UserName))
                throw new AppException("UserName " + user.UserName + " already exists!");

            byte[] passwordHash, passwordSalt;
            CreatePasswordHash(password, out passwordHash, out passwordSalt);

            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            //_context.Users.Add(user);
            //var result = _context.SaveChanges();
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        private static void CreatePasswordHash (string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null)
                throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace", "password");
        
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }

        public void Delete (int id)
        {
            var user = _context.Users.Find(id);
            if(user!= null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }

        public IEnumerable<Users> GetAll ()
        {
            return _context.Users;
        }

        public Users GetById (int id)
        {
            return _context.Users.Find(id);
        }

        public void Update (Users userParam, string password = null)
        {
            var user = _context.Users.Find(userParam.Id);

            if (user == null)
                throw new AppException("User not found");

            if (userParam.UserName!= user.UserName)
            {
                if (_context.Users.Any(x => x.UserName == userParam.UserName))
                    throw new AppException("Username " + userParam.UserName + " already exists");

                user.UserName = userParam.UserName;
                user.FirstName = userParam.FirstName;
                user.LastName = userParam.LastName;

                if (!string.IsNullOrWhiteSpace(password))
                {
                    byte[] passwordHash, passwordSalt;
                    CreatePasswordHash(password, out passwordHash, out passwordSalt);

                    user.PasswordHash = passwordHash;
                    user.PasswordSalt = passwordSalt;
                }

                _context.Users.Update(user);
                _context.SaveChanges();
            }
        }
    }
}
