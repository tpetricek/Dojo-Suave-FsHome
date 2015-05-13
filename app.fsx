// --------------------------------------------------------------------------------------
// Minimal Suave.io server!
// --------------------------------------------------------------------------------------

#r "packages/Suave/lib/net40/Suave.dll"
open Suave
open Suave.Web
open Suave.Types
open Suave.Http.Successful

let app : WebPart = OK("<h1>minimalism</h1>")

// If you prefer to run things manually in F# Interactive (over running 'build' in 
// command line), then you can use the following commands to start the server
#if TESTING
// Starts the server on http://localhost:8083
let config = { defaultConfig with homeFolder = Some __SOURCE_DIRECTORY__ }
let _, server = startWebServerAsync config app
let cts = new System.Threading.CancellationTokenSource()
Async.Start(server, cts.Token)
// Kill the server (so that you can restart it)
cts.Cancel()
#endif