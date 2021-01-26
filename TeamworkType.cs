using FullSerializer;
using System;
using System.Collections.Generic;

namespace TeamWorkSharp
{
    public class Category
    {
        public string name { get; set; }
        public string id { get; set; }
    }

    public class Comment
    {
        public string body { get; set; }

        public string notify = "";

        public bool isprivate { get; set; }
        public string pendingFileAttachments { get; set; }

        [fsProperty("content-type")]
        public string content_type = "TEXT";
    }

    public class Company
    {
        public string name { get; set; }

        [fsProperty("is-owner")]
        public int isOwner { get; set; }

        public string id { get; set; }
    }

    public class PendingFile
    {
        [fsProperty("ref")]
        public string refString { get; set; }
    }

    public class Person
    {
        public bool administrator { get; set; }

        [fsProperty("site-owner")]
        public bool siteOwner { get; set; }

        [fsProperty("last-name")]
        public string lastName { get; set; }

        [fsProperty("first-name")]
        public string firstName { get; set; }

        [fsProperty("user-name")]
        public string userName { get; set; }

        public string id { get; set; }

        public string FullName
        {
            get
            {
                return firstName + " " + lastName;
            }
        }
    }

    public class Project
    {
        public bool starred { get; set; }
        public string name { get; set; }

        [fsProperty("show-announcement")]
        public bool showAnnouncement { get; set; }

        public string announcement { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public bool isProjectAdmin { get; set; }

        [fsProperty("created-on")]
        public string createdOn { get; set; }

        [fsProperty("start-page")]
        public string startPage { get; set; }

        public string startDate { get; set; }

        public string logo { get; set; }
        public bool notifyeveryone { get; set; }
        public string id { get; set; }

        [fsProperty("last-changed-on")]
        public string lastChangedOn { get; set; }

        public string endDate { get; set; }
    }

    public class Task
    {
        public class Priority
        {
            private Priority(string value) { Value = value; }
            public string Value { get; set; }

            public static Priority None { get { return new Priority(""); } }
            public static Priority Low { get { return new Priority("low"); } }
            public static Priority Medium { get { return new Priority("medium"); } }
            public static Priority High { get { return new Priority("high"); } }
        }

        [fsProperty("project-id")]
        public Int64 projectId { get; set; }

        [fsProperty("project-name")]
        public string projectName { get; set; }

        public int order { get; set; }

        [fsProperty("comments-count")]
        public int commentsCount { get; set; }

        [fsProperty("created-on")]
        public string createdOn { get; set; }

        public bool canEdit { get; set; }

        [fsProperty("has-predecessors")]
        public int hasPredecessors { get; set; }

        public Int64 id { get; set; }

        public bool completed { get; set; }

        public int position { get; set; }

        [fsProperty("estimated-minutes")]
        public Int64 estimatedMinutes { get; set; }

        public string description { get; set; }

        public int progress { get; set; }

        [fsProperty("harvest-enabled")]
        public bool harvestEnabled { get; set; }

        [fsProperty("responsible-party-lastname")]
        public string responsiblePartyLastName { get; set; }

        public string parentTaskId { get; set; }

        [fsProperty("company-id")]
        public int companyId { get; set; }


        [fsProperty("company-name")]

        public string companyName { get; set; }

        [fsProperty("creator-avatar-url")]
        public string creatorAvatarUrl { get; set; }

        public string creatorId { get; set; }

        [fsProperty("start-date")]
        public string startDate { get; set; }

        [fsProperty("tasklist-private")]
        public bool taskListPrivate { get; set; }

        public string lockdownId { get; set; }

        public bool canComplete { get; set; }

        [fsProperty("responsible-party-id")]
        public string responsiblePartyId { get; set; }

        [fsProperty("creator-lastname")]
        public string creatorLastName { get; set; }

        [fsProperty("has-reminders")]
        public bool hasReminders { get; set; }

        [fsProperty("todo-list-name")]
        public string todoListName { get; set; }

        [fsProperty("todo-list-id")]
        public int todoListId { get; set; }

        [fsProperty("has-unread-comments")]
        public bool hasUnreadComments { get; set; }

        public string status { get; set; }
        public string content { get; set; }

        [fsProperty("creator-firstname")]
        public string creatorFirstName { get; set; }

        public string priority { get; set; }


    }

    public class TaskList
    {
        public string projectid { get; set; }
        public string name { get; set; }
        public string description { get; set; }

        [fsProperty("milestone-id")]
        public string milestoneId { get; set; }

        [fsProperty("uncompleted-count")]
        public int uncompletedCount { get; set; }

        public bool complete { get; set; }

        [fsProperty("private")]
        public bool isPrivate { get; set; }

        [fsProperty("overdue-count")]
        public int overdueCount { get; set; }

        [fsProperty("project-name")]
        public string projectName { get; set; }

        public bool pinned { get; set; }

        public string id { get; set; }

        public int position { get; set; }

        [fsProperty("completed-count")]
        public int completedCount { get; set; }
    }

    class CurrentUserResponse
    {
        public Person person { get; set; }

        [fsProperty("STATUS")]
        public string status { get; set; }
    }

    class PendingFileResponse
    {
        public PendingFile pendingFile { get; set; }
    }

    class PeopleResponse
    {
        public List<Person> people { get; set; }
    }

    class ProjectsResponse
    {
        public string STATUS { get; set; }

        public List<Project> projects { get; set; }
    }
    class ProjectResponse
    {
        public Project project { get; set; }
    }

    public class TaskListResponse
    {
        [fsProperty("todo-list")]
        public TaskList taskList { set; get; }

        public string STATUS { set; get; }
    }

    class TasksListResponse
    {
        public List<TaskList> tasklists = null;

        [fsProperty("STATUS")]
        public string status { get; set; }
    }

    class TasksResponse
    {
        [fsProperty("STATUS")]
        public string status { get; set; }

        [fsProperty("todo-items")]
        public List<Task> tasks = null;
    }

    class TaskResponse
    {
        [fsProperty("STATUS")]
        public string status { get; set; }

        [fsProperty("todo-item")]
        public Task task = null;
    }

    class CompaniesResponse
    {
        public List<Company> companies = null;
    }

    class CompanyResponse
    {
        [fsProperty("STATUS")]
        public string status { get; set; }

        public Company company = null;
    }

    class CompanysForProjectResponse
    {
        public List<Company> companies = null;
        [fsProperty("STATUS")]
        public string status { get; set; }
    }

    public class CreateComment
    {
        public Comment comment { get; set; }
    }

    class CreateTask
    {
        public class Body
        {
            public string content { get; set; }
            public string description { get; set; }
            public string pendingFileAttachments = "";

            public string priority { get; set; }

            [fsProperty("start-date")]
            public string startDate { get; set; }

            [fsProperty("responsible-party-id")]
            public string responsiblePartyId = "-1";  // -1 == anyone

            public string commentFollowerIds = "";
            public string changeFollowerIds = "";

            public bool notify { get; set; }
        }

        [fsProperty("todo-item")]
        public Body task = new Body();
    }

    public class CreateTaskList
    {
        public class Body
        {
            public string name { set; get; }

            [fsProperty("private")]
            public bool isPrivate { set; get; }

            public bool pinned = true;

            [fsProperty("milestone-id")]
            public string milestoneId { set; get; }

            public string description { set; get; }

            [fsProperty("todo-list-template-id")]
            public string todoListTemplateId { set; get; }
        }

        [fsProperty("todo-list")]
        public Body taskList = new Body();
    }
}
