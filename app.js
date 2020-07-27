$(document).ready(function () {
    var isOpen = true;
    $("#btnOpen").click(function () {
        if (isOpen) {
            document.getElementById("mySidenav").style.width = "350px";
            document.getElementById("main").style.marginLeft = "350px";
            isOpen = false;
        }
        else {
            document.getElementById("mySidenav").style.width = "0";
            document.getElementById("main").style.marginLeft = "0";
            isOpen = true;
        }
    });
    $("#btnOpen").click();

    $('#SearchResult').hide();
    var SearchString = "";
    var startBracket = "( ";
    var endBracket = " )";
    function getUrlVars() {
        var vars = [], hash;
        var hashes = window.location.href.slice(window.location.href.indexOf('?') + 1).split('&');
        for (var i = 0; i < hashes.length; i++) {
            hash = hashes[i].split('=');
            vars.push(hash[0]);
            vars[hash[0]] = hash[1];
        }
        return vars;
    }

    $('.sidenav').keypress(function (e) {
        var key = e.which;
        if (key == 13)  // the enter key code
        {
            if ($('#MainFilter').val().trim().length > 0 || $('#FillingType').val().trim().length > 0 ||
                $('#location').val().trim().length > 0 || $('#trustee').val().trim().length > 0 ||
                $('#estateadmin').val().trim().length > 0 || $('#homephone').val().trim().length > 0 ||
                $('#email').val().trim().length > 0
            ) {
                $('#SearchRecords').click();
            } else {
                alert("Search field can't be empty.");
            }
            return false;
        }
    });

    var riding = getUrlVars()["riding"];
    $('#riding').val(riding);

    var Employee_Id = getUrlVars()["employeeId"];

    $('.collapse').collapse({
        toggle: true
    });

    $('#btnSearch').click(function () {
        $('#SearchButton').click();
    });

    $('#SearchButton').click(function () {
        if ($('#MainFilter').val().trim().length > 0 || $('#FillingType').val().trim().length > 0 ||
            $('#location').val().trim().length > 0 || $('#filemanager').val().trim().length > 0 ||
            $('#status').val().trim().length > 0 || $('#homephone').val().trim().length > 0 ||
            $('#email').val().trim().length > 0
            ) {
            draw = 0;
            waitingDialog.show("Loading Data...Please Wait.");
            $('.collapse').collapse('hide');

            var data = $('form').serialize();
            var f = {};

            f.MainFilter = $('#MainFilter').val();
            f.Location = $('#location').val();
            f.FileManager = $('#filemanager').val();
            f.Status = $('#status').val();
            f.Email = $('#email').val();
            f.HomePhone = $('#homephone').val();
            f.FillingType = $('#FillingType').val();
            f.division = riding;
            f.PageSize = 0;
            f.PageNumber = 1;
            f.ViewAll = true;

            if (f.MainFilter && f.MainFilter.length > 0) {
                SearchString += f.MainFilter;
            }

            if (f.Location && f.Location.length > 0 && f.Location != " ") {
                SearchString += ", " + f.Location;
            }

            if (f.FileManager && f.FileManager.length > 0) {
                SearchString += ", " + f.FileManager;
            }

            if (f.Status && f.Status.length > 0) {
                SearchString += ", " + f.Status;
            }

            if (f.Email && f.Email.length > 0) {
                SearchString += ", " + f.Email;
            }

            if (f.HomePhone && f.HomePhone.length > 0) {
                SearchString += ", " + f.HomePhone;
            }

            if (f.FillingType && f.FillingType.length > 0) {
                SearchString += ", " + f.FillingType;
            }

            if ($.fn.DataTable.isDataTable("#searchTable")) {
                $('#searchTable').DataTable().clear().destroy();
            }

            InitializeTable(f);
            $('#SearchResult').show();
            waitingDialog.hide();
        } else {
            alert("Search field can't be empty.");
        }
        
    });

    function InitializeTable(f) {
        if ($.fn.DataTable.isDataTable("#searchTable")) {
            $('#searchTable').DataTable().clear().destroy();
        }
        var table = $('#searchTable').DataTable({
            dom: 'l<"export">Bfrtip',
            buttons: [
                { extend: 'print', text: 'Print', title: 'Client Records - Print' },
                { extend: 'csv', text: 'Export to CSV', title: 'Client Records - CSV' }
                
            ],
            "lengthMenu": [[25, 50, 100, -1], [25, 50, 100, "All"]],
            fixedHeader: {
                header: true
            },
            "language": {
                "emptyTable": "Your search did not match any records"
            },
            "scrollY": "60vh",
            "bAutoWidth": false,
            "bPaginate": true, // hides the prev, next paging buttons
            "bInfo": true, // hides the showing 20 of 50 entries of the datatable
            "columnDefs": [
                { "width": "8%", "targets": 0 },
                { "width": "8%", "targets": 1 },
                { "width": "8%", "targets": 2 },
                { "width": "8%", "targets": 3 },
                { "width": "8%", "targets": 4 },
                { "width": "8%", "targets": 5 },
                { "width": "8%", "targets": 6 },
                { "width": "8%", "targets": 7 },
                { "width": "8%", "targets": 8 },
                { "width": "8%", "targets": 9 },
                { "width": "8%", "targets": 10 },
                { "width": "1%", "targets": 11 }
            ]     
        });

        //$('#searchTable_filter').css('float', 'left');
        $('#searchTable_length').css('float', 'right');
    };

    //get location
    var division = { 'division': getUrlVars()["riding"] };

    $.ajax({
        type: 'POST', url: 'search.aspx/getEmployees', data: JSON.stringify(division), contentType: 'application/json; charset=utf-8', dataType: 'json',
        success: function (r) {
            $.each(r.d, function (i, item) {
                $('#estateadmin').append($('<option>', {
                    value: item.Employee_ID,
                    text: item.EmployeeName
                }));
                $('#trustee').append($('<option>', {
                    value: item.Employee_ID,
                    text: item.EmployeeName
                }));
            });
        },
        error: function (r) {
            console.log(r);
        }
    });

    $.ajax({
        type: 'POST', url: 'search.aspx/getLocation', data: JSON.stringify(division), contentType: 'application/json; charset=utf-8', dataType: 'json',
        success: function (r) {
            $.each(r.d, function (i, item) {
                $('#location').append($('<option>', {
                    value: item.Id,
                    text: item.LocationName
                }));
            });
        },
        error: function (r) {
            console.log(r);
        }
    });

    $.ajax({
        type: 'POST', url: 'search.aspx/getFileManager', data: JSON.stringify(division), contentType: 'application/json; charset=utf-8', dataType: 'json',
        success: function (r) {
            $.each(r.d, function (i, item) {
                $('#filemanager').append($('<option>', {
                    value: item,
                    text: item
                }));
            });
        },
        error: function (r) {
            console.log(r);
        }
    });

    $.ajax({
        type: 'POST', url: 'search.aspx/getStatus', data: JSON.stringify(division), contentType: 'application/json; charset=utf-8', dataType: 'json',
        success: function (r) {
            $.each(r.d, function (i, item) {
                $('#status').append($('<option>', {
                    value: item,
                    text: item
                }));
            });
        },
        error: function (r) {
            console.log(r);
        }
    });

    function search(nameKey, myArray) { //search in array of object
        for (var i = 0; i < myArray.length; i++) {
            if (myArray[i].Source === nameKey) {
                return myArray[i];
            }
        }
    }

    function GetLocalString() {
        $.ajax({
            type: 'POST',
            url: '../../Localization/Convert.aspx/GetLocalString',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: '{EmployeeId:' + Employee_Id + ',SectionId:' + 108 + '}', //Sectionid 108 is for Search screen
            success: function (data) {
                var strings = data.d;

                for (var j = 0; j < $(".lnkTxt").length; j++) { //change text of buttons and column headers                    
                    if (j < 14) { // removing duplicate column headers
                        var elm = search($(".lnkTxt")[j].innerText, strings);
                        $(".lnkTxt")[j].innerText = elm.Destination;
                    }
                }

                var elm = search("Type in the debtor's name, file name or estate number", strings); //change text of placeholder                
                $("#MainFilter")[0].placeholder = elm.Destination;
            }
        });
    }

    $("#SearchRecords").click(function () {
        var data = {
            Division: division.division,
            MainFilter: $("#MainFilter").val(),
            Email: $("#email").val(),
            Phone: $("#homephone").val(),
            FilingType: $("#FillingType").val(),
            Location: $("#location").val(),
            TrusteeId: parseInt($("#trustee").val()),
            EstateAdminId: parseInt($("#estateadmin").val()),
            HasPhone: $("#hasphone").prop("checked"),
            HasEmail: $("#hasemail").prop("checked"),
            HasEstate: $("#hasestate").prop("checked")
        };
        console.log(data);
        waitingDialog.show("Loading Data...Please Wait.");
        $.ajax({
            type: 'POST',
            url: 'search.aspx/SearchClient',
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            data: JSON.stringify({ searchParam: data}), 
            success: function (data) {
                dataBindTable(data);
            }
        });
    });

    function dataBindTable(rows) {
        if ($.fn.DataTable.isDataTable("#searchTable")) {
            $('#searchTable').DataTable().clear().destroy();
        }
        $('#tbody').empty();
        var res = rows.d;
        var rows = '';
        $.each(res, function (i, item) {
            try {
                rows += createRow(item);                
            } catch (e) {
                console.log(e);
            }            
        });
        $('#tbody').append(rows);
        InitializeTable();
        waitingDialog.hide();
    }

    function createRow(row) {
        try {
            var td = "<td><a href='../../client/constituent.asp?id=" + row["Id"] + "&FN=" + encodeURIComponent(row["FileName"]) + "&SC=A' target='_parent' id='abc'>{0}</a></td>";
            var tr = "<tr>";
            var FileName = td.replace("{0}", row["FileName"]);
            var EstateNumber = td.replace("{0}", row["EstateNumber"]);
            var FirstName = td.replace("{0}", row["FirstName"]); 
            var MiddleName = td.replace("{0}", row["MiddleName"]); 
            var LastName = td.replace("{0}", row["LastName"]); 
            var FilingType = td.replace("{0}", row["FilingType"]); 
            var Email = td.replace("{0}", row["Email"]); 
            var Phone = td.replace("{0}", row["Phone"]); 
            var Trustee = td.replace("{0}", row["Trustee"]); 
            var EstateAdmin = td.replace("{0}", row["EstateAdmin"]); 
            var OfficeLocation = td.replace("{0}", row["OfficeLocation"]); 
            var Doc = "<td><a class='btn btnEdit' title='View Documents' href='/login/RedirectDocCase.asp?clientid=" + row["Id"] + "&Red=Docs'><span style='color:#337ab7' class='glyphicon glyphicon-file'></span></a></td>";
            var endtr = "</tr>";
            var _row = tr + FileName + EstateNumber + FirstName + MiddleName + LastName + FilingType + Email + Phone + Trustee + EstateAdmin + OfficeLocation + Doc + endtr;
            return _row;
        } catch (e) {
            throw e;
        }        
    }

    $("#btnClear").click(function () {
        $("#MainFilter").val("");
        $("#email").val("");
        $("#homephone").val("");
        $("#FillingType").val("-1");
        $("#location").val("-1");
        $("#trustee").val("-1");
        $("#estateadmin").val("-1");
        $("#hasphone").prop("checked", false);
        $("#hasemail").prop("checked", false);
        $("#hasestate").prop("checked", false);
    });
    
    //GetLocalString();
    $("#MainFilter").focus();
});
