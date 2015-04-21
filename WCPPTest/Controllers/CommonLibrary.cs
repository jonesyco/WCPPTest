using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Validation;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
using Microsoft.Ajax.Utilities;
using SEM.APP.Models;
using SEM.DAL;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using SendGrid;

namespace SEM.APP.Controllers
{
    public class CommonLibrary : Controller
    {
        private static readonly SEMInspection Db = new SEMInspection();

        public async static Task<bool> UpdateUtilityRecord(string last)
        {
            var util = Db.Utilities.Find(1);
            util.LastUtilityCodePrinted = last;
            Db.Entry(util).State = EntityState.Modified;
            await Db.SaveChangesAsync();
            Db.Entry(util).Reload();
            return true;
        }

        public async static Task<bool> SendEmail(string message)
        {
            var myMessage = new SendGridMessage();

            const string username = "azure_7abf8fd56e84ea01fc39b447fec3f091@azure.com";
            const string pswd = "u8i3w5hXVKtG6s2";
            var credentials = new NetworkCredential(username, pswd);


            // Add multiple addresses to the To field.
            var recipients = new List<String>
                {
                     @"Jon Jones <soulside@gmail.com>"
                };

            myMessage.AddTo(recipients);
            myMessage.From = new MailAddress("noreply@sause.com", "SOMAR-EM");
            myMessage.Subject = "SOMAR-EM Application Message";
            myMessage.Html += "<p><u>SOMAR Application Message:<u></p>";
            myMessage.Html += "<p>" + message.ToString() + "</p>";
            myMessage.Html += "<p style=\"color:#efefef\">This email and any files transmitted with it are confidential and intended solely for the use of the individual or entity to whom they are addressed. If you have received this email in error please notify the system manager. This message contains confidential information and is intended only for the individual named. If you are not the named addressee you should not disseminate, distribute or copy this e-mail. Please notify the sender immediately by e-mail if you have received this e-mail by mistake and delete this e-mail from your system. If you are not the intended recipient you are notified that disclosing, copying, distributing or taking any action in reliance on the contents of this information is strictly prohibited.</p>";
          
            myMessage.Text = message.ToString();

            // Create an Web transport for sending email.
            var transportWeb = new Web(credentials);

            // Send the email.
            // You can also use the **DeliverAsync** method, which returns an awaitable task.
            await transportWeb.DeliverAsync(myMessage);

            return true;
        } 

        #region "Status - Provide Status List"

        public static SelectList StatusList(int status)
        {
            var statuses = new Dictionary<int, string> {{1, "Active"}, {2, "Maintenance"}, {3, "Retired"}};
            var statusList = new SelectList(statuses, "Key", "Value", status);
            return statusList;
        }

        #endregion

        #region EquipmentId (utilityId)

        public static int EquipmentId(string utilityId)
        {
            using (var uContext = new SEMInspection())
            {
                var firstOrDefault = uContext.Equipment.FirstOrDefault(e => e.UtilityId == utilityId);
                if (firstOrDefault == null) return 0;
                var equipmentId = firstOrDefault.EquipmentId;
                return equipmentId > 0 ? equipmentId : 0;
            }
        }

        #endregion

        #region "Equipment - Get Detail"

        public static EquipmentVm GetEquipmentDetails(int equipmentId)
        {
            using (var dContext = new SEMInspection())
            {
                var m = dContext.Equipment.FirstOrDefault(e => e.EquipmentId == equipmentId);
                if (m != null)
                {
                    var equipmentDetail = new EquipmentVm
                    {
                        EquipmentId = m.EquipmentId,
                        EquipmentDescription = m.EquipmentDescription,
                        EquipmentLength = m.EquipmentLength,
                        EquipmentSize = m.EquipmentSize,
                        UtilityId = m.UtilityId,
                        Status = m.Status,
                        EquipmentTypeId = m.EquipmentTypeId,
                        EquipmentType = from et in dContext.EquipmentTypes
                            where et.EquipmentTypeId == m.EquipmentTypeId
                            select et.EquipmentTypeDesc,
                        ParentId = m.ParentId,
                        Manufacturer = m.Manufacturer,
                        Serial = m.Serial,
                        InitialInvDate = m.InitialInvDate,
                        NextInspectionDue = m.NextInspectionDue,
                        Level = m.Level,
                        Condition = m.Condition,
                        RemoveFromServiceDate = m.RemoveFromServiceDate,
                        DeletableCh = dContext.Equipment.Any(e => e.ParentId == m.EquipmentId)
                    };
                    return equipmentDetail;
                }
                return null;
            }
        }

