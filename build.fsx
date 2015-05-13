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

  let fromDir, toDir = findParam "--from:", findParam "--to:"
  printfn "Deploying...\nFrom:%s\nTo:%s" fromDir toDir

  printfn "More sutff..."
  System.Environment.GetCommandLineArgs() |> printfn "%A"
)

RunTargetOrDefault "All"
