using System;
namespace jiratest.Models
{
    public class Fields
    {
        public Project project { get; set; }
        public IssueType issuetype { get; set; }
        public string summary { get; set; }
        public string description { get; set; }
    }
}