        #endregion

        #region "Equipment - Get Top Level Equipment"

        public static List<EquipmentVm> GetTopLevelEquipmentList(bool showretired = true)
        {
            using (var eContext = new SEMInspection())
            {
                var filter = 0;
                if (showretired == false) filter = 3;
                var equipmentSource = (from m in eContext.Equipment
                    where m.Status != (StatusEnum) filter
                    where m.ParentId == null
                    select new EquipmentVm
                    {
                        Level = m.Level,
                        EquipmentId = m.EquipmentId,
                        EquipmentDescription = m.EquipmentDescription,
                        EquipmentLength = m.EquipmentLength,
                        EquipmentSize = m.EquipmentSize,
                        UtilityId = m.UtilityId,
                        InitialInvDate = m.InitialInvDate,
                        NextInspectionDue = m.NextInspectionDue,
                        Status = m.Status,
                        Manufacturer = m.Manufacturer,
                        Serial = m.Serial,
                        EquipmentTypeId = m.EquipmentTypeId,
                        EquipmentType = from et in eContext.EquipmentTypes
                            where et.EquipmentTypeId == m.EquipmentTypeId
                            select et.EquipmentTypeDesc,
                        ParentId = m.ParentId,
                        Condition = m.Condition,
                        RemoveFromServiceDate = m.RemoveFromServiceDate,
                        DeletableCh = eContext.Equipment.Any(e => e.ParentId == m.EquipmentId),
                        DeletableIns = eContext.InspectionResults.Any(i => i.Inspection.EquipmentId == m.EquipmentId)
                    }).ToList();
                var orderedEquipmentList = OrderEquipmentVmList(equipmentSource);
                orderedEquipmentList.Reverse();
                return orderedEquipmentList;
            }
        }

        #endregion

        #region "Last Inspection Date"

        public static string LastInspectionDate(int equipmentId)
        {
            using (var lContext = new SEMInspection())
            {
                var firstOrDefault =
                    lContext.Inspections.Where(d => d.EquipmentId == equipmentId).OrderBy(d => d.InspectionDate).Take(1);
                var lastInspectionDate = firstOrDefault.ToString();
                return lastInspectionDate;
            }
        }

        #endregion

        #region "Equipment - Build Ordered List"

