// --------------------------------------------------------------------------------------
// Start up Suave.io
// --------------------------------------------------------------------------------------

#r "packages/Suave/lib/net40/Suave.dll"
open Suave
open Suave.Http.Successful
open Suave.Web
open Suave.Types
open System.Net

let app : WebPart = OK("minimalism")

// --------------------------------------------------------------------------------------
// The 'app.fsx' script can be called by 'build run', in which case it skips all the 
// code below thanks to 'DO_NOT_START_SERVER'. It is also called on Heroku/Azure - on
// Heroku, we read the port from %PORT%. On Azure, we get it as parameter. We also
// need to listen on 0.0.0.0 on Heroku, but 127.0.0.1 on Azure... apparently?
// --------------------------------------------------------------------------------------

#if DO_NOT_START_SERVER
#else
let (|TryInt|_|) s = 
  match System.Int32.TryParse s with true, n -> Some n | _ -> None
let serverConfig =
  let ip, port = 
    match System.Environment.GetEnvironmentVariable("PORT"), getBuildParam "port" with
    | TryInt port, _ -> "0.0.0.0", port   // Running on Heroku
    | _, TryInt port -> "127.0.0.1", port // Running on Azure
    | _ -> "127.0.0.1", 8083              // Running locally
  { defaultConfig with
      homeFolder = Some __SOURCE_DIRECTORY__
      logger = Logging.Loggers.saneDefaultsFor Logging.LogLevel.Warn
      bindings = [ HttpBinding.mk' HTTP  ip port ] }

startWebServer serverConfig app
#endif