#r "packages/FAKE/tools/FakeLib.dll"
open Fake

// --------------------------------------------------------------------------------------
// Minimal Azure deploy script - just overwrite old files with new ones
// --------------------------------------------------------------------------------------

Target "Deploy" (fun _ ->
  let sourceDirectory = __SOURCE_DIRECTORY__
  let wwwrootDirectory = __SOURCE_DIRECTORY__ @@ "../wwwroot"
  CleanDir wwwrootDirectory
  CopyRecursive sourceDirectory wwwrootDirectory false |> ignore
)

Target "All" DoNothing
RunTargetOrDefault "All"