        public static List<EquipmentVm> OrderEquipmentVmList(List<EquipmentVm> sourceList)
        {
          var orderedEquipmentList = new List<EquipmentVm>();
                //iterate through masterlist
                for (var i = 0; i < sourceList.Count(); i++)
                {
                    if (sourceList[i].ParentId != null &&
                        sourceList.Exists(t => t.EquipmentId == sourceList[i].ParentId))
                    {
                        //find parent position
                        var parent = sourceList.First(c => c.EquipmentId == sourceList[i].ParentId);
                        var parentIndex =
                            sourceList.FindIndex(c => c.EquipmentId == sourceList[i].ParentId);

                        //spawn new Result item then copy and reassign values
                        var listItem = new EquipmentVm
                        {
                            EquipmentId = sourceList[i].EquipmentId,
                            EquipmentTypeId = sourceList[i].EquipmentTypeId,
                            EquipmentType = sourceList[i].EquipmentType,
                            ParentId = sourceList[i].ParentId,
                            EquipmentDescription = sourceList[i].EquipmentDescription,
                            EquipmentSize = sourceList[i].EquipmentSize,
                            EquipmentLength = sourceList[i].EquipmentLength,
                            NextInspectionDue = sourceList[i].NextInspectionDue,
                            InitialInvDate = sourceList[i].InitialInvDate,
                            RemoveFromServiceDate = sourceList[i].RemoveFromServiceDate,
                            UtilityId = sourceList[i].UtilityId,
                            Manufacturer = sourceList[i].Manufacturer,
                            Serial = sourceList[i].Serial,
                            Status = sourceList[i].Status,
                            Level = parent.Level + 1,
                            Condition = sourceList[i].Condition,
                            EquipmentCondition = sourceList[i].EquipmentCondition
                        };
                        //insert item into list at one level below parent
                        orderedEquipmentList.Insert(parentIndex + 1, listItem);
                        //reassign masterlist item to null
                        // sourceList[i] = null;
                    }
                    else
                    {
                        //insert into list at end
                        orderedEquipmentList.Add(sourceList[i]);
                        //reassign masterlist item to null
                        //sourceList[i] = null;
                    }
                }
                //orderedEquipmentList.Reverse();
                return orderedEquipmentList;
            }

        #endregion

        #region "Equipment - Get Children"

        public static List<EquipmentVm> GetOrderedEquipmentList(bool showretired = true)
        {
            using (var oContext = new SEMInspection())
            {
                var filter = 0;
                if (showretired == false) filter = 3;
                var equipmentSource = (from m in oContext.Equipment
                    where m.Status != (StatusEnum) filter
                    select new EquipmentVm
                    {
                        Level = m.Level,
                        EquipmentId = m.EquipmentId,
                        EquipmentDescription = m.EquipmentDescription,
                        EquipmentLength = m.EquipmentLength,
                        EquipmentSize = m.EquipmentSize,
                        UtilityId = m.UtilityId,
                        InitialInvDate = m.InitialInvDate,
                        NextInspectionDue = m.NextInspectionDue,
                        Status = m.Status,
                        Manufacturer = m.Manufacturer,
                        Serial = m.Serial,
                        EquipmentTypeId = m.EquipmentTypeId,
                        EquipmentType = from et in oContext.EquipmentTypes
                            where et.EquipmentTypeId == m.EquipmentTypeId
                            select et.EquipmentTypeDesc,
                        ParentId = m.ParentId,
                        Condition = m.Condition
                    }).ToList();
                var orderedEquipmentList = OrderEquipmentVmList(equipmentSource);
                orderedEquipmentList.Reverse();
                return orderedEquipmentList;
            }
        }

        #endregion

        #region "Equipment - Get Equipment Family"

        public static List<EquipmentVm> GetEquipmentFamily(int equipmentId)
        {
            using (var fContext = new SEMInspection())
            {
                var sourceList = (from d in fContext.Equipment
                    where d.EquipmentId == equipmentId
                    where d.ParentId == equipmentId
                    select new EquipmentVm
                    {
                        EquipmentTypeId = d.EquipmentTypeId,
                        EquipmentDescription = d.EquipmentDescription,
                        EquipmentLength = d.EquipmentLength,
                        EquipmentSize = d.EquipmentSize,
                        UtilityId = d.UtilityId,
                        Status = d.Status,
                        EquipmentType = from et in fContext.EquipmentTypes
                            where et.EquipmentTypeId == d.EquipmentTypeId
                            select et.EquipmentTypeDesc,
                        ParentId = d.ParentId,
                        Level = d.Level,
                        NextInspectionDue = d.NextInspectionDue,
                        Condition = d.Condition
                    });
                return sourceList.ToList();
            }
        }

        #endregion

        #region "Equipment - Get Equipment Family"

