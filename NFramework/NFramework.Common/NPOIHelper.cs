using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NFramework.Common
{
    /// <summary>
    /// npoi工具。。用于导出数据
    /// </summary>
    public class NPOIHelper
    {
        /// <summary>
        /// 用NPOI工具创建excel
        /// </summary>
        /// <returns></returns>
        public static MemoryStream CreateExcelFile(params SheetDynamicInfo[] sheetDynamicInfo)
        {
            //创建Excel文件的对象
            NPOI.HSSF.UserModel.HSSFWorkbook book = new NPOI.HSSF.UserModel.HSSFWorkbook();
            foreach (var sheetInfo in sheetDynamicInfo)
            {
                //添加一个sheet
                NPOI.SS.UserModel.ISheet sheet1 = book.CreateSheet(sheetInfo.SheetName);
                if (sheetInfo.EntityList != null && sheetInfo.EntityList.Count > 0)
                {
                    //给sheet1添加第一行的头部标题
                    NPOI.SS.UserModel.IRow row1 = sheet1.CreateRow(0);
                    for (int i = 0; i < sheetInfo.ColumnInfos.Count; i++)
                    {
                        row1.CreateCell(i).SetCellValue(sheetInfo.ColumnInfos[i].ExcelColumnHeadName);
                    }
                    PropertyInfo[] propertyInfoArray = null;
                    //将数据逐步写入sheet1各个行
                    for (int i = 0; i < sheetInfo.EntityList.Count; i++)
                    {
                        NPOI.SS.UserModel.IRow rowtemp = sheet1.CreateRow(i + 1);
                        var entity = sheetInfo.EntityList[i];
                        propertyInfoArray = entity.GetType().GetProperties();
                        int j = 0;
                        foreach (var mapCol in sheetInfo.ColumnInfos)
                        {
                            var p = propertyInfoArray.Single(t => t.Name.ToUpper() == mapCol.SourceHeadName.ToUpper());
                            var valInfo = p.GetValue(entity, null);
                            string valStr = valInfo == null ? "" : valInfo.ToString();
                            rowtemp.CreateCell(j).SetCellValue(valStr);
                            j++;
                        }
                    }
                }
                //book.Add(sheet1);
            }
            // 写入到客户端 
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            try
            {
                book.Write(ms);
            }
            catch (Exception ex)
            {
                var ddd = ex;
            }
            System.GC.Collect();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }

    /// <summary>
    /// 导出excel的列名映射
    /// </summary>
    public class NPOIColumnMapping
    {
        public NPOIColumnMapping(string excelColumnHeadName, string sourceHeadName)
        {
            ExcelColumnHeadName = excelColumnHeadName;
            SourceHeadName = sourceHeadName;
        }

        public string ExcelColumnHeadName { get; set; }
        public string SourceHeadName { get; set; }
    }

    /// <summary>
    /// excel sheet页描述
    /// </summary>
    public class SheetDynamicInfo
    {
        public SheetDynamicInfo()
        {
        }

        /// <summary>
        /// 带映射参数的构造函数，无需给ColumnInfos赋值即可映射导出excel
        /// </summary>
        /// <param name="headList"></param>
        /// <param name="propertyList"></param>
        public SheetDynamicInfo(List<string> headList, List<string> propertyList)
        {
            ColumnInfos = new List<NPOIColumnMapping>();
            for (var i = 0; i < headList.Count; i++)
            {
                ColumnInfos.Add(new NPOIColumnMapping(headList[i], propertyList[i]));
            }
        }

        public List<dynamic> EntityList { get; set; }
        public List<NPOIColumnMapping> ColumnInfos { get; set; }
        public string SheetName { get; set; }
    }
}
