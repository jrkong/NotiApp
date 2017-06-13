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

            dr.Read();
            var test = dr;

            int intCounter = 0;
            while (dr.Read())
            {
                Db dbTemp = new Db();
                dbTemp.setName((string)dr[intCounter]);

                query = @"Select t1.* from "+ dbTemp.getName() +".csv_service t1 inner join (select max(csv_timestmp) recent from "+ dbTemp.getName() +".csv_service) t2 on t1.csv_timestmp = t2.recent;";
                cmd = new MySqlCommand(query, connect);

                MySqlDataReader dr2 = cmd.ExecuteReader();

                dr2.Read();
                test = dr2;

                while (dr.Read())
                {
                    Tinfo tTemp = new Tinfo();

                    tTemp.setService((string)dr2[4]);
                    tTemp.setSubservice((string)dr2[5]);
                    tTemp.setServer((string)dr2[2]);
                    tTemp.setStatus((string)dr2[3]);
                    DateTime dtStore = (DateTime)dr2[1];
                    tTemp.setStartup(dtStore.ToString());
                    tTemp.setError((string)dr2[6]);

                    tableInfo.Add(tTemp);
                    
                }
                dr2.Close();
                dbTemp.setTables(tableInfo);
                dbList.Add(dbTemp);
                intCounter++;
            }
            dr.Close();

                try
            {
                

                
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //HTML BUILDING
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
            makeEmail(strHTML);
            wb1.NavigateToString(strHTML);
        }


        private void makeEmail(string input)
        {



            MailMessage msg = new MailMessage("test@test.com", "test@test.com");
            msg.Subject = "test";
            msg.Body = input;
            msg.IsBodyHtml = true;
            try
            {
                SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
                client.EnableSsl = true;
                client.DeliveryMethod = SmtpDeliveryMethod.Network;
                client.Credentials = new NetworkCredential("test", "test");
                client.Timeout = 20000;
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

    public class Db
    {
        string strName;
        List<Tinfo> tTables;

        public void setName(string strIn)
        {
            strName = strIn;
        }

        public string getName()
        {
            return strName;
        }

        public void setTables(List<Tinfo> tIn)
        {
            tTables = tIn;
        }

        public List<Tinfo> getTables()
        {
            return tTables;
        }
    }
}
