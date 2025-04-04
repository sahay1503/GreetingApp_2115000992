using BusinessLayer.Interface;
using Microsoft.AspNetCore.Mvc;
using ModelLayer.Model;
using NLog; // Import NLog namespace
using BusinessLayer.Interface;
using Middleware.GlobalExceptionHandler;

namespace HelloGreetingApplication.Controllers
{
    /// <summary>
    /// Class providing API for HelloGreetingApp
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class HelloGreetingAppController : ControllerBase
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly IGreetingBL _greetingBL;
        public HelloGreetingAppController(IGreetingBL greetingBL) 
        { 
            _greetingBL = greetingBL;
        }


        List<UserModel> users = new List<UserModel> {
            new UserModel { Id = 1, FirstName = "Sid",LastName="Sahay", Email = "sid@gmail.com",Password="sid123" },
            new UserModel { Id = 2, FirstName = "Vaibhav", LastName = "Chaudhary", Email = "vaib@gmail.com", Password = "sid123" },
            new UserModel { Id = 3, FirstName = "Ayush", LastName = "Singh", Email = "ayu@gmail.com", Password = "sid123" },
            new UserModel { Id = 4, FirstName = "Aditya", LastName = "Sharma", Email = "adi@gmail.com", Password = "sid123" }
        };

        /// <summary>
        /// Get Method to get the greeting Message
        /// </summary>
        /// <returns>Hello World!</returns>
        [HttpGet]
        public IActionResult Get()
        {
            logger.Info("GET request received for Greeting API");

            var responseModel = new ResponseModel<string>
            {
                Success = true,
                Message = "Hello to Greeting App API Endpoint",
                Data = "Hello World!"
            };

            logger.Info("GET request successful. Response: {@responseModel}", responseModel);
            return Ok(responseModel);
        }

        /// <summary>
        /// Post method for processing a request
        /// </summary>
        [HttpPost]
        public IActionResult Post(RequestModel requestModel)
        {
            logger.Info("POST request received with data: {@requestModel}", requestModel);

            var responseModel = new ResponseModel<string>
            {
                Success = true,
                Message = "Data received successfully",
                Data = requestModel.value
            };

            logger.Info("POST request processed successfully. Response: {@responseModel}", responseModel);
            return Ok(responseModel);
        }

