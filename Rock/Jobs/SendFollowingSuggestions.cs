// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quartz;
using Rock.Attribute;
using Rock.Communication;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Jobs
{
    /// <summary>
    /// Sends a birthday email
    /// </summary>
    [GroupRoleField( "", "Recipient Group Type", "The group type/role that can receive following suggestions", true, order: 1, key: "RecipientGroupTypeRole" )]
    [DisallowConcurrentExecution]
    public class CalculateFollowingSuggestions : IJob
    {
        /// <summary> 
        /// Empty constructor for job initialization
        /// <para>
        /// Jobs require a public empty constructor so that the
        /// scheduler can instantiate the class whenever it needs.
        /// </para>
        /// </summary>
        public CalculateFollowingSuggestions()
        {
        }

        /// <summary>
        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public void Execute( IJobExecutionContext context )
        {
            JobDataMap dataMap = context.JobDetail.JobDataMap;
            Guid? groupRoleGuid = dataMap.GetString( "RecipientGroupTypeRole" ).AsGuidOrNull();
            if ( groupRoleGuid.HasValue )
            {
                var rockContext = new RockContext();
                var followingSuggestedService = new FollowingSuggestedService( rockContext );

                // The people who are eligible to get following suggestions based on the group type setting for this job
                var eligiblePersonIds = new GroupMemberService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( m =>
                        m.GroupRole.Guid.Equals( groupRoleGuid.Value ) &&
                        m.GroupMemberStatus == GroupMemberStatus.Active )
                    .Select( m => m.PersonId )
                    .Distinct();

                // The eligible people that have actually subscribed to one or more following events
                var followerPersonIds = new FollowingEventSubscriptionService( rockContext )
                    .Queryable()
                    .Where( f => eligiblePersonIds.Contains( f.PersonAlias.PersonId ) )
                    .Select( f => f.PersonAlias.PersonId )
                    .Distinct()
                    .ToList();

                // Get the primary person alias id for each of the followers
                var primaryAliasIds = new Dictionary<int, int>();
                new PersonAliasService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( a =>
                        followerPersonIds.Contains( a.PersonId ) &&
                        a.PersonId == a.AliasPersonId )
                    .ToList()
                    .ForEach( a => primaryAliasIds.AddOrIgnore( a.PersonId, a.Id ) );

                // Get current date/time. 
                var timestamp = RockDateTime.Now;

                foreach ( var suggestionType in new FollowingSuggestionTypeService( rockContext ).Queryable() )
                {
                    // Get the suggestion type component
                    var suggestionComponent = suggestionType.GetSuggestionComponent();
                    if ( suggestionComponent != null )
                    {
                        // Get the entitytype for this suggestion type
                        var entityType = EntityTypeCache.Read( suggestionComponent.FollowedType );
                        if ( entityType != null )
                        {
                            // Call the components method to return all of it's suggestions
                            var personEntitySuggestions = suggestionComponent.GetSuggestions( followerPersonIds, rockContext );

                            // If any suggestions were returned by the component
                            if ( personEntitySuggestions.Any() )
                            {
                                int entityTypeId = entityType.Id;
                                string reasonNote = suggestionType.ReasonNote;

                                // Get a list of the suggested followers
                                var suggestedFollowerPersonIds = personEntitySuggestions
                                    .Select( s => s.PersonId )
                                    .Distinct()
                                    .ToList();

                                // Read all the existing suggestions for this type and the returned followers
                                var existingSuggestions = followingSuggestedService
                                    .Queryable()
                                    .Where( s =>
                                        s.SuggestionTypeId == suggestionType.Id &&
                                        suggestedFollowerPersonIds.Contains( s.PersonAlias.PersonId ) )
                                    .ToList();

                                // Look  through the returned suggestions
                                foreach ( var personEntitySuggestion in personEntitySuggestions )
                                {
                                    // If this person had a primary alias id
                                    if ( primaryAliasIds.ContainsKey( personEntitySuggestion.PersonId ) )
                                    {
                                        // Look for existing suggestion for this person and entity
                                        var suggestion = existingSuggestions
                                            .Where( s =>
                                                s.PersonAlias.PersonId == personEntitySuggestion.PersonId &&
                                                s.EntityId == personEntitySuggestion.EntityId )
                                            .OrderByDescending( s => s.StatusChangedDateTime )
                                            .FirstOrDefault();

                                        // If not found, add one
                                        if ( suggestion == null )
                                        {
                                            suggestion = new FollowingSuggested();
                                            suggestion.EntityTypeId = entityTypeId;
                                            suggestion.EntityId = personEntitySuggestion.EntityId;
                                            suggestion.PersonAliasId = primaryAliasIds[personEntitySuggestion.PersonId];
                                            suggestion.SuggestionTypeId = suggestionType.Id;
                                            suggestion.Status = FollowingSuggestedStatus.PendingNotification;
                                            suggestion.StatusChangedDateTime = timestamp;
                                            followingSuggestedService.Add( suggestion );
                                        }
                                        else
                                        {
                                            // If found, and it has not been ignored, and it's time to promote again, update the promote date
                                            if ( suggestion.Status != FollowingSuggestedStatus.Ignored &&
                                                (
                                                    !suggestionType.ReminderDays.HasValue ||
                                                    !suggestion.LastPromotedDateTime.HasValue ||
                                                    suggestion.LastPromotedDateTime.Value.AddDays( suggestionType.ReminderDays.Value ) <= timestamp
                                                ) )
                                            {
                                                suggestion.StatusChangedDateTime = timestamp;
                                                suggestion.Status = FollowingSuggestedStatus.PendingNotification;
                                            }
                                        }
                                    }
                                }

                                // Save the suggestions for this type
                                rockContext.SaveChanges();

                            }
                        }
                    }
                }
            }
        }
    }
}
