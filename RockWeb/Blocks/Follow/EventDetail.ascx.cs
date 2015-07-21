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
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Follow;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Following
{
    [DisplayName( "Event Detail" )]
    [Category( "Follow" )]
    [Description( "Displays the details of the given financial event." )]
    public partial class EventDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        public int EventId
        {
            get { return ViewState["EventId"] as int? ?? 0; }
            set { ViewState["EventId"] = value; }
        }

        public int? EventEntityTypeId
        {
            get { return ViewState["EventEntityTypeId"] as int?; }
            set { ViewState["EventEntityTypeId"] = value; }
        }

        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            var followingEvent = new FollowingEvent { Id = EventId, EntityTypeId = EventEntityTypeId };
            BuildDynamicControls( followingEvent, false );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "eventId" ).AsInteger() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                FollowingEvent followingEvent = null;

                var eventService = new Rock.Model.FollowingEventService( rockContext );

                if ( EventId != 0 )
                {
                    followingEvent = eventService.Get( EventId );
                }

                if ( followingEvent == null )
                {
                    followingEvent = new Rock.Model.FollowingEvent();
                    eventService.Add( followingEvent );
                }

                followingEvent.Name = tbName.Text;
                followingEvent.IsActive = cbIsActive.Checked;
                followingEvent.Description = tbDescription.Text;
                followingEvent.EntityTypeId = cpEventType.SelectedEntityTypeId;

                rockContext.SaveChanges();

                followingEvent.LoadAttributes( rockContext );
                Rock.Attribute.Helper.GetEditValues( phAttributes, followingEvent );
                followingEvent.SaveAttributeValues( rockContext );
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the cpEventType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void cpEventType_SelectedIndexChanged( object sender, EventArgs e )
        {
            var event = new FollowingEvent { Id = EventId, EntityTypeId = cpEventType.SelectedEntityTypeId };
            BuildDynamicControls( event, true );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="eventId">The event identifier.</param>
        public void ShowDetail( int eventId )
        {
            FollowingEvent event = null;

            bool editAllowed = IsUserAuthorized( Authorization.EDIT );

            if ( !eventId.Equals( 0 ) )
            {
                event = new FollowingEventService( new RockContext() ).Get( eventId );
                editAllowed = editAllowed || event.IsAuthorized( Authorization.EDIT, CurrentPerson );
            }

            if ( event == null )
            {
                event = new FollowingEvent { Id = 0, IsActive = true };
            }

            EventId = event.Id;

            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( !editAllowed || !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FollowingEvent.FriendlyTypeName );
            }

            if ( readOnly )
            {
                ShowReadonlyDetails( event );
            }
            else
            {
                ShowEditDetails( event );
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="event">The event.</param>
        private void ShowEditDetails( FollowingEvent event )
        {
            if ( event.Id == 0 )
            {
                lActionTitle.Text = ActionTitle.Add( FollowingEvent.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = event.Name.FormatAsHtmlTitle();
            }

            hlInactive.Visible = !event.IsActive;

            SetEditMode( true );

            tbName.Text = event.Name;
            cbIsActive.Checked = event.IsActive;
            tbDescription.Text = event.Description;
            cpEventType.SetValue( event.EntityType != null ? event.EntityType.Guid.ToString().ToUpper() : string.Empty );
            tpBatchTimeOffset.SelectedTime = event.GetBatchTimeOffset();

            BuildDynamicControls( event, true );
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="event">The event.</param>
        private void ShowReadonlyDetails( FollowingEvent event )
        {
            SetEditMode( false );

            lActionTitle.Text = event.Name.FormatAsHtmlTitle();
            hlInactive.Visible = !event.IsActive;
            lEventDescription.Text = event.Description;

            DescriptionList descriptionList = new DescriptionList();

            if ( event.EntityType != null )
            {
                descriptionList.Add( "Event Type", event.EntityType.Name );
            }

            var timeSpan = event.GetBatchTimeOffset();
            if ( timeSpan.Ticks > 0 )
            {
                descriptionList.Add( "Batch Time Offset", timeSpan.ToString() );
            }

            lblMainDetails.Text = descriptionList.Html;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        private void BuildDynamicControls( FollowingEvent followingEvent, bool SetValues )
        {
            EventEntityTypeId = FollowingEvent.EntityTypeId;
            if ( followingEvent.EntityTypeId.HasValue )
            {
                var EventComponentEntityType = EntityTypeCache.Read( FollowingEvent.EntityTypeId.Value );
                var EventEntityType = EntityTypeCache.Read( "Rock.Model.FollowingEvent " );
                if ( EventComponentEntityType != null && EventEntityType != null )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        Rock.Attribute.Helper.UpdateAttributes( EventComponentEntityType.GetEntityType(), EventEntityType.Id, "EntityTypeId", EventComponentEntityType.Id.ToString(), rockContext );
                        followingEvent.LoadAttributes( rockContext );
                    }
                }
            }

            phAttributes.Controls.Clear();
            Rock.Attribute.Helper.AddEditControls( followingEvent, phAttributes, SetValues, BlockValidationGroup, new List<string> { "Active", "Order" } );
        }

        #endregion
    }
}