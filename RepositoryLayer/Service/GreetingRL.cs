using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.Interface;
using RepositoryLayer.Contexts;
using ModelLayer.Model;
using RepositoryLayer.Entity;
using NLog;

namespace RepositoryLayer.Service
{
    /// <summary>
    /// GreetingRL Class inheriting IGreetingRL
    /// </summary>
    public class GreetingRL : IGreetingRL
    {
        /// <summary>
        /// Creating DBContext of GreetingApp
        /// </summary>
        private readonly GreetingAppContext _dbContext;
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Creating GreetingRL constructor
        /// </summary>
        /// <param name="dbContext"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public GreetingRL(GreetingAppContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        //UC4
        /// <summary>
        /// for saving Greeting in database
        /// </summary>
        /// <param name="greetingModel"></param>
        /// <returns></returns>
        public GreetEntity SaveGreetingRL(GreetingModel greetingModel)
        {
            try
            {
                var existingMessage = _dbContext.Greet.FirstOrDefault(e => e.Id == greetingModel.Id);
                if (existingMessage == null)
                {
                    var newMessage = new GreetEntity { Message = greetingModel.Message };
                    _dbContext.Greet.Add(newMessage);
                    _dbContext.SaveChanges();
                    Logger.Info("Greeting saved successfully with ID: {0}", newMessage.Id);
                    return newMessage;
                }
                Logger.Warn("Greeting already exists with ID: {0}", greetingModel.Id);
                return existingMessage;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while saving greeting.");
                throw;
            }
        }

        //UC5
        /// <summary>
        /// for getting Greeting Message By ID from database
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public GreetingModel GetGreetingByIdRL(int Id)
        {
            try
            {
                var entity = _dbContext.Greet.FirstOrDefault(g => g.Id == Id);
                if (entity != null)
                {
                    Logger.Info("Greeting fetched successfully for ID: {0}", Id);
                    return new GreetingModel { Id = entity.Id, Message = entity.Message };
                }
                Logger.Warn("Greeting not found for ID: {0}", Id);
                return null;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while fetching greeting by ID: {0}", Id);
                throw;
            }
        }

        //UC6
        /// <summary>
        /// For Getting All Greetings from Database
        /// </summary>
        /// <returns>List of Greetings</returns>
        public List<GreetEntity> GetAllGreetingsRL()
        {
            try
            {
                var greetings = _dbContext.Greet.ToList();
                Logger.Info("Fetched all greetings successfully. Total Count: {0}", greetings.Count);
                return greetings;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error while fetching all greetings.");
                throw;
            }
        }

        //UC7
        /// <summary>
        /// Editing Greeting
        /// </summary>
        /// <param name="id"></param>
        /// <param name="greetingModel"></param>
        /// <returns></returns>
        public GreetEntity EditGreetingRL(int id, GreetingModel greetingModel)
        {
            try
            {
                var entity = _dbContext.Greet.FirstOrDefault(g => g.Id == id);
                if (entity != null)
                {
                    entity.Message = greetingModel.Message;
                    _dbContext.Greet.Update(entity);
                    _dbContext.SaveChanges();
                    Logger.Info("Greeting updated successfully for ID: {0}", id);
                    return entity;
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
        /// Delete Greeting
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public bool DeleteGreetingRL(int id)
        {
            try
            {
                var entity = _dbContext.Greet.FirstOrDefault(g => g.Id == id);
                if (entity != null)
                {
                    _dbContext.Greet.Remove(entity);
                    _dbContext.SaveChanges();
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
