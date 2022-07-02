using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using Test2022.Models;
using System.IO.Compression;

namespace Test2022.Controllers
{
    public class FormatCSVTask6Controller : Controller
    {
        // GET: FormatCSVTask6
        public ActionResult FormatCSVTask6View()
        {
            
            return View();
        }

      

        [HttpPost]
        public ActionResult ValidateEDIData( HttpPostedFileBase file)
        {
          
            try
            {

                DataTable EDITable = new DataTable();
                List<EDIInformation> EDIDataList = new List<EDIInformation>();
                if (file != null)
                {
                    var destinationPath = Path.Combine(Server.MapPath("~/FileImport"), file.FileName);

                    ////Delete all files in the Folder Directory.
                    clearFolder(Server.MapPath("~/FileImport"));

                    file.SaveAs(destinationPath);

                    if (System.IO.File.Exists(destinationPath))
                    {
                        EDITable = ConvertCSVtoDataTable(destinationPath);

                        file.InputStream.Dispose();

                        if (EDITable.Rows.Count > 0)
                        {
                            var result11 = EDITable.AsEnumerable()
                                       .Select(row => new
                                       {
                                           UserID = row.Field<string>("UserID"),
                                           InsuranceCompany = row.Field<string>("InsuranceCompany"),
                                           Version = Convert.ToInt32( row["Version"])
                                       }).GroupBy(i => new { UserID = i.UserID, InsuranceCompany = i.InsuranceCompany })
                                         .Select(gr => new
                                         {
                                             UserID = gr.Key.UserID,
                                             InsuranceCompany = gr.Key.InsuranceCompany,                                          
                                             Version = gr.Max(z => z.Version)
                                         }).ToList();

                            if (result11 != null)
                            {
                                DataTable dt = new DataTable();
                                dt.Columns.Add("UserID");
                                dt.Columns.Add("FirstName");
                                dt.Columns.Add("LastName");
                                dt.Columns.Add("Version");
                                dt.Columns.Add("InsuranceCompany");
                              
                                for (int i =0; i< result11.Count; i++)
                                {
                                    DataRow DR = dt.NewRow();
                                    DR["UserID"] = result11[i].UserID;
                                    DR["Version"] = result11[i].Version;
                                    DR["InsuranceCompany"] = result11[i].InsuranceCompany;

                                    var res = (from r in EDITable.AsEnumerable()                                              
                                               where r.Field<string>("UserID")== (result11[i].UserID)
                                               && r.Field<string>("Version") ==(result11[i].Version.ToString())
                                               && r.Field<string>("InsuranceCompany")== (result11[i].InsuranceCompany)
                                               select r).FirstOrDefault();
                                    if(res != null)
                                    {
                                        DR["FirstName"] = res["FirstName"].ToString();
                                        DR["LastName"] = res["LastName"].ToString();
                                    }
                                    dt.Rows.Add(DR);
                                }

                                dt = RemoveEmptyRowsFromDataTable(dt);

                                dt.DefaultView.Sort = "LastName ASC,FirstName ASC ";
                                dt = dt.DefaultView.ToTable();

                                System.Data.DataView view = new System.Data.DataView(dt);
                                DataTable dtInsuranceCompany = view.ToTable("dt1", true, "InsuranceCompany");                               
                                dtInsuranceCompany = dtInsuranceCompany.DefaultView.ToTable();



                                for (int i = 0; i < dtInsuranceCompany.Rows.Count; i++) //for all distinct Invoice Type
                                {
                                    DataRow[] drInsuranceCompany = dt.Select("InsuranceCompany= '" + dtInsuranceCompany.Rows[i][0].ToString().Trim() + "' ");
                                    if (dtInsuranceCompany.Rows[i][0].ToString().Trim() == "")
                                    {

                                    }
                                    else
                                    {
                                        PrepareFileForInsurance(drInsuranceCompany, dtInsuranceCompany.Rows[i][0].ToString());
                                    }

                                }

                            }

                          
                        }


                      
                    }
                }
              
                return Json("Success");
            }
            catch (SystemException ex)
            {
                return Json(ex.Message);
            }

           
        }

