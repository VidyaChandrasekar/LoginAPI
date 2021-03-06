﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using LoginDomain.Dtos;
using LoginDomain.Entity;
using LoginDomain.Helpers;
using LoginDomain.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace LoginApp.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private IUserService _userservice;
        private IMapper _mapper;
        private readonly AppSettings _appSettings;

        public UsersController (IUserService userService, IMapper mapper, IOptions<AppSettings> appSettings)
        {
            _userservice = userService;
            _mapper = mapper;
            _appSettings = appSettings.Value;
        }


        [AllowAnonymous]
        [HttpPost("authenticate")]
        public IActionResult Authenticate ([FromBody]UserDto userDto)
        {
            var user = _userservice.Authenticate(userDto.Username, userDto.Password);

            if (user == null)
                return Unauthorized();

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, user.Id.ToString())
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            // return basic user info (without password) and token to store client side
            return Ok(new
            {
                Id = user.Id,
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Token = tokenString
            });
        }


        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody]UserDto userDto)
        {
            var user = _mapper.Map<Users>(userDto);

            try
            {
                _userservice.Create(user, userDto.Password);
                return Ok();
            }
            catch (AppException ex)
            {
                return BadRequest( ex.Message );
            }
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userservice.GetAll();
            var userDto = _mapper.Map<IList<UserDto>>(users);
            return Ok(userDto);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var user = _userservice.GetById(id);
            var userDto = _mapper.Map<UserDto>(user);
            return Ok(userDto);
        }
        

        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody]UserDto userDto)
        {
            var user = _mapper.Map<Users>(userDto);
            user.Id = id;

            try
            {
                _userservice.Update(user, userDto.Password);
                return Ok();
            }
            catch (AppException ex)
            {
                return BadRequest( ex.Message );
            }
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _userservice.Delete(id);
            return Ok();
        }

        
    }
}
