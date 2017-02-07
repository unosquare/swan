using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Unosquare.Swan.AspNetCore.Models
{
    /// <summary>
    /// Represents an AuditTrail Entry
    /// </summary>
    public interface IAuditTrailEntry
    {
        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>
        /// The user identifier.
        /// </value>
        string UserId { get; set; }
        /// <summary>
        /// Gets or sets the name of the table.
        /// </summary>
        /// <value>
        /// The name of the table.
        /// </value>
        string TableName { get; set; }
        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        /// <value>
        /// The action.
        /// </value>
        int Action { get; set; }
        /// <summary>
        /// Gets or sets the json body.
        /// </summary>
        /// <value>
        /// The json body.
        /// </value>
        string JsonBody { get; set; }
        /// <summary>
        /// Gets or sets the date created.
        /// </summary>
        /// <value>
        /// The date created.
        /// </value>
        DateTime DateCreated { get; set; }
    }
}
