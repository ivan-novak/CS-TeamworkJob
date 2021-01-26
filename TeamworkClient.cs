using FullSerializer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;

namespace TeamWorkSharp
{
    public class TeamworkClient
    {
        #region Events
        public delegate void OnProjectsReceivedDelegate(List<Project> projects);
        public event OnProjectsReceivedDelegate onProjectsReceived;

        public delegate void OnTasksListReceivedDelegate(List<TaskList> taskLists);
        public event OnTasksListReceivedDelegate onTasksListReceived;

        public delegate void OnTaskListReceivedDelegate(TaskList taskList);
        public event OnTaskListReceivedDelegate onTaskListReceived;

        public delegate void OnPeopleReceivedDelegate(List<Person> people);
        public event OnPeopleReceivedDelegate onPeopleReceived;

        public delegate void OnCurrentUserDetailsReceivedDelegate(Person me);
        public event OnCurrentUserDetailsReceivedDelegate onCurrentUserDetailsReceived;

        public delegate void OnTasksReceivedDelegate(List<Task> tasks);
        public event OnTasksReceivedDelegate onTasksReceived;

        public delegate void OnFileUploadedDelegate(PendingFile f);
        public event OnFileUploadedDelegate onFileUploaded;

        public delegate void OnTaskPostDelegate(string taskID);
        public event OnTaskPostDelegate onTaskPost;

        public delegate void OnCommentPostDelegate();
        public event OnCommentPostDelegate onCommentPost;

        public delegate void OnErrorDelegate(string descr);
        public event OnErrorDelegate onError;

        public delegate void OnTaskListCreated(string id);
        public event OnTaskListCreated onTaskListCreated;
        #endregion

        #region protected variables
        static int requestCounter;

        private fsSerializer m_serializer = new fsSerializer();
        #endregion

        #region API
        public TeamworkClient(string domainName, string token)
        {
            DomainName = domainName;
            Token = token;
        }

        public void RequestProjects()
        {
            try
            {
                Interlocked.Increment(ref requestCounter);
                HttpWebRequest req = CreateWebRequest(GetSubDomain("projects.json"));
                isError = false;
                req.BeginGetResponse(new AsyncCallback(OnProjectsReceived), req);
            }
            catch(Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;
            }
        }

        public void RequestTasksList(Project proj)
        {
            try
            {
                Interlocked.Increment(ref requestCounter);
                HttpWebRequest req = CreateWebRequest(GetSubDomain("projects/" + proj.id + "/tasklists.json"));
                isError = false;
                req.BeginGetResponse(new AsyncCallback(OnTaskListsReceived), req);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;
            }
        }

        public void RequestTaskList(string id)
        {
            try
            {
                Interlocked.Increment(ref requestCounter);
                HttpWebRequest req = CreateWebRequest(GetSubDomain("tasklists/" + id + ".json"));
                isError = false;
                req.BeginGetResponse(new AsyncCallback(OnTaskListReceived), req);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;
            }
        }

        public void RequestCurrentUserDetails()
        {
            try
            {
                Interlocked.Increment(ref requestCounter);
                HttpWebRequest req = CreateWebRequest(GetSubDomain("me.json"));
                isError = false;
                req.BeginGetResponse(new AsyncCallback(OnCurrentUserDetailsReceived), req);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;
            }
        }

        public void RequestPeople(string projId)
        {
            try
            {
                Interlocked.Increment(ref requestCounter);
                HttpWebRequest req = CreateWebRequest(GetSubDomain("projects/" + projId + "/people.json"));
                isError = false;
                req.BeginGetResponse(new AsyncCallback(OnPeopleReceived), req);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;
            }
        }

        public void RequestTasks(string projId)
        {
            try
            {
                Interlocked.Increment(ref requestCounter);
                HttpWebRequest req = CreateWebRequest(GetSubDomain("projects/" + projId + "/tasks.json"));
                isError = false;
                req.BeginGetResponse(new AsyncCallback(OnTasksReceived), req);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;
            }
        }

        public void PostTaskComment(string taskID, string comment)
        {
            try
            {
                Interlocked.Increment(ref requestCounter);
                HttpWebRequest req = CreatePostWebRequest(GetSubDomain("tasks/" + taskID.ToString() + "/comments.json"));
                isError = false;
                CreateComment pc = new CreateComment();
                pc.comment = new Comment();
                pc.comment.body = comment;
                SerializeIntoRequest<CreateComment>(req, pc);
                req.BeginGetResponse(new AsyncCallback(OnCommentPosted), req);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;
            }
        }
        
