﻿using Microsoft.AspNetCore.Mvc;
using ModelLayer.DTO;
using NLog;
using BusinessLayer.Interface;
using Middleware.HashingAlgo;
using Middleware.JwtHelper;
using ModelLayer.Model;
using System.Security.Claims;
using Middleware.Email;
using Middleware.RabbitMQClient;
using System.IdentityModel.Tokens.Jwt;

namespace HelloGreetingApplication.Controllers
{
    /// <summary>
    /// User Controller Class
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly Logger _logger = LogManager.GetCurrentClassLogger();
        private readonly IUserBL _userBL;
        private readonly JwtTokenHelper _jwtTokenHelper;
        private readonly SMTP _smtp;
        private readonly IRabbitMQService _rabbitMQService;
        /// <summary>
        /// Constructor of UserController
        /// </summary>
        /// <param name="userBL"></param>
        /// <param name="jwtTokenHelper"></param>
        /// <param name="smtp"></param>
        /// <param name="rabbitMQService"></param>
        public UserController(IUserBL userBL, JwtTokenHelper jwtTokenHelper, SMTP smtp, IRabbitMQService rabbitMQService)
        {
            _userBL = userBL;
            _jwtTokenHelper = jwtTokenHelper;
            _smtp = smtp;
            _rabbitMQService = rabbitMQService;
        }
        /// <summary>
        /// Register User Method in Controller
        /// </summary>
        /// <param name="registerDTO"></param>
        /// <returns></returns>
        [HttpPost("registerUser")]
        public IActionResult Register(RegisterDTO registerDTO)
        {
            try
            {
                _logger.Info($"Register attempt for email: {registerDTO.Email}");

                var newUser = _userBL.RegistrationBL(registerDTO);

                if (newUser == null)
                {
                    _logger.Warn($"Registration failed. Email already exists: {registerDTO.Email}");
                    return Conflict(new { Success = false, Message = "User with this email already exists." });
                }

                _logger.Info($"User registered successfully: {registerDTO.Email}");

                // Publish a message to RabbitMQ after successful registration
                _rabbitMQService.SendMessage(registerDTO.Email + ", You have successfully registered!");

                return Created("user registered", new { Success = true, Message = "User registered successfully." });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Registration failed for {registerDTO.Email}");
                return BadRequest(new { Success = false, Message = "Registration failed.", Error = ex.Message });
            }
        }
        /// <summary>
        /// Login User in Controller
        /// </summary>
        /// <param name="loginDTO"></param>
        /// <returns></returns>

        [HttpPost("loginUser")]
        public IActionResult Login(LoginDTO loginDTO)
        {
            try
            {
                _logger.Info($"Login attempt for user: {loginDTO.Email}");

                var (user, token) = _userBL.LoginnUserBL(loginDTO);

                if (user == null || string.IsNullOrEmpty(token))
                {
                    _logger.Warn($"Invalid login attempt for user: {loginDTO.Email}");
                    return Unauthorized(new { Success = false, Message = "Invalid username or password." });
                }

                _logger.Info($"User {loginDTO.Email} logged in successfully.");

                // Publish a login success message to RabbitMQ
                _rabbitMQService.SendMessage(loginDTO.Email + " logged in successfully!");

                // Simulating message consumption
                _rabbitMQService.ReceiveMessage();

                return Ok(new
                {
                    Success = true,
                    Message = "Login Successful.",
                    Token = token
                });
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"Login failed for {loginDTO.Email}");
                return BadRequest(new { Success = false, Message = "Login failed.", Error = ex.Message });
            }
        }
        /// <summary>
        /// Forgot Password Method in Controller
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost("forgot-password")]
        public IActionResult ForgotPassword([FromBody] ForgotPasswordDTO request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest(new { message = "Email is required." });
                }

                bool result = _userBL.ValidateEmail(request.Email);

                if (!result)
                {
                    return Ok(new { message = "Not a valid email" });
                }

                string mail = request.Email;
                var resetToken = _jwtTokenHelper.GeneratePasswordResetToken(mail);

                string subject = "Reset Your Password";
                string body = $"Click the link to reset your password: \n https://HelloGreetingApp.com/reset-password?token={resetToken}";

                _smtp.SendEmailAsync(request.Email, subject, body);

                // Send password reset request notification to RabbitMQ
                _rabbitMQService.SendMessage($"Password reset requested for {request.Email}");

                return Ok(new { message = "Password reset email has been sent." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error occurred while processing the password reset", error = ex.Message });
            }
        }
        /// <summary>
        /// Reset Password Method in Controller
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] ResetPasswordDTO model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.Token))
                    return BadRequest(new ResponseModel<string> { Success = false, Message = "Invalid token." });

                var principal = _jwtTokenHelper.ValidateToken(model.Token);
                if (principal == null)
                    return BadRequest(new ResponseModel<string> { Success = false, Message = "Invalid or expired token." });

                var emailClaim = principal.FindFirst(ClaimTypes.Email)?.Value
                              ?? principal.FindFirst(JwtRegisteredClaimNames.Email)?.Value;

                var isResetTokenClaim = principal.FindFirst("isPasswordReset")?.Value;

                if (string.IsNullOrEmpty(isResetTokenClaim) || isResetTokenClaim != "true")
                {
                    return BadRequest(new ResponseModel<string> { Success = false, Message = "Invalid reset token." });
                }

                if (emailClaim != model.Email)
                {
                    return BadRequest(new ResponseModel<string> { Success = false, Message = "Invalid Email." });
                }

                if (string.IsNullOrEmpty(emailClaim))
                    return BadRequest(new ResponseModel<string> { Success = false, Message = "Email not found in token." });

                var user = _userBL.GetByEmail(emailClaim);
                if (user == null)
                    return NotFound(new ResponseModel<string> { Success = false, Message = "User not found" });

                string password = HashingMethods.HashPassword(model.NewPassword);
                _userBL.UpdateUserPassword(model.Email, password);

                // Notify RabbitMQ about successful password reset
                _rabbitMQService.SendMessage($"Password reset successfully for {model.Email}");

                return Ok(new ResponseModel<string> { Success = true, Message = "Password reset successfully" });
            }
            catch (Exception e)
            {
                return BadRequest(new ResponseModel<string> { Success = false, Message = e.Message });
            }
        }
    }
}
