using System.Linq;
using System.Web.Mvc;

namespace WCPPTest.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly SEMInspection _db = new SEMInspection();

        public ActionResult Index()
        {
            //var ecount = db.Equipment.Count();
            //ViewBag.EquipmentCount = ecount;
            //var ccount = db.InspectionCriteria.Count();
            //ViewBag.CriteriaCount = ccount;
            //var icount = db.Inspections.Count();
            //ViewBag.InspectionCount = icount;
            //var tcount = db.EquipmentTypes.Count();
            //ViewBag.TypeCount = tcount;
            // ViewData["repairs"] = db.Equipment.Where(a => a.Status == StatusEnum.Maintenance).OrderBy(a => a.NextInspectionDue).ToList();


            //var pending = CommonLibrary.GetTopLevelEquipmentList(false).OrderBy(e => e.NextInspectionDue);


            ViewBag.Dashboard = "Dashboard";
            return View();
        }
    }
}