        public void MarkTaskComplete (long taskID)
        {
            var command = "/tasks/" + taskID.ToString() + "/complete.json";
            //Interlocked.Increment(ref requestCounter);
            HttpWebRequest req = CreatePutWebRequest(GetSubDomain(command));
            req.ContentLength = 0;
            req.GetResponse();
        }

        public void MarkTaskUncomplete(long taskID)
        {
            var command = "tasks/" + taskID.ToString() + "/uncomplete.json";
            //Interlocked.Increment(ref requestCounter);
            HttpWebRequest req = CreatePutWebRequest(GetSubDomain(command));
            req.ContentLength = 0;
            req.GetResponse();
        }

        public void CreateTaskList(string projId, string name, string description)
        {
            try
            {
                Interlocked.Increment(ref requestCounter);
                HttpWebRequest req = CreatePostWebRequest(GetSubDomain("projects/" + projId + "/tasklists.json"));
                isError = false;
                CreateTaskList t = new CreateTaskList();
                t.taskList.name = name;
                t.taskList.description = description;
                SerializeIntoRequest<CreateTaskList>(req, t);
                req.BeginGetResponse(new AsyncCallback(OnTaskListCreationSucceeded), req);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;
            }
        }

        public void PostTask(string listId, string name, string description, TeamWorkSharp.Task.Priority priority, PendingFile[] fileAttachments, Person[] assignTo, bool notify, Person[] watchers)
        {
            if (listId == null) return;
            try
            {
                Interlocked.Increment(ref requestCounter);
                HttpWebRequest req = CreatePostWebRequest(GetSubDomain("tasklists/" + listId + "/tasks.json"));
                isError = false;
                CreateTask ptr = new CreateTask();
                CreateTask.Body tsk = ptr.task;
                tsk.content = name;
                tsk.description = description;
                tsk.priority = priority.Value;
                tsk.startDate = DateTime.Now.ToString("yyyyMMdd");
                tsk.notify = notify;
                if (fileAttachments != null)
                {
                    foreach (var f in fileAttachments)
                    {
                        if (tsk.pendingFileAttachments.Length > 0) tsk.pendingFileAttachments += ",";
                        tsk.pendingFileAttachments += f.refString;
                    }
                }
                // add assignees
                if (assignTo != null)
                {
                    foreach (var p in assignTo)
                    {
                        if (tsk.responsiblePartyId.Length > 0) tsk.responsiblePartyId += ",";
                        tsk.responsiblePartyId += p.id;
                    }
                }
                // add followers
                if(watchers != null)
                {
                    foreach(var v in watchers)
                    {
                        if (tsk.commentFollowerIds.Length > 0) tsk.commentFollowerIds += ",";
                        if (tsk.changeFollowerIds.Length > 0) tsk.changeFollowerIds += ",";
                        tsk.commentFollowerIds += v.id;
                        tsk.changeFollowerIds += v.id;
                    }
                }
                SerializeIntoRequest<CreateTask>(req, ptr);
                req.BeginGetResponse(new AsyncCallback(OnTaskPosted), req);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;
            }
        }

        public void UploadFile(string file)
        {
            try
            {
                Interlocked.Increment(ref requestCounter);

                string contentType = GetContentType(file);

                string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
                byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

                string sdom = GetSubDomain("pendingfiles.json");

                isError = false;

                //Creation and specification of the request
                HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(sdom); //sVal is id for the webService
                wr.Method = "POST";
                wr.KeepAlive = true;
                wr.Accept = "*/*";

                var credentialBuffer = new UTF8Encoding().GetBytes(Token + ":xxxx");
                wr.Headers["Authorization"] = "Basic " + Convert.ToBase64String(credentialBuffer);

                wr.ContentType = "multipart/form-data; boundary=" + boundary;

                Stream rs = wr.GetRequestStream();

                string fileName = Path.GetFileName(file);

                //Writting of the file
                byte[] starter = System.Text.Encoding.ASCII.GetBytes("--" + boundary + "--\r\n");
                rs.Write(starter, 0, starter.Length);

                string headerTemplate = "Content-Disposition: form-data; name=\"file\"; filename=\"{0}\"\r\nContent-Type: {1}\r\n\r\n";
                string header = string.Format(headerTemplate, fileName, contentType);
                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                rs.Write(headerbytes, 0, headerbytes.Length);

                FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                byte[] buffer = new byte[4096];
                int bytesRead = 0;
                while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    rs.Write(buffer, 0, bytesRead);
                }
                fileStream.Close();

                byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                rs.Write(trailer, 0, trailer.Length);
                rs.Close();

                wr.BeginGetResponse(new AsyncCallback(OnFileUploaded), wr);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;
            }
        }

