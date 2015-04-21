using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Neodynamic.SDK.Web;

namespace WCPPTest.Controllers
{
    public class CodeGeneratorController : Controller
    {
        SEMInspection _db = new SEMInspection();
        public string Last { get; set; }

        public ActionResult Start()
        {
            string lastUtilityPrinted = "SB.000000";
            if (_db.Utilities.Any())
            {
                lastUtilityPrinted = "SB." + _db.Utilities.Find(1).LastUtilityCodePrinted.Trim();
            }
            
            ViewData["lastUtilityCodeUsed"] = lastUtilityPrinted;
            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Start(FormCollection form)
        {
            int qty = Convert.ToInt32(form["quantity"]);

            int value = Convert.ToInt32(_db.Utilities.Find(1).LastUtilityCodePrinted.Trim().ToString());


            string printcode = "";
            string printerCommands = "";
            for (var i = 0; i < qty; i++)
            
            {
                value = value + 1;
                var paddingNeeded = 6 - value.ToString().Length;
                var decimalLength = value.ToString("D").Length + paddingNeeded;
                printcode = value.ToString("D" + decimalLength.ToString());
                printerCommands += "0x02L"; //<STX>L
                printerCommands += "0x02n"; //Use imperial inches as measure
                printerCommands += "D11"; //1x1 dot format        
                printerCommands += "0x0D1711A1600650054SauseBros"; // + printcode;
                printerCommands += "0x0D"; //Carriage Return
                printerCommands += "1e00"; // 1=no rotation E=Code128 11=repeats D11 above
                printerCommands += "040"; //Barcode height 0.4 inches
                printerCommands += "0022"; //Row Address Y-value Up and down 0015
                printerCommands += "0009"; //Column Address X-Value Left and Right
                printerCommands += "SB." + printcode; //Set printable area to be SB. and printcode var
                printerCommands += "0x0D1711A2000000008SB." + printcode; // + printcode;
                printerCommands += "0x0D"; //Carriage Return
                //printerCommands += "E";
              
            }
            TempData["tdCommands"] = printerCommands.ToString();

           // ViewData["printerCommands"] = printerCommands.ToString();
            Last = printcode;
            var util = _db.Utilities.Find(1);
            util.LastUtilityCodePrinted = Last;
            _db.Entry(util).State = EntityState.Modified;
            await _db.SaveChangesAsync();
            _db.Entry(util).Reload();
            return RedirectToAction("Index");

        }

        public ActionResult Index()
        {
            var printerCommands = TempData["tdCommands"].ToString();
            ViewBag.printerCommands = printerCommands;
            return View();
        }

        
     

        public void PrintCommands(string sid, FormCollection form)
        {
           
                HttpApplicationStateBase app = HttpContext.Application;

                //Create a ClientPrintJob obj that will be processed at the client side by the WCPP
                ClientPrintJob cpj = new ClientPrintJob();

                var pc = form["printerCommands"];
                cpj.PrinterCommands += pc;
                cpj.PrinterCommands += "E";
                cpj.FormatHexValues = true;
              

                //get printer commands for this user id
                //var pc = form["printerCommands"].ToString();
                //var printerCommands = app[sid + PRINTER_COMMANDS];
                //if (printerCommands == null)
                //{
                //    cpj.PrinterCommands = pc.ToString();
                //    cpj.FormatHexValues = true;
                //}

                var printerTypeId = Convert.ToInt32(form["pid"]);
          
                //get printer settings for this user id
                //int printerTypeId = (int)app[sid + PRINTER_ID];

                if (printerTypeId == 0) //use default printer
                {
                    cpj.ClientPrinter = new DefaultPrinter();
                }
                else if (printerTypeId == 1) //show print dialog
                {
                    cpj.ClientPrinter = new UserSelectedPrinter();
                }
                else if (printerTypeId == 2) //use specified installed printer
                {
                    cpj.ClientPrinter = new InstalledPrinter(app[sid + INSTALLED_PRINTER_NAME].ToString());
                }
                else if (printerTypeId == 3) //use IP-Ethernet printer
                {
                    cpj.ClientPrinter = new NetworkPrinter(app[sid + NET_PRINTER_HOST].ToString(), int.Parse(app[sid + NET_PRINTER_PORT].ToString()));
                }
                else if (printerTypeId == 4) //use Parallel Port printer
                {
                    cpj.ClientPrinter = new ParallelPortPrinter(app[sid + PARALLEL_PORT].ToString());
                }
                else if (printerTypeId == 5) //use Serial Port printer
                {
                    cpj.ClientPrinter = new SerialPortPrinter(app[sid + SERIAL_PORT].ToString(),
                                                              int.Parse(app[sid + SERIAL_PORT_BAUDS].ToString()),
                                                              (System.IO.Ports.Parity)Enum.Parse(typeof(System.IO.Ports.Parity), app[sid + SERIAL_PORT_PARITY].ToString()),
                                                              (System.IO.Ports.StopBits)Enum.Parse(typeof(System.IO.Ports.StopBits), app[sid + SERIAL_PORT_STOP_BITS].ToString()),
                                                              int.Parse(app[sid + SERIAL_PORT_DATA_BITS].ToString()),
                                                              (System.IO.Ports.Handshake)Enum.Parse(typeof(System.IO.Ports.Handshake), app[sid + SERIAL_PORT_FLOW_CONTROL].ToString()));
                }

                //Send ClientPrintJob back to the client

                cpj.SendToClient(System.Web.HttpContext.Current.Response);

          

            

        }

         //CommonLibrary.UpdateUtilityRecord(Last);

        const string PRINTER_ID = "-PID";
        const string INSTALLED_PRINTER_NAME = "-InstalledPrinterName";
        const string NET_PRINTER_HOST = "-NetPrinterHost";
        const string NET_PRINTER_PORT = "-NetPrinterPort";
        const string PARALLEL_PORT = "-ParallelPort";
        const string SERIAL_PORT = "-SerialPort";
        const string SERIAL_PORT_BAUDS = "-SerialPortBauds";
        const string SERIAL_PORT_DATA_BITS = "-SerialPortDataBits";
        const string SERIAL_PORT_STOP_BITS = "-SerialPortStopBits";
        const string SERIAL_PORT_PARITY = "-SerialPortParity";
        const string SERIAL_PORT_FLOW_CONTROL = "-SerialPortFlowControl";
        const string PRINTER_COMMANDS = "-PrinterCommands";
       

        [HttpPost]
        public void ClientPrinterSettings(string sid,
                                             string pid,
                                             string installedPrinterName,
                                             string netPrinterHost,
                                             string netPrinterPort,
                                             string parallelPort,
                                             string serialPort,
                                             string serialPortBauds,
                                             string serialPortDataBits,
                                             string serialPortStopBits,
                                             string serialPortParity,
                                             string serialPortFlowControl,
                                             string printerCommands)
        {
            try
            {
                HttpApplicationStateBase app = HttpContext.Application;
             
               
                //save the type of printer selected by the user
                //int i = 0;
                int i = int.Parse(pid);
                app[sid + PRINTER_ID] = i;

                if (i == 2)
                {
                    app[sid + INSTALLED_PRINTER_NAME] = installedPrinterName;
                }
                else if (i == 3)
                {
                    app[sid + NET_PRINTER_HOST] = netPrinterHost;
                    app[sid + NET_PRINTER_PORT] = netPrinterPort;
                }
                else if (i == 4)
                {
                    app[sid + PARALLEL_PORT] = parallelPort;
                }
                else if (i == 5)
                {
                    app[sid + SERIAL_PORT] = serialPort;
                    app[sid + SERIAL_PORT_BAUDS] = serialPortBauds;
                    app[sid + SERIAL_PORT_DATA_BITS] = serialPortDataBits;
                    app[sid + SERIAL_PORT_FLOW_CONTROL] = serialPortFlowControl;
                    app[sid + SERIAL_PORT_PARITY] = serialPortParity;
                    app[sid + SERIAL_PORT_STOP_BITS] = serialPortStopBits;
                }

                //save the printer commands specified by the user
                app[sid + PRINTER_COMMANDS] = printerCommands;
            

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public ActionResult Reprint(string utilityId)
        {
            ClientPrintJob cpj = new ClientPrintJob { ClientPrinter = new UserSelectedPrinter() };
            cpj.PrinterCommands += "0x02L0x02nD110x0D1711A1600650054SauseBros0x0D1e0004000220009" + utilityId + "0x0D1711A2000000008" + utilityId + "0x0DE"; //<STX>L
            cpj.FormatHexValues = true;
            cpj.SendToClient(System.Web.HttpContext.Current.Response);
            //cpj.PrinterCommands += "0x02n"; //Use imperial inches as measure
            //cpj.PrinterCommands += "D11"; //1x1 dot format        
            //cpj.PrinterCommands += "0x0D1711A1600650054SauseBros"; // + printcode;
            //cpj.PrinterCommands += "0x0D"; //Carriage Return
            //cpj.PrinterCommands += "1e00"; // 1=no rotation E=Code128 11=repeats D11 above
            //cpj.PrinterCommands += "040"; //Barcode height 0.4 inches
            //cpj.PrinterCommands += "0022"; //Row Address Y-value Up and down 0015
            //cpj.PrinterCommands += "0009"; //Column Address X-Value Left and Right
            //cpj.PrinterCommands += utilityId; //Set printable area to be SB. and printcode var
            //cpj.PrinterCommands += "0x0D1711A2000000008" + utilityId; // + printcode;
            //cpj.PrinterCommands += "0x0D"; //Carriage Return
            //cpj.PrinterCommands += "E";
            return null;
        }

	}
}