        public async static Task<List<EquipmentVm>> GetEquipmentChildren(int? equipmentId)
        {
            using (var cContext = new SEMInspection())
            {
                var utilityid = cContext.Equipment.Find(equipmentId).UtilityId + ".";
                var sourceList = (from d in cContext.Equipment
                    where (d.UtilityId.Contains(utilityid))
                    where (d.EquipmentId != equipmentId)
                    select new EquipmentVm
                    {
                        EquipmentId = d.EquipmentId,
                        EquipmentTypeId = d.EquipmentTypeId,
                        EquipmentDescription = d.EquipmentDescription,
                        EquipmentLength = d.EquipmentLength,
                        EquipmentSize = d.EquipmentSize,
                        UtilityId = d.UtilityId,
                        Status = d.Status,
                        EquipmentType = from et in cContext.EquipmentTypes
                            where et.EquipmentTypeId == d.EquipmentTypeId
                            select et.EquipmentTypeDesc,
                        ParentId = d.ParentId,
                        Level = d.Level,
                        NextInspectionDue = d.NextInspectionDue,
                        Condition = d.Condition
                    });
                return sourceList.ToList();

            }
        }

        #endregion

        #region "Equipment Types - Get Detail"

        public static EquipmentTypesVm GetComponentDetail(int equipmentTypeId)
        {
            using (var gContext = new SEMInspection())
            {
                var equipmentTypeDetail = (from m in gContext.EquipmentTypes
                    where m.EquipmentTypeId == equipmentTypeId
                    select new EquipmentTypesVm
                    {
                        EquipmentTypeId = m.EquipmentTypeId,
                        EquipmentTypeParentId = m.EquipmentTypeParentId,
                        EquipmentTypeDesc = m.EquipmentTypeDesc,
                        EquipmentMaintIntervalDays = m.EquipmentMaintIntervalDays,
                        IsSelected = true,
                        EquipmentTypeParentDesc =
                            gContext.EquipmentTypes.Where(p => p.EquipmentTypeId == m.EquipmentTypeParentId)
                                .Select(p => p.EquipmentTypeDesc).FirstOrDefault(),
                        TypeLevel = 1,
                        SelectedComponent = new CheckBox
                        {
                            Text = m.EquipmentTypeDesc
                        }
                    }).FirstOrDefault();
                return equipmentTypeDetail;
            }
        }

        #endregion

        #region "Equipment Types - List"

        public async static Task<List<EquipmentTypesVm>> GetEquipmentComponentsTypes(int equipmentTypeId)
        {
            var family = new List<EquipmentTypesVm>();

            //first build list of all equipmentTypeIds 
            //var familytreelist = new List<int> {equipmentTypeId};
            var familytreelist = new List<int>();
            var componentList = FamilyBuilder(familytreelist, equipmentTypeId);

            //iterate through that list injecting the componentList[i] which is the equipmentTypeId
            for (var i = 0; i < componentList.Count(); i++)
            {
                var component = GetComponentDetail(componentList[i]);
                if (component == null) throw new ArgumentNullException("equipmentTypeId");

                //is the parent id in the list already? if so add this as a child just below and increase the level by 1
                if (family.Exists(e => e.EquipmentTypeId == component.EquipmentTypeParentId))
                {
                    var parentIndex = family.FindIndex(c => c.EquipmentTypeId == component.EquipmentTypeParentId);
                    var parent = family.First(c => c.EquipmentTypeId == component.EquipmentTypeParentId);
                    var newComponent = new EquipmentTypesVm
                    {
                        EquipmentTypeId = component.EquipmentTypeId,
                        EquipmentTypeParentId = component.EquipmentTypeParentId,
                        EquipmentTypeParentDesc = (from p in Db.EquipmentTypes
                            where p.EquipmentTypeId == component.EquipmentTypeId
                            select p.EquipmentTypeDesc).ToString(),
                        EquipmentMaintIntervalDays = component.EquipmentMaintIntervalDays,
                        EquipmentTypeDesc = component.EquipmentTypeDesc,
                        TypeLevel = parent.TypeLevel + 1,
                        SelectedComponent = new CheckBox
                        {
                            Text = component.EquipmentTypeDesc
                        },
                        IsSelected = true
                    };

                    family.Insert(parentIndex + 1, newComponent);
                }
                else
                {
                    family.Add(component);
                }
            }

            return family;
        }