        public TaskList FindTaskList(string name, IEnumerable<TaskList> lists)
        {
            if (lists == null)
                return null;

            foreach(var t in lists)
            {
                if (t.name == name)
                    return t;
            }

            return null;
        }

        public Project FindProject(string name, IEnumerable<Project> projs)
        {
            if (projs == null)
                return null;

            foreach (var p in projs)
            {
                if (p.name == name)
                    return p;
            }

            return null;
        }

        public string DomainName
        {
            get;
            private set;
        }

        public string Token
        {
            get;
            private set;
        }

        public bool requestPending
        {
            get { return requestCounter > 0; }
            //private set;
        }

        public bool isError
        {
            get;
            private set;
        }

        public string errorDesc
        {
            get;
            private set;
        }

        public string FullDomain
        {
            get
            {
                return "https://" + DomainName + ".teamwork.com";
            }
        }
        #endregion

        #region responses
        private void OnProjectsReceived(IAsyncResult result)
        {
            try
            {
                ProjectsResponse deserialized = Deserialize<ProjectsResponse>(result);

                if (onProjectsReceived != null)
                    onProjectsReceived(deserialized.projects);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;

                if (onError != null)
                    onError(ex.Message);
            }
            finally
            {
                Interlocked.Decrement(ref requestCounter);
            }
        }

        private void OnTaskListsReceived(IAsyncResult result)
        {
            try
            {
                TasksListResponse deserialized = Deserialize<TasksListResponse>(result);

                if (onTasksListReceived != null)
                    onTasksListReceived(deserialized.tasklists);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;

                if (onError != null)
                    onError(ex.Message);
            }
            finally
            {
                Interlocked.Decrement(ref requestCounter);
            }
        }

        private void OnTaskListReceived(IAsyncResult result)
        {
            try
            {
                TaskListResponse deserialized = Deserialize<TaskListResponse>(result);

                if (onTaskListReceived != null)
                    onTaskListReceived(deserialized.taskList);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;

                if (onError != null)
                    onError(ex.Message);
            }
            finally
            {
                Interlocked.Decrement(ref requestCounter);
            }
        }

        private void OnCurrentUserDetailsReceived(IAsyncResult result)
        {
            try
            {
                CurrentUserResponse deserialized = Deserialize<CurrentUserResponse>(result);

                if (onCurrentUserDetailsReceived != null)
                    onCurrentUserDetailsReceived(deserialized.person);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;

                if (onError != null)
                    onError(ex.Message);
            }
            finally
            {
                Interlocked.Decrement(ref requestCounter);
            }
        }

        private void OnPeopleReceived(IAsyncResult result)
        {
            try
            {
                PeopleResponse deserialized = Deserialize<PeopleResponse>(result);

                if (onPeopleReceived != null)
                    onPeopleReceived(deserialized.people);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;

                if (onError != null)
                    onError(ex.Message);
            }
            finally
            {
                Interlocked.Decrement(ref requestCounter);
            }
        }

        private void OnTasksReceived(IAsyncResult result)
        {
            try
            {
                TasksResponse deserialized = Deserialize<TasksResponse>(result);

                if (onTasksReceived != null)
                    onTasksReceived(deserialized.tasks);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;

                if (onError != null)
                    onError(ex.Message);
            }
            finally
            {
                Interlocked.Decrement(ref requestCounter);
            }
        }

        private void OnFileUploaded(IAsyncResult result)
        {
            try
            {
                PendingFileResponse deserialized = Deserialize<PendingFileResponse>(result);

                if (onFileUploaded != null)
                    onFileUploaded(deserialized.pendingFile);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;

                if (onError != null)
                    onError(ex.Message);
            }
            finally
            {
                Interlocked.Decrement(ref requestCounter);
            }
        }

        private void OnTaskPosted(IAsyncResult result)
        {
            try
            {
                HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
                response.Close();

                string taskID = response.Headers["id"];

                if (onTaskPost != null)
                    onTaskPost(taskID);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;

                if (onError != null)
                    onError(ex.Message);
            }
            finally
            {
                Interlocked.Decrement(ref requestCounter);
            }
        }

        private void OnTaskListCreationSucceeded(IAsyncResult result)
        {
            try
            {
                HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
                response.Close();

                if (onTaskListCreated != null)
                    onTaskListCreated(response.Headers["id"]);
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;

                if (onError != null)
                    onError(ex.Message);
            }
            finally
            {
                Interlocked.Decrement(ref requestCounter);
            }
        }

