// --------------------------------------------------------------------------------------
// Start up Suave.io
// --------------------------------------------------------------------------------------

#r "packages/FAKE/tools/FakeLib.dll"
#r "packages/Suave/lib/net40/Suave.dll"

open Fake
open Suave
open Suave.Http.Successful
open Suave.Web
open Suave.Types
open System.Net

let serverConfig =
    let port = getBuildParamOrDefault "port" "8083" |> Sockets.Port.Parse
    { defaultConfig with bindings = [ HttpBinding.mk HTTP IPAddress.Loopback port ] }

startWebServer serverConfig (OK("Minimalism!"))