        #endregion

        #region "Maintenance - Get Equipment List"
        public static List<EquipmentVm> GetMaintenanceList()
        {
           
            var equipmentSource = (from m in Db.Equipment
                                   where m.Status == StatusEnum.Maintenance                                 
                                   select new EquipmentVm
                                   {
                                       Level = m.Level,
                                       EquipmentId = m.EquipmentId,
                                       EquipmentDescription = m.EquipmentDescription,
                                       EquipmentLength = m.EquipmentLength,
                                       EquipmentSize = m.EquipmentSize,
                                       UtilityId = m.UtilityId,
                                       InitialInvDate = m.InitialInvDate,
                                       NextInspectionDue = m.NextInspectionDue,
                                       Status = m.Status,
                                       Manufacturer = m.Manufacturer,
                                       Serial = m.Serial,
                                       EquipmentTypeId = m.EquipmentTypeId,
                                       EquipmentType = from et in Db.EquipmentTypes
                                                       where et.EquipmentTypeId == m.EquipmentTypeId
                                                       select et.EquipmentTypeDesc,
                                       ParentId = m.ParentId,
                                       Condition = m.Condition
                                   }).ToList();
            var orderedEquipmentList = OrderEquipmentVmList(equipmentSource);
            orderedEquipmentList.Reverse();
            return orderedEquipmentList;
        }
        #endregion

        #region Conditions

        public static SelectList ConditionList()
        {
            return new SelectList(Db.Conditions, "ConditionID", "ConditionName");
        }

        #endregion

        #region "Inspection - PartialView"

        public static List<InspectionResultsVm> PartialInspectionView(int equipmentid)
        {
            var inspectionList = (from inspection in Db.Inspections
                where inspection.EquipmentId == equipmentid
                select new InspectionResultsVm
                {
                    Inspector = inspection.Inspector,
                    InspectionId = inspection.InspectionId,
                    InspectionDateTime = inspection.InspectionDate,
                    InspectionResults =
                        Db.InspectionResults.Where(ir => ir.InspectionId == inspection.InspectionId).ToList()
                }).ToList();
            return inspectionList;
        }

        #endregion

        #region EquipmentTypeDetails

        public static EquipmentType GetTypeDetails(int equipmentTypeId)
        {
            try
            {
                var typeDetails = Db.EquipmentTypes.Find(equipmentTypeId);
                return typeDetails;
            }
            catch
            {
                return null;
            }
        }

        #endregion

        #region EquipmentTypeList

