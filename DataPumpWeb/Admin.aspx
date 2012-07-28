<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Admin.aspx.cs" Inherits="DataPumpWeb.Admin" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>DataPump Administration</title>
    <link type="text/css" rel="Stylesheet" href="Styles/Site.css" />
</head>
<body>
    <form id="form1" runat="server">
    <div class="SharepointGrid">
        <table id="sourceTargeGrid" class="display  SharepointGrid">
            <thead>
                <tr>
                    <th><a href="">Insert</a></th>
                    <th>SourceTargetId</th>
                    <th>SQL Source</th>
                    <th>Connection Name</th>
                    <th>Connection String</th>
                    <th>Sharepoint Target</th>
                    <th>List Id</th>
                </tr>
            </thead>
            <tbody></tbody>
            <tfoot>
                <tr>
                    <th></th>
                    <th></th>
                    <th></th>
                    <th></th>
                    <th></th>
                    <th></th>
                    <th></th>
                </tr>
            </tfoot>
        </table>    
    </div>
    <div id="editArea" style="display: none"></div>
    <asp:HiddenField ID="sourceTargetData" runat="server" />
    <script type="text/javascript" src="Scripts/jquery-1.7.1.min.js"></script>
    <script type="text/javascript" src="Scripts/jquery.dataTables.min.js"></script>
    <script type="text/javascript" src="http://ajax.microsoft.com/ajax/jquery.templates/beta1/jquery.tmpl.min.js"></script>
    <script type="text/javascript" src="Scripts/json2.js"></script>
    <script id="dataEntryTemplate" type="text/x-jquery-tmpl"> 
        <a href="" onclick="DeleteSourceTarget(${SourceTargetId}); return false;">Delete</a> | 
        <a href="" onclick="UpdateSourceTarget(${SourceTargetId}); return false;">Update</a>
        <table>
            <tr>
                <td>SQL Source</td>
                <td><input id="sqlSource${SourceTargetId}" value="${SQLSource}" class="DataEntry"/></td>
            </tr>
            <tr>
                <td>Connection Name</td>
                <td><input id="connectionName${SourceTargetId}" value="${ConnectionName}" class="DataEntry"/></td>
            </tr>
            <tr>
                <td>Connection String</td>
                <td><input id="connectionString${SourceTargetId}" value="${ConnectionString}" class="DataEntry"/></td>
            </tr>
            <tr>
                <td>SharePoint Target</td>
                <td><input id="sharePointTarget${SourceTargetId}" value="${SharePointTarget}" class="DataEntry"/></td>
            </tr>
            <tr>
                <td>List Id</td>
                <td><input id="listId${SourceTargetId}" value="${ListId}" class="DataEntry"/></td>
            </tr>
        </table>
    </script>
    <script id="insertTemplate" type="text/x-jquery-tmpl"> 
        <table>
            <tr>
                <td>SQL Source</td>
                <td><input id="sqlSourceInsert" class="DataEntry"/></td>
            </tr>
            <tr>
                <td>Connection Name</td>
                <td><input id="connectionNameInsert" class="DataEntry"/></td>
            </tr>
            <tr>
                <td>Connection String</td>
                <td><input id="connectionStringInsert" class="DataEntry"/></td>
            </tr>
            <tr>
                <td>SharePoint Target</td>
                <td><input id="sharePointTargetInsert" class="DataEntry"/></td>
            </tr>
            <tr>
                <td>List Id</td>
                <td><input id="listIdInsert" class="DataEntry"/></td>
            </tr>
        </table>
    </script>
    <script type="text/javascript">
        function fnFormatDetails(nTr) {
            var aData = oTable.fnGetData(nTr);

            var tableBody = $.tmpl("dataEntryTemplate", aData);
            $("#editArea").html(tableBody);
            oTable.fnOpen(nTr, $("#editArea").html(), 'details');
        }

        function UpdateSourceTarget(id) {
            var sourceTarget = {};
            sourceTarget.SQLSource = $("#sqlSource" + id.toString()).val();
            sourceTarget.ConnectionName = $("#connectionName" + id.toString()).val();
            sourceTarget.ConnectionString = $("#connectionString" + id.toString()).val();
            sourceTarget.SharePointTarget = $("#sharePointTarget" + id.toString()).val();
            sourceTarget.ListId = $("#listId" + id.toString()).val();
            sourceTarget.SourceTargetId = id;

            $.ajax({
                url: "Admin.aspx/",
                async: false,
                type: "POST",
                data: "{SourceTarget : " + JSON.stringify(sourceTarget) + "}",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    alert(XMLHttpRequest.status);
                    alert(XMLHttpRequest.responseText);

                },
                success: function (msg) {
                    detailData = eval('(' + msg.d + ')');

                    var tableBody = $.tmpl("detailTableTemplate", detailData);
                    $("#detailBody").html(tableBody);

                    //  Is there a search term to highlight?
                    var searchTerm = document.getElementById("searchInput").value;

                    if (searchTerm.length > 0) {
                        Highlight(searchTerm, "#updateArea");
                    }

                    oTable.fnOpen(nTr, $("#updateArea").html(), 'details');
                }
            });
        }
    </script>
    <script type="text/javascript">
        var aaData = JSON.parse($("#sourceTargetData").val());
        var oTable;

        $(document).ready(function () {
            oTable = $('#sourceTargeGrid').dataTable({
                "bProcessing": true,
                "bSort": true,
                "sPaginationType": "full_numbers",
                "aoColumnDefs": [
                    {
                        "fnRender": function (oObj) {
                            return '<img src="' + oObj.aData.ImageRef + '"/>';
                        },
                        "mDataProp": "ImageRef",
                        "aTargets": [0],
                        "sWidth": ".3em",
                        "bSortable": false
                    },
					{ "mDataProp": "SourceTargetId", "aTargets": [1], "bSortable": true, "bSearchable": true },
                    { "mDataProp": "SQLSource", "aTargets": [2], "bSortable": true, "bSearchable": true },
                    { "mDataProp": "ConnectionName", "aTargets": [3], "bSortable": true, "bSearchable": true },
                    { "mDataProp": "ConnectionString", "aTargets": [4], "bSortable": true, "bSearchable": true },
                    { "mDataProp": "SharePointTarget", "aTargets": [5], "bSortable": true, "bSearchable": true },
                    { "mDataProp": "ListId", "aTargets": [6], "bSortable": true, "bSearchable": true }
                ],
                "aaData": aaData,
                "oLanguage": {
                    "sSearch": "Search all columns:"
                },
                "aaSorting": [[1, "asc"]]
            });

            //  Assign click to images
            $('#sourceTargeGrid tbody td img').live('click', function () {
                var nTr = this.parentNode.parentNode;
                if (this.src.match('minus')) {
                    /* This row is already open - close it */
                    this.src = "images/plus.jpg";
                    oTable.fnClose(nTr);
                }
                else {
                    /* Open this row */
                    this.src = "images/minus.jpg";
                    fnFormatDetails(nTr);
                }
            });

            $("#dataEntryTemplate").template("dataEntryTemplate");
        });
    </script>
    </form>
</body>
</html>