        [HttpPost]
        public void PrepareFileForInsurance(DataRow[] drInsuranceCompany, string InsuranceName)
        {
            try
            {
                string savePath = Server.MapPath("~/FileImport"); //

                bool exists = System.IO.Directory.Exists(savePath);

                if (exists == false)
                {
                    System.IO.Directory.CreateDirectory(savePath);
                }
              
                savePath = savePath + @"\" + InsuranceName + ".csv";

                if (System.IO.File.Exists(savePath) == true)
                {
                    System.IO.File.Delete(savePath);
                }

                if (drInsuranceCompany.Count() > 0)
                {
                    MemoryStream output = CreateCSV(drInsuranceCompany);
                  
                    //convert the excel package to a byte array
                    byte[] bin = output.ToArray();

                    string mimeType = "application/vnd.ms-excel";

                    //write the file to the disk
                    System.IO.File.WriteAllBytes(savePath, bin);
                }
               
            }
            catch (SystemException ex)
            {
               
            }
        }

        public MemoryStream CreateCSV(DataRow[]  DR)
        {
            string sResult = "";

            MemoryStream output = new MemoryStream();
            StreamWriter writer = new StreamWriter(output, Encoding.UTF8);

           

            writer.Write("UserID,");
            writer.Write("FirstName,");
            writer.Write("LastName,");
            writer.Write("Version,");
            writer.Write("InsuranceCompany");           

            writer.WriteLine();
          
            if (DR.Count() > 0)
            {               

                for (int i = 0; i < DR.Count(); i++)
                {
                    writer.Write(DR[i]["UserID"].ToString());
                    writer.Write(",");
                    writer.Write(DR[i]["FirstName"].ToString());
                    writer.Write(",");
                    writer.Write(DR[i]["LastName"].ToString());
                    writer.Write(",");
                    writer.Write(DR[i]["Version"].ToString());
                    writer.Write(",");
                    writer.Write(DR[i]["InsuranceCompany"].ToString());
                    writer.WriteLine();
                }
            }
            writer.Flush();
            output.Position = 0;

            return output;
        }
        [HttpGet]
        public ActionResult GetCSVFiles()
        {           
            var archive = Server.MapPath("~/archive.zip");
            var temp = Server.MapPath("~/FileImport"); //

            // clear any existing archive
            if (System.IO.File.Exists(archive))
            {
                System.IO.File.Delete(archive);
            }

            // create a new archive
             ZipFile.CreateFromDirectory(temp, archive);

            return File(archive, "application/zip", "archive.zip");

        }

        public static DataTable ConvertCSVtoDataTable(string strFilePath)
        {
            DataTable dt = new DataTable();
            using (StreamReader sr = new StreamReader(strFilePath))
            {
                string[] headers = sr.ReadLine().Split(',');
                foreach (string header in headers)
                {
                    dt.Columns.Add(header);
                }
                while (!sr.EndOfStream)
                {
                    string[] rows = Regex.Split(sr.ReadLine(), ",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)");

                    DataRow dr = dt.NewRow();
                    for (int i = 0; i < headers.Length; i++)
                    {
                        //  dr[i] = rows[i];
                        dr[i] = rows[i].Replace("\"", "");
                    }
                    dt.Rows.Add(dr);
                }

            }
            return dt;
        }
        public DataTable RemoveEmptyRowsFromDataTable(DataTable dt)
        {
            List<int> rowIndexesToBeDeleted = new List<int>();
            int indexCount = 0;
            foreach (var row in dt.Rows)
            {
                var r = (DataRow)row;
                int emptyCount = 0;
                int itemArrayCount = r.ItemArray.Length;
                foreach (var i in r.ItemArray) if (string.IsNullOrWhiteSpace(i.ToString())) emptyCount++;

                if (emptyCount == itemArrayCount) rowIndexesToBeDeleted.Add(indexCount);

                indexCount++;
            }

            int count = 0;
            foreach (var i in rowIndexesToBeDeleted)
            {
                dt.Rows.RemoveAt(i - count);
                count++;
            }

            return dt;
        }

        private void clearFolder(string FolderName)
        {
            DirectoryInfo dir = new DirectoryInfo(FolderName);

            foreach (FileInfo fi in dir.GetFiles())
            {
                fi.Delete();
            }

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                clearFolder(di.FullName);
                di.Delete();
            }
        }
    }


}