        /// <summary>
        ///     GetEquipmentTypeList()
        /// </summary>
        /// <returns>List<EquipmentTypesVM /></returns>
        public static List<EquipmentTypesVm> GetEquipmentTypeList()
        {
            var equipmentTypeSource = (Db.EquipmentTypes.Select(m => new EquipmentTypesVm
            {
                EquipmentTypeId = m.EquipmentTypeId,
                EquipmentTypeParentId = m.EquipmentTypeParentId,
                EquipmentTypeDesc = m.EquipmentTypeDesc,
                EquipmentMaintIntervalDays = m.EquipmentMaintIntervalDays,
                EquipmentTypeParentDesc = (from p in Db.EquipmentTypes
                    where p.EquipmentTypeId == m.EquipmentTypeParentId
                    select p.EquipmentTypeDesc).FirstOrDefault(),
                TypeCount = (from row in Db.Equipment
                            where row.EquipmentTypeId == m.EquipmentTypeId
                            select row).Count()
            })).ToList();

            //initialize new empty list with length matching masterlist.count()
            var equipmentTypes = new List<EquipmentTypesVm>();

            //iterate through masterlist
            for (var i = 0; i < equipmentTypeSource.Count(); i++)
            {
                if (equipmentTypeSource[i].EquipmentTypeParentId != 1 &&
                    equipmentTypes.Any() &&
                    equipmentTypes.Exists(t => t.EquipmentTypeId == equipmentTypeSource[i].EquipmentTypeParentId))
                {
                    //find parent position
                    var parent =
                        equipmentTypes.First(c => c.EquipmentTypeId == equipmentTypeSource[i].EquipmentTypeParentId);
                    var parentIndex =
                        equipmentTypes.FindIndex(c => c.EquipmentTypeId == equipmentTypeSource[i].EquipmentTypeParentId);
                   
                       
                    //spawn new Result item then copy and reassign values
                    var listItem = new EquipmentTypesVm
                    {
                        EquipmentTypeId = equipmentTypeSource[i].EquipmentTypeId,
                        EquipmentTypeParentId = equipmentTypeSource[i].EquipmentTypeParentId,
                        EquipmentMaintIntervalDays = equipmentTypeSource[i].EquipmentMaintIntervalDays,
                        EquipmentTypeDesc = equipmentTypeSource[i].EquipmentTypeDesc,
                        EquipmentTypeParentDesc = equipmentTypeSource[i].EquipmentTypeParentDesc,
                        TypeLevel = parent.TypeLevel + 1,
                        TypeCount = equipmentTypeSource[i].TypeCount
                    };
                    //insert item into list at one level below parent
                    equipmentTypes.Insert(parentIndex + 1, listItem);
                    //reassign masterlist item to null
                    equipmentTypeSource[i] = null;
                }
                else
                {
                    //insert into list at end
                    equipmentTypes.Add(equipmentTypeSource[i]);
                    //reassign masterlist item to null
                    equipmentTypeSource[i] = null;
                }
            }
            return equipmentTypes;
        }

        #endregion

        #region GenericListBuilder

        /// <summary>
        ///     Return List of type T of the size entries
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entries"></param>
        /// <returns></returns>
        public static List<T> CreateDefaultList<T>(int entries)
        {
            return new List<T>(new T[entries]);
        }

        #endregion

        #region PartialRequest

        /// <summary>
        ///     Partial Request Class
        /// </summary>
        public class PartialRequest
        {
            public PartialRequest(object routeValues)
            {
                RouteValues = new RouteValueDictionary(routeValues);
            }

            public RouteValueDictionary RouteValues { get; private set; }

            public void Invoke(ControllerContext context)
            {
                var rd = new RouteData(context.RouteData.Route, context.RouteData.RouteHandler);
                foreach (var pair in RouteValues)
                    rd.Values.Add(pair.Key, pair.Value);
                IHttpHandler handler = new MvcHandler(new RequestContext(context.HttpContext, rd));
                handler.ProcessRequest(System.Web.HttpContext.Current);
            }
        }

        #endregion

        #region EquipmentDetails

        public static Equipment EquipmentDetails(int equipmentId)
        {
             using (var edContext = new SEMInspection())
            {
           
                var equipmentDetails = edContext.Equipment.Find(equipmentId);
                return equipmentDetails;
            }
}
        

        public static Equipment EquipmentDetails(string utilityId)
        { 
            
                var equipmentDetails = Db.Equipment.SingleOrDefault(c => c.UtilityId == utilityId);
                return equipmentDetails;
            
        }

        #endregion

        #region "Equipment Types - Family Builder"

        public static List<int> FamilyBuilder(List<int> familytree, int equipmentTypeId)
        {
            var familytreelist = familytree;
            var myList = GetChildren(equipmentTypeId);
            for (var i = 0; i < myList.Count(); i++)
            {
                familytreelist.Add(myList[i]);
                FamilyBuilder(familytreelist, myList[i]);
            }
            return familytreelist;
        }

