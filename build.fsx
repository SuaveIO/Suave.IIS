// include Fake lib
#I "packages/FAKE/tools/"
#r "FakeLib.dll"

open System
open System.IO
open Fake 
open Fake.AssemblyInfoFile
open System.Diagnostics
open Fake.Testing

let title = "Suave.IIS"

let appBuildDir = "./build/app/"
let appSrcDir = "./src/"
let testsBuildDir = "./build/tests/"
let testsSrcDir = "./tests"

let nugetBinDir = "./nuget/bin/"
let nugetOutputDir = "./nuget/output/"

let project = "Suave.IIS"
let description = "Set of helper functions for smooth running Suave web server on Internet Information Services (IIS)"

// Read release notes & version info from RELEASE_NOTES.md
let release = File.ReadLines "RELEASE_NOTES.md" |> ReleaseNotesHelper.parseReleaseNotes

// Targets
Target "?" (fun _ ->
    printfn " *********************************************************"
    printfn " *        Available options (call 'build <Target>')      *"
    printfn " *********************************************************"
    printfn " [Build]"
    printfn "  > BuildApp"
    printfn " "
    printfn " [Deploy]"
    printfn "  > Nuget"
    printfn " "
    printfn " [Help]"
    printfn "  > ?"
    printfn " "
    printfn " *********************************************************"
)

Target "AssemblyInfo" <| fun () ->
    for file in !! ("./src/**/AssemblyInfo*.fs") do
        let version = release.AssemblyVersion
        let dirName = FileInfo(file).Directory.Name
        CreateFSharpAssemblyInfo file
           [ Attribute.Title dirName
             Attribute.Product title
             Attribute.Description description
             Attribute.Version version
             Attribute.FileVersion version]

Target "CleanApp" (fun _ ->
    CleanDir appBuildDir
)

Target "BuildApp" (fun _ ->
    for file in !! (appSrcDir + "**/*.fsproj") do
        let dir = appBuildDir + FileInfo(file).Directory.Name
        MSBuildRelease dir "Build" [file] |> Log "Build-Output:"
)

Target "Nuget" <| fun () ->
    CreateDir nugetOutputDir
    CreateDir nugetBinDir
    let nugetFiles = [
        "Suave.IIS.xml"
        "Suave.IIS.dll"
    ]
    nugetFiles |> List.map (fun f -> appBuildDir + "Suave.IIS/" + f) |> CopyFiles nugetBinDir
    
    // Format the release notes
    let releaseNotes = release.Notes |> String.concat "\n"
    NuGet (fun p -> 
        { p with   
            Project = project
            Description = description
            Version = release.NugetVersion
            ReleaseNotes = releaseNotes
            OutputPath = nugetOutputDir
            References = nugetFiles |> List.filter (fun x -> x.EndsWith(".dll"))
            Files = nugetFiles |> List.map (fun f -> ("bin/" + f, Some("lib/net45"), None))
            Dependencies =
            [
                "Suave", GetPackageVersion ("./packages") "Suave"
            ]
        })
        "nuget/Suave.IIS.nuspec"

// Dependencies
"CleanApp" ==> "AssemblyInfo" ==> "BuildApp"
"BuildApp" ==> "Nuget"


// start build
RunTargetOrDefault "?"