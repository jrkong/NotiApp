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
            


            try
            {
                string query = @"Select t1.* from server_programs.csv_service t1 inner join (select max(csv_timestmp) recent from server_programs.csv_service) t2 on t1.csv_timestmp = t2.recent;";
                MySqlCommand cmd = new MySqlCommand(query, connect);

                MySqlDataReader dr = cmd.ExecuteReader();

                dr.Read();
                var test = dr;
                
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
                }

                dr.Close();
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //HTML BUILDING
            string strRows = "";
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
                            <th style=background-color:" + strBackgroundColour + "; color:"+ strFontColour +">"+ table.getService() + @"</th>
                            <th style=background-color:" + strBackgroundColour + "; color:" + strFontColour + ">" + table.getSubservice() + @"</th>
                            <th style=background-color:" + strBackgroundColour + "; color:" + strFontColour + ">" + table.getServer() + @"</th>
                            <th style=background-color:" + strBackgroundColour + "; color:" + strFontColour + ">" + table.getStatus() + @"</th>
                            <th style=background-color:" + strBackgroundColour + "; color:" + strFontColour + ">" + table.getStartup() + @"</th>
                            <th style=background-color:" + strBackgroundColour + "; color:" + strFontColour + ">" + table.getError() + @"</th>
                            </tr>
                            ";
            }

            string strTable = @"<table style='width:100%'>
                                <tr>
                                    <th>Service</th>
                                    <th>Subservice</th>
                                    <th>Server</th>
                                    <th>Status</th>
                                    <th>Startup</th>
                                    <th>Error</th>
                                </tr>
                                "+ strRows +@"
                                
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
                                        border: 1px solid black;
                                        border-collapse: collapse;
                                    }
                                </style>
                                <body>"
                                    + strTable + 
                                    @"</br>
                                    <p>TEST</p>
                                    </br>
                                    <p style='font-size:16;'>
                                        <b>NOTIFICATION BOT</b>
                                    </p>
                                    </br>
                                    <img src='http://websdepot.com/wp-content/uploads/2012/01/newsite_websdepot_logo.jpg'>
                                    <p style='font-size:16; color:#66ccff'><b><i>Powered By Eurapp &#8482; Your Apps. Your Way.</i></b></p>
                                </body>
                            </html>";
            wb1.NavigateToString(strHTML);
        }


        private void makeEmail()
        {
            SmtpClient client = new SmtpClient("smtp.gmail.com",587);
            client.EnableSsl = true;
            client.DeliveryMethod = SmtpDeliveryMethod.Network;
            client.Credentials = new NetworkCredential("His only gap close", "is walking towards you");
            client.Timeout = 20000;
            

            MailMessage msg = new MailMessage("His only gap close", "menacingly");
            msg.Subject = "test";
            msg.Body = "Walks menacingly";
            msg.IsBodyHtml = true;
            try
            {                
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
}