        public static List<int> GetChildren(int equipmentTypeId)
        {
            var childlist = (from d in Db.EquipmentTypes
                where d.EquipmentTypeParentId == equipmentTypeId
                select d.EquipmentTypeId).ToList();
            return childlist;
        }

        #endregion

        #region InspectionsList(equipmentid)

        public static List<Inspection> GetInspections(int equipmentId)
        {
            var inspections = Db.Inspections.Where(i => i.EquipmentId == equipmentId).ToList();
            return inspections.Any() ? inspections : null;
        }

        public static List<Inspection> GetInspections()
        {
            return Db.Inspections.Include(i => i.Equipment).ToList();
            
        }

        #endregion

        #region Equipment List

        /// <summary>
        ///     Returns SelectList object containing Equipment.
        ///     Used for selecting an equipment parent
        /// </summary>
        /// <returns></returns>
        public static SelectList GetEquipmentList()
        {
            return GetEquipmentList(null);
        }

        /// <summary>
        ///     Returns SelectList object containing Equipment.
        ///     Used for selecting an equipment parent
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public static SelectList GetEquipmentList(int? parentId)
        {
            var equipment = new Dictionary<int, string> {{-1, "None"}};
            foreach (var el in Db.Equipment)
            {
                equipment.Add(el.EquipmentId, el.EquipmentDescription);
            }
            var equipmentList = new SelectList(equipment, "Key", "Value", parentId);
            return equipmentList;
        }

        #endregion

        #region EquipmentTypeSelectList

        /// <summary>
        ///     Returns SelectList object containing EquipmentTypes.
        /// </summary>
        /// <returns></returns>
        public static SelectList GetEquipmentTypeSelectList()
        {
            using (SEMInspection et = new SEMInspection())
            {
                var equipmentType = new Dictionary<int, string> {{-1, "Select Type"}};
                foreach (var el in et.EquipmentTypes)
                {
                    equipmentType.Add(el.EquipmentTypeId, el.EquipmentTypeDesc);
                }

                var equipmentTypeList = new SelectList(equipmentType, "Key", "Value");

                return equipmentTypeList;
            }
        }

        public static SelectList GetConditionsSelectList(int? condId)
        {
            using (var coContext = new SEMInspection())
            {
                var conditionValue = new Dictionary<int, string> {{0, "Set Condition"}};
                foreach (var con in coContext.Conditions)
                {
                    conditionValue.Add(con.ConditionId, con.ConditionName);
                }

                if (condId != null)
                {
                    var conditionsList = new SelectList(conditionValue, "Key", "Value", condId);
                    return conditionsList;
                }
                else
                {
                    var conditionsList = new SelectList(conditionValue, "Key", "Value");
                    return conditionsList;
                }
            }
        }

        public static SelectList GetConditionsSelectList()
        {
            return GetConditionsSelectList(null);
        }

        //public static SelectList GetConditionsSelectList(int? conditionId)
        //{
        //    var condition = new Dictionary<int, string> { { -1, "Set Condition" } };
        //    foreach (var el in db.Conditions)
        //    {
        //        condition.Add(el.ConditionId, el.ConditionName);

        //    }
        //    if (conditionId == null)
        //    {
        //        var conditionList = new SelectList(condition, "Key", "Value");
        //        return conditionList;
        //    }
        //    else
        //    {
        //        var conditionList = new SelectList(condition, "Key", "Value", conditionId);
        //        return conditionList;
        //    }
        //}

        public static SelectList GetEquipmentSelectList()
        {
            return GetEquipmentSelectList(null);
        }

        public static SelectList GetEquipmentSelectList(int? equipmentId)
        {
            var equipment = new Dictionary<int, string> {{-1, "Select Type"}};
            foreach (var el in Db.Equipment.Where(a => a.ParentId != equipmentId))
            {
                equipment.Add(el.EquipmentId, el.EquipmentDescription);
            }

            var equipmentSelectList = new SelectList(equipment, "Key", "Value", equipmentId);

            return equipmentSelectList;
        }

