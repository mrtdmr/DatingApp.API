﻿using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }
        [HttpPost("register")]
        //public async Task<IActionResult> Register([FromBody]RegisterUserDTO userDTO)
        public async Task<IActionResult> Register(RegisterUserDTO registerUserDTO)
        {
            //if (!ModelState.IsValid) // [ApiController]'ı kullandığımız için buraya ve [FromBody]'e gerek kalmadı. [ApiController] bu kontrolleri otomatik yapıyor. 
            //{
            //    return BadRequest(ModelState);
            //}

            registerUserDTO.UserName = registerUserDTO.UserName.ToLower();
            if (await _repo.UserExists(registerUserDTO.UserName))
                return BadRequest("Username already exists.");
            var userToCreate = new User { UserName = registerUserDTO.UserName };
            var createdUser = await _repo.Register(userToCreate, registerUserDTO.Password);
            return StatusCode(201);
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginUserDTO loginUserDTO)
        {
            var userFromRepo = await _repo.Login(loginUserDTO.UserName.ToLower(), loginUserDTO.Password);
            if (userFromRepo == null)
                return Unauthorized();
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier,userFromRepo.Id.ToString()),
                new Claim(ClaimTypes.Name,userFromRepo.UserName)
            };
            var key = new SymmetricSecurityKey(Encoding.UTF8
                .GetBytes(_config.GetSection("AppSettings:Token").Value));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = cred
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}