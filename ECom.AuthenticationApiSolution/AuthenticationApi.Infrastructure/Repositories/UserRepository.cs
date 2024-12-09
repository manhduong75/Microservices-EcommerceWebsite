﻿using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthenticationApi.Application.DTOs;
using AuthenticationApi.Application.Interfaces;
using AuthenticationApi.Domain.Entities;
using AuthenticationApi.Infrastructure.Data;
using ECom.SharedLibrary.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthenticationApi.Infrastructure.Repositories
{
    public class UserRepository(AuthenticationDbContext context, IConfiguration config) : IUser
    {
        private async Task<AppUser> GetUserByEmail(string email)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
            return user is null ? null! : user;
        }   
        public async Task<GetUserDto> GetUser(int userId)
        {
            var user = await context.Users.FindAsync(userId);
            return user is null ? null! : new GetUserDto(user.Id, user.Name!, user.TelephoneNumber!, user.Address!, user.Email!, user.Role!);
        }

        public async Task<Response> Login(LoginDTO loginDTO)
        {
            var getUser = await GetUserByEmail(loginDTO.Email);
            if(getUser is null)
                return new Response(false, "Invalid email or password");
            bool verifyPassword = BCrypt.Net.BCrypt.Verify(loginDTO.Password, getUser.Password);
            if(!verifyPassword)
                return new Response(false, "Invalid email or password");

            string token = GenerateToken(getUser);
            return new Response(true, token);
        }

        private string GenerateToken(AppUser user)
        {
            var key = Encoding.UTF8.GetBytes(config.GetSection("Authentication:Key").Value!);
            var securityKey = new SymmetricSecurityKey(key);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Name!),
                new Claim(ClaimTypes.Email, user.Email!),
            };
            if(!string.IsNullOrEmpty(user.Role) || !Equals("string", user.Role))
                claims.Add(new Claim(ClaimTypes.Role, user.Role!));
            var token = new JwtSecurityToken(
                issuer: config["Authentication:Issuer"],
                audience: config["Authentication:Audience"],
                claims: claims,
                expires: null,
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<Response> Register(AppUserDTO appUserDTO)
        {
            var getUser = await GetUserByEmail(appUserDTO.Email);
            if(getUser is not null)
                return new Response(false, "Email already used. You cannot use this email for registration");
            var result = context.Users.Add(new AppUser()
            {
                Name = appUserDTO.Name,
                Email = appUserDTO.Email,
                TelephoneNumber = appUserDTO.TelephoneNumber,
                Password = BCrypt.Net.BCrypt.HashPassword(appUserDTO.Password),
                Address = appUserDTO.Address,
                Role = appUserDTO.Role
            });

            await context.SaveChangesAsync();

            return result.Entity.Id > 0? new Response(true, "User registered successfully") : new Response(false, "Invalid data provided");
        }
    }
}
