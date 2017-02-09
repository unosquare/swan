using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Reflection;

namespace Unosquare.Swan.AspNetCore
{
    /// <summary>
    /// Represents a Business Rules Controller
    /// </summary>
    public interface IBusinessRulesController
    {
        /// <summary>
        /// Runs the business rules.
        /// </summary>
        void RunBusinessRules();
    }

    /// <summary>
    /// Creates a new DbContext with support to run BusinessControllers
    /// </summary>
    public interface IBusinessDbContext
    {
        /// <summary>
        /// Adds the controller.
        /// </summary>
        /// <param name="controller">The controller.</param>
        void AddController(IBusinessRulesController controller);

        /// <summary>
        /// Removes the controller.
        /// </summary>
        /// <param name="controller">The controller.</param>
        void RemoveController(IBusinessRulesController controller);

        /// <summary>
        /// Determines whether the specified controller contains controller.
        /// </summary>
        /// <param name="controller">The controller.</param>
        /// <returns>
        ///   <c>true</c> if the specified controller contains controller; otherwise, <c>false</c>.
        /// </returns>
        bool ContainsController(IBusinessRulesController controller);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <seealso cref="Unosquare.Swan.AspNetCore.IBusinessRulesController" />
    public abstract class BusinessRulesController<T> : IBusinessRulesController
        where T : DbContext
    {
        /// <summary>
        /// Gets or sets the context.
        /// </summary>
        /// <value>
        /// The context.
        /// </value>
        public T Context { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BusinessRulesController{T}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        protected BusinessRulesController(T context)
        {
            Context = context;
        }

        /// <summary>
        /// Runs the business rules.
        /// </summary>
        public void RunBusinessRules()
        {
            var methodInfoSet = GetType().GetMethods().Where(m => m.ReturnType == typeof(void) && m.IsPublic
                                                                  && !m.IsConstructor &&
                                                                  m.GetCustomAttributes(typeof(BusinessRuleAttribute),
                                                                      true).Any()).ToArray();

            ExecuteBusinessRulesMethods(EntityState.Added, ActionFlags.Create, methodInfoSet);
            ExecuteBusinessRulesMethods(EntityState.Modified, ActionFlags.Update, methodInfoSet);
            ExecuteBusinessRulesMethods(EntityState.Deleted, ActionFlags.Delete, methodInfoSet);
        }

        /// <summary>
        /// Gets the type of the entity.
        /// </summary>
        /// <param name="entity">The entity.</param>
        /// <returns></returns>
        public Type GetEntityType(object entity)
        {
            return entity.GetType();
        }

        private void ExecuteBusinessRulesMethods(EntityState state, ActionFlags action, MethodInfo[] methodInfoSet)
        {
            var selfTrackingEntries = Context.ChangeTracker.Entries().Where(x => x.State == state).ToList();

            foreach (var entry in selfTrackingEntries)
            {
                var entity = entry.Entity;

                var entityType = entity.GetType();

                var methods = methodInfoSet.Where(m => m.GetCustomAttributes(typeof(BusinessRuleAttribute), true)
                    .Select(a => a as BusinessRuleAttribute)
                    .Any(b => (b.EntityTypes == null ||
                               b.EntityTypes.Any(t => t == entityType)) &&
                               b.Action == action));

                foreach (var methodInfo in methods)
                {
                    methodInfo.Invoke(this, new[] {entity});
                }
            }
        }
    }
}