<%@ Control Language="C#" AutoEventWireup="true" CodeFile="EventSubscription.ascx.cs" Inherits="RockWeb.Blocks.Follow.EventSubscription" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class='fa fa-flag'></i> Following Events</h1>
            </div>

            <div class="panel-body">

                <asp:Repeater ID="rptEntityType" runat="server" OnItemDataBound="rptEntityType_ItemDataBound">
                    <ItemTemplate>
                        <%# Eval("FriendlyName") %>
                        <div class="clearfix">    
                            <ul>
                                <asp:Repeater ID="rptEvent" runat="server" >
                                    <ItemTemplate>
                                        <li>
                                            <asp:HiddenField ID="hfEvent" runat="server" Value='<%# Eval("Id") %>' />
                                            <Rock:RockCheckBox ID="cbEvent" runat="server" Label='<%# Eval("Name") %>' />
                                            <small><%# Eval("description") %></small>
                                        </li>
                                    </ItemTemplate>
                                </asp:Repeater>
                            </ul>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
                
                <Rock:NotificationBox ID="nbSaved" runat="server" NotificationBoxType="Success" Text="Your settings have been saved." Dismissable="true" Visible="false" />
                
                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                </div>

            </div>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
