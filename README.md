### Use Jira container to create jira env

```docker
docker run --detach --publish 8080:8080 -v localpath:/var jiratest
```