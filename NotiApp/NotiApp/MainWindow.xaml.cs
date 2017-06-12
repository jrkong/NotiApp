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

        public MainWindow()
        {
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
            List<string> strStatus = new List<string>();
            List<string> strService = new List<string>();
            try
            {
                string query = @"Select t1.* from server_programs.csv_service t1 inner join (select max(csv_timestmp) recent from server_programs.csv_service) t2 on t1.csv_timestmp = t2.recent;";
                MySqlCommand cmd = new MySqlCommand(query, connect);

                MySqlDataReader dr = cmd.ExecuteReader();

                dr.Read();
                var test = dr;
                
                while (dr.Read())
                {
                    strStatus.Add((string)dr[3]);
                    strService.Add((string)dr[4]);
                }

                dr.Close();
            }catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            string strAdd = "<table style='width:100%><tr><th>Service</th><th>Status</th></tr><tr><th>"+strService[0]+ "</th><th>" + strStatus[0] + "</th></tr>";
            string strHTML = @"<html><head><title>Report </title><meta charset='UTF-8'></head><style>p{font-family:Arial;}</style><body>" + strAdd + "</br><p>TEST</p></br><p style='font-size:16;'><b>NOTIFICATION BOT</b></p></br><img src='http://websdepot.com/wp-content/uploads/2012/01/newsite_websdepot_logo.jpg'><p style='font-size:16; color:#66ccff'><b><i>Powered By Eurapp &#8482; Your Apps. Your Way.</i></b></p></body></html>";
            wb1.NavigateToString(strHTML);
        }
    }
}
