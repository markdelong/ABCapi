using System;
using System.Collections.Generic;
using System.Reflection;
using System.Data.SqlClient;
using System.Text;
using Efinity;
using System.Data;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace ABC.Managers
{
    public class ORM
    {
        public static Boolean FieldLogging = false;
        public static Boolean ORMLogging = true;
        public static Boolean Debug = false;

        public static class DatabaseConnection
        {
            public static string ConnectionString { get; set; }

            public static SqlConnection ConnectionFactory()
            {
                return new SqlConnection(ConnectionString);
            }
        }


        public static SqlConnection makeConnection(string connection)
        {
            SqlConnection sc = new SqlConnection(connection);
            return sc;
        }

        //public static string ConnectionString //shouldn't use this
        //{
        //    get
        //    {
        //        return ConfigurationManager.ConnectionStrings["VSMConnectionString"].ConnectionString;
        //    }
        //}

        public static SqlCommand DbCommand(SqlConnection connection, string query)
        {
            if (connection != null && connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            return new SqlCommand(query, connection);
        }

        public static SqlCommand DbCommand(SqlConnection connection)
        {
            return new SqlCommand("", connection);
        }

        //public static SqlCommand DbCommand(string query, )
        //{
        //    SqlConnection con = new SqlConnection(DbHelper.ConnectionString);
        //    con.Open();
        //    return new SqlCommand(query, con);
        //}

        public static SqlCommand DbCommand()
        {
            return new SqlCommand();
        }

        public static void ExecuteNoReturn(SqlCommand command)
        {
            string strSQL = command.CommandText;

            if (command.Connection.State != System.Data.ConnectionState.Open)
                command.Connection.Open();
            try
            {
                command.ExecuteNonQuery();
            }
            finally
            {
                command.Connection.Close();
                command.Connection.Dispose();
            }

            //Save2Log("true", strSQL, "", "", "", -1, "", new object(), "", ConnectionString); //removed for now, need to grab more valid data
        }

        public static T ExecuteSingleReturn<T>(SqlCommand command)
        {
            object instance = new object(); // (T)Activator.CreateInstance(typeof(T));   

            if (command.Connection.State != System.Data.ConnectionState.Open)
                command.Connection.Open();
            try
            {
                instance = command.ExecuteScalar();
            }
            finally
            {
                command.Connection.Close();
                command.Connection.Dispose();
            }

            return (T)instance;
        }

        public static string ExecuteSingleReturn(SqlCommand command)
        {
            string instance = ""; // (T)Activator.CreateInstance(typeof(T));   

            if (command.Connection.State != System.Data.ConnectionState.Open)
                command.Connection.Open();
            try
            {
                instance = command.ExecuteScalar().ToString();
            }
            finally
            {
                command.Connection.Close();
                command.Connection.Dispose();
            }
            return instance;
        }

        public static List<T> DbReader<T>(SqlCommand command)
        {
            return DbReader<T>(command, false);
        }

        public static List<T> DbReader<T>(string connection, string sqlquery, Boolean forceEmptyInstanceIfNull)
        {
            SqlCommand command = DbCommand(new SqlConnection(connection), sqlquery);
            return DbReader<T>(command, forceEmptyInstanceIfNull, new string[] { });
        }

        public static List<T> DbReader<T>(SqlCommand command, Boolean forceEmptyInstanceIfNull)
        {
            return DbReader<T>(command, forceEmptyInstanceIfNull, new string[] { });
        }

        /// <summary>
        /// forceEmptyInstanceIfNull should be only used for single record returns such as ORM.DbReader<T>(command,true)[0];
        /// for lists use single param above or false for forceEmptyInstanceIfNull
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="command"></param>
        /// <param name="forceEmptyInstance"></param>
        /// <returns></returns>
        public static List<T> DbReader<T>(SqlCommand command, bool forceEmptyInstanceIfNull, string[] exemptions)
        {
            if (command.Connection.State != System.Data.ConnectionState.Open)
                command.Connection.Open();
            SqlDataReader reader = command.ExecuteReader();

            List<T> itemList = new List<T>();
            Type type = typeof(T);
            //Type[] nested = type.GetNestedTypes();

            try
            {
                while (reader.Read())
                {
                    //foreach (Type n in nested) {
                    //itemList.Add((n.GetType())GetRecords<n.GetType()>(reader));
                    //}

                    itemList.Add((T)GetRecords<T>(reader, exemptions));
                }
            }
            finally
            {
                reader.Close();
                reader.Dispose();
                command.Connection.Close();
                command.Connection.Dispose();
            }

            if (itemList.Count == 0 && forceEmptyInstanceIfNull)
            {
                object instance = (T)Activator.CreateInstance(typeof(T));
                itemList.Add((T)instance);
            }

            return itemList;
        }

        public static List<dynamic> DbReaderDynamic(string connection, string sqlquery)
        {
            List<dynamic> itemList = new List<dynamic>();

            sqlquery = sqlquery.ToLower().Replace("update ", "")
                .Replace("delete ", "")
                .Replace("drop ", "")
                .Replace("alter ", "")
                .Replace("remove ", "")
                .Replace("select", "");

            sqlquery = "select" + sqlquery;

            SqlCommand command = DbCommand(new SqlConnection(connection), sqlquery);
            if (command.Connection.State != System.Data.ConnectionState.Open)
                command.Connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                while (reader.Read())
                {
                    var columns = new List<string>();
                    dynamic row = new JObject();

                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = JToken.FromObject(reader[reader.GetName(i)]);
                    }

                    //foreach (Type n in nested) {
                    //itemList.Add((n.GetType())GetRecords<n.GetType()>(reader));
                    //}

                    itemList.Add(row);
                }
            }
            catch (Exception ex)
            {
                if (Debug)
                {
                    itemList.Add(ex);
                }
                else
                {
                    itemList.Add("An error occurred.");
                }
            }
            finally
            {
                reader.Close();
                reader.Dispose();
                command.Connection.Close();
                command.Connection.Dispose();
            }


            //if (itemList.Count == 0 && forceEmptyInstanceIfNull)
            //{
            //    object instance = (T)Activator.CreateInstance(typeof(T));
            //    itemList.Add((T)instance);
            //}

            return itemList;
        }

        public static object GetRecords<T>(SqlDataReader reader, string[] exemptions)
        {
            object instance = (T)Activator.CreateInstance(typeof(T));
            Type type = typeof(T);

            foreach (PropertyInfo property in type.GetProperties())
            {
                bool omitRec = false;

                if (exemptions.Length > 0)
                {
                    foreach (string s in exemptions)
                    {
                        if (property.Name == s)
                        {
                            omitRec = true;
                            break;
                        }
                    }
                }
                if (!omitRec)
                {
                    try
                    {

                        switch (property.PropertyType.ToString())
                        {
                            case "System.Single":
                                property.SetValue(instance, General.tryFloat(reader[property.Name]), null);
                                break;
                            case "System.Int32":
                                property.SetValue(instance, General.tryInt(reader[property.Name]), null);
                                break;
                            case "System.Double":
                                property.SetValue(instance, General.tryDouble(reader[property.Name]), null);
                                break;
                            case "System.Decimal":
                                property.SetValue(instance, General.tryFloat(reader[property.Name]), null);
                                break;
                            case "System.DateTime":
                                property.SetValue(instance, General.tryDate(reader[property.Name], "1/1/1900"), null);
                                break;
                            case "System.Boolean":
                                property.SetValue(instance, General.tryBoolean(reader[property.Name]), null);
                                break;
                            default:
                                property.SetValue(instance, reader[property.Name].ToString(), null);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        //Need to add storage collection for errors raised.
                        //if (Efinity.ORM.FieldLogging)
                        //{
                        //    HttpContext.Current.Response.Write("<br><b>ClassName: " + type.Name + "</b><br><b>Name: " + property.Name + "</b>; Type:" + property.PropertyType.ToString() + "<br>Error Source:" + ex.Source + "<br>Message: " + ex.Message + "<br>Source: " +
                        //        ex.Source + "<BR><BR>Stack: " + ex.StackTrace + "<BR><BR>Target: " + ex.TargetSite);
                        //}
                    }
                }
            }

            return instance;
        }

        public static object GetRecords<T>(SqlDataReader reader)
        {
            return GetRecords<T>(reader, new string[] { });
        }

        //Use to get values from submitted form to populate Collection
        //Example: BlogSyndication c = (BlogSyndication)DbHelper.GetRecords(new BlogSyndication(), sentform);
        public static object GetRecords(object instance, Dictionary<string, string> sentform)
        {
            Type type = instance.GetType();
            foreach (PropertyInfo property in type.GetProperties())
            {
                try
                {
                    switch (property.PropertyType.ToString())
                    {
                        case "System.Int32":
                            property.SetValue(instance, General.tryInt(sentform[property.Name]), null);
                            break;
                        case "System.Double":
                            property.SetValue(instance, General.tryDouble(sentform[property.Name]), null);
                            break;
                        case "System.Decimal":
                            property.SetValue(instance, General.tryFloat(sentform[property.Name]), null);
                            break;
                        case "System.DateTime":
                            property.SetValue(instance, General.tryDate(sentform[property.Name], "1/1/1900"), null);
                            break;

                        case "System.Single":
                            property.SetValue(instance, General.tryFloat(sentform[property.Name]), null);
                            break;
                        case "System.Boolean":
                            if (sentform[property.Name].ToString() == "on")
                                property.SetValue(instance, true, null);
                            else
                                property.SetValue(instance, false, null);
                            break;
                        default:
                            property.SetValue(instance, sentform[property.Name].ToString(), null);
                            break;
                    }
                }
                catch (Exception ex)
                {
                    //Need to add storage collection for errors raised.
                    //HttpContext.Current.Response.Write("Error with " + property.Name + " (" + property.PropertyType.ToString() + ")<br><br>" + ex.Message);
                }
            }
            return instance;
        }




        /// <summary>
        /// one idcolumn - most common usage
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="update"></param>
        /// <param name="idcolumn"></param>
        /// <param name="tblname"></param>
        /// <param name="connection"></param>
        /// <param name="exemptions"></param>
        /// <returns></returns>
        public static int SaveRecord(object instance, bool update, string idcolumn, string tblname, string connection)
        {
            return SaveRecord(instance, update, new string[] { idcolumn }, tblname, connection);
        }

        public static int SaveRecord(object instance, bool update, string[] idcolumn, string tblname, string connection)
        {
            return SaveRecord(instance, update, idcolumn, tblname, connection, new string[] { });
        }

        public static int SaveRecord(object instance, bool update, string idcolumn, string tblname, string connection, string[] exemptions)
        {
            return SaveRecord(instance, update, new string[] { idcolumn }, tblname, connection, exemptions);
        }

        public static int SaveRecord(object instance, bool update, string[] idcolumn, string tblname, string connection, string[] exemptions)
        {
            return SaveRecord(instance, update, idcolumn, tblname, connection, exemptions, new Dictionary<string, string>());
        }

        public static int SaveRecord(object instance, bool update, string[] idcolumn, string tblname, string connection, string[] exemptions, Dictionary<string, string> dbfieldmapping)
        {
            SaveInputParams s = new SaveInputParams();
            s.Instance = instance;
            s.Update = update;
            s.IdColumn = idcolumn;
            s.TblName = tblname;
            s.Connection = connection;
            s.Exemptions = exemptions;
            s.DbFieldMapping = dbfieldmapping;
            return SaveRecord(s);
        }

        /// <summary>
        /// if idcolumn is blank, the system will autogen the id based upon instance + 'Id'
        /// idcolumn is optional
        /// 
        /// multiple id's used for where statement
        /// 
        /// instance is the class being passed in
        /// 
        /// update true if db is to be updated, else false for insert command
        /// 
        /// Leave blank if tblname is "tbl" + class, otherwise pass in, optional param
        /// 
        /// for COORELATION table with no id
        /// leave id column blank, update = false
        /// Use this to support coorelation tables with more than one idcolumn
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="update"></param>
        /// <param name="idcolumn"></param>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static int SaveRecord(SaveInputParams s)
        {
            SqlCommand command = new SqlCommand();
            string issuccessful = "", updateInsert = "Update";
            int newid = 0; string strSQL = "", strSQL4Log = "";
            Type type = s.Instance.GetType();
            string thisTable = s.TblName;
            if (thisTable.Length == 0)
                thisTable = "tbl" + type.ToString();
            //string sqlupdate = "UPDATE " + thisTable + " SET ";
            StringBuilder sbUpdate = new StringBuilder();
            //sbUpdate.Append("UPDATE " + thisTable + " SET ");
            string sqlinsert = "INSERT INTO " + thisTable + " (";
            //string sqlinsertVal = "";
            StringBuilder sbInsertValue = new StringBuilder();
            StringBuilder sbInsertColumn = new StringBuilder();
            StringBuilder sbUpdateValue = new StringBuilder();
            string sqlwhere = "";

            //gets the class element/property for each in class
            foreach (PropertyInfo property in type.GetProperties())
            {
                bool omitRec = false;
                bool emptyId = false;
                if (s.Exemptions.Length > 0)
                {
                    foreach (string x in s.Exemptions)
                    {
                        if (property.Name == x && x.Length > 0)
                        {
                            omitRec = true;
                            break;
                        }
                    }
                }

                if (!omitRec)
                {
                    Boolean notid = false;
                    foreach (string x in s.IdColumn)
                    {
                        if (x.Length == 0)
                        {
                            emptyId = true;
                        }
                        if (property.Name.ToString().ToLower() == x.ToLower())
                        {
                            notid = true;
                            break;
                        }
                    }

                    if (property.Name.ToString() != type.ToString() + "Id" && !notid)// property.Name.ToString().ToLower() != idcolumn.ToLower())
                    {
                        //if using a Dictionary for the fieldmapping
                        if (s.DbFieldMapping.Count > 0)
                        {
                            Boolean fieldFound = false;
                            foreach (KeyValuePair<string, string> d in s.DbFieldMapping)
                            {
                                if (d.Key == property.Name.ToString())
                                {
                                    fieldFound = true;
                                    sbUpdate.Append("[" + d.Value + "]=@" + property.Name.ToString() + ",");
                                    sbInsertColumn.Append("[" + d.Value + "],");
                                    break;
                                }
                            }
                            if (!fieldFound) //check to see if there is a match on the class outside of defined mappings
                            {
                                sbUpdate.Append("[" + property.Name.ToString() + "]=@" + property.Name.ToString() + ",");
                                sbInsertColumn.Append("[" + property.Name.ToString() + "],");
                            }
                        }
                        else
                        {
                            //else the mapping is implied based upon class properties.  Class property must match exactly the db column name
                            sbUpdate.Append("[" + property.Name.ToString() + "]=@" + property.Name.ToString() + ",");
                            sbInsertColumn.Append("[" + property.Name.ToString() + "],");
                        }
                        sbInsertValue.Append("@" + property.Name.ToString() + ",");
                    }
                    else //this is an id column.
                    {
                        if (s.IdColumn.Length > 0 && !emptyId)
                        {
                            sqlwhere = " WHERE 1=1 ";
                            foreach (string c in s.IdColumn)
                            {
                                sqlwhere += " AND [" + c + "]=@" + c;
                            }
                            sqlwhere += ";";
                        }
                        else
                        {
                            sqlwhere = " WHERE " + type.ToString() + "Id=@" + type.ToString() + "Id;";
                        }
                        if (s.Update)
                        {
                            newid = General.tryInt(property.GetValue(s.Instance, null));
                        }
                    }
                }
            }

            if (s.Update)
            {
                strSQL = "UPDATE " + thisTable + " SET " + sbUpdate.ToString().TrimEnd(',') + sqlwhere;
            }
            else
            {
                //sqlinsertVal = sqlinsertVal.TrimEnd(',');
                updateInsert = "Insert"; //for logging
                strSQL = "INSERT INTO " + thisTable + " (" + sbInsertColumn.ToString().TrimEnd(',') + ") VALUES (" + sbInsertValue.ToString().TrimEnd(',') + "); SELECT @@IDENTITY;";
                //strSQL = sqlinsert;
            }

            command = DbCommand(new SqlConnection(s.Connection), strSQL);
            string listexemptions = "";
            if (s.Exemptions.Length > 0)//for logging
            {
                foreach (string x in s.Exemptions)
                {
                    listexemptions += x + ",";
                }
            }


            StringBuilder paramlist = new StringBuilder();
            foreach (PropertyInfo property in type.GetProperties())
            {
                bool omitRec = false;
                if (s.Exemptions.Length > 0)
                {
                    foreach (string x in s.Exemptions)
                    {
                        if (property.Name == x)
                        {
                            omitRec = true;
                            break;
                        }
                    }
                }

                if (!omitRec)
                {
                    command.Parameters.Add(new SqlParameter("@" + property.Name.ToString(), property.GetValue(s.Instance, null)));
                    //below for logging sql query
                    paramlist.Append("'" + property.GetValue(s.Instance, null) + "',");
                    sbUpdateValue.Append(property.Name + "='" + property.GetValue(s.Instance, null) + "',");
                }

                //Prep for logging
                if (s.Update)
                {
                    strSQL4Log = "UPDATE " + thisTable + " SET " + sbUpdateValue.ToString().TrimEnd(',') + sqlwhere;
                }
                else
                {
                    strSQL4Log = "INSERT INTO " + thisTable + " (" + sbInsertValue.ToString().TrimEnd(',').Replace("@", "") + ") VALUES (" + paramlist.ToString().TrimEnd(',') + "); SELECT @@IDENTITY;";
                }
                //end Prep for logging
            }

            if (command.Connection.State != System.Data.ConnectionState.Open)
                command.Connection.Open();

            try
            {
                if (s.Update)
                {
                    command.ExecuteNonQuery();
                }
                else
                {
                    newid = General.tryInt(command.ExecuteScalar());
                }
            }
            catch (Exception ex)
            {

                //Need to add storage collection for errors raised.
                //if (Efinity.ORM.FieldLogging)
                //{
                //    HttpContext.Current.Response.Write("Error Source:" + ex.Source + "<br>Message: " + ex.Message + "<BR><BR>Stack: " + ex.StackTrace + "<BR><BR>Target: " + ex.TargetSite);
                //}

                issuccessful = ex.Message;
            }
            finally
            {
                command.Connection.Close();
            }

            //save to log
            if (ORMLogging)
            {
                try
                {
                    Save2Log(issuccessful, strSQL4Log, updateInsert, thisTable, string.Join(",", s.IdColumn), newid, listexemptions, s.Instance, type.ToString(), s.Connection);
                }
                catch { }
            }

            return newid;
        }

        public static void Save2Log(string issuccessful, string queryexecuted, string queryaction, string tablename, string idcolumn, int idvalue, string columnexemptions, object instance, string classname, string connection)
        {
            //save the information to the log;
            if (classname != "VSM.Page.PageHistory" && classname != "VSM.Sites.TemplateArchive" && classname != "MasterLog")
            {
                SqlCommand command = new SqlCommand();
                string referringPage = "", ipaddress = "", browser = "";
                //try
                //{
                //    referringPage = HttpContext.Current.Request.ServerVariables["HTTP_REFERER"].ToString();
                //}
                //catch { }
                //try
                //{
                //    browser = HttpContext.Current.Request.ServerVariables["HTTP_USER_AGENT"].ToString();
                //}
                //catch { }
                if (issuccessful.Length == 0)
                {
                    issuccessful = "Successful";
                }

                try
                {
                    ipaddress = General.GetIPAddress();
                }
                catch { }

                string strSQL = @"INSERT INTO tblORMLog (LogDate,Referrer,TableName,IdColumnName,IdColumnValue,ExecutionResult,ColumnExemptions,ClassName,ClassCollection,QueryExecuted,QueryAction,UserIP,Browser) 
                VALUES (@LogDate,@Referrer,@TableName,@IdColumnName,@IdColumnValue,@ExecutionResult,@ColumnExemptions,@ClassName,@ClassCollection,@QueryExecuted,@QueryAction,@UserIP,@Browser);";

                command = DbCommand(new SqlConnection(connection), strSQL);
                command.Parameters.Add(new SqlParameter("@LogDate", DateTime.Now));
                command.Parameters.Add(new SqlParameter("@Referrer", referringPage));
                command.Parameters.Add(new SqlParameter("@TableName", tablename));
                command.Parameters.Add(new SqlParameter("@IdColumnName", idcolumn));
                command.Parameters.Add(new SqlParameter("@IdColumnValue", idvalue));
                command.Parameters.Add(new SqlParameter("@ExecutionResult", issuccessful));
                command.Parameters.Add(new SqlParameter("@ColumnExemptions", columnexemptions.TrimEnd(',')));
                command.Parameters.Add(new SqlParameter("@ClassName", classname));
                command.Parameters.Add(new SqlParameter("@ClassCollection", TestCase.DisplayClassCollection(instance)));
                command.Parameters.Add(new SqlParameter("@QueryExecuted", queryexecuted));
                command.Parameters.Add(new SqlParameter("@QueryAction", queryaction));
                command.Parameters.Add(new SqlParameter("@UserIP", ipaddress));
                command.Parameters.Add(new SqlParameter("@Browser", browser));

                if (command.Connection.State != System.Data.ConnectionState.Open)
                    command.Connection.Open();
                try
                {
                    command.ExecuteScalar();
                }
                finally
                {
                    command.Connection.Close();
                    command.Connection.Dispose();
                }
            }
        }

        /// <summary>
        /// Will return the object instance of the class for T.  Requires a formcollection from web submit
        /// Example usage:
        /// 
        /// public void saveEcomAddress(object sender, EventArgs e) {
        ///  EcomAddress a = (EcomAddress)Efinity.ORM.CreateObjectFromForm<EcomAddress>(Request.Form, "ctl00$ContentPlaceHolder1$MemberControl$ecomAddress");
        ///  ORM.SaveRecord(a, (a.EcomAddressId > 0 ? true : false), "EcomAddressId", "tblEcomAddress", ConfigurationManager.ConnectionStrings["VSMConnectionString"].ConnectionString);        
        ///}
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="formcollection"></param>
        /// <param name="formprefix"></param>
        /// <returns></returns>
        public static object CreateObjectFromForm<T>(System.Collections.Specialized.NameValueCollection formcollection, string formprefix)
        {
            Dictionary<string, string> sentform = new Dictionary<string, string>();
            for (int r = 0; r < formcollection.Count; r++)
            {
                //check for href
                string[] x = formcollection.GetValues(r);
                string y = formcollection.GetKey(r);
                sentform.Add(y.Replace(formprefix, ""), x[0]);
            }

            object instance = (T)Activator.CreateInstance(typeof(T));
            return GetRecords(instance, sentform);
        }

        public static object CreateObjectFromForm<T>(System.Collections.Specialized.NameValueCollection formcollection)
        {
            return CreateObjectFromForm<T>(formcollection, "");
        }

        /// <summary>
        /// Saves the sort order of a specified element
        /// </summary>
        /// <param name="classname">The class name of the element you are modifying (Product, BlogPost, EcomAddress, etc.)</param>
        /// <param name="argNames">The ID column name (ProductId, BlogPostId, EcomAddressId, etc.)</param>
        /// <param name="argValues">The ID of the element you are modifying</param>
        /// <param name="sortorder">The new sort order of the element you are modifying. This should come from the HTML index of the element within the list</param>
        /// <param name="connection"></param>
        public static void SaveSortOrder(string classname, string argNames, string argValues, int sortorder, Boolean checkIfExists, string connection)
        {
            SqlCommand command = DbCommand(new SqlConnection(connection));
            string thisTable = "tbl" + classname;
            string where = "";
            string insertColumns = "";
            string insertValues = "";
            if (argNames.Contains("|"))
            {
                string[] argNameArray = argNames.TrimEnd('|').Split('|');
                string[] argValueArray = argValues.TrimEnd('|').Split('|');
                for (int i = 0; i < argNameArray.Length; i++)
                {
                    where += " " + General.cleanSQL(argNameArray[i]) + " = @" + argNameArray[i];
                    command.Parameters.Add(new SqlParameter("@" + argNameArray[i], argValueArray[i]));
                    if (i != (argNameArray.Length - 1))
                        where += " AND";

                    if (checkIfExists)
                    { //for insert statement if needed
                        insertColumns += argNameArray[i] + ",";
                        insertValues += argValueArray[i] + ",";
                    }
                }
                insertColumns = insertColumns.TrimEnd(',');
                insertValues = insertValues.TrimEnd(',');
            }
            else //single argument
            {
                if (argNames.Length > 0)
                {
                    where = " " + General.cleanSQL(argNames) + " = @" + argNames;
                    command.Parameters.Add(new SqlParameter("@" + argNames, argValues));
                }
                else
                {
                    where = " " + General.cleanSQL(classname + "Id") + " = @" + classname + "Id";
                    command.Parameters.Add(new SqlParameter("@" + classname + "Id", argValues));
                }
            }
            command.Parameters.Add(new SqlParameter("@SortOrder", sortorder));
            Boolean doUpdate = true;
            if (checkIfExists)
            {
                command.CommandText = "SELECT COUNT(*) AS 'Count' FROM " + thisTable + " WHERE " + where;
                ItemCount itemcount = DbReader<ItemCount>(command)[0];
                if (itemcount.Count == 0) //this record doesn't exist, so we need an INSERT instead of UPDATE
                    doUpdate = false;

                if (!doUpdate) //INSERT
                {
                    command.CommandText = "INSERT INTO " + thisTable + "(" + insertColumns + ", SortOrder) VALUES (" + insertValues + ", @SortOrder)";
                    command.Connection = new SqlConnection(connection);
                    ExecuteNoReturn(command);
                }
            }
            if (doUpdate)
            {
                string sqlupdate = "UPDATE " + thisTable + " SET SortOrder=@SortOrder WHERE " + where;
                command.CommandText = sqlupdate;
                command.Connection = new SqlConnection(connection);
                ExecuteNoReturn(command);
            }
        }

        /// <summary>
        /// Saves the sort order of a specified element
        /// </summary>
        /// <param name="classtype">The class name of the element you are modifying (Product, BlogPost, EcomAddress, etc.)</param>
        /// <param name="elementid">The ID of the element you are modifying</param>
        /// <param name="sortorder">The new sort order of the element you are modifying</param>
        /// <param name="connection"></param>
        public static void SaveSortOrder(string classname, string elementid, int sortorder, string connection)
        {
            SaveSortOrder(classname, "", elementid, sortorder, false, connection);
        }

        private class ItemCount
        {
            public int Count { get; set; }
        }

        public class SaveInputParams
        {
            object _instance = new object();
            bool _update = false;
            string[] _idcolumn = new string[] { };
            string _tblname = "";
            string _connection = "";
            string[] _exemptions = new string[] { };
            Dictionary<string, string> _dbfieldmapping = new Dictionary<string, string>();


            public object Instance
            {
                get { return _instance; }
                set { _instance = value; }
            }
            public bool Update
            {
                get { return _update; }
                set { _update = value; }
            }
            public string[] IdColumn
            {
                get { return _idcolumn; }
                set { _idcolumn = value; }
            }
            public string TblName
            {
                get { return _tblname; }
                set { _tblname = value; }
            }
            public string Connection
            {
                get { return _connection; }
                set { _connection = value; }
            }
            public string[] Exemptions
            {
                get { return _exemptions; }
                set { _exemptions = value; }
            }
            public Dictionary<string, string> DbFieldMapping
            {
                get { return _dbfieldmapping; }
                set { _dbfieldmapping = value; }
            }


        }
    }
}


