let asm = System.Reflection.Assembly.LoadFile(System.IO.Path.Combine(__SOURCE_DIRECTORY__, "packages/Microsoft.AspNet.Razor/lib/net45/System.Web.Razor.dll"))
asm.GetTypes() |> ignore

#r "packages/Microsoft.AspNet.Razor/lib/net45/System.Web.Razor.dll"
if (typeof<System.Web.Razor.ParserResults>.Assembly.GetName().Version.Major <= 2) then 
  failwith "Wrong System.Web.Razor Version loaded!" 
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/Suave.Razor/lib/net40/Suave.Razor.dll"
#r "packages/RazorEngine/lib/net40/RazorEngine.dll"

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

/// The main handler for Suave server!
let app ctx = async {
  match ctx.request.url.LocalPath with
  | "/" -> 
      let! events = getEvents ()
      return! ctx |> Razor.razor "/web/index.cshtml" (List.map box events)

  // Otherwise, just serve the files from 'web' using 'index.html' as default
  | _ ->
      let webDir = Path.Combine(ctx.runtime.homeDirectory, "web")
      let subRuntime = { ctx.runtime with homeDirectory = webDir }
      let webPart =
        if ctx.request.url.LocalPath <> "/" then Files.browseHome
        else Files.browseFileHome "index.html"
      return! webPart { ctx with runtime = subRuntime } }