        private void OnCommentPosted(IAsyncResult result)
        {
            try
            {
                HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
                response.Close();

                if (onCommentPost != null)
                    onCommentPost();
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;

                if (onError != null)
                    onError(ex.Message);
            }
            finally
            {
                Interlocked.Decrement(ref requestCounter);
            }
        }
        #endregion

        #region protected methods
        protected string GetSubDomain(string subDomain)
        {
            return FullDomain + "/" + subDomain;
        }

        protected HttpWebRequest CreateWebRequest(string requestUriString)
        {
            HttpWebRequest req = HttpWebRequest.Create(requestUriString) as HttpWebRequest;
           var credentialBuffer = new UTF8Encoding().GetBytes(Token + ":xxxx");
            //var credentialBuffer = new UTF8Encoding().GetBytes(Token);
            req.Headers["Authorization"] = "Basic " + Convert.ToBase64String(credentialBuffer);
            req.ContentType = "application/json";
            return req;
        }

        protected HttpWebRequest CreatePostWebRequest(string requestUriString)
        {
            HttpWebRequest ret = CreateWebRequest(requestUriString);
            ret.Method = "POST";
            return ret;
        }

        protected HttpWebRequest CreatePutWebRequest(string requestUriString)
        {
            HttpWebRequest ret = CreateWebRequest(requestUriString);
            ret.Method = "PUT";
            return ret;
        }

        class PostFile
        {
            public string file { get; set; }
        }

        protected string GetContentType(string fileName)
        {
            if (fileName.EndsWith(".txt") || fileName.EndsWith(".log"))
                return "text/plain";
            else if (fileName.EndsWith(".jpg"))
                return "image/jpeg";
            else if (fileName.EndsWith(".png"))
                return "image/png";
            else if (fileName.EndsWith(".zip"))
                return "application/zip";

            return "application/octet-stream";
        }

        private void SerializeIntoRequest<T>(HttpWebRequest req, T obj)
        {
            fsData data;
            m_serializer.TrySerialize<T>(obj, out data).AssertSuccessWithoutWarnings();
            string postData = fsJsonPrinter.CompressedJson(data);
            byte[] byteArray = Encoding.UTF8.GetBytes(postData);
            req.ContentLength = byteArray.Length;
            using (var dataStream = req.GetRequestStream())
            {
                dataStream.Write(byteArray, 0, byteArray.Length);
                dataStream.Close();
            }
        }

        private T Deserialize<T>(HttpWebResponse response)
        {
            System.IO.StreamReader reader = new StreamReader(response.GetResponseStream());
            string str = reader.ReadToEnd();
            fsData data = fsJsonParser.Parse(str);
            T deserialized = default(T);
            m_serializer.TryDeserialize(data, ref deserialized).AssertSuccessWithoutWarnings();
            return deserialized;
        }

        private T Deserialize<T>(IAsyncResult result)
        {
            HttpWebResponse response = (result.AsyncState as HttpWebRequest).EndGetResponse(result) as HttpWebResponse;
            return Deserialize<T>(response);
        }
        #endregion

        public List<Project> GetAllProjects(string status = null, string updatedAfterDate = null, string orderby = null, string createdAfterDate = null, string createdAfterTime = null,
            int? catId = null, bool? includePeople = null, bool? includeProjectOwner = null, int? page = null, int? pageSize = null, string orderMode = null,
            bool? onlyStarredProjects = null, string companyId = null, string projectOwnerIds = null, string searchTerm = null, bool? getDeleted = null, bool? includeTags = null,
            string userId = null, string updatedAfterDateTime = null)
        {
            var command = "projects.json";
            var parameters = "";
            if (status != null) parameters = parameters + "&status=" + status;
            if (updatedAfterDate != null) parameters = parameters + "&updatedAfterDate=" + updatedAfterDate;
            if (orderby != null) parameters = parameters + "&orderby=" + orderby;
            if (createdAfterDate != null) parameters = parameters + "&createdAfterDate=" + createdAfterDate;
            if (createdAfterTime != null) parameters = parameters + "&createdAfterTime=" + createdAfterTime;
            if (catId != null) parameters = parameters + "&catId=" + catId.ToString();
            if (includePeople != null) parameters = parameters + "&includePeople=" + includePeople.ToString();
            if (includeProjectOwner != null) parameters = parameters + "&includeProjectOwner=" + includeProjectOwner.ToString();
            if (page != null) parameters = parameters + "&page=" + page.ToString();
            if (pageSize != null) parameters = parameters + "&pageSize=" + pageSize.ToString();
            if (orderMode != null) parameters = parameters + "&orderMode=" + orderMode;
            if (onlyStarredProjects != null) parameters = parameters + "&onlyStarredProjects=" + onlyStarredProjects.ToString();
            if (companyId != null) parameters = parameters + "&companyId=" + companyId;
            if (projectOwnerIds != null) parameters = parameters + "&projectOwnerIds=" + projectOwnerIds;
            if (searchTerm != null) parameters = parameters + "&searchTerm=" + searchTerm;
            if (getDeleted != null) parameters = parameters + "&getDeleted=" + getDeleted.ToString();
            if (includeTags != null) parameters = parameters + "&includeTags=" + includeTags.ToString();
            if (userId != null) parameters = parameters + "&userId=" + userId;
            if (updatedAfterDateTime != null) parameters = parameters + "&updatedAfterDateTime=" + updatedAfterDateTime;
            if (parameters != "") command = command + "?" + parameters;
            var deserialized = Deserialize<ProjectsResponse>((HttpWebResponse)CreateWebRequest(GetSubDomain(command)).GetResponse());              
            return deserialized.projects;          
        }