        /// <summary>
        ///     Returns SelectList object containing EquipmentTypes.
        /// </summary>
        /// <param name="equipmentTypeId"></param>
        /// <returns></returns>
        public static SelectList GetEquipmentTypeSelectList(int equipmentTypeId)
        {
            using (SEMInspection sm = new SEMInspection()
            )
            {
                var equipmentType = new Dictionary<int, string> {{-1, "Select Type"}};
                foreach (var el in sm.EquipmentTypes)
                {
                    equipmentType.Add(el.EquipmentTypeId, el.EquipmentTypeDesc);
                }
                //int equipmentTypeParentId;
                var singleOrDefault = sm.EquipmentTypes.SingleOrDefault(e => e.EquipmentTypeId == equipmentTypeId);
                if (singleOrDefault != null)
                {
                    var equipmentTypeList = new SelectList(equipmentType, "Key", "Value",
                        singleOrDefault.EquipmentTypeParentId);
                    return equipmentTypeList;
                }
                else
                {
                    var equipmentTypeList = new SelectList(equipmentType, "Key", "Value");
                    return equipmentTypeList;
                }
            }
        }

        #endregion

        #region InspectionCriteria

        /// <summary>
        /// GetInspectionCriteria(int equipmentId)
        /// </summary>
        /// <param name="equipmentId"></param>
        /// <returns>List<InspectionCriteria /></returns>
        public static List<InspectionCriteria> GetInspectionCriteria(int equipmentId)
        {
            var equipment = EquipmentDetails(equipmentId);
            return
                Db.InspectionCriteria.Where(c => c.EquipmentTypeId == equipment.EquipmentTypeId)
                    .OrderByDescending(m => m.InspectionCriteriaId)
                    .ToList();
        }

        public static List<InspectionCriteria> GetInspectionCriteria(string utilityId)
        {
            var equipment = EquipmentDetails(utilityId);
            return Db.InspectionCriteria.Where(c => c.EquipmentTypeId == equipment.EquipmentTypeId).ToList();
        }

        public static List<InspectionCriteria> GetInspectionCriteriaByType(int equipmentTypeId)
        {
            //var equipment = EquipmentDetails(utilityId);
            return Db.InspectionCriteria.Where(c => c.EquipmentTypeId == equipmentTypeId).ToList();
        }

        #endregion

        #region "Retire/Repair Equipment"

        public static bool RetireEquipment(int equipmentId)
        {
            var equipment = Db.Equipment.Find(equipmentId);
            equipment.Status = (StatusEnum) 3;
            Db.Entry(equipment).State = EntityState.Modified;
            try
            {
                Db.SaveChangesAsync();
            }
            catch
            {
                return false;
            }
            return true;
        }

       

        public async static Task<bool> RepairRestoreRetire(int equipmentId, string moveType, string comment, string inspector)
        {
           
            var eid = equipmentId;
            var move = moveType;
            var cmt = comment;
            var ins = inspector;
            //update equipment record
            var equipment = Db.Equipment.Find(equipmentId);
            if (move == "repair") equipment.Status = (StatusEnum)2;
            if (move == "restore") equipment.Status = (StatusEnum)1;
            if (move == "retire") equipment.Status = StatusEnum.Retired;
            Db.Entry(equipment).State = EntityState.Modified;
            //add EquipmentRepairHistory record
            
            var repairRecord = new EquipmentRepairHistory
            {
                EquipmentId = eid,
                MoveType = move,
                MoveDate = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Local),
                Comment = cmt,
                Inspector = ins
                };
            Db.Entry(repairRecord).State = EntityState.Added;
            try
            {
               await Db.SaveChangesAsync();
            }
            catch (DbEntityValidationException dbEx)
            {
                foreach (var validationErrors in dbEx.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Trace.TraceInformation("Property: {0} Error: {1}", validationError.PropertyName, validationError.ErrorMessage);
                    }
                }
                return false;
            }
            
            return true;
        }

        #endregion
    }
}