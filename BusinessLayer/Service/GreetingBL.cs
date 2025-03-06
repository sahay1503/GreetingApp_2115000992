using System;
using System.Collections.Generic;
using System.Linq;
using BusinessLayer.Interface;
using ModelLayer.Model;
using RepositoryLayer.Entity;
using RepositoryLayer.Interface;
using NLog;

namespace BusinessLayer.Service
{
    /// <summary>
    /// Business Layer class for Greeting functionalities
    /// </summary>
    public class GreetingBL : IGreetingBL
    {
        private readonly IGreetingRL _greetingRL;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Constructor for GreetingBL
        /// </summary>
        /// <param name="greetingRL"></param>
        public GreetingBL(IGreetingRL greetingRL)
        {
            _greetingRL = greetingRL;
        }
        //UC2
        /// <summary>
        /// Returns a simple Hello World message
        /// </summary>
        /// <returns>String</returns>
        public string GetGreetingBL()
        {
            return "Hello World";
        }

        //UC3
        /// <summary>
        /// Generates a greeting message based on provided first and last name
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <returns>Formatted greeting message</returns>
        public string GetGreeting(string? firstName, string? lastName)
        {
            try
            {
                string greetingMessage;

                if (!string.IsNullOrEmpty(firstName) && !string.IsNullOrEmpty(lastName))
                {
                    greetingMessage = $"Hello {firstName} {lastName}!";
                }
                else if (!string.IsNullOrEmpty(firstName))
                {
                    greetingMessage = $"Hello {firstName}!";
                }
                else if (!string.IsNullOrEmpty(lastName))
                {
                    greetingMessage = $"Hello {lastName}!";
                }
                else
                {
                    greetingMessage = "Hello, World!";
                }

                Logger.Info("Generated greeting message: {0}", greetingMessage);
                return greetingMessage;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while generating greeting message.");
                throw;
            }
        }

        //UC4
        /// <summary>
        /// Saves a greeting message in the database
        /// </summary>
        /// <param name="greetingModel"></param>
        /// <returns>Saved GreetEntity</returns>
        public GreetEntity SaveGreetingBL(GreetingModel greetingModel)
        {
            try
            {
                var result = _greetingRL.SaveGreetingRL(greetingModel);
                Logger.Info("Greeting saved successfully with ID: {0}", result.Id);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while saving greeting.");
                throw;
            }
        }

        //UC5
        /// <summary>
        /// Retrieves a greeting by ID
        /// </summary>
        /// <param name="Id"></param>
        /// <returns>GreetingModel</returns>
        public GreetingModel GetGreetingByIdBL(int Id)
        {
            try
            {
                var result = _greetingRL.GetGreetingByIdRL(Id);
                Logger.Info("Fetched greeting for ID: {0}", Id);
                return result;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while fetching greeting by ID: {0}", Id);
                throw;
            }
        }

        //UC6
        /// <summary>
        /// Retrieves all greetings from the database
        /// </summary>
        /// <returns>List of GreetingModel</returns>
        public List<GreetingModel> GetAllGreetingsBL()
        {
            try
            {
                var entityList = _greetingRL.GetAllGreetingsRL();
                if (entityList != null)
                {
                    var greetings = entityList.Select(g => new GreetingModel { Id = g.Id, Message = g.Message }).ToList();
                    Logger.Info("Fetched all greetings successfully. Total Count: {0}", greetings.Count);
                    return greetings;
                }
                Logger.Warn("No greetings found in the database.");
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while fetching all greetings.");
                throw;
            }
        }

        //UC7
        /// <summary>
        /// Updates a greeting message
        /// </summary>
        /// <param name="id"></param>
        /// <param name="greetingModel"></param>
        /// <returns>Updated GreetingModel</returns>
        public GreetingModel EditGreetingBL(int id, GreetingModel greetingModel)
        {
            try
            {
                var result = _greetingRL.EditGreetingRL(id, greetingModel);
                if (result != null)
                {
                    Logger.Info("Greeting updated successfully for ID: {0}", id);
                    return new GreetingModel { Id = result.Id, Message = result.Message };
                }
                Logger.Warn("Greeting not found for ID: {0} to update", id);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while updating greeting with ID: {0}", id);
                throw;
            }
        }

        //UC8
        /// <summary>
        /// Deletes a greeting message
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Boolean indicating success</returns>
        public bool DeleteGreetingBL(int id)
        {
            try
            {
                var result = _greetingRL.DeleteGreetingRL(id);
                if (result)
                {
                    Logger.Info("Greeting deleted successfully for ID: {0}", id);
                    return true;
                }
                Logger.Warn("Greeting not found for ID: {0} to delete", id);
                return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while deleting greeting with ID: {0}", id);
                throw;
            }
        }
    }
}