        public Project GetSingleProject(string projId, string projectOwnerIds = null, string projectHealth = null, bool? includeProjectOwner = null)
        {
            if (projId == null) return null;
            var command = "projects/" + projId.ToString() + ".json";        
            var parameters = "";
            if (projectOwnerIds != null) parameters = parameters + "&projectOwnerIds=" + projectOwnerIds;
            if (projectHealth != null) parameters = parameters + "&projectHealth=" + projectHealth;
            if (includeProjectOwner != null) parameters = parameters + "&includeProjectOwner=" + includeProjectOwner.ToString();
            if (parameters != "") command = command + "?" + parameters;
            return Deserialize<ProjectResponse>((HttpWebResponse)CreateWebRequest(GetSubDomain(command)).GetResponse()).project;
        }

        public List<TaskList> GetAllTaskLists(string status = null, int? page = null, int? pageSize = null, bool? showDeleted = null, bool? includeArchivedProjects = null)
        {
            var command = "/tasklists.json";          
            var parameters = "";
            if (status != null) parameters = parameters + "&status=" + status;
            if (page != null) parameters = parameters + "&page=" + page.ToString();
            if (pageSize != null) parameters = parameters + "&pageSize=" + pageSize.ToString();
            if (showDeleted != null) parameters = parameters + "&showDeleted=" + showDeleted.ToString();
            if (includeArchivedProjects != null) parameters = parameters + "&includeArchivedProjects=" + includeArchivedProjects.ToString();
            if (parameters != "") command = command + "?" + parameters;
            var deserialized = Deserialize<TasksListResponse>((HttpWebResponse)CreateWebRequest(GetSubDomain(command)).GetResponse());
            return deserialized.tasklists;

        }

        public List<TaskList> GetTaskListsForProject(string projId, string responsiblePartyId = null, bool? getOverdueCount = null, string status = null,
            bool? showMilestones = null, bool? getCompletedCount = null, string filter = null)
        {
            if (projId == null) return null;
            var command = "projects/" + projId.ToString() + "/tasklists.json";  
            var parameters = "";
            if (responsiblePartyId != null) parameters = parameters + "&responsiblePartyId=" + responsiblePartyId;
            if (getOverdueCount != null) parameters = parameters + "&getOverdueCount=" + getOverdueCount.ToString();
            if (showMilestones != null) parameters = parameters + "&showMilestones=" + showMilestones.ToString();
            if (getCompletedCount != null) parameters = parameters + "&getCompletedCount=" + getCompletedCount.ToString();
            if (filter != null) parameters = parameters + "&filter=" + filter;
            if (parameters != "") command = command + "?" + parameters;
            var deserialized = Deserialize<TasksListResponse>((HttpWebResponse)CreateWebRequest(GetSubDomain(command)).GetResponse());
            return deserialized.tasklists;         
        }

        public TaskList GetSingleTaskList(string listId, string status = null, int? page = null, int? pageSize = null, bool? showDeleted = null, bool? includeArchivedProjects = null)
        {
            if (listId == null) return null;
            var command = "tasklists/" + listId.ToString() + ".json";
            var parameters = "";
            if (status != null) parameters = parameters + "&status=" + status;
            if (page != null) parameters = parameters + "&page=" + page.ToString();
            if (pageSize != null) parameters = parameters + "&pageSize=" + pageSize.ToString();
            if (showDeleted != null) parameters = parameters + "&showDeleted=" + showDeleted.ToString();
            if (includeArchivedProjects != null) parameters = parameters + "&includeArchivedProjects=" + includeArchivedProjects.ToString();
            if (parameters != "") command = command + "?" + parameters;
            var deserialized = Deserialize<TaskListResponse>((HttpWebResponse)CreateWebRequest(GetSubDomain(command)).GetResponse());
            return deserialized.taskList;
        }

