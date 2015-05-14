#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"

open System
open System.IO
open Suave
open Suave.Web
open Suave.Http
open Suave.Types
open FSharp.Data

type GithubSearch = JsonProvider<"jsons/github-search.json">
type GithubEvents = JsonProvider<"jsons/github-events.json">

let res = 
  Http.RequestString
    ( "https://api.github.com/search/repositories?q=language:fsharp&sort=stars&order=desc", 
      httpMethod="GET", headers=[
        HttpRequestHeaders.Accept "application/vnd.github.v3+json"; 
        HttpRequestHeaders.UserAgent "SuaveDemo"] )

let search = GithubSearch.Parse(res)
for it in search.Items do
  printfn "%s" it.Name

type GitHubEvent = 
  { User : string 
    UserIcon : string
    Ago : string
    Action : string
    Text : string 
    Url : string }
  static member Create(user, userIcon, ago) = 
    { User = user; UserIcon = userIcon; Ago = ago; Action = ""; Text = ""; Url = "" }

let formatComment (comment:string) =
  let comment = comment.Replace("\n", " ").Replace("\r", " ")
  let short = comment.Substring(0, min 100 (comment.Length))
  if short.Length < comment.Length then short + "..." else short

let formatDate (date:DateTime) = 
  let ts = DateTime.Now - date
  if ts.TotalDays > 1.0 then sprintf "%d days ago" (int ts.TotalDays)
  elif ts.TotalHours > 1.0 then sprintf "%d hours ago" (int ts.TotalHours)
  elif ts.TotalMinutes > 1.0 then sprintf "%d minutes ago" (int ts.TotalMinutes)
  else "just now"

let getEvents () = async { 
  let! eventsJson = 
    Http.AsyncRequestString
      ( "https://api.github.com/orgs/fsharp/events", 
        httpMethod="GET", headers=[
          HttpRequestHeaders.Accept "application/vnd.github.v3+json"; 
          HttpRequestHeaders.UserAgent "SuaveDemo"] )

  let events = GithubEvents.Parse(eventsJson)
  let parsed = 
    [ for evt in events do
        let info = GitHubEvent.Create(evt.Actor.Login, evt.Actor.AvatarUrl, formatDate evt.CreatedAt)
        match evt.Payload.Comment, evt.Payload.PullRequest with
        | Some cmt, _ -> 
            yield { info with Action = "commented"; Text = formatComment cmt.Body; Url = cmt.HtmlUrl }
        | _, Some pr -> 
            let action = evt.Payload.Action.Value
            let action = if action = "closed" && pr.Merged then "merged" else action
            yield { info with Action = action + " pull request"; Text = pr.Title; Url = pr.HtmlUrl }
        | _ -> () ]
  return parsed }

let formatEvents events = 
  [ for e in events do
      yield "<li>"
      yield sprintf "<img src=\"%s\" />" e.UserIcon
      yield sprintf "<p>%s <a href=\"http://github.com/%s\">@%s</a>" e.Ago e.User e.User
      yield sprintf "  <a href=\"%s\">%s</a>:</p>" e.Url e.Action
      yield sprintf "<p class=\"body\">%s</p>" e.Text
      yield "</li>" ] |> String.concat ""

let template = File.ReadAllText(Path.Combine(__SOURCE_DIRECTORY__, "web/index.html"))

/// The main handler for Suave server!
let app ctx = async {
  let! ghEvents = getEvents ()
  let ghNews = formatEvents ghEvents
  return! ctx |> Successful.OK(template.Replace("[GITHUB-NEWS]", ghNews)) }
