using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;

namespace NotiApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MySqlConnection connect;
        string serverIp = "192.168.7.24";
        string serverPort = "3306";
        

        List<Tinfo> tableInfo = new List<Tinfo>();
        List<Db> dbList = new List<Db>();

        List<string> databaseNameList = new List<string>();

        public MainWindow()
        {
            //makeEmail();
            DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
            builder.Add("Server", serverIp);
            builder.Add("Port", serverPort);
            builder.Add("Uid", "wddev_prog");
            builder.Add("Pwd", "d3Ve7op%#4wd$?");

            connect = new MySqlConnection();
            connect.ConnectionString = builder.ConnectionString;
            try
            {
                System.Console.WriteLine("TEST");
                connect.Open();

            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Connection failed: " + ex.Message);
            }
            
            InitializeComponent();
            string uri = @"C:\Users\Alex Kong\Desktop\test.html";

            //wb1.Navigate(new Uri(uri, UriKind.Absolute));

            string query = "SELECT table_schema `Database` FROM INFORMATION_SCHEMA.TABLES WHERE table_name='csv_service';";
            MySqlCommand cmd = new MySqlCommand(query, connect);
            MySqlDataReader dr = cmd.ExecuteReader();

            List<string> dbNameList = new List<string>();

            while (dr.Read())
            {
                Db dbTemp = new Db();
                dbTemp.setName((string)dr[0]);
                dbList.Add(dbTemp);
            }
            var test = dr;

            int intCounter = 0;
            dr.Close();

            foreach(Db dB in dbList)
            {

                query = @"SELECT csv_server from "+dB.getName()+".csv_service group by csv_server;";
                cmd = new MySqlCommand(query, connect);
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    Server tempServ = new Server();
                    tempServ.setName((string)dr[0]);
                    dB.addServer(tempServ);
                }
                
                dr.Close();

                List<Server> sTemp = dB.getServers();
                foreach (Server sServer in sTemp)
                {
                    query = @"Select t1.* from " + dB.getName() + ".csv_service t1 inner join (select max(csv_timestmp) recent from " + dB.getName() + ".csv_service) t2 on t1.csv_timestmp = t2.recent where csv_server='" + sServer.getName() + "';";
                    cmd = new MySqlCommand(query, connect);

                    dr = cmd.ExecuteReader();

                    //dr.Read();
                    test = dr;
                    List<Tinfo> tAdd = new List<Tinfo>();
                    while (dr.Read())
                    {
                        Tinfo tTemp = new Tinfo();

                        tTemp.setId((int)dr[0]);
                        dB.addId((int)dr[0]);


                        tTemp.setService((string)dr[4]);
                        tTemp.setSubservice((string)dr[5]);
                        tTemp.setServer((string)dr[2]);
                        tTemp.setStatus((string)dr[3]);
                        DateTime dtStore = (DateTime)dr[1];
                        tTemp.setStartup(dtStore.ToString());
                        tTemp.setError((string)dr[6]);

                        tableInfo.Add(tTemp);
                        
                        tAdd.Add(tTemp);
                    }
                    sServer.setTable(tAdd);
                    dr.Close();
                    intCounter++;
                }
                dB.setServers(sTemp);
            }
            dr.Close();
            

            string strTable = "";
            string strHTML = "";

            foreach (Db dB in dbList)
            {
                foreach(int x in dB.getIds())
                {
                    query = @"UPDATE `"+ dB.getName() +"`.`csv_service` SET `csv_checked`='1' WHERE `csv_id`='"+x.ToString()+"';";
                    cmd = new MySqlCommand(query, connect);
                    cmd.ExecuteNonQuery();
                    
                }                                               
            }
            


            //go through each database and generate a report based on the stuff inside
            foreach (Db dLoop in dbList)
            {
                strTable = strTable + headerBuilder(dLoop);
            }


            //TODO: PUSH ME TO SQL (this is the completed email HTML)
            strHTML = htmlBuilder(strTable);

            

            makeEmail(strHTML,"email.websdepot.com","alert@websdepot.com","rlam@websdepot.com","test");
            wb1.NavigateToString(strHTML);
        }

        //build html string for the body
        public string headerBuilder(Db dIn)
        {
            string strDbName = "";
            string strReturn = "";
            List<Server> sList = new List<Server>();
            sList = dIn.getServers();

            //initiate building process...
            //loading database name
            strDbName = @"<h2> " + dIn.getName() + @" </h2>";
            strReturn = strDbName;

            //go through each server and generate a table for them
            //uses the "Server" tag from the CSV
            foreach (Server sServer in sList)
            {
                strReturn = strReturn + tableBuilder(sServer);
            }
            return strReturn;
        }

        public string tableBuilder(Server sIn)
        {
            string strRows = "";
            List<Tinfo> tTemp = sIn.getTables();
            strRows = rowBuilder(tTemp);


            string strBg = "red";
            string strFc = "white";

            
            if (!sIn.getHealth())
            {
                strBg = "red";
                strFc = "white";
            }
            else
            {
                strBg = "green";
                strFc = "black";
            }
            
            string strReturn = @"<table style='width:100%'>
                                 <tr><th style='font-size: 20px; background-color:" + strBg + "; color:" + strFc + "'>" + sIn.getName() + @"</th></tr>
                                 <tr></tr>
                                 <tr>
                                     <table style='width:100%'>
                                        <tr>
                                            <th>Service</th>
                                            <th>Subservice</th>
                                            <th>Status</th>
                                            <th>Startup</th>
                                            <th>Error</th>
                                        </tr>
                                        " + strRows + @"
                                    </table>
                            </table>
                            </br></br></br>";
            return strReturn;
        }
        
        public string rowBuilder(List<Tinfo> tIn)
        {
            string strRows = "";
            //add logic for strPost
            foreach (Tinfo table in tIn)
            {
                string strBackgroundColour;
                string strFontColour;
                if (table.getStatus() == "false")
                {
                    strBackgroundColour = "red";
                    strFontColour = "white";
                }
                else
                {
                    strBackgroundColour = "green";
                    strFontColour = "black";
                }
                strRows = strRows + @"
                            <tr>
                            <td style='background-color:" + strBackgroundColour + "; color:" + strFontColour + "'>" + table.getService() + @"</th>
                            <td style='background-color:" + strBackgroundColour + "; color:" + strFontColour + "'>" + table.getSubservice() + @"</th>
                            <td style='background-color:" + strBackgroundColour + "; color:" + strFontColour + "'>" + table.getStatus() + @"</th>
                            <td style='background-color:" + strBackgroundColour + "; color:" + strFontColour + "'>" + table.getStartup() + @"</th>
                            <td style='background-color:" + strBackgroundColour + "; color:" + strFontColour + "'>" + table.getError() + @"</th>
                            </tr>
                            ";
                if (table.getStatus() == "false")
                {
                    //dIn.setHealth(false);
                }
            }
            return strRows;
        }

        public string htmlBuilder(string strIn)
        {

            string strDatabase = strIn;
            string strReturn =
                          @"<html>
                                <head>
                                    <title>Report </title>
                                    <meta charset='UTF-8'>
                                </head>
                                <style>
                                    p{
                                        font-family:Arial;
                                    }
                                    table, th, td {
                                        font-family:Arial;
                                        border: 1px solid black;
                                        border-collapse: collapse;
                                    }
                                </style>
                                <body>
                                    <h1> TESTING HEADER</h1>"
                                    + strDatabase +
                                    @"</br>
                                    
                                    </br>
                                    <p style='font-size:16;'>
                                        <b>NOTIFICATION BOT</b>
                                    </p>
                                    </br>
                                    <img src='http://websdepot.com/wp-content/uploads/2012/01/newsite_websdepot_logo.jpg'>
                                    <p style='font-size:16; color:#66ccff'><b><i>Powered By Eurapp &#8482; Your Apps. Your Way.</i></b></p>

                                </body>
                            </html>";
            return strReturn;
        }

        //build list of upcoming server reboots
        public string serverBuilder(Db dbIn, TimeSpan tIn)
        {

            string strTimespan = "PLACEHOLDER TIME ";
            string strReturn = "";

            string strRows = "";

            //TODO: Start logic for finding if a reboot will occur in between the next interval
            List<Server> sTemp = new List<Server>();
            List<string> strDisplayList = new List<string>();
            DateTime dtReboot = DateTime.Now + tIn;

            //TODO: Complete parsing and verification logic
            foreach (Server sServer in sTemp)
            {
                bool blnInterval = false;
                bool blnAllowed = false;

                List<string> lTemp = new List<string>();
                lTemp = grabSql(dbIn.getName(), sServer.getName(), 0);
                


                //if the interval check and allowed times check passes then add it
                if (blnInterval && blnAllowed)
                {
                    strDisplayList.Add(sServer.getName());
                }

            }


            strReturn= @"<table style='width:100%'>
                                 <tr><th style='font-size: 20px;'> Servers rebooting in the next "+ strTimespan +@"</th></tr>
                                 <tr></tr>
                                 <tr>
                                     <table style='width:100%'>
                                        " + strRows + @"
                                    </table>
                            </table>
                            </br></br></br>";
            return strReturn;
        }

        public List<string> grabSql(string strDb, string strServ, int intIn)
        {
            //THE QUERY
            //select conf_settings from server_programs.configfile_info where conf_tagline = '[reboot config]' and conf_server = 'RAYLAMOFFICE-PC';

            //Choice 1: look for reboot config
            //Choice 2: look for configured reboot times

            //Choose which query to use
            int intChoice = 0;
            List<string> lReturn = new List<string>();

            MySqlCommand sqlCmd = new MySqlCommand();
            sqlCmd.Connection = connect;
            string strQuery = "";
            if (intChoice == 1)
            {
                 strQuery = "select conf_settings from " + strDb + @".configfile_info where conf_tagline = '[reboot config]' and conf_server = 'RAYLAMOFFICE-PC'";
            }else if(intChoice == 2)
            {
                strQuery = "select conf_settings from " + strDb + @".configfile_info where conf_tagline = '[configured reboot times]' and conf_server = 'RAYLAMOFFICE-PC'";
            }
            sqlCmd.CommandText = strQuery;

            var vReturn = sqlCmd.ExecuteScalar();

            string strReturn = vReturn.ToString();

            lReturn.Add(strReturn);

            return lReturn;
        }


        //TODO: THE MOTHER OF ALL PARSERS ON EMAILS
        public bool verifyTime(DateTime dtIn, List<string> lIn, int intIn)
        {
            //Choice 1: look for reboot config
            //Choice 2: look for configured reboot times
            bool blnReturn = false;
            int intChoice = intIn;

            //split subtags apart

            //strLines[0] = AllowedTime=...
            //strLines[1] = CheckDelay=...
            string[] strLines = lIn[0].Split('\n');
            if(intChoice == 1)
            {
                string strParse = "AllowedTime=";

                string[] splitA = strLines[0].Split(',');

                List<DayRange> allowedRebootTimes = new List<DayRange>();
            }
            else if(intChoice == 2)
            {

            }

            return blnReturn;
        }

        private void makeEmail(string input, string host, string from, string to, string subject, string uname = null, string pass = null)
        {



            MailMessage msg = new MailMessage(from, to);
            msg.Subject = subject;
            msg.Body = input;
            msg.IsBodyHtml = true;
            try
            {
                SmtpClient client = new SmtpClient(host, 25);
                client.EnableSsl = false;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //client.Credentials = new NetworkCredential("test", "test");
                client.Timeout = 20000;

                /*
                ServicePointManager.ServerCertificateValidationCallback =
                delegate (object s, X509Certificate certificate,
                 X509Chain chain, SslPolicyErrors sslPolicyErrors)
                { return true; };
                */
                client.Send(msg);

                storeEmail(input, host, from, to, subject, true, uname, pass);
            }catch(Exception ex)
            {
                storeEmail(input, host, from, to, subject, false, uname, pass);
                MessageBox.Show(ex.Message);
            }finally
            {
                msg.Dispose();
            }
        }

        private void storeEmail(string input, string host, string from, string to, string subject, bool v, string uname, string pass)
        {
            MySqlCommand cmd = new MySqlCommand();
            cmd.Connection = connect;
            cmd.CommandText = "INSERT INTO " +
                "server_programs.smtp_info(smtp_id, smtp_host, smtp_to, smtp_from, smtp_subject, smtp_body, smtp_uname,  smtp_pass, smtp_sent) " +
                "VALUES(@smtp_id, @smtp_host, @smtp_to, @smtp_from, @smtp_subject, @smtp_body, @smtp_uname,  @smtp_pass, @smtp_sent)";
            cmd.Prepare();

            cmd.Parameters.AddWithValue("@smtp_id", null);
            cmd.Parameters.AddWithValue("@smtp_host", host);
            cmd.Parameters.AddWithValue("@smtp_to", to);
            cmd.Parameters.AddWithValue("@smtp_from", from);
            cmd.Parameters.AddWithValue("@smtp_subject", subject);
            cmd.Parameters.AddWithValue("@smtp_body", input);
            cmd.Parameters.AddWithValue("@smtp_uname", uname);
            cmd.Parameters.AddWithValue("@smtp_pass", pass);
            cmd.Parameters.AddWithValue("@smtp_sent", v ? 1 : 0);
            cmd.ExecuteNonQuery();
        }
    }

    public class Tinfo
    {
        int intId;
        string strStatus;
        string strService;
        string strSubservice;
        string strStartup;
        string strError;
        string strServer;

        //ID getter setter set
        public void setId(int intIn)
        {
            intId = intIn;
        }

        public int getId()
        {
            return intId;
        }

        //status getter setter set
        public void setStatus(string strIn)
        {
            strStatus = strIn;
        }

        public string getStatus()
        {
            return strStatus;
        }

        //service getter setter set
        public void setService(string strIn)
        {
            strService = strIn;
        }

        public string getService()
        {
            return strService;
        }

        //subservice getter setter set
        public void setSubservice(string strIn)
        {
            strSubservice = strIn;
        }

        public string getSubservice()
        {
            return strSubservice;
        }

        //startup setter getter set
        public void setStartup(string strIn)
        {
            strStartup = strIn;
        }

        public string getStartup()
        {
            return strStartup;
        }

        //error setter getter set
        public void setError(string strIn)
        {
            strError = strIn;
        }

        public string getError()
        {
            return strError;
        }

        //error setter getter set
        public void setServer(string strIn)
        {
            strServer = strIn;
        }

        public string getServer()
        {
            return strServer;
        }

    }

    public class Server
    {
        string strName;
        List<Tinfo> tTable = new List<Tinfo>();
        bool blnHealth = true;

        public bool getHealth()
        {
            foreach (Tinfo tEntry in tTable)
            {
                if (tEntry.getStatus() == "false")
                {
                    blnHealth = false;
                }
            }
            return blnHealth;
        }

        public void setName(string strIn)
        {
            strName = strIn;
        }

        public string getName()
        {
            return strName;
        }

        public void setTable(List<Tinfo> tIn)
        {
            tTable = tIn;
        }

        public List<Tinfo> getTables()
        {
            return tTable;
        }
    }
    public class Db
    {
        string strName;
        List<Server> sServe = new List<Server>();
        List<int> lIds = new List<int>();

        //ID getter setter set
        public void addId(int intIn)
        {
            lIds.Add(intIn);
        }

        public List<int> getIds()
        {
            return lIds;
        }


        public void setName(string strIn)
        {
            strName = strIn;
        }

        public string getName()
        {
            return strName;
        }

        public void addServer(Server sIn)
        {
            sServe.Add(sIn);
        }

        public List<Server> getServers()
        {
            return sServe;
        }

        public void setServers(List<Server> sIn)
        {
            sServe = sIn;
        }
    }

    /* =======================================================================================================================================================================================
     * DateRange
     *  - This object stores all the information we need to store all the information to determine the range between two dates and times
     * =======================================================================================================================================================================================
     */
    class DayRange
    {
        int dayX;
        int dayY;
        TimeSpan timeX;
        TimeSpan timeY;

        /* =======================================================================================================================================================================================
         * DateRange.DateRange()
         *  - The default constructor of the DateRange object
         * =======================================================================================================================================================================================
         */
        public DayRange() { }

        /* =======================================================================================================================================================================================
         * DateRange.DateRange(string)
         *  - Takes a string, parses it and stores it into the DateRange object
         * =======================================================================================================================================================================================
         */
        public DayRange(string i)
        {
            string[] splitA = i.Split('|');

            string[] splitDay = splitA[0].Split('-');
            if (splitDay.Length == 1)
            {
                dayX = twoLetterDay(splitDay[0]);
                dayY = dayX;
            }
            else
            {
                dayX = twoLetterDay(splitDay[0]);
                dayY = twoLetterDay(splitDay[1]);
            }
            string[] splitTime = splitA[1].Split('-');
            timeX = Convert.ToDateTime(splitTime[0]).TimeOfDay;
            timeY = Convert.ToDateTime(splitTime[1]).TimeOfDay;
        }

        //=====================================================
        // Check to see if today's day of week is within range
        // of the days of weeks outlined in parameters
        //=====================================================
        public bool inDayRange()
        {
            int i = (int)DateTime.Today.DayOfWeek;
            if (dayX < dayY)
            {
                if (i >= dayX && i <= dayY)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (dayX == dayY)
                {
                    if (i == dayX)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    if (i >= dayY || i <= dayX)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        //=============================================================
        // Check if current time is in the range of the times outlined
        // in parameters
        //=============================================================
        public bool inTimeRange()
        {
            TimeSpan i = DateTime.Now.TimeOfDay;

            if (timeY > timeX)
            {
                if (i >= timeX && i <= timeY)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                if (i >= timeX && i > timeY)
                {
                    return true;
                }
                else if (i < timeX && i <= timeY)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

        }

        //=================================================
        //Returns two letter weekday format as int.
        //Follows the same format the DateTime object uses.
        //=================================================
        public int twoLetterDay(string i)
        {
            if (String.Compare("su", i) == 0)
            {
                return 0;
            }
            else if (String.Compare("mo", i) == 0)
            {
                return 1;
            }
            else if (String.Compare("tu", i) == 0)
            {
                return 2;
            }
            else if (String.Compare("we", i) == 0)
            {
                return 3;
            }
            else if (String.Compare("th", i) == 0)
            {
                return 4;
            }
            else if (String.Compare("fr", i) == 0)
            {
                return 5;
            }
            else if (String.Compare("sa", i) == 0)
            {
                return 6;
            }
            else //In case of invalid input
            {
                return -1;
            }
        }
    }
}

