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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Follow
{
    /// <summary>
    /// Block for displaying people that current person follows.  
    /// </summary>
    [DisplayName( "Person Suggestion List" )]
    [Category( "Follow" )]
    [Description( "Block for displaying people that have been suggested to current person to follow." )]

    public partial class PersonSuggestionList : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            base.OnInit( e );

            gSuggestions.DataKeyNames = new string[] { "Id" };
            gSuggestions.IsDeleteEnabled = false;
            gSuggestions.GridRebind += gSuggestions_GridRebind;
            gSuggestions.PersonIdField = "Id";

            var lbFollow = new LinkButton();
            lbFollow.ID = "lbFollow";
            lbFollow.CssClass = "btn btn-default btn-sm pull-left";
            lbFollow.Text = "<i class='fa fa-flag'></i> Unfollow";
            lbFollow.Click += lbFollow_Click;
            gSuggestions.Actions.AddCustomActionControl( lbFollow );

            var lbIgnore = new LinkButton();
            lbIgnore.ID = "lbIgnore";
            lbIgnore.CssClass = "btn btn-default btn-sm pull-left js-ignore";
            lbIgnore.Text = "<i class='fa fa-flag-o'></i> Unfollow";
            lbIgnore.Click += lbIgnore_Click;
            gSuggestions.Actions.AddCustomActionControl( lbIgnore );

            string ignoreScript = @"
    $('a.js-ignore').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to ignore the selected people?', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( gSuggestions, gSuggestions.GetType(), "ignoreScript", ignoreScript, true );
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
                BindGrid();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbUnfollow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void lbFollow_Click( object sender, EventArgs e )
        {
            var itemsSelected = new List<int>();
            gSuggestions.SelectedKeys.ToList().ForEach( f => itemsSelected.Add( f.ToString().AsInteger() ) );

            if ( itemsSelected.Any() )
            {
                var rockContext = new RockContext();
                var personAliasService = new PersonAliasService( rockContext );
                var followingService = new FollowingService( rockContext );

                var paQry = personAliasService.Queryable()
                    .Where( p => itemsSelected.Contains( p.PersonId ) )
                    .Select( p => p.Id );

                int personAliasEntityTypeId = EntityTypeCache.Read( "Rock.Model.PersonAlias" ).Id;
                foreach ( var following in followingService.Queryable()
                    .Where( f =>
                        f.EntityTypeId == personAliasEntityTypeId &&
                        paQry.Contains( f.EntityId ) &&
                        f.PersonAliasId == CurrentPersonAlias.Id ) )
                {
                    followingService.Delete( following );
                }

                rockContext.SaveChanges();
            }

            BindGrid();
        }

        void lbIgnore_Click( object sender, EventArgs e )
        {
            var itemsSelected = new List<int>();
            gSuggestions.SelectedKeys.ToList().ForEach( f => itemsSelected.Add( f.ToString().AsInteger() ) );

            if ( itemsSelected.Any() )
            {
                var rockContext = new RockContext();
                var personAliasService = new PersonAliasService( rockContext );
                var followingService = new FollowingService( rockContext );

                var paQry = personAliasService.Queryable()
                    .Where( p => itemsSelected.Contains( p.PersonId ) )
                    .Select( p => p.Id );

                int personAliasEntityTypeId = EntityTypeCache.Read( "Rock.Model.PersonAlias" ).Id;
                foreach ( var following in followingService.Queryable()
                    .Where( f =>
                        f.EntityTypeId == personAliasEntityTypeId &&
                        paQry.Contains( f.EntityId ) &&
                        f.PersonAliasId == CurrentPersonAlias.Id ) )
                {
                    followingService.Delete( following );
                }

                rockContext.SaveChanges();
            }

            BindGrid();
        }
        /// <summary>
        /// Handles the GridRebind event of the gSuggestions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gSuggestions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the UserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            if ( CurrentPersonAlias != null )
            {
                var rockContext = new RockContext();

                int personAliasEntityTypeId = EntityTypeCache.Read( "Rock.Model.PersonAlias" ).Id;

                var personAliasQry = new PersonAliasService( rockContext ).Queryable();
                var followedPersonIds = new FollowingService( rockContext ).Queryable()
                    .Where( f =>
                        f.EntityTypeId == personAliasEntityTypeId &&
                        f.PersonAliasId == CurrentPersonAlias.Id )
                    .Join( personAliasQry, s => s.EntityId, p => p.Id, ( s, p ) => p.PersonId )
                    .Distinct();

                var qry = new FollowingSuggestedService( new RockContext() )
                    .Queryable("SuggestionType")
                    .Where( s =>
                        s.SuggestionType != null &&
                        s.EntityTypeId == personAliasEntityTypeId &&
                        s.PersonAliasId == CurrentPersonAlias.Id )
                    .Join( personAliasQry, s => s.EntityId, p => p.Id, ( s, p ) => new { s, p } )
                    .Where( j => !followedPersonIds.Contains( j.p.PersonId ) )
                    .Select( j => new 
                    {
                        j.s.Id,
                        j.s.LastPromotedDateTime,
                        j.s.StatusChangedDateTime,
                        j.s.SuggestionType.ReasonNote,
                        j.s.Status,
                        Person = j.p.Person,
                        LastName = j.p.Person.LastName,
                        NickName = j.p.Person.NickName
                    } );

                // Sort
                SortProperty sortProperty = gSuggestions.SortProperty;
                if ( sortProperty == null )
                {
                    sortProperty = new SortProperty( new GridViewSortEventArgs( "LastName,NickName", SortDirection.Ascending ) );
                }

                gSuggestions.DataSource = qry.Sort( sortProperty )
                    .Select( s => new
                    {
                        s.Id,
                        s.LastPromotedDateTime,
                        s.StatusChangedDateTime,
                        s.ReasonNote,
                        s.Status,
                        StatusLabel = s.Status == FollowingSuggestedStatus.Ignored ?
                            "<span class='label label-warning'>Ignored</span>" :
                            "<span class='label label-success'>Suggested</span>",
                        s.Person,
                        s.LastName,
                        s.NickName
                    } )
                    .ToList();

                gSuggestions.DataBind();
            }
        }

        #endregion
}
}