        /// <summary>
        /// Update existing user data (PUT - Full update)
        /// </summary>
        [HttpPut("{id}")]
        public IActionResult Put(int id, UserModel UpdatedUser)
        {
            logger.Info($"PUT request received for UserId: {id} with data: {@UpdatedUser}");

            var existingUser = users.FirstOrDefault(u => u.Id == id);
            if (existingUser == null)
            {
                logger.Warn($"PUT request failed. UserId {id} not found.");
                return NotFound(new ResponseModel<string>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            existingUser.FirstName = UpdatedUser.FirstName;
            existingUser.LastName = UpdatedUser.LastName;
            existingUser.Email = UpdatedUser.Email;
            existingUser.Password = UpdatedUser.Password;

            logger.Info($"PUT request successful. Updated User: {@existingUser}");
            return Ok(new ResponseModel<UserModel>
            {
                Success = true,
                Message = "User updated successfully",
                Data = existingUser
            });
        }

        /// <summary>
        /// Update a part of the user data (PATCH - Partial update)
        /// </summary>
        [HttpPatch("{id}")]
        public IActionResult Patch(int id, [FromBody] UserModel userModel)
        {
            logger.Info($"PATCH request received for UserId: {id} with partial data: {@userModel}");

            if (userModel == null)
            {
                logger.Warn("PATCH request failed. Invalid request body.");
                return BadRequest(new ResponseModel<string>
                {
                    Success = false,
                    Message = "Invalid request body"
                });
            }

            var existingUser = users.FirstOrDefault(u => u.Id == id);
            if (existingUser == null)
            {
                logger.Warn($"PATCH request failed. UserId {id} not found.");
                return NotFound(new ResponseModel<string>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            // Update only the provided fields
            if (userModel.FirstName != null) existingUser.FirstName = userModel.FirstName;
            if (userModel.LastName != null) existingUser.LastName = userModel.LastName;
            if (!string.IsNullOrEmpty(userModel.Email)) existingUser.Email = userModel.Email;
            if (!string.IsNullOrEmpty(userModel.Password)) existingUser.Password = userModel.Password;

            logger.Info($"PATCH request successful. Updated User: {@existingUser}");
            return Ok(new ResponseModel<UserModel>
            {
                Success = true,
                Message = "User updated successfully",
                Data = existingUser
            });
        }

        /// <summary>
        /// Delete a user by ID
        /// </summary>
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            logger.Info($"DELETE request received for UserId: {id}");

            var existingUser = users.FirstOrDefault(u => u.Id == id);
            if (existingUser == null)
            {
                logger.Warn($"DELETE request failed. UserId {id} not found.");
                return NotFound(new ResponseModel<string>
                {
                    Success = false,
                    Message = "User not found"
                });
            }

            users.Remove(existingUser);
            logger.Info($"DELETE request successful. UserId {id} deleted.");

            return Ok(new ResponseModel<string>
            {
                Success = true,
                Message = "User deleted successfully"
            });
        }
        //UC2
        /// <summary>
        /// Greeting request called successfully
        /// </summary>
        /// <returns></returns>
        [HttpGet("Greeting")]
        public IActionResult GetGreeting()
        {
            logger.Info("Greeting request called successfully");
            return Ok(_greetingBL.GetGreetingBL());
        }
        //UC3
        /// <summary>
        /// Greeting request from HelloRoute Post called
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns></returns>
        [HttpGet("hello")]
        public IActionResult GetGreeting([FromQuery] string? firstName, [FromQuery] string? lastName)
        {
            logger.Info("Greeting request from HelloRoute Post called successfully");
            string greetingMessage = _greetingBL.GetGreeting(firstName, lastName);
            return Ok(new { Message = greetingMessage });
        }

        //UC4
        /// <summary>
        /// Save Greeting Method in Controller 
        /// </summary>
        /// <param name="greetingModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("save")]

        public IActionResult SaveGreeting([FromBody] GreetingModel greetingModel)
        {
            var result = _greetingBL.SaveGreetingBL(greetingModel);
            if (result == null)
            {
                var response1 = new ResponseModel<object>
                {
                    Success = false,
                    Message = "Unable to save Greeting. Please verify that the user exists. !",
                    Data = result

                };
                return BadRequest(response1);

            }
            var response = new ResponseModel<object>
            {
                Success = true,
                Message = "Greeting Created",
                Data = result

            };
            return Created("Greeting Created", response);

        }


        //UC5
        /// <summary>
        /// Get Greeting By ID Method in controller
        /// </summary>
        /// <param name="id"></param>
        [HttpGet("GetGreetingById/{id}")]
        public IActionResult GetGreetingById(int id)
        {
            try
            {
                var entity = _greetingBL.GetGreetingByIdBL(id);
                if (entity != null)
                {
                    var model = new GreetingModel
                    {
                        Id = entity.Id,
                        Message = entity.Message,
                        Uid = entity.Uid
                    };
                    return Ok(new ResponseModel<GreetingModel>
                    {
                        Success = true,
                        Message = "Greeting Message Found",
                        Data = model
                    });
                }
                return NotFound(new ResponseModel<GreetingModel>
                {
                    Success = false,
                    Message = "Greeting Message Not Found"
                });
            }
            catch (Exception ex)
            {
                var errorResponse = ExceptionHandler.CreateErrorResponse(ex);
                return StatusCode(500, errorResponse);
            }
        }



        //UC6
        /// <summary>
        /// Get All Greetings Method
        /// </summary>
        /// <returns></returns>

        [HttpGet("GetAllGreetings")]
        public IActionResult GetAllGreetings()
        {
            try
            {
                var entities = _greetingBL.GetAllGreetingsBL();
                if (entities != null && entities.Count > 0)
                {
                    var models = entities.Select(entity => new GreetingModel
                    {
                        Id = entity.Id,
                        Message = entity.Message,
                        Uid = entity.Uid
                    }).ToList();

                    return Ok(new ResponseModel<List<GreetingModel>>
                    {
                        Success = true,
                        Message = "Greeting Messages Found",
                        Data = models
                    });
                }
                return NotFound(new ResponseModel<List<GreetingModel>>
                {
                    Success = false,
                    Message = "No Greeting Messages Found"
                });
            }
            catch (Exception ex)
            {
                var errorResponse = ExceptionHandler.CreateErrorResponse(ex);
                return StatusCode(500, errorResponse);
            }
        }


        //UC7
        /// <summary>
        /// Edit greeting Method in Controller
        /// </summary>
        /// <param name="id"></param>
        /// <param name="greetModel"></param>
        /// <returns></returns>

        [HttpPut("EditGreeting/{id}")]
        public IActionResult EditGreeting(int id, [FromBody] GreetingModel greetModel)
        {
            try
            {
                var entity = _greetingBL.EditGreetingBL(id, greetModel);
                if (entity == null)
                {
                    return BadRequest(new ResponseModel<object>
                    {
                        Success = false,
                        Message = $"No Greeting found with ID {id} to update!"
                    });
                }

                var updatedModel = new GreetingModel
                {
                    Id = entity.Id,
                    Message = entity.Message,
                    Uid = entity.Uid
                };

                return Ok(new ResponseModel<GreetingModel>
                {
                    Success = true,
                    Message = "Greeting Message Updated Successfully",
                    Data = updatedModel
                });
            }
            catch (Exception ex)
            {
                var errorResponse = ExceptionHandler.CreateErrorResponse(ex);
                return StatusCode(500, errorResponse);
            }
        }


        //UC8
        /// <summary>
        /// Delete Greeting Method in controller
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>

        [HttpDelete("DeleteGreeting/{id}")]
        public IActionResult DeleteGreeting(int id)
        {
            try
            {
                bool result = _greetingBL.DeleteGreetingBL(id);
                if (result)
                {
                    return Ok(new ResponseModel<string>
                    {
                        Success = true,
                        Message = "Greeting Message Deleted Successfully"
                    });
                }
                return NotFound(new ResponseModel<string>
                {
                    Success = false,
                    Message = "Greeting Message Not Found"
                });
            }
            catch (Exception ex)
            {
                var errorResponse = ExceptionHandler.CreateErrorResponse(ex);
                return StatusCode(500, errorResponse);
            }
        }
    }
}
