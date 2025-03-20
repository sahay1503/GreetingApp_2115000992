using System;
using ModelLayer.DTO;
using Microsoft.Extensions.Logging;
using ModelLayer.Entity;
using RepositoryLayer.Interface;
using BusinessLayer.Interface;
using Middleware.JwtHelper; 

namespace BusinessLayer.Service
{
    public class UserBL : IUserBL
    {
        private readonly ILogger<UserBL> _logger;
        private readonly IUserRL _userRL;
        private readonly JwtTokenHelper _jwtTokenHelper; 
        /// <summary>
        /// Constructor of UserBL
        /// </summary>
        /// <param name="userRL"></param>
        /// <param name="logger"></param>
        /// <param name="jwtTokenHelper"></param>
        public UserBL(IUserRL userRL, ILogger<UserBL> logger, JwtTokenHelper jwtTokenHelper)
        {
            _logger = logger;
            _userRL = userRL;
            _jwtTokenHelper = jwtTokenHelper; 
        }
        /// <summary>
        /// Registration method
        /// </summary>
        /// <param name="registerDTO"></param>
        /// <returns></returns>
        public UserEntity RegistrationBL(RegisterDTO registerDTO)
        {
            try
            {
                _logger.LogInformation("Attempting to register user: {Email}", registerDTO.Email);
                var result = _userRL.Registration(registerDTO);
                if (result != null)
                {
                    _logger.LogInformation("User registration successful for: {Email}", registerDTO.Email);
                }
                else
                {
                    _logger.LogWarning("User registration failed for: {Email}", registerDTO.Email);
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration for {Email}", registerDTO.Email);
                throw;
            }
        }
        /// <summary>
        /// Login User Method 
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <returns></returns>
        public (UserEntity user, string token) LoginnUserBL(LoginDTO loginDTO)
        {
            try
            {
                _logger.LogInformation("Attempting to log in user: {Email}", loginDTO.Email);
                var user = _userRL.LoginnUserRL(loginDTO);

                if (user != null)
                {
                    _logger.LogInformation("Login successful for user: {Email}", loginDTO.Email);
                    var token = _jwtTokenHelper.GenerateToken(user); 
                    return (user, token);
                }

                _logger.LogWarning("Login failed for user: {Email}", loginDTO.Email);
                return (null, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for {Email}", loginDTO.Email);
                throw;
            }
        }
        /// <summary>
        /// Update User Password
        /// </summary>
        /// <param name="email"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public bool UpdateUserPassword(string email, string newPassword)
        {
            // Lookup user by email
            var user = _userRL.FindByEmail(email);
            if (user == null) return false;

            // Hash and update the password
            user.Password = newPassword;
            return _userRL.Update(user);
        }
        /// <summary>
        /// Get by Email Method 
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public UserEntity GetByEmail(string email)
        {
            return _userRL.FindByEmail(email);
        }
        /// <summary>
        /// Validate Email Method
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool ValidateEmail(string email)
        {
            return _userRL.ValidateEmail(email);
        }
    }
}
