// --------------------------------------------------------------------------------------
// Start the 'app' WebPart defined in 'app.fsx' on Azure using %HTTP_PLATFORM_PORT%
// --------------------------------------------------------------------------------------

#load "app.fsx"
open App
open System
open Suave

let serverConfig =
  let port = int (Environment.GetEnvironmentVariable("HTTP_PLATFORM_PORT"))
  { Web.defaultConfig with
      homeFolder = Some __SOURCE_DIRECTORY__
      logger = Logging.Loggers.saneDefaultsFor Logging.LogLevel.Warn
      bindings = [ Types.HttpBinding.mk' Types.HTTP "127.0.0.1" port ] }

Web.startWebServer serverConfig app