﻿using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;


namespace DoctorsTravellers.Models
{
    public class MYSQLServices
    {

        public void SendCommand(string sqlcommand) //For Rahal
        {
            using (MySqlConnection connection = new MySqlConnection(Config.MyConnectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = sqlcommand;
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public List<string> LoadData(string sqlcommand)
        {
            List<string> result = new List<string>();
            using (MySqlConnection connection = new MySqlConnection(Config.MyConnectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand cmd = connection.CreateCommand())
                    {
                       cmd.CommandText = sqlcommand;

                        using (MySqlDataReader rdr = cmd.ExecuteReader())
                        {

                            while (rdr.Read())
                            {
                                int i = 0;
                                string temp = "";
                                while (i < rdr.VisibleFieldCount)
                                {
                                    if (i != 0)
                                        temp += "%";
                                    temp += rdr.GetString(i);
                                    i++;
                                }
                                result.Add(temp);
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }

            }
            return result;
        }

        public void LoadDataOfList(string sqlcommand, List<string> qidlist, List<Question> questionurls)
        {

            using (MySqlConnection connection = new MySqlConnection(Config.MyConnectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand cmd = connection.CreateCommand())
                    {
                        int index = 0;
                        foreach (var n in qidlist)
                        {
                            cmd.CommandText = sqlcommand + n.ToString();
                            questionurls.Add(new Question() { tags = new List<string>(), qid = Int32.Parse(n) });
                            using (MySqlDataReader rdr = cmd.ExecuteReader())
                            {
                               
                                while (rdr.Read())
                                {
                                    int i = 0;
                                    string temp = "";
                                    while (i < rdr.VisibleFieldCount)
                                    {
                                        if (i != 0)
                                            temp += "%";
                                        temp += rdr.GetString(i);
                                        i++;
                                    }
                                   questionurls[index].tags.Add(temp);
                                }

                                index++;
                            }
                            
                        }
                    }
                }
                catch (Exception)
                {
                    throw;
                }

            }

        }

        public void CreateResponseTable(int qid)
        {
            string command = "CREATE TABLE IF NOT EXISTS response" + qid.ToString() + " (id INT AUTO_INCREMENT, respondentID VARCHAR(25), responseText MEDIUMTEXT, PRIMARY KEY (id) )";
            SendCommand(command);
        }

        //qid is table name for responses table name = response'qid' ,eg response77
        public int AddToResponseTable(int qid, string response)
        {
            int rid = -1;
            string rdate = DateTime.Now.ToString("HH:mm:ss tt");
            string command = "INSERT INTO responses(ResponseText,UID,QID,RDate,RLikes) VALUES('" + response + "', '" + GetId().ToString() + "','" + qid + "','" + rdate + "',0)";
            SendCommand(command);
            Question qhelp = new Question();
            return rid = qhelp.GetRID(response, qid);
        }

        public int AddToQuestionTable(string question)
        {
            int qid = -1;
            string qdate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");;
            int uid = GetId();
            string command = "INSERT INTO questions(QuestionscolText,QLikes,QDate,UID) VALUES('" + question + "',0,'" + qdate + "','" + uid + "')";
            SendCommand(command);
            Question qhelp = new Question();
            return qid = qhelp.GetQID(question);
        }

        //need to check if hashtag exits or not though
        public void AddToHashtable(int qid, string question)
        {
            HomePageServices hps = new HomePageServices();
            List<string> hashtagList = new List<string>();
            Question qhelp = new Question();
            hashtagList = qhelp.GetTags(question);
            string hashstring = "";
            using (MySqlConnection connection = new MySqlConnection(Config.MyConnectionString))
            {
                try
                {
                    connection.Open();
                    using (MySqlCommand cmd = connection.CreateCommand())
                    {
                        foreach (string i in hashtagList)
                        {
                            cmd.CommandText = "INSERT INTO hash_tag_table(Tag, QID) VALUES('" + i + "', " + qid.ToString() + " )";
                            cmd.ExecuteNonQuery();
                        }
                    }

                }
                catch (Exception)
                {
                    throw;
                }
            }
        }


        public int AddToRegisterInfoTable(string username, string password, string email, string type, string speciality, string location)
        {
            int qid = -1;

            string values = "'" + username + "','" + username + "','" + email + "','" + password + "','" + type + "'";
            string command = "INSERT INTO register_info(name,username,email,password,type) VALUES(" + values + ")";
            SendCommand(command);

            // location
            command = "INSERT INTO location(UID,location) VALUES(LAST_INSERT_ID(),'" + location + "')";
            SendCommand(command);

            if (type.Equals("doctor"))
            {
                values = "LAST_INSERT_ID(),'" + speciality + "'";
                command = "INSERT INTO speciality(UID,speciality) VALUES(" + values + ")";
                SendCommand(command);
            }

            return qid = 1;
        }

        public int CheckIfRegisteredUserHandler(string username, string password)
        {
            List<String> returnStrings = new List<string>();
            string command = "SELECT UID from register_info where username='" + username + "' and password='" + password + "'";
            returnStrings = LoadData(command);

            if (returnStrings.Count == 0)
            {
                return -1;
            }

            int result = Int32.Parse(returnStrings[0]);
            return result;
        }

        public List<string> getUserInfo(int UID)
        {
            List<String> returnStrings = new List<string>();
            string command = "SELECT * from register_info where UID='" + UID + "'";
            returnStrings = LoadData(command);

            if (returnStrings.Count == 0)
            {
                return null;
            }

            return returnStrings;

        }


        public string getUserSpeciality(int UID)
        {
            List<String> returnStrings = new List<string>();
            string command = "SELECT * from speciality where UID='" + UID + "'";
            returnStrings = LoadData(command);

            if (returnStrings.Count == 0)
            {
                return null;
            }

            string[] temp = returnStrings[0].Split('%');
            string speciality = temp[1];
            return speciality;

        }

        public List<string> getMatchingDoctors(string question)
        {
            List<String> returnStrings = new List<string>();

            List<string> hashResult = new List<string>();
            string[] hashtemp = question.Split(null);
            foreach (string i in hashtemp)
            {
                if (i.Contains('#'))
                    hashResult.Add(i.TrimStart('#'));
            }

            List<string> locationResult = new List<string>();
            string[] loctemp = question.Split(null);
            foreach (string i in loctemp)
            {
                if (i.Contains('@'))
                    locationResult.Add(i.TrimStart('@'));
            }

            string SQL = "SELECT speciality.UID FROM speciality,location WHERE speciality.UID=location.UID AND (speciality.speciality IN ('" + string.Join("','", hashResult) + "') OR location.location IN ('" + string.Join("','", locationResult) + "'))";

            returnStrings = LoadData(SQL);

            if (returnStrings.Count == 0)
            {
                return null;
            }

            return returnStrings;
        }

        public void AddTONotifications(List<string> doctorIDs, string URL)
        {

            string SQL = "";

            for (int i = 0; i < doctorIDs.Count; i++)
            {
                SQL = SQL + "INSERT INTO notifications (UID,URL) VALUES('" + doctorIDs[i] + "','" + URL + "');";
            }
            SendCommand(SQL);
            
        }
        


        /******RAHAL ADD THESE TO A USER MODEL CLASS****/
        public string GetType()
        {
            return "doctor";
        }

        public int GetId()
        {
            return 5;
        }
        /******RAHAL ADD THESE TO A USER MODEL CLASS****/
    }
}