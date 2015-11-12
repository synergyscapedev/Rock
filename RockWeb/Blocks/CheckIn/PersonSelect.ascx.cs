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
using Rock.CheckIn;
using Rock.Model;

namespace RockWeb.Blocks.CheckIn
{
    [DisplayName("Person Select")]
    [Category("Check-in")]
    [Description("Lists people who match the selected family to pick to checkin.")]
    public partial class PersonSelect : CheckInBlock
    {
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            RockPage.AddScriptLink( "~/Scripts/iscroll.js" );
            RockPage.AddScriptLink( "~/Scripts/CheckinClient/checkin-core.js" );

            if ( CurrentWorkflow == null || CurrentCheckInState == null )
            {
                NavigateToHomePage();
            }
            else
            {
                if ( !Page.IsPostBack )
                {
                    ClearSelection();

                    var family = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                        .FirstOrDefault();

                    if ( family == null )
                    {
                        GoBack();
                    }

                    lFamilyName.Text = family.ToString();

                    if ( family.People.Count == 1 )
                    {
                        if ( UserBackedUp )
                        {
                            GoBack();
                        }
                        else
                        {
                            family.People.FirstOrDefault().Selected = true;
                            ProcessSelection();
                        }
                    }
                    else
                    {

                        string script = string.Format(@"
    <script>
        function GetTimeSelection() {{
            var ids = '';
            $('div.checkin-timelist button.active').each( function() {{
                ids += $(this).attr('person-id') + ',';
            }});
            if (ids == '') {{
                bootbox.alert('Please select at least one time');
                return false;
            }}
            else
            {{
                $('#{0}').button('loading')
                $('#{1}').val(ids);
                return true;
            }}
        }}
    </script>
", lbSelect.ClientID, hfTimes.ClientID);
                        Page.ClientScript.RegisterClientScriptBlock(this.GetType(), "SelectTime", script);

                        rSelection.DataSource = family.People
                            .OrderByDescending( p => p.FamilyMember )
                            .ThenBy( p => p.Person.BirthYear )
                            .ThenBy( p => p.Person.BirthMonth )
                            .ThenBy( p => p.Person.BirthDay )
                            .ToList();

                        rSelection.DataBind();
                    }

                }
            }
        }

        /// <summary>
        /// Clear any previously selected people.
        /// </summary>
        private void ClearSelection()
        {
            foreach ( var family in CurrentCheckInState.CheckIn.Families )
            {
                foreach ( var person in family.People )
                {
                    person.ClearFilteredExclusions();
                    person.Selected = false;
                }
            }
        }

        protected void rSelection_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            if ( KioskCurrentlyActive )
            {
                int id = Int32.Parse( e.CommandArgument.ToString() );
                var person = CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                    .SelectMany( f => f.People.Where( p => p.Person.Id == id ) )
                    .FirstOrDefault();

                if ( person != null )
                {
                    person.Selected = true;
                    ProcessSelection();
                }
            }
        }

        protected void lbSelect_Click(object sender, EventArgs e)
        {
            if (KioskCurrentlyActive)
            {
                var family = CurrentCheckInState.CheckIn.Families.Where(f => f.Selected).FirstOrDefault();
                var selectedPeople = hfTimes.Value.SplitDelimitedValues();

                foreach (var person in selectedPeople)
                {
                    int id = int.Parse(person);
                    var sp = family.People.Where(p => p.Person.Id == id).FirstOrDefault();
                    if (sp != null)
                        sp.Selected = true;

                    var groupTypes = sp.GroupTypes.FirstOrDefault();
                    if (groupTypes != null)
                        groupTypes.Selected = true;

                    var groups = groupTypes.Groups.FirstOrDefault();
                    if (groups != null)
                        groups.Selected = true;

                    var location = groups.Locations.FirstOrDefault();
                    if (location != null)
                        location.Selected = true;

                    var schedule = location.Schedules.FirstOrDefault();
                    if (schedule != null)
                        schedule.Selected = true;

                }

                ProcessSelection(maWarning);

                //var location = CurrentCheckInState.CheckIn.Families.Where(f => f.Selected)
                //    .SelectMany(f => f.People.Where(p => p.Selected)
                //       .SelectMany(p => p.GroupTypes.Where(t => t.Selected)
                //          .SelectMany(t => t.Groups.Where(g => g.Selected)
                //             .SelectMany(g => g.Locations.Where(l => l.Selected)))))
                //    .FirstOrDefault();

                //if (location != null)
                //{
                //    foreach (var scheduleId in hfTimes.Value.SplitDelimitedValues())
                //    {
                //        int id = Int32.Parse(scheduleId);
                //        var schedule = location.Schedules.Where(s => s.Schedule.Id == id).FirstOrDefault();
                //        if (schedule != null)
                //        {
                //            schedule.Selected = true;
                //        }
                //    }

                //    ProcessSelection(maWarning);
                //}
            }
        }
        protected void lbSave_Click(object sender, EventArgs e)
        {
            //GoBack();
        }
        protected void lbBack_Click( object sender, EventArgs e )
        {
            GoBack();
        }

        protected void lbCancel_Click( object sender, EventArgs e )
        {
            CancelCheckin();
        }

        protected void rSelection_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            var person = e.Item.DataItem as CheckInPerson;

            if ( ! string.IsNullOrEmpty( person.SecurityCode ) )
            {
                var linkButton = e.Item.FindControl( "lbSelect" ) as LinkButton;
                linkButton.AddCssClass( "btn-dimmed" );
            }
        }

        protected void ProcessSelection()
        {
            var obj = CurrentCheckInState.CheckIn.Families.Where(f => f.Selected)
                        .FirstOrDefault();


            ProcessSelection( maWarning, () => CurrentCheckInState.CheckIn.Families.Where( f => f.Selected )
                .SelectMany( f => f.People.Where( p => p.Selected )
                    .SelectMany( p => p.GroupTypes.Where( t => !t.ExcludedByFilter ) ) )
                .Count() <= 0,
                "<p>Sorry, based on your selection, there are currently not any available locations that can be checked into.</p>" );
        }

    }
}