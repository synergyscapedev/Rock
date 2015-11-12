<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.PersonSelect" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('a.btn-checkin-select').click(function () {
            $(this).siblings().attr('onclick', 'return false;');
        });
    });
</script>


<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>


    <Rock:ModalAlert ID="maWarning" runat="server" />


    <div class="checkin-header">
        <h1><asp:Literal ID="lFamilyName" runat="server"></asp:Literal></h1>
    </div>
                
    <div class="checkin-body">
        
        <div class="checkin-scroll-panel">
            <div class="scroller">

                <div class="control-group checkin-body-container">
                    <label class="control-label">Select Person(s)</label>
                    <div class="controls checkin-timelist btn-group" data-toggle="buttons-checkbox">
                        <asp:Repeater ID="rSelection" runat="server" OnItemCommand="rSelection_ItemCommand" OnItemDataBound="rSelection_ItemDataBound">
                                <ItemTemplate>
                                <button type="button" person-id='<%# Eval("Person.Id") %>' class="btn btn-default btn-lg btn-checkbox">
                                    <i class="fa fa-square-o"></i>
                                    <div><%# Container.DataItem.ToString() %></div>
                                </button>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </div>
                  <asp:HiddenField ID="hfTimes" runat="server"></asp:HiddenField>
            </div>
        </div>

    </div>
        
   

    <div class="checkin-footer">   
        <div class="checkin-actions">
            <asp:LinkButton CssClass="btn btn-default" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
            <asp:LinkButton CssClass="btn btn-default" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
           <%-- <asp:LinkButton CssClass="btn btn-Info" ID="lbSave" runat="server" OnClick="lbSave_Click" Text="Save" />--%>
            <asp:LinkButton CssClass="btn btn-primary" ID="lbSelect" runat="server" data-loading-text="Printing..." OnClientClick="return GetTimeSelection();" OnClick="lbSelect_Click" Text="Check-in" />
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
