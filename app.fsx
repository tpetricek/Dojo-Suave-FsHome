#r "System.Xml.Linq.dll"
#r "packages/Suave/lib/net40/Suave.dll"
#r "packages/FSharp.Data/lib/net40/FSharp.Data.dll"
open System
open System.IO
open Suave
open Suave.Web
open Suave.Http
open Suave.Types
open FSharp.Data

// ----------------------------------------------------------------------------
// Step #1: Introducing Suave
// ----------------------------------------------------------------------------

// In Suave 'WebPart' represents a (part of a) web server. The following creates
// a simple web part that always succeeds and returns "Hello world!"
let app_1 = Successful.OK("Hello world!")


// You can now start the server in a number of ways - the basic is
// 'startWebServer' function. In this Dojo, you have two options - one is
// to use the `build` command (which automatically reloads your site when
// the `app.fsx` file changes) or you can run the code below from F# Interactive

if false then
  // Starts the server on http://localhost:8083
  let config = { defaultConfig with homeFolder = Some __SOURCE_DIRECTORY__ }
  let _, server = startWebServerAsync config app_1
  let cts = new System.Threading.CancellationTokenSource()
  Async.Start(server, cts.Token)
  // Kill the server (to free the port before you can restart it)
  cts.Cancel()


// ----------------------------------------------------------------------------
// Step #2: Routing using Suave combinators
// ----------------------------------------------------------------------------

open Suave.Http.Successful
open Suave.Http.RequestErrors
open Suave.Http.Applicatives

let app_2 : WebPart =
  choose
    [ path "/" >>= OK "See <a href=\"/add/40/2\">40 + 2</a>"
      pathScan "/add/%d/%d" (fun (a,b) -> OK((a + b).ToString()))
      NOT_FOUND "Found no handlers" ]

// If you feel like the world needs one more FizzBuzz, then add a handler
// that handles "/fizzbuzz/%d" and prints the result of fizz buzz for a
// number %d together with a link to the next number!


// ----------------------------------------------------------------------------
// Step #3: Doing more useful things!
// ----------------------------------------------------------------------------

// The following snippet uses the JSON type provider to call OpenWeatherMap
// to get the current temperature and weather icon for London:

type Weather = JsonProvider<"http://api.openweathermap.org/data/2.5/weather?units=metric&q=Prague">

if false then
  let city = "London,UK"
  let london = Weather.Load("http://api.openweathermap.org/data/2.5/weather?units=metric&q=" + city)
  printfn "%A" london.Main.Temp
  printfn "http://openweathermap.org/img/w/%s.png" london.Weather.[0].Icon

// Create a web server that returns a page with the current weather
// when we request: http://localhost:8083/weather/london


// ----------------------------------------------------------------------------
// Step #4: Understanding Suave WebParts and writing asynchronous servers
// ----------------------------------------------------------------------------

// Under the cover WebPart is a function 'Context -> Async<Context>'. We
// can write it directly as a function and use asynchronous operations
// in the body to avoid blocking

let waitAndReturn : WebPart = fun ctx -> async {
  let time = "1000" |> defaultArg (ctx.request.queryParam "time")
  do! Async.Sleep(int time)
  return! ctx |> OK (sprintf "Waited %d ms" (int time)) }

let app_4 =
  choose
    [ path "/wait" >>= waitAndReturn
      NOT_FOUND "Found no handlers" ]

// TODO: Improve the previous server so that it calls
// `Weather.AsyncLoad` and avoids blocking

// ----------------------------------------------------------------------------
// Step #5: Writing F# home page web site using Suave.io
// ----------------------------------------------------------------------------

// Now, we want to write "F# homepage" that will show all the things
// that matter on the internet - recent F# blogs, starred F# projects
// on GitHub and recent comments and pull requests from the F# github org.
// Here are some useful snippets: http://fssnip.net/r0

// The following uses F# Data to define types that you'll need:

type RssFeed = XmlProvider<"http://fpish.net/rss/blogs/tag/1/f~23">
type GithubSearch = JsonProvider<"jsons/github-search.json">
type GithubEvents = JsonProvider<"jsons/github-events.json">

// To get the RSS feed or to call GitHub API, you need `Http.RequestString`
// (the following sample shows `async` version of those, but feel free to
// start with a synchronous version)

let demo = async {
  // Read the RSS feed
  let! rss = Http.AsyncRequestString("http://fpish.net/rss/blogs/tag/1/f~23")

  // Get recent starred F# projects from GitHub
  let! res =
    Http.AsyncRequestString
      ( "https://api.github.com/search/repositories?q=language:fsharp&sort=stars&order=desc",
        httpMethod="GET", headers=[
          HttpRequestHeaders.Accept "application/vnd.github.v3+json";
          HttpRequestHeaders.UserAgent "SuaveDemo"] )

  // Finally, to request the F# org events, you can use the
  // following URL: https://api.github.com/orgs/fsharp/events
  // (Otherwise, the request you need is exactly the same)
  return 0 }


let getFeedNews () = async {
  // TODO: Format the news from RSS feed as HTML and return it
  let html =
    [ for item in 1 .. 10 do
        yield "<li>"
        yield sprintf "<h3><a href=\"%s\">Nothing happened (#%d)</a></h3>" "#" item
        yield sprintf "<p class=\"date\">%s</p>" "Just now"
        yield sprintf "<p>Nothing happened, nothing is happening and nothing will ever happen</p>"
        yield "</li>" ]
  return String.concat "" html }

let template = Path.Combine(__SOURCE_DIRECTORY__, "web/index.html")
let html = File.ReadAllText(template)

/// The main handler for Suave server!
let app_5 ctx = async {
  let! data = [ getFeedNews() ] |> Async.Parallel
  let html =
    html.Replace("[FEED-NEWS]", data.[0])
        .Replace("[GITHUB-NEWS]", "")
        .Replace("[GITHUB-PROJECTS]", "")
  return! ctx |> Successful.OK(html) }

// ----------------------------------------------------------------------------
// Entry point - the 'build' script assumes there is a top-level value
// called `app` - so define `app` to refer to the current step!
// ----------------------------------------------------------------------------

let app = app_1
