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
                                 <tr><th style='background-color:" + strBg + "; color:" + strFc + "'>" + sIn.getName() + @"</th></tr>
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
                            </br>";
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

        private void makeEmail(string input, string host, string from, string to, string subject)
        {



            MailMessage msg = new MailMessage(from, to);
            msg.Subject = subject;
            msg.Body = input;
            msg.IsBodyHtml = true;
            try
            {
                SmtpClient client = new SmtpClient(host, 25);
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                //client.Credentials = new NetworkCredential("test", "test");
                client.Timeout = 20000;


                ServicePointManager.ServerCertificateValidationCallback =
                delegate (object s, X509Certificate certificate,
                 X509Chain chain, SslPolicyErrors sslPolicyErrors)
                { return true; };

                client.Send(msg);

            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }finally
            {
                msg.Dispose();
            }
        }
    }

    public class Tinfo
    {
        string strStatus;
        string strService;
        string strSubservice;
        string strStartup;
        string strError;
        string strServer;

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
}
/*
            //HTML BUILDING
            //after line 134
            string strRows = "";
            string strPost = "";
            bool blnCheck = true;

            //add logic for strPost
            foreach(Tinfo table in tableInfo)
            {
                string strBackgroundColour;
                string strFontColour;
                if (table.getStatus()=="false")
                {
                    strBackgroundColour = "red";
                    strFontColour = "white";
                }else
                {
                    strBackgroundColour = "green";
                    strFontColour = "black";
                }
                strRows = strRows+@"
                            <tr>
                            <td style='background-color:" + strBackgroundColour + "; color:"+ strFontColour +"'>"+ table.getService() + @"</th>
                            <td style='background-color:" + strBackgroundColour + "; color:" + strFontColour + "'>" + table.getSubservice() + @"</th>
                            <td style='background-color:" + strBackgroundColour + "; color:" + strFontColour + "'>" + table.getStatus() + @"</th>
                            <td style='background-color:" + strBackgroundColour + "; color:" + strFontColour + "'>" + table.getStartup() + @"</th>
                            <td style='background-color:" + strBackgroundColour + "; color:" + strFontColour + "'>" + table.getError() + @"</th>
                            </tr>
                            ";
                if(table.getStatus() == "false")
                {
                    blnCheck = false;
                }
            }

            string strBg;
            string strFc;
            if (!blnCheck)
            {
                strBg = "red";
                strFc = "white";
            }else
            {
                strBg = "green";
                strFc = "black";
            }
            string strTable = @"<table style='width:100%'>
                                <tr><th style='background-color:" + strBg + "; color:" + strFc + "'>" + strBg + @"</th></tr>
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
                                        " + strPost + @"
                                    </table>
                            </table>";

            string strHTML = @"<html>
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
                                    <p>Websdepot Server Report</p>"
                                    + strTable + 
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
            */