        public List<Task> GetAllTasks(string filter = null, int? page = null, int? pageSize = null, string startDate = null, string endDate = null, 
            string updatedAfterDate = null, string completedAfterDate = null, string completedBeforeDate = null, string showDeleted = null,
            bool? includeCompletedTasks = null, bool? includeCompletedSubtasks = null, string creatorIds = null, string include = null, 
            string responsiblePartyIds = null, string sort = null, string getSubTasks = null, string nestSubTasks = null, bool? getFiles = null, 
            bool? includeToday = null, bool? ignoreStartDates = null, string tagIds = null, bool? includeTasksWithoutDueDates = null, 
            bool? includeTasksFromDeletedLists = null, bool? includeArchivedProjects = null, string dateupdatedASC = null)
        {
            var command = "/tasks.json";
            try
            {
                isError = false;
                var parameters = "";
                if (filter != null) parameters = parameters + "&filter=" + filter;
                if (page != null) parameters = parameters + "&page=" + page.ToString();
                if (pageSize != null) parameters = parameters + "&pageSize=" + pageSize.ToString();
                if (startDate != null) parameters = parameters + "&startDate=" + startDate;
                if (endDate != null) parameters = parameters + "&endDate=" + endDate;
                if (updatedAfterDate != null) parameters = parameters + "&updatedAfterDate=" + updatedAfterDate;
                if (completedAfterDate != null) parameters = parameters + "&completedAfterDate=" + completedAfterDate;
                if (completedBeforeDate != null) parameters = parameters + "&completedBeforeDate=" + completedBeforeDate;
                if (showDeleted != null) parameters = parameters + "&showDeleted=" + showDeleted;
                if (includeCompletedTasks != null) parameters = parameters + "&includeCompletedTasks=" + includeCompletedTasks.ToString();
                if (includeCompletedSubtasks != null) parameters = parameters + "&includeCompletedSubtasks=" + includeCompletedSubtasks.ToString();
                if (creatorIds != null) parameters = parameters + "&creator-ids=" + creatorIds;
                if (include != null) parameters = parameters + "&include=" + include;
                if (responsiblePartyIds != null) parameters = parameters + "&responsible-party-ids=" + responsiblePartyIds;
                if (sort != null) parameters = parameters + "&responsiblePartyIds=" + sort;
                if (getSubTasks != null) parameters = parameters + "&getSubTasks=" + getSubTasks;
                if (nestSubTasks != null) parameters = parameters + "&nestSubTasks=" + nestSubTasks;
                if (getFiles != null) parameters = parameters + "&getFiles=" + getFiles.ToString();
                if (includeToday != null) parameters = parameters + "&includeToday=" + includeToday.ToString();
                if (ignoreStartDates != null) parameters = parameters + "&ignore-start-dates=" + ignoreStartDates.ToString();
                if (tagIds != null) parameters = parameters + "&tag-ids=" + tagIds;
                if (includeTasksWithoutDueDates != null) parameters = parameters + "&includeTasksWithoutDueDates=" + includeTasksWithoutDueDates.ToString();
                if (includeTasksFromDeletedLists != null) parameters = parameters + "&includeTasksFromDeletedLists=" + includeTasksFromDeletedLists.ToString();
                if (includeArchivedProjects != null) parameters = parameters + "&includeArchivedProjects=" + includeArchivedProjects.ToString();
                if (dateupdatedASC != null) parameters = parameters + "&dateupdatedASC=" + dateupdatedASC.ToString();
                if (parameters != "") command = command + "?" + parameters;
                var deserialized = Deserialize<TasksResponse>((HttpWebResponse)CreateWebRequest(GetSubDomain(command)).GetResponse());
                return deserialized.tasks;
            }
            catch (Exception ex)
            {
                isError = true;
                errorDesc = ex.Message;
                if (onError != null) onError(ex.Message);
                return null;
            }
        }
    
