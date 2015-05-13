// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/FAKE/tools/FakeLib.dll"

open Fake



// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "All" DoNothing

Target "Test" (fun _ ->
  System.Environment.GetCommandLineArgs() |> printfn "%A"
)

Target "Deploy" (fun _ ->
  printfn "Deploying..."
  System.Environment.GetCommandLineArgs() |> printfn "%A"
)

RunTargetOrDefault "All"
