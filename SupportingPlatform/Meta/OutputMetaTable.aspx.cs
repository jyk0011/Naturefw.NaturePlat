﻿

using System;
using System.Data;
using System.Web;
using System.Web.UI.WebControls;
using Nature.BaseWebform;
using Nature.Common;
using Nature.Data;

namespace NatureFramework.SupportingPlatform.Meta
{
    public partial class OutputMetaTable : BasePageForm
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                LoadSource();
            }
        }

        #region 显示两个数据库里的模块表，以确认没弄错数据库
        protected void btnCheck_Click(object sender, EventArgs e)
        {
            //显示源
            string dataBaseID = GetDataBaseIDbyCookie();
            LoadModule(GV_Source, dataBaseID);

            //显示目标
            LoadModule(GV_Target, lstSource.SelectedValue);

        }
        #endregion

        #region 导入数据
        protected void btnStart_Click(object sender, EventArgs e)
        {
            string dataBaseID = GetDataBaseIDbyCookie();
            DataAccessLibrary dalSource = CreateDalbyDataBaseID(dataBaseID);

            dataBaseID = lstSource.SelectedValue;
            DataAccessLibrary dalTarget = CreateDalbyDataBaseID(dataBaseID);

            DataAccessLibrary dalTargetSelect = CreateDalbyDataBaseID(dataBaseID);

            //在 OutputMetaTable.cs实现
            string err = IntoTable(dalSource, dalTarget, dalTargetSelect);

            if (err == "0")
            {
                //没有记录
                txtMsg.Text += "\n源数据库里没有记录！";
                return;
            }

            IntoColumn(dalSource, dalTarget, dalTargetSelect);
            
        }
        #endregion


        #region 加载可以导出的数据库选择
        private void LoadSource()
        {
            string dataBaseID = GetDataBaseIDbyCookie();

            const string sql = @"SELECT  DataBaseID AS id, DataTitle AS txt
                            FROM   Manage_DataBase
                            WHERE  (DataBaseID <> {0} and KindID =1 )";
            lstSource.DataSource = Dal.DalMetadata.ExecuteFillDataTable(string.Format(sql, dataBaseID));
            lstSource.DataBind();
        }
        #endregion

        #region 加载模块信息
        private void LoadModule(GridView gv, string dataBaseID)
        {
            DataAccessLibrary dal = CreateDalbyDataBaseID(dataBaseID);

            //提取数据的SQL
            const string sqlGvSource = @"SELECT[TableID],[TableName],[PKColumnID],[TypeID],[HaveTableIDs],[Content],[ExcelTableName],[PDGuid],[DisOrder],[AddUserid],[AddTime],[IsDel]
                            FROM  [Manage_Table] 
                            where [TableID] in ({0})  order by TableID";

            bool isNotHasData = false;  //是否取到记录

            DataTable dt = dal.ExecuteFillDataTable(string.Format(sqlGvSource, DataIDs));
            if (dt.Rows.Count == 0)
            {
                dt.Rows.Add(dt.NewRow());
                isNotHasData = true;
            }
            gv.DataSource = dt;
            gv.DataBind();

            if (isNotHasData)
            {
                int columnCount = dt.Columns.Count;
                gv.Rows[0].Cells.Clear();
                gv.Rows[0].Cells.Add(new TableCell());
                gv.Rows[0].Cells[0].ColumnSpan = columnCount;
                gv.Rows[0].Cells[0].Text = "没有记录";
                gv.Rows[0].Cells[0].Style.Add("text-align", "center");
            }

        }
        #endregion


        #region 内部函数
        //从Cookie里面获取DataBaseID
        private string GetDataBaseIDbyCookie()
        {
            HttpCookie dataBase = Request.Cookies["DataBaseID"];
            string dataBaseID = "0";

            if (dataBase != null)
                dataBaseID = dataBase.Value;

            if (!Functions.IsInt(dataBaseID))
                dataBaseID = "0";

            return dataBaseID;
        }

        //获取数据访问函数库的实例
        private DataAccessLibrary CreateDalbyDataBaseID(string dataBaseID)
        {
            //数据库的连接字符串
            const string sqlDataBase = @"SELECT   ConnString, Provider
                            FROM      Manage_DataBase
                            WHERE   (DataBaseID = {0})";

            string[] cnInfo = Dal.DalMetadata.ExecuteStringsBySingleRow(string.Format(sqlDataBase, dataBaseID));

            DataAccessLibrary dal = DalFactory.CreateDal(cnInfo[0], cnInfo[1]);

            return dal;
        }
        #endregion

    }
}