        public List<Task> GetTasksForProject(string projId, string filter = null, int? page = null, int? pageSize = null, string startDate = null, string endDate = null, 
            string updatedAfterDate = null, string completedAfterDate = null, string completedBeforeDate = null, string showDeleted = null,
            bool? includeCompletedTasks = null, bool? includeCompletedSubtasks = null, string creatorIds = null, string include = null,
            string responsiblePartyIds = null, string sort = null, string getSubTasks = null, string nestSubTasks = null, bool? getFiles = null,
            bool? includeToday = null, bool? ignoreStartDates = null, string tagIds = null, bool? includeTasksWithoutDueDates = null, 
            bool? includeTasksFromDeletedLists = null, bool? includeArchivedProjects = null, string dateupdatedASC = null)
        {
            if (projId == null) return null;
            var command = "/projects/"+ projId.ToString()+"+/tasks.json";
            var parameters = "";
            if (filter != null) parameters = parameters + "&filter=" + filter;
            if (page != null) parameters = parameters + "&page=" + page.ToString();
            if (pageSize != null) parameters = parameters + "&pageSize=" + pageSize.ToString();
            if (startDate != null) parameters = parameters + "&startDate=" + startDate;
            if (endDate != null) parameters = parameters + "&endDate=" + endDate;
            if (updatedAfterDate != null) parameters = parameters + "&updatedAfterDate=" + updatedAfterDate;
            if (completedAfterDate != null) parameters = parameters + "&completedAfterDate=" + completedAfterDate;
            if (completedBeforeDate != null) parameters = parameters + "&completedBeforeDate=" + completedBeforeDate;
            if (showDeleted != null) parameters = parameters + "&showDeleted=" + showDeleted;
            if (includeCompletedTasks != null) parameters = parameters + "&includeCompletedTasks=" + includeCompletedTasks.ToString();
            if (includeCompletedSubtasks != null) parameters = parameters + "&includeCompletedSubtasks=" + includeCompletedSubtasks.ToString();
            if (creatorIds != null) parameters = parameters + "&creator-ids=" + creatorIds;
            if (include != null) parameters = parameters + "&include=" + include;
            if (responsiblePartyIds != null) parameters = parameters + "&responsible-party-ids=" + responsiblePartyIds;
            if (sort != null) parameters = parameters + "&responsiblePartyIds=" + sort;
            if (getSubTasks != null) parameters = parameters + "&getSubTasks=" + getSubTasks;
            if (nestSubTasks != null) parameters = parameters + "&nestSubTasks=" + nestSubTasks;
            if (getFiles != null) parameters = parameters + "&getFiles=" + getFiles.ToString();
            if (includeToday != null) parameters = parameters + "&includeToday=" + includeToday.ToString();
            if (ignoreStartDates != null) parameters = parameters + "&ignore-start-dates=" + ignoreStartDates.ToString();
            if (tagIds != null) parameters = parameters + "&tag-ids=" + tagIds;
            if (includeTasksWithoutDueDates != null) parameters = parameters + "&includeTasksWithoutDueDates=" + includeTasksWithoutDueDates.ToString();
            if (includeTasksFromDeletedLists != null) parameters = parameters + "&includeTasksFromDeletedLists=" + includeTasksFromDeletedLists.ToString();
            if (includeArchivedProjects != null) parameters = parameters + "&includeArchivedProjects=" + includeArchivedProjects.ToString();
            if (dateupdatedASC != null) parameters = parameters + "&dateupdatedASC=" + dateupdatedASC.ToString();
            if (parameters != "") command = command + "?" + parameters;
            var deserialized = Deserialize<TasksResponse>((HttpWebResponse)CreateWebRequest(GetSubDomain(command)).GetResponse());
            return deserialized.tasks;
        }

