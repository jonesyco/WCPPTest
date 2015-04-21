function doClientPrint() {
    
    var printerSettings = $("#codeGenForm").serialize();

    //store printer settings in the server...

 
    $.post("CodeGenerator/PrintCommands/ClientPrinterSettings", printerSettings);

    // Launch WCPP at the client side for printing...
    var sessionId = $("#sid").val();
    //var quantity = $("#qty").val();
   // jsWebClientPrint.print("sid=" + sessionId, "quantity=" + qty);
    jsWebClientPrint.print("sid=" + sessionId);
}



$(document).ready(function () {
    //jQuery-based Wizard
    $("#codeGenForm").formToWizard();

    //change printer options based on user selection
    $("#pid").change(function () {
        var printerId = $("select#pid").val();

        displayInfo(printerId);
        hidePrinters();
        if (printerId == 2)
            $("#installedPrinters").show();
        else if (printerId == 3)
            $("#netPrinter").show();
        else if (printerId == 4)
            $("#parallelPrinter").show();
        else if (printerId == 5)
            $("#serialPrinter").show();
    });

    hidePrinters();
    displayInfo(0);


});

function displayInfo(i) {
    if (i == 0)
        $("#info").html("Print commands will be sent to the \"Default Printer\" without a print dialog.");
    else if (i == 1)
        $("#info").html("This will make the WCPP to display the Printer dialog so you can select which printer you want to use.");
    else if (i == 2)
        $("#info").html("Please specify the <b>Printer's Name</b> as it figures installed under your system.");
    else if (i == 3)
        $("#info").html("Please specify the Network Printer info.<br /><strong>On Linux &amp; Mac</strong> it's recommended you install the printer through <strong>CUPS</strong> and set the assigned printer name to the <strong>\"Use an installed Printer\"</strong> option on this demo.");
    
}

function hidePrinters() {
    $("#installedPrinter").hide(); $("#netPrinter").hide(); $("#parallelPrinter").hide(); $("#serialPrinter").hide();
}