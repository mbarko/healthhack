using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DoctorsTravellers.Models
{
    public class HomePageServices
    {

        public List<Question> QuestionSearchHandelr(string question)
        {
            MYSQLServices ms = new MYSQLServices();
            List<Question> result = new List<Question>();

            try
            {
                Question qhelp = new Question();
                List<int> qidlist = new List<int>();
                List<string> questionlist = new List<string>();
                List<string> tags = qhelp.GetTags(question);
                if (tags.Count == 0) { return result; }
                string temp = "'";
                string qidliststring = "";

                foreach (string i in tags)
                { temp += i + "','"; }
                temp = temp.Remove(temp.Length - 2, 2);

                qidlist = ms.LoadData("SELECT QID From hash_tag_table WHERE Tag IN(" + temp + ")").OrderBy(i => i.Count()).Select(x => int.Parse(x)).Distinct().ToList();

                foreach (int k in qidlist)////////////////////////slow I will adjust///////////////////////
                {

                    qidliststring += k.ToString();
                    if (qidlist[qidlist.Count() - 1] != k)
                        qidliststring += " OR QID = ";

                }///////////////////////////////////////////////////////////////////////////////////////////

                if (qidliststring != "")
                    questionlist = ms.LoadData("SELECT QuestionscolText From questions WHERE QID = " + qidliststring);

                int count = 0;
                if (questionlist.Count == 0) { return result; }
                foreach (string j in questionlist)
                {
                    result.Add(new Question { question = j, qid = qidlist[count], url = "home/QuestionPage/" + qidlist[count] + "/" + string.Join("", qhelp.GetTags(j).ToArray()).Replace('#', '-') });
                    count++;
                }

            }
            catch (Exception e) { throw; }
            return result;
        }

        public int QuestionPostHandelr(string question)
        {
            int qid = -1;
            List<String> doctorsUID = new List<string>();
            MYSQLServices ms = new MYSQLServices();
            try
            {
                qid = ms.AddToQuestionTable(question);
                ms.CreateResponseTable(qid);
                ms.AddToHashtable(qid, question);

                // when we post the question we should check if there is a doctor that qualifies to answer the question
                // and if we can find a doctor to answer that question we send a notification to that doctor
                doctorsUID = ms.getMatchingDoctors(question);
                if (doctorsUID == null)
                {
                    return qid;
                }
                else
                {
                    string URL = QuestionSearchHandelr(question)[0].url;
                    ms.AddTONotifications(doctorsUID, URL);
                }

                
            }
            catch (Exception e) { throw; }
            return qid;
        }

        public int ResponsePostHandelr(int qid, string response)
        {
            MYSQLServices ms = new MYSQLServices();
            int rid = -1;
            try
            {
                rid = ms.AddToResponseTable(qid, response);


            }
            catch (Exception e) { throw; }
            return rid;
        }

        public int RegisterInfoPostHandler(System.Web.Mvc.FormCollection collection)
        {
            string username = collection.Get("username");
            string password = collection.Get("password1");
            string email = collection.Get("email");
            string type = collection.Get("radio-group-1");
            string speciality = collection.Get("speciality");
            string location = collection.Get("location");

            int qid = -1;
            MYSQLServices ms = new MYSQLServices();
            try
            {
                qid = ms.AddToRegisterInfoTable(username, password, email, type, speciality, location);
                return qid;
            }
            catch (Exception e) { throw; }
            return qid;
        }

        public int CheckIfRegisteredUserHandler(System.Web.Mvc.FormCollection collection)
        {
            int returnId = -1;
            string username = collection.Get("username");
            string password = collection.Get("password");

            MYSQLServices ms = new MYSQLServices();
            try
            {
                returnId = ms.CheckIfRegisteredUserHandler(username, password);
            }
            catch (Exception e) { throw; }

            return returnId;
        }

        public List<string> getUserInfo(int UID)
        {
            List<String> returnStrings = new List<string>();
            MYSQLServices ms = new MYSQLServices();
            try
            {
                returnStrings = ms.getUserInfo(UID);
            }
            catch (Exception e) { throw; }

            return returnStrings;
        }

        public string getUserSpeciality(int UID)
        {

            String speciality;
            MYSQLServices ms = new MYSQLServices();
            try
            {
                speciality = ms.getUserSpeciality(UID);
            }
            catch (Exception e) { throw; }

            return speciality;
        }

    }
}