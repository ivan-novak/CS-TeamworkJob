using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using TeamWorkSharp;

namespace TeamworkJob
{
    class TeamworkApp
    {

        static void Main(string[] args)
        {
            if (args.Length == 1) if (args[0] == "/h" || args[0] == "/H") Console.WriteLine("For send parameters run: TeamworkJob.exe [userId [domainName [userToken]]] or use TeamworkJob.exe.config file");
            var userID = "XXXXX548";
            var domainName = "XXXXdata";
            var userToken = "ZZZZZZZZZZZZZZZZZZZZZZZZZ:TSSSSSSSSSSSSSS-#$$#";
            var appSettings = ConfigurationManager.AppSettings;
            if (appSettings["userID"] != null) userID = appSettings["userID"];
            if (appSettings["domainName"] != null) domainName = appSettings["domainName"];
            if (appSettings["userToken"] != null) userToken = appSettings["userToken"];
            if (args.Length >= 1) userID = args[0];
            if (args.Length >= 2) domainName = args[1];
            if (args.Length >= 3) userToken = args[2];
            var client = new TeamworkClient(domainName, userToken);
            var tasks = client.GetAllTasks(responsiblePartyIds: userID, includeCompletedTasks: false, includeToday:true, filter:"thisweek");
            if (client.isError) Console.WriteLine("\n" + DateTime.Now+" "+client.errorDesc);
            var path = Directory.GetCurrentDirectory();
            Regex regex = new Regex(@"\\|/|:|\*|\?|<|>|\||""");
            foreach (var task in tasks) if (task.canComplete)                               
            try
            {
                if (task.responsiblePartyId == null) continue;
                if (task.responsiblePartyId.ToString() != userID) continue;
                var projectName = regex.Replace(task.projectName, "_").Trim();
                var projectId = task.projectId.ToString();
                var taskName = regex.Replace(task.content, "_").Trim();
                var listName = regex.Replace(task.todoListName, "_");
                var companyName = regex.Replace(task.companyName, "_").Trim();
                var companyCode = regex.Replace(companyName.Substring(6), "_").Trim();
                var listId = task.todoListId.ToString().Trim();
                var taskId = task.id.ToString().Trim();
                var comList = task.description.Split('\n');
                var year = regex.Replace(task.startDate.Substring(0, 4), "_").Trim();
                var parentId = task.parentTaskId;
                var parentName = " ";
                if(parentId != "") parentName = regex.Replace(client.GetSingleTask(parentId).content, "_").Trim();
                Console.WriteLine("\n" + DateTime.Now );
                foreach (var comFile in comList) if (comFile!="") if (!File.Exists(path + "\\" + comFile.Split(' ')[0])) Console.WriteLine("Denid to run a command [" + comFile + "]  for task " + task.id);
                else 
                {
                    Console.WriteLine("Start to run autoscripts "+ comFile + " for task " + task.id);
                    var process = new Process();
                    process.StartInfo.FileName = "cmd.exe";
                    process.StartInfo.Arguments = @"/c  cd " + path + "&" + comFile;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.Environment.Add("PROJECTNAME", projectName);
                    process.StartInfo.Environment.Add("PROJECTID", projectId);
                    process.StartInfo.Environment.Add("TASKNAME", taskName);
                    process.StartInfo.Environment.Add("YEAR", year);
                    process.StartInfo.Environment.Add("LISTNAME", listName);
                    process.StartInfo.Environment.Add("COMPANYNAME", companyName);
                    process.StartInfo.Environment.Add("COMPANYCODE", companyCode);
                    process.StartInfo.Environment.Add("LISTID", listId);
                    process.StartInfo.Environment.Add("TASKID", taskId);
                    process.StartInfo.Environment.Add("PARENTID", parentId);
                    process.StartInfo.Environment.Add("PARENTNAME", parentName);
                    process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = false;
                    process.StartInfo.RedirectStandardOutput = false;
                    process.StartInfo.Verb = "runas";   
                    process.Start();
                    process.WaitForExit();
                }
                Console.WriteLine("\nClose Teamwork task " + task.id+ "\n");
                client.MarkTaskComplete(task.id);
                }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
            }
            //Console.ReadKey();
        }       
    }
}


