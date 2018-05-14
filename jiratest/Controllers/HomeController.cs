using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using jiratest.Models;
using Microsoft.AspNetCore.Http;
using System.IO;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;

namespace jiratest.Controllers
{
    //Refer Document
    //1.await&async https://stackoverflow.com/questions/21779206/how-to-use-restsharp-with-async-await
    //2.convert steam to bytes[] https://stackoverflow.com/questions/36432028/how-to-convert-a-file-into-byte-array-directly-without-its-pathwithout-saving-f
    //3.https://www.codeproject.com/Articles/1203408/Upload-Download-Files-in-ASP-NET-Core
    //4.Add attchment https://gist.github.com/gandarez/c2c5b2b27dbaf62a0d634253529bcb59

    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            ConfirmFile(file);
            string issuekey = "";
            var path = Path.Combine(
                        Directory.GetCurrentDirectory(), "wwwroot",
                        Path.GetTempFileName());

            var client = new RestClient("http://localhost:8080/rest/api/2");
            var request = new RestRequest("issue/", Method.POST);


            var res = CreateJiraIssue(client, request);


            if (res.StatusCode == HttpStatusCode.Created)
            {
                Console.WriteLine("Issue: {0} successfully created", res.Data.key);

                #region Attachment
                issuekey = res.Data.key;
                request = new RestRequest(string.Format("issue/{0}/attachments", issuekey), Method.POST);

                request.AddHeader("X-Atlassian-Token", "nocheck");

                using (var ms = new MemoryStream())
                {
                    file.CopyTo(ms);
                    var fileBytes = ms.ToArray();

                    //var file = File.ReadAllBytes(@"/Users/abehsu/Downloads/test.docx");

                    //request.AddHeader("Content-Type", "multipart/form-data");
                    request.AddFileBytes("file", fileBytes, "test.docx", "application/octet-stream");

                    //var res2 = client.Execute(request);


                    Task<IRestResponse> res2 = client.ExecuteTaskAsync(request);
                    res2.Wait();
                    var restResponse = await res2;

                    Console.WriteLine(restResponse.StatusCode == HttpStatusCode.OK ? "Attachment added!" : restResponse.Content);
                    #endregion
                }
            }
            else
                return Content("Issue create failed!!!!");

            return Content("Issue"+issuekey+"create succeed and add attachment!!!!");
        }

        private IFormFile ConfirmFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new Exception();
                //return Content("file not selected");
            return file;
        }

        private IRestResponse<Issue> CreateJiraIssue(RestClient client,RestRequest request)
        {
            client.Authenticator = new HttpBasicAuthenticator("Jiraaccount", "JiraPassword");

            var issue = new Issue
            {
                fields =
                    new Fields
                    {
                        description = "Issue Description",
                        summary = "Issue Summary",
                        project = new Project { key = "ITEADSWB" },
                        issuetype = new IssueType { name = "Bug" }
                    }
            };

            request.AddJsonBody(issue);

            var res = client.Execute<Issue>(request);

            return res;
        }
    }
}
