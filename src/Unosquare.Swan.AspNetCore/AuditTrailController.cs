using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Unosquare.Swan.AspNetCore
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class AuditTrailController<T, TEntity> : BusinessRulesController<T> where T : DbContext
    {
        private readonly List<Type> _validCreateTypes = new List<Type>();
        private readonly List<Type> _validUpdateTypes = new List<Type>();
        private readonly List<Type> _validDeleteTypes = new List<Type>();
        private readonly string _currentuserId;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditTrailController{T, TEntity}"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="currentUserId">The current user identifier.</param>
        public AuditTrailController(T context, string currentUserId) : base(context)
        {
            _currentuserId = currentUserId;
        }

        /// <summary>
        /// Adds the types.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="types">The types.</param>
        public void AddTypes(ActionFlags action, Type[] types)
        {
            switch (action)
            {
                case ActionFlags.None:
                    break;
                case ActionFlags.Create:
                    _validCreateTypes.AddRange(types);
                    break;
                case ActionFlags.Update:
                    _validUpdateTypes.AddRange(types);
                    break;
                case ActionFlags.Delete:
                    _validDeleteTypes.AddRange(types);
                    break;
            }
        }

        /// <summary>
        /// Called when [entity created].
        /// </summary>
        /// <param name="entity">The entity.</param>
        [BusinessRule(ActionFlags.Create)]
        public virtual void OnEntityCreated(object entity)
        {
            var entityType = GetEntityType(entity);
            if (_validCreateTypes.Contains(entityType) == false && _validCreateTypes.Any()) return;

            AuditEntry(ActionFlags.Create, entity, entityType.Name);
        }

        /// <summary>
        /// Called when [entity updated].
        /// </summary>
        /// <param name="entity">The entity.</param>
        [BusinessRule(ActionFlags.Update)]
        public virtual void OnEntityUpdated(object entity)
        {
            var entityType = GetEntityType(entity);
            if (_validUpdateTypes.Contains(entityType) == false && _validUpdateTypes.Any()) return;

            AuditEntry(ActionFlags.Update, entity, entityType.Name);
        }

        /// <summary>
        /// Called when [delete created].
        /// </summary>
        /// <param name="entity">The entity.</param>
        [BusinessRule(ActionFlags.Delete)]
        public virtual void OnDeleteCreated(object entity)
        {
            var entityType = GetEntityType(entity);
            if (_validDeleteTypes.Contains(entityType) == false && _validDeleteTypes.Any()) return;

            AuditEntry(ActionFlags.Delete, entity, entityType.Name);
        }

        private void AuditEntry(ActionFlags flag, object entity, string name)
        {
            if (string.IsNullOrWhiteSpace(_currentuserId)) return;

            //var entityState = GetObjectState
            //    ((IObjectContextAdapter));
        }

        private static Dictionary<string, object> ToDictionary(IDataRecord record)
        {
            var result = new Dictionary<string, object>();

            for (var keyIndex = 0; keyIndex < record.FieldCount; keyIndex++)
            {
                var fieldType = record.GetFieldType(keyIndex);

                if (fieldType == typeof(byte[]))
                    result[record.GetName(keyIndex)] = "(Blob)";
                else if (Definitions.AllBasicValueAndStringTypes.Contains(fieldType))
                    result[record.GetName(keyIndex)] = record.GetValue(keyIndex);
            }

            return result;
        }
    }

    /// <summary>
    /// Extension methods
    /// </summary>
    public static class FluentAuditTrailExtension
    {
        /// <summary>
        /// Extension method to add AuditTrail to a DbContext
        /// </summary>
        /// <param name="context"></param>
        /// <param name="currentUserId"></param>
        /// <returns></returns>
        public static IBusinessDbContext UseAuditTrail<T, TEntity>(this IBusinessDbContext context, string currentUserId)
            where T : DbContext
        {
            context.AddController(new AuditTrailController<T, TEntity>((T)context, currentUserId));

            return context;
        }

    }
}