        public List<Task> GetTasksForList(string listId, string filter = null, int? page = null, int? pageSize = null, string startDate = null, string endDate = null,
            string updatedAfterDate = null, string completedAfterDate = null, string completedBeforeDate = null, string showDeleted = null,
            bool? includeCompletedTasks = null, bool? includeCompletedSubtasks = null, string creatorIds = null, string include = null, string responsiblePartyIds = null,
            string sort = null, string getSubTasks = null, string nestSubTasks = null, bool? getFiles = null,
            bool? includeToday = null, bool? ignoreStartDates = null, string tagIds = null, bool? includeTasksWithoutDueDates = null, bool? includeTasksFromDeletedLists = null,
            bool? includeArchivedProjects = null, string dateupdatedASC = null)
        {
            if (listId == null) return null;
            var command = "/tasklists/" + listId.ToString() + "+/tasks.json";
            var parameters = "";
            if (filter != null) parameters = parameters + "&filter=" + filter;
            if (page != null) parameters = parameters + "&page=" + page.ToString();
            if (pageSize != null) parameters = parameters + "&pageSize=" + pageSize.ToString();
            if (startDate != null) parameters = parameters + "&startDate=" + startDate;
            if (endDate != null) parameters = parameters + "&endDate=" + endDate;
            if (updatedAfterDate != null) parameters = parameters + "&updatedAfterDate=" + updatedAfterDate;
            if (completedAfterDate != null) parameters = parameters + "&completedAfterDate=" + completedAfterDate;
            if (completedBeforeDate != null) parameters = parameters + "&completedBeforeDate=" + completedBeforeDate;
            if (showDeleted != null) parameters = parameters + "&showDeleted=" + showDeleted;
            if (includeCompletedTasks != null) parameters = parameters + "&includeCompletedTasks=" + includeCompletedTasks.ToString();
            if (includeCompletedSubtasks != null) parameters = parameters + "&includeCompletedSubtasks=" + includeCompletedSubtasks.ToString();
            if (creatorIds != null) parameters = parameters + "&creator-ids=" + creatorIds;
            if (include != null) parameters = parameters + "&include=" + include;
            if (responsiblePartyIds != null) parameters = parameters + "&responsible-party-ids=" + responsiblePartyIds;
            if (sort != null) parameters = parameters + "&responsiblePartyIds=" + sort;
            if (getSubTasks != null) parameters = parameters + "&getSubTasks=" + getSubTasks;
            if (nestSubTasks != null) parameters = parameters + "&nestSubTasks=" + nestSubTasks;
            if (getFiles != null) parameters = parameters + "&getFiles=" + getFiles.ToString();
            if (includeToday != null) parameters = parameters + "&includeToday=" + includeToday.ToString();
            if (ignoreStartDates != null) parameters = parameters + "&ignore-start-dates=" + ignoreStartDates.ToString();
            if (tagIds != null) parameters = parameters + "&tag-ids=" + tagIds;
            if (includeTasksWithoutDueDates != null) parameters = parameters + "&includeTasksWithoutDueDates=" + includeTasksWithoutDueDates.ToString();
            if (includeTasksFromDeletedLists != null) parameters = parameters + "&includeTasksFromDeletedLists=" + includeTasksFromDeletedLists.ToString();
            if (includeArchivedProjects != null) parameters = parameters + "&includeArchivedProjects=" + includeArchivedProjects.ToString();
            if (dateupdatedASC != null) parameters = parameters + "&dateupdatedASC=" + dateupdatedASC.ToString();
            if (parameters != "") command = command + "?" + parameters;
            var deserialized = Deserialize<TasksResponse>((HttpWebResponse)CreateWebRequest(GetSubDomain(command)).GetResponse());
            return deserialized.tasks;

        }

        public Task GetSingleTask(string taskId)
        {
            if (taskId == null) return null;
            var command = "/tasks/" + taskId.ToString() + ".json";                    
            return Deserialize<TaskResponse>((HttpWebResponse)CreateWebRequest(GetSubDomain(command)).GetResponse()).task;
        }

        public Person GetCurrentUserDetails(bool? getPreferences = null, bool? fullProfile = null, bool? getDefaultFilters = null, bool? sharedFilter = null,
            bool? getAccounts = null, bool? includeAuth = null)
        {
            var command = "me.json";
             var parameters = "";
             if (getPreferences != null) parameters = parameters + "&getPreferences=" + getPreferences.ToString();
             if (fullProfile != null) parameters = parameters + "&fullProfile=" + fullProfile.ToString();
             if (getDefaultFilters != null) parameters = parameters + "&getDefaultFilters=" + getDefaultFilters.ToString();
             if (sharedFilter != null) parameters = parameters + "&sharedFilter=" + sharedFilter.ToString();
             if (getAccounts != null) parameters = parameters + "&getAccounts=" + getAccounts.ToString();
             if (includeAuth != null) parameters = parameters + "&includeAut=" + includeAuth.ToString();
             if (parameters != "") command = command + "?" + parameters;
             var deserialized = Deserialize<CurrentUserResponse>((HttpWebResponse)CreateWebRequest(GetSubDomain(command)).GetResponse());
             return deserialized.person;          
        }

        public List<Company> GetAllCompanys()
        {
            var command = "/companies.json";                    
            return Deserialize<CompaniesResponse>((HttpWebResponse)CreateWebRequest(GetSubDomain(command)).GetResponse()).companies;
        }    

        public List<Company> GetCompanysForProject(string projId)
        {
            if (projId == null) return null;
            var command = "/projects/"+projId+"/companies.json";
            return Deserialize<CompanysForProjectResponse>((HttpWebResponse)CreateWebRequest(GetSubDomain(command)).GetResponse()).companies;          
        }
            
        public Company GetSingleCompany(string companyId)
        {
            if (companyId == null) return null;
            var command = "/companies/"+ companyId + ".json";
            return Deserialize<CompanyResponse>((HttpWebResponse)CreateWebRequest(GetSubDomain(command)).GetResponse()).company;          
        }
}
}
