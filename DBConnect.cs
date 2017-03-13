using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using Emgu.CV;
using System.Data;

namespace TugasAkhir
{
    class DBConnect
    {
        private MySqlConnection connection;
        private string server;
        private string database;
        private string uid;
        private string password;

        //Constructor
        public DBConnect()
        {
            Initialize();
        }

        //Initialize values
        private void Initialize()
        {
            server = "localhost";
            database = "db_tugas_akhir";
            uid = "root";
            password = "";

            string connectionString;
            connectionString = "SERVER=" + server + ";" + "DATABASE=" +
            database + ";" + "UID=" + uid + ";" + "PASSWORD=" + password + ";";

            connection = new MySqlConnection(connectionString);
        }

        //open connection to database
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                //When handling errors, you can your application's response based 
                //on the error number.
                //The two most common error numbers when connecting are as follows:
                //0: Cannot connect to server.
                //1045: Invalid user name and/or password.
                switch (ex.Number)
                {
                    case 0:
                        Console.WriteLine("Cannot connect to server.  Contact administrator");
                        break;

                    case 1045:
                        Console.WriteLine("Invalid username/password, please try again");
                        break;
                }
                return false;
            }
        }

        //Close connection
        private bool CloseConnection()
        {
            try
            {
                connection.Close();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        //Insert statement
        public void Insert(List<Matrix<float>> GLCMFeatures)
        {
            StringBuilder sCommand = new StringBuilder("INSERT INTO tbl_fitur (fitur_asm, fitur_contrast, fitur_correlation, fitur_idm, fitur_entropy) VALUES ");

            //open connection
            if (this.OpenConnection() == true)
            {
                string query = "TRUNCATE TABLE tbl_fitur";
                using (MySqlCommand myCmd = new MySqlCommand(query.ToString(), connection))
                {
                    //Execute command
                    myCmd.ExecuteScalar();
                }

                query = "ALTER TABLE tbl_fitur AUTO_INCREMENT = 1";
                using (MySqlCommand myCmd = new MySqlCommand(query.ToString(), connection))
                {
                    //Execute command
                    myCmd.ExecuteScalar();
                }

                List<string> Rows = new List<string>();
                for (int i = 0; i < GLCMFeatures.Count; i++)
                {
                    Matrix<float> fitur = (Matrix<float>)GLCMFeatures[i];
                    Rows.Add(string.Format("('{0}','{1}','{2}','{3}','{4}')", Convert.ToDouble(fitur.Data[0, 0]), Convert.ToDouble(fitur.Data[0, 1]), Convert.ToDouble(fitur.Data[0, 2]),
                        Convert.ToDouble(fitur.Data[0, 3]), Convert.ToDouble(fitur.Data[0, 4])));
                }
                sCommand.Append(string.Join(",", Rows));
                sCommand.Append(";");
                
                //create command and assign the query and connection from the constructor
                using (MySqlCommand myCmd = new MySqlCommand(sCommand.ToString(), connection))
                {
                    myCmd.CommandType = CommandType.Text;
                    //Execute command
                    myCmd.ExecuteNonQuery();
                }

                //close connection
                this.CloseConnection();
            }
        }

        //Select statement
        public Matrix<float> Select()
        {
            string query = "SELECT * FROM tbl_fitur";

            //Create a list to store the result
            List<Matrix<float>> result = new List<Matrix<float>>();
            Matrix<float> coba = new Matrix<float>(1, 5);
            int count = 1;

            //Open connection
            if (this.OpenConnection() == true)
            {
                //Create Command
                MySqlCommand cmd = new MySqlCommand(query, connection);
                //Create a data reader and Execute the command
                using (MySqlDataReader dataReader = cmd.ExecuteReader()) {
                    //Read the data and store them in the list
                    while (dataReader.Read())
                    {
                        //Console.WriteLine(count);
                        Matrix<float> elemen = new Matrix<float>(1, 5);
                        elemen.Data[0, 0] = (float)dataReader.GetDouble(1);
                        elemen.Data[0, 1] = (float)dataReader.GetDouble(2);
                        elemen.Data[0, 2] = (float)dataReader.GetDouble(3);
                        elemen.Data[0, 3] = (float)dataReader.GetDouble(4);
                        elemen.Data[0, 4] = (float)dataReader.GetDouble(5);
                        result.Add(elemen.Clone());
                        
                        //Console.WriteLine(coba.Data[count, 0] + " " + coba.Data[count, 1] + " " + coba.Data[count, 2] + " " + coba.Data[count, 3] + " " + coba.Data[count, 4]);
                        //count++;
                    }
                    //close Data Reader
                    dataReader.Close();
                }
                

                Matrix<float> hasil = new Matrix<float>(1, 5);
                Console.WriteLine(result.Count);
                foreach (Matrix<float> row in result)
                {
                    hasil = hasil.ConcateVertical(row).Clone();
                }
                hasil = hasil.RemoveRows(0, 1).Clone();

                for (int i = 0; i < hasil.Rows; i++)
                {
                    Console.WriteLine(i+1);
                    for (int j = 0; j < hasil.Cols; j++)
                    {
                        Console.WriteLine(hasil.Data[i, j].ToString("G9"));
                    }
                }
                //close Connection
                this.CloseConnection();

                //return list to be displayed
                return hasil;
            }
            else
            {
                Console.WriteLine("asdasdasd");
                return null;
            }
        }
        /*
        //Update statement
        public void Update()
        {
        }

        //Delete statement
        public void Delete()
        {
        }

        //Count statement
        public int Count()
        {
        }
        */
    }
}
