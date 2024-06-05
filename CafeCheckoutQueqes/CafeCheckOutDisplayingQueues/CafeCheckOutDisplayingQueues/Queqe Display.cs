using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;

namespace CafeCheckOutDisplayingQueues
{
    public partial class Form1 : Form
    {
        private string connectionString = @"Data Source=LAPTOP-R45B7D8N\SQLEXPRESS;Initial Catalog=Cafedatabase;Integrated Security=True;";
        private System.Windows.Forms.Timer updateTimer;
        private System.Windows.Forms.Timer blinkTimer;
        private bool isBlinkingVisible = true;

        public Form1()
        {
            InitializeComponent();
            InitializeUpdateTimer();
            InitializeBlinkTimer();
            LoadQueueData();
        }

        private void InitializeUpdateTimer()
        {
            updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 10000; // Set the interval to 10 seconds for updating data source
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private void InitializeBlinkTimer()
        {
            blinkTimer = new System.Windows.Forms.Timer();
            blinkTimer.Interval = 5500; // Interval for the blinking effect
            blinkTimer.Tick += BlinkTimer_Tick;
            blinkTimer.Start();
        }

        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            LoadQueueData();
        }

        private void BlinkTimer_Tick(object sender, EventArgs e)
        {
            isBlinkingVisible = !isBlinkingVisible;
            ToggleLabelVisibility(isBlinkingVisible);
        }

        private void ToggleLabelVisibility(bool isVisible)
        {
            labelToGoFirst.Visible = isVisible;
            labelToGoNext.Visible = isVisible;
            labelForHereFirst.Visible = isVisible;
            labelForHereNext.Visible = isVisible;
        }

        private void LoadQueueData()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Fetch "To Go" data
                string toGoQuery = @"SELECT DISTINCT c.Queuing_num 
                                     FROM Customer c 
                                     JOIN Transactions t ON c.Customer_Id = t.Customer_Id 
                                     WHERE c.Entry_type = 'To Go' 
                                     AND t.Status = 'Queued' 
                                     AND c.Entry_type != 'Cancelled' 
                                     ORDER BY c.Queuing_num";

                SqlDataAdapter toGoAdapter = new SqlDataAdapter(toGoQuery, connection);
                DataTable toGoTable = new DataTable();
                toGoAdapter.Fill(toGoTable);

                // Fetch "For Here" data
                string forHereQuery = @"SELECT DISTINCT c.Queuing_num 
                                        FROM Customer c 
                                        JOIN Transactions t ON c.Customer_Id = t.Customer_Id 
                                        WHERE c.Entry_type = 'Here' 
                                        AND t.Status = 'Queued' 
                                        AND c.Entry_type != 'Cancelled' 
                                        ORDER BY c.Queuing_num";

                SqlDataAdapter forHereAdapter = new SqlDataAdapter(forHereQuery, connection);
                DataTable forHereTable = new DataTable();
                forHereAdapter.Fill(forHereTable);

                UpdateLabels(toGoTable, forHereTable);
            }
        }

        private void UpdateLabels(DataTable toGoTable, DataTable forHereTable)
        {
            List<string> toGoQueue = toGoTable.AsEnumerable().Select(row => row["Queuing_num"].ToString()).Distinct().ToList();
            List<string> forHereQueue = forHereTable.AsEnumerable().Select(row => row["Queuing_num"].ToString()).Distinct().ToList();

            labelToGoFirst.Text = toGoQueue.Count > 0 ? "Current Customer: " + toGoQueue[0] : "Current Customer: None";
            labelToGoNext.Text = toGoQueue.Count > 1 ? "Next Customer: " + toGoQueue[1] : "Next Customer: None";

            labelForHereFirst.Text = forHereQueue.Count > 0 ? "Current Customer: " + forHereQueue[0] : "Current Customer: None";
            labelForHereNext.Text = forHereQueue.Count > 1 ? "Next Customer: " + forHereQueue[1] : "Next Customer: None";
        }
    }
}