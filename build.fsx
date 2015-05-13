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
  let findParam name = 
    System.Environment.GetCommandLineArgs() |> Seq.pick (fun param ->
      if param.StartsWith(name) then Some(param.Substring(name.Length)) else None)

  printfn "More sutff..."
  System.Environment.GetCommandLineArgs() |> printfn "%A"

  let fromDir = findParam "--from:"
  printfn "Deploying...\nFrom:%s\n" fromDir 

  CleanDir (fromDir @@ "../wwwroot")
)

RunTargetOrDefault "All"
